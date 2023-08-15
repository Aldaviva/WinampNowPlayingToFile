#nullable enable

using System;
using System.Reflection;
using System.Windows.Forms;
using Daniel15.Sharpamp;
using WinampNowPlayingToFile.Business;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Presentation;
using WinampNowPlayingToFile.Settings;

namespace WinampNowPlayingToFile;

public class NowPlayingToFilePlugin: GeneralPlugin {

    public override string Name =>
        $"Now Playing to File v{Assembly.GetAssembly(typeof(NowPlayingToFilePlugin)).GetName().Version.ToString(3)}";

    private readonly ISettings settings = new RegistrySettings();

    internal NowPlayingToFileManager? manager;
    private  WinampControllerImpl?    winampController;

    public override void Initialize() {
        AppContext.SetSwitch("Switch.System.IO.UseLegacyPathHandling", false); // #5

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        settings.load();

        winampController = new WinampControllerImpl(Winamp);
        manager          = new NowPlayingToFileManager(settings, winampController);
        manager.error += (_, e) => MessageBox.Show($"{e.Message}\nSong filename: {e.song?.Filename}\nStacktrace: {e.InnerException!.StackTrace}", "Now Playing To File error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public override void Config() {
        new SettingsDialog(settings, winampController!).ShowDialog();
    }

    public override void Quit() {
        manager?.onQuit();
        manager = null;
    }

}