using System;

namespace WinampNowPlayingToFile.Settings
{
    public abstract class BaseSettings : ISettings
    {
        public string NowPlayingFilename { get; set; }
        public string NowPlayingTemplate { get; set; }

        public event EventHandler SettingsUpdated;

        public abstract void Load();
        public abstract void Save();

        public ISettings LoadDefaults()
        {
            NowPlayingFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing.txt");
            NowPlayingTemplate = "{{#if Artist}}{{Artist}} \u2013 {{/if}}{{Title}}{{#if Album}} \u2013 {{Album}}{{/if}}";
            return this;
        }

        protected void OnSettingsUpdated()
        {
            SettingsUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}