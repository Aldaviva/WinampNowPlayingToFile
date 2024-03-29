﻿#nullable enable

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
        Year     = int.TryParse(song.Year, out int year) ? year : null;
        Filename = song.Filename;
    }

    public override string ToString() {
        return
            $"{nameof(Artist)}: {Artist}, {nameof(Album)}: {Album}, {nameof(Title)}: {Title}, {nameof(Year)}: {Year}, {nameof(Filename)}: {Filename}";
    }

}