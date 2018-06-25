using Microsoft.Win32;

namespace WinampNowPlayingToFile.Settings
{
    public class RegistrySettings : BaseSettings
    {
        internal string Key = @"Software\WinampNowPlayingToFile";

        public override void Load()
        {
            LoadDefaults();

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(Key))
            {
                if (key != null)
                {
                    TextFilename = key.GetValue(nameof(TextFilename)) as string ?? TextFilename;
                    AlbumArtFilename = key.GetValue(nameof(AlbumArtFilename)) as string ?? AlbumArtFilename;
                    TextTemplate = key.GetValue(nameof(TextTemplate)) as string ?? TextTemplate;
                }
            }
        }

        public override void Save()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(Key))
            {
                if (key != null)
                {
                    key.SetValue(nameof(TextFilename), TextFilename);
                    key.SetValue(nameof(AlbumArtFilename), AlbumArtFilename);
                    key.SetValue(nameof(TextTemplate), TextTemplate);
                    OnSettingsUpdated();
                }
            }
        }
    }
}