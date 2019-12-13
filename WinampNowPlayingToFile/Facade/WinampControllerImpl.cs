using System;
using Daniel15.Sharpamp;

namespace WinampNowPlayingToFile.Facade {

    public interface WinampController {

        Status Status { get; }
        Song CurrentSong { get; }

        void Play();
        void PlayPause();
        void Stop();
        void NextTrack();
        void PreviousTrack();

        event SongChangedEventHandler SongChanged;
        event StatusChangedEventHandler StatusChanged;

    }

    public class WinampControllerImpl: WinampController {

        private readonly Winamp winamp;

        public event SongChangedEventHandler SongChanged;
        public event StatusChangedEventHandler StatusChanged;

        public WinampControllerImpl(Winamp winamp) {
            this.winamp = winamp;
            winamp.SongChanged += (sender, args) => SongChanged?.Invoke(sender, new SongChangedEventArgs(args));
            winamp.StatusChanged += (sender, args) => StatusChanged?.Invoke(sender, args);
        }

        public Status Status => winamp.Status;

        public Song CurrentSong => new Song(winamp.CurrentSong);

        public void NextTrack() {
            winamp.NextTrack();
        }

        public void PlayPause() {
            winamp.PlayPause();
        }

        public void Play() {
            winamp.Play();
        }

        public void PreviousTrack() {
            winamp.PrevTrack();
        }

        public void Stop() {
            winamp.Stop();
        }

    }

    public delegate void SongChangedEventHandler(object sender, SongChangedEventArgs args);

    public class SongChangedEventArgs: EventArgs {

        public Song Song { get; set; }

        public SongChangedEventArgs(Song song) {
            Song = song;
        }

        public SongChangedEventArgs(Daniel15.Sharpamp.SongChangedEventArgs args): this(new Song(args.Song)) { }

    }

}