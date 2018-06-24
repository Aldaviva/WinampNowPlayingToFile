using System.Windows.Forms;
using Daniel15.Sharpamp;
using WinampNowPlayingToFile.Business;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Presentation;
using WinampNowPlayingToFile.Settings;

namespace WinampNowPlayingToFile
{
    public class NowPlayingToFilePlugin : GeneralPlugin
    {
        public override string Name => "Now Playing to File";

        private NowPlayingToFileManager manager;

        public override void Initialize()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ISettings settings = new RegistrySettings();
            settings.Load();

            manager = new NowPlayingToFileManager(new WinampControllerImpl(Winamp), settings);
        }

        public override void Config()
        {
            new SettingsDialog(manager.Settings).Show();
        }
    }
}