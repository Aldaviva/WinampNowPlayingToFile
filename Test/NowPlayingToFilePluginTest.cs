using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using ManagedWinapi.Windows;
using WinampNowPlayingToFile;
using WinampNowPlayingToFile.Business;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Settings;
using Xunit;

namespace Test;

public class NowPlayingToFilePluginTest: IDisposable {

    private readonly NowPlayingToFilePlugin   plugin;
    private readonly INowPlayingToFileManager manager;

    private readonly SimpleSettings settings = new() {
        textFilename     = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing_test.txt"),
        albumArtFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing_test.png"),
        textTemplate     = "{{Title}}"
    };

    public NowPlayingToFilePluginTest() {
        manager = A.Fake<INowPlayingToFileManager>();
        plugin  = new NowPlayingToFilePlugin { manager = manager, settings = settings };
    }

    public void Dispose() {
        File.Delete(settings.albumArtFilename);
        File.Delete(settings.textFilename);
    }

    [Fact]
    public void pluginName() {
        plugin.Name.Should().StartWith("Now Playing to File v");
    }

    [Fact]
    public void initialize() {
        plugin.Init(IntPtr.Zero);
    }

    [Fact]
    public void quit() {
        A.CallTo(() => manager.onQuit()).DoesNothing();

        plugin.Quit();

        A.CallTo(() => manager.onQuit()).MustHaveHappened();
    }

    [Fact]
    public async Task uncaughtException() {
        plugin.manager = manager;
        plugin.initManager();

        using Process selfProcess = Process.GetCurrentProcess();
        int           selfPid     = selfProcess.Id;
        SystemWindow? dialogBox   = null;

        CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task assertions = Task.Run(() => {
            while (dialogBox == null && !cts.IsCancellationRequested) {
                dialogBox = SystemWindow.DesktopWindow.FilterDescendantWindows(false, window => {
                    if (window.ClassName != "#32770") return false;
                    using Process proc = window.Process; // expensive
                    return proc.Id == selfPid;
                }).FirstOrDefault();

                if (dialogBox != null) {
                    try {
                        dialogBox.Title.Should().Be("Now Playing To File error");
                        dialogBox.AllChildWindows.Last().Text.Should().Be("test error message\nSong filename: myfilename.mp3\nStacktrace: ");
                    } finally {
                        dialogBox.SendClose();
                    }
                }
            }
        }, cts.Token);

        manager.error += Raise.With(null, new NowPlayingException("test error message", new ApplicationException("shit's fucked yo"), new Song { Filename = "myfilename.mp3" }));

        await assertions;
        dialogBox.Should().NotBeNull();
    }

}