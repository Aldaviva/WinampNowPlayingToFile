#nullable enable

using WinampNowPlayingToFile.Data;

namespace WinampNowPlayingToFile.Plugins;

/// <summary>
/// <para>Plugin for the WinampNowPlayingToFile plugin, allowing the user to achieve extra functionality not normally available in the WinampNowPlayingToFile plugin.</para>
/// <para>Initialization logic can be put in a no-arg constructor. Cleanup logic can be put in <see cref="IDisposable.Dispose"/>.</para>
/// </summary>
public interface IWinampNowPlayingToFilePlugin {

    /// <summary>
    /// Called by WinampNowPlayingToFile when either the current song or playback state changes.
    /// </summary>
    /// <param name="currentSong">The song that is currently playing, or <c>null</c> if the playlist is empty.</param>
    /// <param name="playbackStatus">Whether Winamp is currently stopped, playing, or paused.</param>
    void OnSongUpdated(Song? currentSong, Status playbackStatus);

}