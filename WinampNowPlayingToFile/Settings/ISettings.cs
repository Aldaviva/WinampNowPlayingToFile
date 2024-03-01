using System;
using System.Collections.Generic;

using WinampNowPlayingToFile.Facade;

namespace WinampNowPlayingToFile.Settings;

public partial interface ISettings {

    string albumArtFilename { get; set; }

    abstract List<textTemplate> textTemplates { get; set; }

    event EventHandler settingsUpdated;

    void load();
    ISettings loadDefaults();
    textTemplate getDefault();
    void save();

}