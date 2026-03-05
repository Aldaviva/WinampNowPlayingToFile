#nullable enable

namespace WinampNowPlayingToFile.Data;

// ReSharper disable InconsistentNaming - Property names are used in public-facing Mustache templates and cannot be changed.
public class Song {

    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string Filename { get; set; } = string.Empty;

    public override string ToString() => $"{nameof(Artist)}: {Artist}, {nameof(Album)}: {Album}, {nameof(Title)}: {Title}, {nameof(Year)}: {Year}, {nameof(Filename)}: {Filename}";

}