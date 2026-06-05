#nullable enable

using System;
using WinampNowPlayingToFile.Data;

namespace WinampNowPlayingToFile.Settings;

public interface ISettings: IReadOnlySettings {

    event EventHandler settingsUpdated;

    void load();
    void load(ISettings source);
    ISettings loadDefaults();
    void save();

}