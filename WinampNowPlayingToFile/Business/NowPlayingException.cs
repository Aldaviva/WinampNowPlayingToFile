#nullable enable

using System;
using WinampNowPlayingToFile.Facade;

namespace WinampNowPlayingToFile.Business;

public class NowPlayingException: ApplicationException {

    public Song? song { get; }

    public NowPlayingException(string message, Exception innerException, Song? song): base(message, innerException) {
        this.song = song;
    }

}