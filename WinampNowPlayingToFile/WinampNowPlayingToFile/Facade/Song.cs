namespace WinampNowPlayingToFile.Facade
{
    public class Song
    {
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Filename { get; set; }

        public Song()
        {
        }

        public Song(Daniel15.Sharpamp.Song song)
        {
            Artist = song.Artist;
            Album = song.Album;
            Title = song.Title;
            Year = int.Parse(song.Year);
            Filename = song.Filename;
        }
    }
}