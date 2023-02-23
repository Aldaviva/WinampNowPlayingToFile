#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace WinampNowPlayingToFile.Facade;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification =
    "Property names are used in public-facing Mustache templates and cannot be changed.")]
public class Song {

    public string Artist { get; set; } = null!;
    public string Album { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int? Year { get; set; }
    public string Filename { get; set; } = null!;

    public Song() { }

    public Song(Daniel15.Sharpamp.Song song) {
        Artist   = song.Artist;
        Album    = song.Album;
        Title    = song.Title;
        Year     = int.TryParse(song.Year, out int year) ? year : null;
        Filename = song.Filename;
    }

    public override string ToString() {
        return
            $"{nameof(Artist)}: {Artist}, {nameof(Album)}: {Album}, {nameof(Title)}: {Title}, {nameof(Year)}: {Year}, {nameof(Filename)}: {Filename}";
    }

}