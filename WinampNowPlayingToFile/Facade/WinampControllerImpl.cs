#nullable enable

using System;
using System.Reflection;
using Daniel15.Sharpamp;

namespace WinampNowPlayingToFile.Facade;

public interface WinampController {

    Status status { get; }
    Song currentSong { get; }

    void play();
    void playPause();
    void stop();
    void nextTrack();
    void previousTrack();
    string fetchMetadataFieldValue(string metadataFieldName);

    event SongChangedEventHandler songChanged;
    event StatusChangedEventHandler statusChanged;

}

public class WinampControllerImpl: WinampController {

    private readonly Winamp                       winamp;
    private readonly Func<string, string, string> getMetadata;

    public event SongChangedEventHandler? songChanged;
    public event StatusChangedEventHandler? statusChanged;

    public WinampControllerImpl(Winamp winamp) {
        this.winamp          =  winamp;
        winamp.SongChanged   += (sender, args) => songChanged?.Invoke(sender, new SongChangedEventArgs(args));
        winamp.StatusChanged += (sender, args) => statusChanged?.Invoke(sender, args);

        getMetadata = (Func<string, string, string>) winamp.GetType()
            .GetMethod("GetMetadata", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string), typeof(string) }, null)!
            .CreateDelegate(typeof(Func<string, string, string>), winamp);
    }

    public Status status => winamp.Status;

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

    public string fetchMetadataFieldValue(string metadataFieldName) {
        string value = getMetadata(winamp.CurrentSong.Filename, metadataFieldName);

        if (metadataFieldName.Equals("length", StringComparison.InvariantCultureIgnoreCase) && long.TryParse(value, out long numericLength)) {
            TimeSpan duration = TimeSpan.FromMilliseconds(numericLength);
            value = $"{duration.Minutes:D1}:{duration.Seconds:D2}";
        }

        return value;
    }

}

public delegate void SongChangedEventHandler(object sender, SongChangedEventArgs args);

public class SongChangedEventArgs: EventArgs {

    public Song song { get; }

    public SongChangedEventArgs(Song song) {
        this.song = song;
    }

    public SongChangedEventArgs(Daniel15.Sharpamp.SongChangedEventArgs args): this(new Song(args.Song)) { }

}