#nullable enable

using System;
using System.Collections.Generic;

namespace WinampNowPlayingToFile.Settings;

public interface ISettings {

    IList<string> textFilenames { get; }
    string albumArtFilename { get; set; }
    IList<string> textTemplates { get; }
    bool preserveAlbumArtFileWhenNotPlaying { get; set; }
    bool preserveTextFileWhenNotPlaying { get; set; }

    event EventHandler settingsUpdated;

    void load();
    void load(ISettings source);
    ISettings loadDefaults();
    void save();

}