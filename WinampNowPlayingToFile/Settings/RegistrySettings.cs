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
                        NowPlayingFilename = key.GetValue(nameof(NowPlayingFilename)) as string ?? NowPlayingFilename;
                        NowPlayingTemplate = key.GetValue(nameof(NowPlayingTemplate)) as string ?? NowPlayingTemplate;
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
                    key.SetValue(nameof(NowPlayingFilename), NowPlayingFilename);
                    key.SetValue(nameof(NowPlayingTemplate), NowPlayingTemplate);
                }
            }
        }
    }
}