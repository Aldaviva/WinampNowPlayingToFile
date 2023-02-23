#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Daniel15.Sharpamp;
using Mustache;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Settings;
using Song = WinampNowPlayingToFile.Facade.Song;

namespace WinampNowPlayingToFile.Business;

public class NowPlayingToFileManager {

    private static readonly FormatCompiler TEMPLATE_COMPILER = new();

    private static byte[] defaultAlbumArt {
        get {
            try {
                return File.ReadAllBytes("emptyAlbumArt.png");
            } catch {
                return Resources.black_png;
            }
        }
    }

    private readonly WinampController winampController;
    private readonly ISettings        settings;

    private Generator? cachedTemplate;

    public NowPlayingToFileManager(ISettings settings, WinampController winampController) {
        this.winampController = winampController;
        this.settings         = settings;

        this.winampController.songChanged   += delegate { update(); };
        this.winampController.statusChanged += delegate { update(); };
        this.settings.settingsUpdated += delegate {
            cachedTemplate = null;
            update();
        };
    }

    internal void update() {
        try {
            Song currentSong = winampController.currentSong;
            saveText(renderText(currentSong));
            saveImage(findAlbumArt(currentSong));
        } catch (Exception e) {
            MessageBox.Show("Unhandled exception while updating song info on song change:\n" + e, "Now Playing To File error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    internal string renderText(Song currentSong) {
        return winampController.status == Status.Playing ? getTemplate().Render(currentSong) : string.Empty;
    }

    private void saveText(string nowPlayingText) {
        File.WriteAllText(settings.textFilename, nowPlayingText, new UTF8Encoding(false, true));
    }

    private Generator getTemplate() {
        return cachedTemplate ??= TEMPLATE_COMPILER.Compile(settings.textTemplate);
    }

    internal byte[]? findAlbumArt(Song currentSong) {
        return winampController.status == Status.Playing ? extractAlbumArt(currentSong) ?? findAlbumArtSidecarFile(currentSong) ?? defaultAlbumArt : null;
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
        IEnumerable<string> artworkExtensions = new[] { ".bmp", ".gif", ".jpeg", ".jpg", ".png" };
        IEnumerable<string> artworkBaseNames  = new[] { "cover", "folder", "front", "albumart" };
        DirectoryInfo       songDirectory;

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
            .Concat(artworkBaseNames.SelectMany(basename => songDirectory.EnumerateFiles(basename + ".*")))
            .Where(file => artworkExtensions.Contains(file.Extension.ToLowerInvariant()))
            .Select(file => {
                try {
                    return File.ReadAllBytes(file.FullName);
                } catch (Exception e) when (e is not OutOfMemoryException) {
                    return null;
                }
            })
            .FirstOrDefault(bytes => bytes != null);
    }

    private void saveImage(byte[]? imageData) {
        string filename = settings.albumArtFilename;
        if (imageData != null) {
            File.WriteAllBytes(filename, imageData);
        } else {
            File.Delete(filename);
        }
    }

    public virtual void onQuit() {
        saveText(string.Empty);
        saveImage(null);
    }

}