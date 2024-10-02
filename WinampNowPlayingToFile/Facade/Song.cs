#nullable enable

using System.Text.RegularExpressions;

namespace WinampNowPlayingToFile.Facade;

// ReSharper disable InconsistentNaming - Property names are used in public-facing Mustache templates and cannot be changed.
public class Song {

    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string Filename { get; set; } = string.Empty;

    public Song() { }

    public Song(Daniel15.Sharpamp.Song song) {
        Artist   = song.Artist;
        Album    = song.Album;
        Title    = song.Title;
        Year     = parseYear(song);
        Filename = song.Filename;
    }

    private static int? parseYear(Daniel15.Sharpamp.Song song) {
        if (int.TryParse(song.Year, out int year)) {
            return year;
        } else if (Regex.Match(song.Year, @"(?<year>\d{4})-\d\d-\d\d") is { Success: true } isoDateMatch) {
            return int.Parse(isoDateMatch.Groups["year"].Value);
        } else if (Regex.Match(song.Year, @"\d{4}") is { Success: true } yearMatch) {
            return int.Parse(yearMatch.Value);
        } else {
            return null;
        }
    }

    public override string ToString() => $"{nameof(Artist)}: {Artist}, {nameof(Album)}: {Album}, {nameof(Title)}: {Title}, {nameof(Year)}: {Year}, {nameof(Filename)}: {Filename}";

}