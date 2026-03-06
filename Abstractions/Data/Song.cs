namespace WinampNowPlayingToFile.Data;

// ReSharper disable InconsistentNaming - Property names are used in public-facing Mustache templates and cannot be changed.
/// <summary>
/// A song or other multimedia file being played by Winamp.
/// </summary>
public class Song {

    /// <summary>
    /// The song's musical artist or band name, or <see cref="string.Empty"/> if it's unknown.
    /// </summary>
    public string Artist { get; set; } = string.Empty;

    /// <summary>
    /// The song's album name, or <see cref="string.Empty"/> if it's unknown.
    /// </summary>
    public string Album { get; set; } = string.Empty;

    /// <summary>
    /// The name of the song, or <see cref="string.Empty"/> if it's unknown.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The year the song or album was released, or <c>null</c> if it's unknown.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// The absolute filename of the song file, or a URL for a network stream.
    /// </summary>
    public string Filename { get; set; } = string.Empty;

    /// <inheritdoc />
    public override string ToString() => $"{nameof(Artist)}: {Artist}, {nameof(Album)}: {Album}, {nameof(Title)}: {Title}, {nameof(Year)}: {Year}, {nameof(Filename)}: {Filename}";

}