#nullable enable

using Microsoft.Win32;

namespace WinampNowPlayingToFile.Settings;

public class RegistrySettings: BaseSettings {

    internal string keyPath = @"Software\WinampNowPlayingToFile";

    public override void load() {
        loadDefaults();

        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath);
        if (key != null) {
            textFilename     = key.GetValue(nameof(textFilename)) as string ?? textFilename;
            albumArtFilename = key.GetValue(nameof(albumArtFilename)) as string ?? albumArtFilename;
            textTemplate     = key.GetValue(nameof(textTemplate)) as string ?? textTemplate;
        }

    }

    public override void save() {
        using RegistryKey? key = Registry.CurrentUser.CreateSubKey(keyPath);
        if (key != null) {
            key.SetValue(nameof(textFilename), textFilename);
            key.SetValue(nameof(albumArtFilename), albumArtFilename);
            key.SetValue(nameof(textTemplate), textTemplate);
            onSettingsUpdated();
        }

    }

}