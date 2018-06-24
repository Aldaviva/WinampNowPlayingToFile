using System;
using System.Windows.Forms;
using WinampNowPlayingToFile.Settings;

namespace WinampNowPlayingToFile.Presentation
{
    public static class PresentationMain
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

//            new SettingsDialog().Show();
            Settings.ISettings settings = new RegistrySettings().LoadDefaults();
            Application.Run(new SettingsDialog(settings));
        }
    }
}