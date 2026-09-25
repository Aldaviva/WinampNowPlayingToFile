namespace WinampNowPlayingToFile.Data;

/// <summary>Settings for the WinampNowPlayingToFile plugin</summary>
public interface IReadOnlySettings {

    /// <summary>One or more absolute filenames of files to write text metadata of the currently playing song</summary>
    IList<string> textFilenames { get; }

    /// <summary>Optional absolute filename of file to write image of album artwork of the currently playing song</summary>
    string? albumArtFilename { get; set; }

    /// <summary>One or more mustache templates to control the format of the contents of <see cref="textFilenames"/>, in the same order</summary>
    IList<string> textTemplates { get; }

    /// <summary><c>false</c> to replace the <see cref="albumArtFilename"/> file with a placeholder or delete it when Winamp pauses, stops, or exits; or <c>true</c> to leave the file untouched on disk</summary>
    bool preserveAlbumArtFileWhenNotPlaying { get; set; }

    /// <summary><c>false</c> to empty (truncate to 0 bytes) the <see cref="textFilenames"/> files when Winamp pauses, stops, or exits; or <c>true</c> to leave the files untouched on disk</summary>
    bool preserveTextFilesWhenNotPlaying { get; set; }

}