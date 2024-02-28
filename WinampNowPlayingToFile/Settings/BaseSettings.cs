#nullable enable

using System;

namespace WinampNowPlayingToFile.Settings;

public abstract class BaseSettings: ISettings {

    public string textFilename { get; set; } = null!;
    public string albumArtFilename { get; set; } = null!;
    public string textTemplate { get; set; } = null!;

    public event EventHandler? settingsUpdated;

    public abstract void load();
    public virtual void save() { }

    public ISettings loadDefaults() {
        textFilename     = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing.txt");
        albumArtFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing.png");
        textTemplate     = "{{#if Artist}}{{Artist}} \u2013 {{/if}}{{Title}}{{#if Album}} \u2013 {{Album}}{{/if}}";
        return this;
    }

    protected void onSettingsUpdated() {
        settingsUpdated?.Invoke(this, EventArgs.Empty);
    }

}