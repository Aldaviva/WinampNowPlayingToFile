using System;

namespace WinampNowPlayingToFile.Settings {

    public abstract class BaseSettings: ISettings {

        public string textFilename { get; set; }
        public string albumArtFilename { get; set; }
        public string textTemplate { get; set; }

        public event EventHandler settingsUpdated;

        public abstract void load();
        public abstract void save();

        public ISettings loadDefaults() {
            textFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing.txt");
            albumArtFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing.png");
            textTemplate = "{{#if Artist}}{{Artist}} \u2013 {{/if}}{{Title}}{{#if Album}} \u2013 {{Album}}{{/if}}";
            return this;
        }

        protected void onSettingsUpdated() {
            settingsUpdated?.Invoke(this, EventArgs.Empty);
        }

    }

}