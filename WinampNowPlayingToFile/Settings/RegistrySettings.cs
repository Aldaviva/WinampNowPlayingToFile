#nullable enable

using System.Linq;

using Microsoft.Win32;

using WinampNowPlayingToFile.Facade;

namespace WinampNowPlayingToFile.Settings;

public class RegistrySettings: BaseSettings {

    internal string keyPath = @"Software\WinampNowPlayingToFile";

    public override void load() {
        loadDefaults();

        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath);
        if (key != null) {
            int savedTemplateCount = key.GetValueNames().Count(x => x.StartsWith($"{nameof(textTemplate.fileName)}"));
            if (savedTemplateCount > 0) {
                textTemplates.Clear();
                for (int i = 0; i < savedTemplateCount; i++) {
                    textTemplates.Add(new textTemplate(
                        fileName: key.GetValue($"{nameof(textTemplate.fileName)}{i}") as string,
                        text: key.GetValue($"{nameof(textTemplate.text)}{i}") as string
                    ));
                }
                albumArtFilename = key.GetValue(nameof(albumArtFilename)) as string ?? albumArtFilename;
            }
        }
    }

    public override void save() {
        base.save();
        using RegistryKey? key = Registry.CurrentUser.CreateSubKey(keyPath);
        if (key != null) {
            for (int i = 0; i < textTemplates.Count; i++) {
                textTemplate template = textTemplates[i];
                key.SetValue($"{nameof(template.fileName)}{i}", template.fileName);
                key.SetValue($"{nameof(template.text)}{i}", template.text);
            }
			key.SetValue(nameof(albumArtFilename), albumArtFilename);
            
            onSettingsUpdated();
        }

    }

}