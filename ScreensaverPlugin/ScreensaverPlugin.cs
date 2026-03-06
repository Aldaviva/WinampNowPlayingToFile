using Microsoft.Win32;
using WinampNowPlayingToFile.Data;

namespace WinampNowPlayingToFile.Plugins.ScreensaverChanger;

public class ScreensaverChangerPlugin: IWinampNowPlayingToFilePlugin, IDisposable {

    private static readonly string SYSTEM32 = Environment.ExpandEnvironmentVariables(@"%WINDIR%\system32");

    private readonly RegistryKey desktopKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true)!;

    private int _wasPlaying;

    public ScreensaverChangerPlugin() {
        setScreensaver(_wasPlaying == 1);
    }

    public void OnSongUpdated(Song? currentSong, Status playbackStatus) {
        int newValue = playbackStatus == Status.Playing ? 1 : 0;
        if (newValue != Interlocked.Exchange(ref _wasPlaying, newValue)) {
            setScreensaver(newValue == 1);
        }
    }

    private void setScreensaver(bool isPlaying) {
        desktopKey.SetValue("SCRNSAVE.EXE", Path.Combine(SYSTEM32, isPlaying ? "ssmypics.scr" : "ssmyst.scr"), RegistryValueKind.String);
    }

    public void Dispose() {
        desktopKey.Dispose();
    }

}