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

        private NowPlayingToFileManager manager;
        private WinampControllerImpl winampController;
        private ISettings settings;

        public override void Initialize()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            settings = new RegistrySettings();
            settings.Load();

            winampController = new WinampControllerImpl(Winamp);
            manager = new NowPlayingToFileManager(settings, winampController);
        }

        public override void Config()
        {
            new SettingsDialog(settings, winampController).ShowDialog();
        }

        public override void Quit()
        {
            manager.OnQuit();
        }
    }
}