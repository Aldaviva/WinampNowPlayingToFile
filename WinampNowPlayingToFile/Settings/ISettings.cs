using System;

namespace WinampNowPlayingToFile.Settings {

    public interface ISettings {

        string textFilename { get; set; }
        string albumArtFilename { get; set; }
        string textTemplate { get; set; }

        event EventHandler settingsUpdated;

        void load();
        ISettings loadDefaults();
        void save();

    }

}