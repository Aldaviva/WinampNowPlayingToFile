using Microsoft.Win32;

namespace WinampNowPlayingToFile.Settings
{
    public class RegistrySettings : BaseSettings
    {
        private const string KEY = @"Software\WinampNowPlayingToFile";

        public override void Load()
        {
            LoadDefaults();

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KEY))
                {
                    if (key != null)
                    {
                        TextFilename = key.GetValue(nameof(TextFilename)) as string ?? TextFilename;
                        AlbumArtFilename = key.GetValue(nameof(AlbumArtFilename)) as string ?? AlbumArtFilename;
                        TextTemplate = key.GetValue(nameof(TextTemplate)) as string ?? TextTemplate;
                    }
                }
            }
            catch
            {
                //Entire registry key does not exist, so leave defaults loaded
            }
        }

        public override void Save()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KEY))
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