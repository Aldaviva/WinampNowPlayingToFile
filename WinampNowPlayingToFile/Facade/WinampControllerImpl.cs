﻿#nullable enable

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
    object fetchMetadataFieldValue(string metadataFieldName);

    event SongChangedEventHandler songChanged;
    event StatusChangedEventHandler statusChanged;

}

public class WinampControllerImpl: WinampController {

    private readonly Winamp winamp;

    // ReSharper disable once InconsistentNaming - this is how the method is named in Sharpamp
    private readonly Func<int, int>               sendIPCCommandInt;
    private readonly Func<string, string, string> getMetadata;

    public event SongChangedEventHandler? songChanged;
    public event StatusChangedEventHandler? statusChanged;

    private volatile bool disposed;

    public WinampControllerImpl(Winamp winamp) {
        this.winamp          =  winamp;
        winamp.SongChanged   += (sender, args) => songChanged?.Invoke(sender, new SongChangedEventArgs(args));
        winamp.StatusChanged += (sender, args) => statusChanged?.Invoke(sender, args);

        getMetadata = (Func<string, string, string>) winamp.GetType()
            .GetMethod("GetMetadata", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(string), typeof(string)], null)!
            .CreateDelegate(typeof(Func<string, string, string>), winamp);

        Type ipcCommand = winamp.GetType().GetNestedType("IPCCommand", BindingFlags.NonPublic);

        sendIPCCommandInt = (Func<int, int>) winamp.GetType()
            .GetMethod("SendIPCCommandInt", BindingFlags.NonPublic | BindingFlags.Instance, null, [ipcCommand], null)!
            .CreateDelegate(typeof(Func<int, int>), winamp);
    }

    public Status status => disposed ? Status.Stopped : winamp.Status;

    public Song currentSong => new(winamp.CurrentSong);

    public void nextTrack() {
        winamp.NextTrack();
    }

    public void playPause() {
        winamp.PlayPause();
    }

    public void play() {
        winamp.Play();
    }

    public void previousTrack() {
        winamp.PrevTrack();
    }

    public void stop() {
        winamp.Stop();
    }

    public object fetchMetadataFieldValue(string metadataFieldName) {
        metadataFieldName = metadataFieldName.ToLowerInvariant();

        try {
            switch (metadataFieldName) {
                case "filebasename":
                    return Path.GetFileName(winamp.CurrentSong.Filename);
                case "filebasenamewithoutextension":
                    return Path.GetFileNameWithoutExtension(winamp.CurrentSong.Filename);
                case "elapsed":
                    return TimeSpan.FromMilliseconds(sendIPCCommandInt(105));
                case "playbackstate":
                    return winamp.Status.ToString().ToLowerInvariant();
            }
        } catch (ArgumentException) {
            return string.Empty;
        }

        string value = getMetadata(winamp.CurrentSong.Filename, metadataFieldName);

        return metadataFieldName switch {
            "length" when long.TryParse(value, out long length)                                                     => TimeSpan.FromMilliseconds(length),
            "lossless" or "stereo" or "vbr"                                                                         => value == "1",
            "replaygain_album_peak" or "replaygain_track_peak" when double.TryParse(value, out double parsedDouble) => parsedDouble,
            "bitrate" or "bpm" or "track" or "disc" or "rating" when int.TryParse(value, out int parsedInt)         => parsedInt,
            "type"                                                                                                  => value == "1" ? "video" : "audio",
            _                                                                                                       => value
        };
    }

    public void Dispose() {
        disposed = true;
        GC.SuppressFinalize(this);
    }

}

public delegate void SongChangedEventHandler(object sender, SongChangedEventArgs args);

public class SongChangedEventArgs(Song song): EventArgs {

    public Song song { get; } = song;

    public SongChangedEventArgs(Daniel15.Sharpamp.SongChangedEventArgs args): this(new Song(args.Song)) { }

}