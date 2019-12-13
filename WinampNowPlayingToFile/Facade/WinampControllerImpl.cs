using System;
using Daniel15.Sharpamp;

namespace WinampNowPlayingToFile.Facade {

    public interface WinampController {

        Status status { get; }
        Song currentSong { get; }

        void play();
        void playPause();
        void stop();
        void nextTrack();
        void previousTrack();

        event SongChangedEventHandler songChanged;
        event StatusChangedEventHandler statusChanged;

    }

    public class WinampControllerImpl: WinampController {

        private readonly Winamp winamp;

        public event SongChangedEventHandler songChanged;
        public event StatusChangedEventHandler statusChanged;

        public WinampControllerImpl(Winamp winamp) {
            this.winamp = winamp;
            winamp.SongChanged += (sender, args) => songChanged?.Invoke(sender, new SongChangedEventArgs(args));
            winamp.StatusChanged += (sender, args) => statusChanged?.Invoke(sender, args);
        }

        public Status status => winamp.Status;

        public Song currentSong => new Song(winamp.CurrentSong);

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

    }

    public delegate void SongChangedEventHandler(object sender, SongChangedEventArgs args);

    public class SongChangedEventArgs: EventArgs {

        public Song song { get; set; }

        public SongChangedEventArgs(Song song) {
            this.song = song;
        }

        public SongChangedEventArgs(Daniel15.Sharpamp.SongChangedEventArgs args): this(new Song(args.Song)) { }

    }

}