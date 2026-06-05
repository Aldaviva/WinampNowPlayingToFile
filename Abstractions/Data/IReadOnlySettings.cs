namespace WinampNowPlayingToFile.Data;

public interface IReadOnlySettings {

    IList<string> textFilenames { get; }
    string? albumArtFilename { get; set; }
    IList<string> textTemplates { get; }
    bool preserveAlbumArtFileWhenNotPlaying { get; set; }
    bool preserveTextFileWhenNotPlaying { get; set; }

}