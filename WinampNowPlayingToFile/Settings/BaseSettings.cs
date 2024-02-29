#nullable enable

using System;
using System.Collections.Generic;

using TagLib.Riff;

using WinampNowPlayingToFile.Facade;

namespace WinampNowPlayingToFile.Settings;

public abstract class BaseSettings: ISettings {

    public string albumArtFilename { get; set; } = null!;
    public List<textTemplate> textTemplates { get; set; } = null!;

	public event EventHandler? settingsUpdated;

    public abstract void load();
    public virtual void save() { }

    public ISettings loadDefaults() {
        textTemplates = new List<textTemplate>() {
            getDefault()
        };
        albumArtFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing.png");
        return this;
    }

    public textTemplate getDefault() {
        return new textTemplate(
			fileName: Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing.txt"),
			text: "{{#if Artist}}{{Artist}} \u2013 {{/if}}{{Title}}{{#if Album}} \u2013 {{Album}}{{/if}}"
			);
    }

    protected void onSettingsUpdated() {
        settingsUpdated?.Invoke(this, EventArgs.Empty);
    }

}