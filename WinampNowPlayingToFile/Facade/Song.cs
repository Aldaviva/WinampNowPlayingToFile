namespace WinampNowPlayingToFile.Facade {

    public class Song {

        public string artist { get; set; }
        public string album { get; set; }
        public string title { get; set; }
        public int? year { get; set; }
        public string filename { get; set; }

        public Song() { }

        public Song(Daniel15.Sharpamp.Song song) {
            artist = song.Artist;
            album = song.Album;
            title = song.Title;
            this.year = int.TryParse(song.Year, out int year) ? year : (int?) null;
            filename = song.Filename;
        }

        public override string ToString() {
            return
                $"{nameof(artist)}: {artist}, {nameof(album)}: {album}, {nameof(title)}: {title}, {nameof(year)}: {year}, {nameof(filename)}: {filename}";
        }

    }

}