#nullable enable

using Daniel15.Sharpamp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Facade.Templating;
using WinampNowPlayingToFile.Settings;
using Song = WinampNowPlayingToFile.Facade.Song;

namespace WinampNowPlayingToFile.Business;

public interface INowPlayingToFileManager {

    event EventHandler<NowPlayingException>? error;

    void onQuit();

}

public class NowPlayingToFileManager: INowPlayingToFileManager {

    private static readonly UTF8Encoding        UTF8               = new(false, true);
    private static readonly ISet<string>        ARTWORK_EXTENSIONS = new HashSet<string> { ".bmp", ".gif", ".jpeg", ".jpg", ".png" };
    private static readonly IEnumerable<string> ARTWORK_BASE_NAMES = ["cover", "folder", "front", "albumart"];

    private static byte[]? albumArtWhenMissingFromSong => getInstallationDirectoryImageOrFallback("emptyAlbumArt.png");
    private static byte[]? albumArtWhenStopped => getInstallationDirectoryImageOrFallback("stoppedAlbumArt.png");

    private readonly  WinampController          winampController;
    private readonly  ISettings                 settings;
    private readonly  UnfuckedTemplateCompiler  templateCompiler = new UnfuckedMustacheCompiler();
    internal readonly Timer                     renderTextTimer  = new(1000);
    private readonly  IList<UnfuckedGenerator?> cachedTemplates;

    private bool  _textTemplateDependsOnTime;
    private Song? previousSong;

    private bool textTemplateDependsOnTime {
        get => _textTemplateDependsOnTime;
        set {
            if (_textTemplateDependsOnTime != value) {
                _textTemplateDependsOnTime = value;
                startOrStopTextRenderingTimer();
            }
        }
    }

    public event EventHandler<NowPlayingException>? error;

    public NowPlayingToFileManager(ISettings settings, WinampController winampController) {
        this.winampController = winampController;
        this.settings         = settings;

        // templateCompiler.RegisterTag(new JsonValueSerializer(), false);
        // templateCompiler.RegisterTag(new JsonObjectSerializer(), false);
        cachedTemplates = new List<UnfuckedGenerator?>(settings.textFilenames.Count);

        templateCompiler.placeholderFound += (_, args) => {
            if (args.key.Equals("Elapsed", StringComparison.OrdinalIgnoreCase)) {
                textTemplateDependsOnTime = true;
            }
        };

        this.winampController.songChanged += delegate { update(); };

        this.winampController.statusChanged += (_, args) => {
            update();
            startOrStopTextRenderingTimer(args.Status);
        };

        settings.settingsUpdated += delegate {
            for (int templateIndex = 0; templateIndex < cachedTemplates.Count; templateIndex++) {
                cachedTemplates[templateIndex] = null;
            }
            textTemplateDependsOnTime = false;
            update();
        };

        renderTextTimer.Elapsed += (_, _) => { update(false); };

        update();
    }

    internal void update(bool updateAlbumArt = true) {
        try {
            if (winampController.currentSong is { Filename: not "" } currentSong) {
                previousSong = currentSong;

                for (int templateIndex = 0; templateIndex < settings.textFilenames.Count; templateIndex++) {
                    saveText(renderText(currentSong, templateIndex), templateIndex);
                }

                if (updateAlbumArt) {
                    saveImage(findAlbumArt(currentSong));
                }
            }
        } catch (Exception e) when (e is not OutOfMemoryException) {
            error?.Invoke(this, new NowPlayingException("Exception while updating song", e, winampController.currentSong));
        }
    }

    internal string renderText(Song currentSong, int templateIndex) {
        Song? songToRender = winampController.status == Status.Playing || (!settings.preserveTextFileWhenNotPlaying && currentSong is { Filename: not "" }) ? currentSong : previousSong;
        return songToRender != null ? getTemplate(templateIndex).render(songToRender) : string.Empty;
    }

    private void saveText(string nowPlayingText, int templateIndex) {
        string filename = settings.textFilenames[templateIndex];
        if (!string.IsNullOrWhiteSpace(filename)) {
            File.WriteAllText(filename, nowPlayingText, UTF8);
        }
    }

    private UnfuckedGenerator getTemplate(int templateIndex) {
        if (cachedTemplates.ElementAtOrDefault(templateIndex) is not { } cachedTemplate) {
            cachedTemplate                 =  templateCompiler.compile(settings.textTemplates[templateIndex]);
            cachedTemplate.keyNotFound     += fetchExtraMetadata;
            cachedTemplates[templateIndex] =  cachedTemplate;
        }

        return cachedTemplate;
    }

