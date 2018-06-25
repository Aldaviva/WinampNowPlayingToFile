using System.Reflection;
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
        public override string Name => $"Now Playing to File v{Assembly.GetAssembly(typeof(NowPlayingToFilePlugin)).GetName().Version}";

        internal NowPlayingToFileManager Manager;
        private WinampControllerImpl winampController;
        private ISettings settings;

        public override void Initialize()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            settings = new RegistrySettings();
            settings.Load();

            winampController = new WinampControllerImpl(Winamp);
            Manager = new NowPlayingToFileManager(settings, winampController);
        }

        public override void Config()
        {
            new SettingsDialog(settings, winampController).ShowDialog();
        }

        public override void Quit()
        {
            Manager.OnQuit();
        }
    }
}