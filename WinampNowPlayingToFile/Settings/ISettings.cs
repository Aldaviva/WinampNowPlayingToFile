using System;

namespace WinampNowPlayingToFile.Settings
{
    public interface ISettings
    {
        string TextFilename { get; set; }
        string AlbumArtFilename { get; set; }
        string TextTemplate { get; set; }

        event EventHandler SettingsUpdated;

        void Load();
        ISettings LoadDefaults();
        void Save();
    }
}