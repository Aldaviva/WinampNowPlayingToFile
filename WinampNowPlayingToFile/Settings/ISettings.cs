namespace WinampNowPlayingToFile.Settings
{
    public interface ISettings
    {
        string NowPlayingFilename { get; set; }
        string NowPlayingTemplate { get; set; }

        void Load();
        ISettings LoadDefaults();
        void Save();
    }
}