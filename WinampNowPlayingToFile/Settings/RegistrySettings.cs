#nullable enable

using Microsoft.Win32;
using System;

namespace WinampNowPlayingToFile.Settings;

public class RegistrySettings: BaseSettings {

    internal string keyPath = @"Software\WinampNowPlayingToFile";

    public override void load() {
        loadDefaults();

        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath);
        if (key != null) {
            albumArtFilename                   = key.GetValue(nameof(albumArtFilename)) as string ?? albumArtFilename;
            preserveAlbumArtFileWhenNotPlaying = Convert.ToBoolean(key.GetValue(nameof(preserveAlbumArtFileWhenNotPlaying)) as int? ?? 0);
            preserveTextFileWhenNotPlaying     = Convert.ToBoolean(key.GetValue(nameof(preserveTextFileWhenNotPlaying)) as int? ?? 0);

            for (int textIndex = 0;; textIndex++) {
                string registryNameSuffix        = getTextRegistryNameSuffix(textIndex);
                string filenameRegistryValueName = $"textFilename{registryNameSuffix}";
                string templateRegistryValueName = $"textTemplate{registryNameSuffix}";

                if (key.GetValue(filenameRegistryValueName) is string textFilename) {
                    string textTemplate = key.GetValue(templateRegistryValueName) as string ?? (textIndex > 0 ? string.Empty : textTemplates[textIndex]);

                    textFilenames.Insert(textIndex, textFilename);
                    textTemplates.Insert(textIndex, textTemplate);
                } else {
                    break;
                }
            }
        }

    }

    private static string getTextRegistryNameSuffix(int textIndex) {
        return textIndex > 0 ? textIndex.ToString() : string.Empty;
    }

    public override void save() {
        base.save();
        using RegistryKey? key = Registry.CurrentUser.CreateSubKey(keyPath);
        if (key != null) {
            key.SetValue(nameof(albumArtFilename), albumArtFilename);
            key.SetValue(nameof(preserveAlbumArtFileWhenNotPlaying), Convert.ToInt32(preserveAlbumArtFileWhenNotPlaying), RegistryValueKind.DWord);
            key.SetValue(nameof(preserveTextFileWhenNotPlaying), Convert.ToInt32(preserveTextFileWhenNotPlaying), RegistryValueKind.DWord);
            for (int textFilenameIndex = 0; textFilenameIndex < textFilenames.Count; textFilenameIndex++) {
                key.SetValue($"textFilename{getTextRegistryNameSuffix(textFilenameIndex)}", textFilenames[textFilenameIndex]);
            }
            for (int textTemplateIndex = 0; textTemplateIndex < textTemplates.Count; textTemplateIndex++) {
                key.SetValue($"textTemplate{getTextRegistryNameSuffix(textTemplateIndex)}", textTemplates[textTemplateIndex]);
            }
            onSettingsUpdated();
        }

    }

}