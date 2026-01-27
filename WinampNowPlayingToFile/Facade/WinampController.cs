#nullable enable

using Daniel15.Sharpamp;
using System;
using System.IO;
using System.Reflection;

namespace WinampNowPlayingToFile.Facade;

public interface WinampController: IDisposable {

    Status status { get; }
    Song currentSong { get; }

    void play();
    void playPause();
    void stop();
    void nextTrack();
    void previousTrack();
    object? fetchMetadataFieldValue(string metadataFieldName);

    event EventHandler<SongChangedEventArgs> songChanged;
    event EventHandler<StatusChangedEventArgs> statusChanged;

}

public class WinampControllerImpl: WinampController {

    private static readonly char[] TRACK_SEPARATORS = ['/'];

    private readonly Winamp winamp;

    // ReSharper disable once InconsistentNaming - this is how the method is named in Sharpamp
    private readonly Func<int, int>               sendIPCCommandIntDelegate;
    private readonly Func<string, string, string> getMetadataDelegate;

    public event EventHandler<SongChangedEventArgs>? songChanged;
    public event EventHandler<StatusChangedEventArgs>? statusChanged;

    private volatile bool disposed;

    public WinampControllerImpl(Winamp winamp) {
        this.winamp          =  winamp;
        winamp.SongChanged   += (sender, args) => songChanged?.Invoke(sender, new SongChangedEventArgs(args));
        winamp.StatusChanged += (sender, args) => statusChanged?.Invoke(sender, args);

        Type winampClass = winamp.GetType();
        getMetadataDelegate = (Func<string, string, string>) winampClass.GetMethod("GetMetadata", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(string), typeof(string)], null)!
            .CreateDelegate(typeof(Func<string, string, string>), winamp);

        Type ipcCommand = winampClass.GetNestedType("IPCCommand", BindingFlags.NonPublic);
        sendIPCCommandIntDelegate = (Func<int, int>) winampClass.GetMethod("SendIPCCommandInt", BindingFlags.NonPublic | BindingFlags.Instance, null, [ipcCommand], null)!
            .CreateDelegate(typeof(Func<int, int>), winamp);
    }

    private string getMetadata(string filename, string metadataFieldName) => getMetadataDelegate(filename, metadataFieldName);
    private int sendIPCCommand(int arg) => sendIPCCommandIntDelegate(arg);

    public Status status => disposed ? Status.Stopped : winamp.Status;

    public Song currentSong => new(winamp.CurrentSong);

    public void nextTrack() => winamp.NextTrack();

    public void playPause() => winamp.PlayPause();

    public void play() => winamp.Play();

    public void previousTrack() => winamp.PrevTrack();

    public void stop() => winamp.Stop();

    public object? fetchMetadataFieldValue(string metadataFieldName) {
        metadataFieldName = metadataFieldName.ToLowerInvariant();

        try {
            switch (metadataFieldName) {
                case "filebasename":
                    return Path.GetFileName(winamp.CurrentSong.Filename);
                case "filebasenamewithoutextension":
                    return Path.GetFileNameWithoutExtension(winamp.CurrentSong.Filename);
                case "directory":
                    try {
                        return Path.GetDirectoryName(winamp.CurrentSong.Filename);
                    } catch (ArgumentException) {
                        return null;
                    }
                case "elapsed":
                    return TimeSpan.FromMilliseconds(sendIPCCommand(105));
                case "playbackstate":
                    return winamp.Status.ToString().ToLowerInvariant();

                // would normally be redundant, but prevents unnecessary IPC when metadata name case does not match
                case "artist":
                    return winamp.CurrentSong.Artist;
                case "album":
                    return winamp.CurrentSong.Album;
                case "title":
                    return winamp.CurrentSong.Title;
                case "year":
                    return winamp.CurrentSong.Year;
                case "filename":
                    return winamp.CurrentSong.Filename;
            }
        } catch (ArgumentException) {
            return string.Empty;
        }

        if (metadataFieldName == "rating_stars") {
            int.TryParse(getMetadata(winamp.CurrentSong.Filename, "rating"), out int starCount);
            return starCount switch {
                1 => "★",
                2 => "★★",
                3 => "★★★",
                4 => "★★★★",
                5 => "★★★★★",
                _ => null
            };
        }

        string metadataValue = getMetadata(winamp.CurrentSong.Filename, metadataFieldName);

        return metadataFieldName switch {
            "length"                                                     => long.TryParse(metadataValue, out long length) ? TimeSpan.FromMilliseconds(length) : null,
            "lossless" or "stereo" or "vbr"                              => metadataValue == "1",
            "replaygain_album_peak" or "replaygain_track_peak"           => double.TryParse(metadataValue, out double parsed) ? parsed : null,
            "bitrate" or "bpm" or "rating"                               => int.TryParse(metadataValue, out int parsed) ? parsed : null,
            "track" or "disc"                                            => int.TryParse(metadataValue.Split(TRACK_SEPARATORS, 2)[0], out int parsed) ? parsed : null,
            "type"                                                       => metadataValue == "1" ? "video" : "audio",
            "gain" or "replaygain_album_gain" or "replaygain_track_gain" => double.TryParse(metadataValue.Replace(" dB", ""), out double parsed) ? parsed : null,
            _ when metadataValue is ""                                   => null,
            _                                                            => metadataValue
        };
    }

    public void Dispose() {
        disposed = true;
        GC.SuppressFinalize(this);
    }

}

public class SongChangedEventArgs(Song song): EventArgs {

    public Song song { get; } = song;

    public SongChangedEventArgs(Daniel15.Sharpamp.SongChangedEventArgs args): this(new Song(args.Song)) {}

}