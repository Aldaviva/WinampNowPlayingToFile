using System;

namespace WinampNowPlayingToFile.Settings {

    public abstract class BaseSettings: ISettings {

        public string TextFilename { get; set; }
        public string AlbumArtFilename { get; set; }
        public string TextTemplate { get; set; }

        public event EventHandler SettingsUpdated;

        public abstract void Load();
        public abstract void Save();

        public ISettings LoadDefaults() {
            TextFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing.txt");
            AlbumArtFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing.png");
            TextTemplate = "{{#if Artist}}{{Artist}} \u2013 {{/if}}{{Title}}{{#if Album}} \u2013 {{Album}}{{/if}}";
            return this;
        }

        protected void OnSettingsUpdated() {
            SettingsUpdated?.Invoke(this, EventArgs.Empty);
        }

    }

}