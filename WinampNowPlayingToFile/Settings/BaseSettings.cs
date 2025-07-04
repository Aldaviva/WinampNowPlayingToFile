#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace WinampNowPlayingToFile.Settings;

public abstract class BaseSettings: ISettings {

    public IList<string> textFilenames { get; } = new List<string>();
    public IList<string> textTemplates { get; } = new List<string>();
    public string albumArtFilename { get; set; } = null!;
    public bool preserveAlbumArtFileWhenNotPlaying { get; set; }
    public bool preserveTextFileWhenNotPlaying { get; set; }

    public event EventHandler? settingsUpdated;

    public abstract void load();
    public virtual void save() { }

    public ISettings loadDefaults() {
        if (!textFilenames.Any()) {
            textFilenames.Add(Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing.txt"));
        }

        if (!textTemplates.Any()) {
            textTemplates.Add("{{#if Artist}}{{Artist}} \u2013 {{/if}}{{Title}}{{#if Album}} \u2013 {{Album}}{{/if}}");
        }

        if (string.IsNullOrEmpty(albumArtFilename)) {
            albumArtFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing.png");
        }

        return this;
    }

    protected void onSettingsUpdated() => settingsUpdated?.Invoke(this, EventArgs.Empty);

    public void load(ISettings source) {
        preserveAlbumArtFileWhenNotPlaying = source.preserveAlbumArtFileWhenNotPlaying;
        preserveTextFileWhenNotPlaying     = source.preserveTextFileWhenNotPlaying;
        albumArtFilename                   = source.albumArtFilename;
        textFilenames.Clear();
        foreach (string textFilename in source.textFilenames) {
            textFilenames.Add(textFilename);
        }
        textTemplates.Clear();
        foreach (string textTemplate in source.textTemplates) {
            textTemplates.Add(textTemplate);
        }
        onSettingsUpdated();
    }

}