    private void fetchExtraMetadata(object sender, UnfuckedKeyNotFoundEventArgs args) {
        args.substitute = winampController.fetchMetadataFieldValue(args.key);
        args.handled    = true;
    }

    internal byte[]? findAlbumArt(Song currentSong) {
        return winampController.status == Status.Playing
            ? extractAlbumArt(currentSong) ?? findAlbumArtSidecarFile(currentSong) ?? albumArtWhenMissingFromSong
            : settings.preserveAlbumArtFileWhenNotPlaying
                ? null
                : albumArtWhenStopped;
    }

    private static byte[]? extractAlbumArt(Song currentSong) {
        try {
            return TagLib.File.Create(currentSong.Filename)
                .Tag
                .Pictures
                .ElementAtOrDefault(0)?
                .Data
                .Data;
        } catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException) {
            /*
             * Probably just a race:
             * 1. Stop playing a song
             * 2. Delete that song or its parent directory
             * 3. Start playing a new song
             * 4. Status property changed, and StatusChanged event fired
             * 5. CurrentSong property updated with new filename and other metadata, and SongChanged event fired
             *
             * It seems like the StatusChanged event is fired before the CurrentSong property is updated, so this plugin tries to extract album art from the deleted file.
             * We can ignore this because the SongChanged event will be fired immediately afterwards, so we will get the correct artwork from that.
             */
            return null;
        } catch (Exception e) when (e is not OutOfMemoryException) {
            /*
             * TagLib cannot read the metadata from the given file. This can happen with MIDI music or URIs, for instance.
             */
            return null;
        }
    }

    private static byte[]? findAlbumArtSidecarFile(Song currentSong) {
        DirectoryInfo songDirectory;

        try {
            if (Path.GetDirectoryName(currentSong.Filename) is { } dir) {
                songDirectory = new DirectoryInfo(dir);
            } else {
                return null;
            }
        } catch (NotSupportedException) {
            // currentSong.Filename is a URI
            return null;
        }

        /*
         * %album%.[bmp|gif|jpeg|jpg|png]
         */
        try {
            return songDirectory.EnumerateFiles(Path.GetInvalidFileNameChars().Aggregate(currentSong.Album, (album, invalid) => album.Replace(invalid.ToString(), string.Empty)) + ".*")
                /*
                 * (.*)\.nfo → $1.[bmp|gif|jpeg|jpg|png]
                 */
                .Concat(songDirectory.EnumerateFiles("*.nfo")
                    .SelectMany(nfoFile => songDirectory.EnumerateFiles(Path.GetFileNameWithoutExtension(nfoFile.Name) + ".*")))
                /*
                 * cover.[bmp|gif|jpeg|jpg|png]
                 * folder.[bmp|gif|jpeg|jpg|png]
                 * front.[bmp|gif|jpeg|jpg|png]
                 * albumart.[bmp|gif|jpeg|jpg|png]
                 */
                .Concat(ARTWORK_BASE_NAMES.SelectMany(basename => songDirectory.EnumerateFiles(basename + ".*")))
                .Where(file => ARTWORK_EXTENSIONS.Contains(file.Extension.ToLowerInvariant()))
                .Select(file => {
                    try {
                        return File.ReadAllBytes(file.FullName);
                    } catch (Exception e) when (e is not OutOfMemoryException) {
                        return null;
                    }
                })
                .FirstOrDefault(bytes => bytes != null);
        } catch (DirectoryNotFoundException) {
            return null;
        }
    }

    private void saveImage(byte[]? imageData) {
        string filename = settings.albumArtFilename;
        if ((settings.preserveAlbumArtFileWhenNotPlaying && winampController.status != Status.Playing) || string.IsNullOrWhiteSpace(filename)) {
            // #19: user wants to keep old album art while not playing
        } else if (imageData != null) {
            File.WriteAllBytes(filename, imageData);
        } else {
            File.Delete(filename);
        }
    }

    private void startOrStopTextRenderingTimer(Status? playbackStatus = null) {
        playbackStatus          ??= winampController.status;
        renderTextTimer.Enabled =   playbackStatus == Status.Playing && textTemplateDependsOnTime;
    }

    public virtual void onQuit() {
        renderTextTimer.Stop();
        update();
        // saveText(string.Empty);
        // saveImage(albumArtWhenStopped);
    }

    private static byte[]? getInstallationDirectoryImageOrFallback(string filename) {
        try {
            return File.ReadAllBytes(filename);
        } catch (Exception) {
            return null;
        }
    }

}