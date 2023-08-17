using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Daniel15.Sharpamp;
using FakeItEasy;
using FluentAssertions;
using WinampNowPlayingToFile.Business;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Settings;
using Xunit;
using Song = WinampNowPlayingToFile.Facade.Song;
using SongChangedEventArgs = WinampNowPlayingToFile.Facade.SongChangedEventArgs;

namespace Test.Business;

public class NowPlayingToFileManagerTest: IDisposable {

    private readonly NowPlayingToFileManager manager;
    private readonly WinampController        winampController        = A.Fake<WinampController>();
    private readonly string                  textFilename            = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing_test.txt");
    private readonly string                  albumArtFilename        = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing_test.png");
    private readonly ISettings               settings                = A.Fake<ISettings>();
    private readonly Encoding                utf8                    = new UTF8Encoding(false, true);
    private readonly IList<string>           filesToDeleteOnDisposal = new List<string>();

    private Song song = new() {
        Album    = "album",
        Artist   = "artist",
        Title    = "title",
        Year     = 2018,
        Filename = @"Tracks\empty.flac"
    };

    public NowPlayingToFileManagerTest() {
        cleanUp();

        A.CallTo(() => winampController.status).Returns(Status.Playing);
        A.CallTo(() => winampController.currentSong).Returns(song);
        A.CallTo(() => settings.textFilename).Returns(textFilename);
        A.CallTo(() => settings.textTemplate).Returns(new RegistrySettings().loadDefaults().textTemplate);
        A.CallTo(() => settings.albumArtFilename).Returns(albumArtFilename);

        manager       =  new NowPlayingToFileManager(settings, winampController);
        manager.error += onManagerError;
    }

    private static void onManagerError(object _, NowPlayingException exception) {
        throw exception;
    }

    public void Dispose() {
        cleanUp();
    }

    private void cleanUp() {
        File.Delete(textFilename);
        File.Delete(albumArtFilename);
        foreach (string file in filesToDeleteOnDisposal) {
            File.Delete(file);
        }
    }

    [Fact]
    public void renderTemplateWithAlbum() {
        string actual = manager.renderText(song);

        actual.Should().Be("artist \u2013 title \u2013 album");
    }

    [Fact]
    public void renderTemplateWithoutAlbum() {
        song = new Song {
            Album    = "",
            Artist   = "artist",
            Title    = "title",
            Year     = 2018,
            Filename = "empty.ogg"
        };

        string actual = manager.renderText(song);

        actual.Should().Be("artist \u2013 title");
    }

    [Fact]
    public void renderTemplateWhenPaused() {
        A.CallTo(() => winampController.status).Returns(Status.Paused);

        manager.renderText(song).Should().BeEmpty();
    }

    [Fact]
    public void renderTemplateWhenStopped() {
        A.CallTo(() => winampController.status).Returns(Status.Stopped);

        manager.renderText(song).Should().BeEmpty();
    }

    [Fact]
    public void writeToFileOnSongChange() {
        winampController.songChanged += Raise.FreeForm.With(winampController, new SongChangedEventArgs(new Song())); //metadata comes from song field

        string actualText = File.ReadAllText(textFilename, utf8);
        actualText.Should().Be("artist \u2013 title \u2013 album");

        byte[] actualAlbumArt   = File.ReadAllBytes(albumArtFilename);
        byte[] expectedAlbumArt = File.ReadAllBytes(@"Tracks\expected.png");
        actualAlbumArt.Should().BeEquivalentTo(expectedAlbumArt, "album art");
    }

    [Fact]
    public void writeToFileOnStatusChange() {
        winampController.statusChanged += Raise.FreeForm.With(winampController, new StatusChangedEventArgs(Status.Playing));

        string actualText = File.ReadAllText(textFilename, utf8);
        actualText.Should().Be("artist \u2013 title \u2013 album");

        byte[] actualAlbumArt   = File.ReadAllBytes(albumArtFilename);
        byte[] expectedAlbumArt = File.ReadAllBytes(@"Tracks\expected.png");
        actualAlbumArt.Should().BeEquivalentTo(expectedAlbumArt, "album art");
    }

    [Fact]
    public void dontCrashOnId3V1V24WithNoArtwork() {
        A.CallTo(() => winampController.currentSong).Returns(new Song {
            Filename = @"Tracks\silent.mp3"
        });

        manager.update();
    }

    [Fact]
    public void dontCrashOnMissingFolder() {
        A.CallTo(() => winampController.currentSong).Returns(new Song {
            Filename = @"C:\A\Completely\Incorrect\Path\song.mp3"
        });

        manager.update();
    }

    [Fact]
    public void dontCrashOnEmptyFilename() {
        A.CallTo(() => winampController.currentSong).Returns(new Song {
            Filename = ""
        });

        manager.update();
    }

    [Theory]
    [InlineData("http://135.125.239.164:8080/dance.mp3")]
    [InlineData("https://relay.rainwave.cc:443/all.mp3")]
    [InlineData("http://allrelays.rainwave.cc/all.mp3")]
    public void dontCrashOnUrisWithFileExtensions(string filename) {
        A.CallTo(() => winampController.currentSong).Returns(new Song {
            Filename = filename,
            Artist   = "Test Artist",
            Title    = "Test Title",
            Album    = "Test Album"
        });

        manager.update();
    }

    [Fact]
    public void clearFilesOnQuit() {
        File.WriteAllText(textFilename, "test");
        File.WriteAllText(albumArtFilename, "test");

        manager.onQuit();

        File.ReadAllText(textFilename).Should().BeEmpty();
        File.Exists(albumArtFilename).Should().BeFalse();
    }

    [Fact]
    public void recompileTemplateWhenSettingsChange() {
        manager.update();

        A.CallTo(() => settings.textTemplate).Returns("{{Artist}}");
        settings.settingsUpdated += Raise.WithEmpty();

        File.ReadAllText(textFilename).Should().Be("artist");
    }

    [Fact]
    public void deleteAlbumArtWhenPlayingAndTrackHasNoAlbumArt() {
        song = new Song {
            Album    = "",
            Artist   = "artist",
            Title    = "title",
            Year     = 2018,
            Filename = @"Tracks\noalbumart.flac"
        };

        byte[]? actual = manager.findAlbumArt(song);
        actual.Should().BeNull();
    }

    [Fact]
    public void saveCustomPlaceholderAlbumArtWhenPlayingAndTrackHasNoAlbumArt() {
        File.Copy(@"Tracks\expected.png", "emptyAlbumArt.png");
        filesToDeleteOnDisposal.Add("emptyAlbumArt.png");

        song = new Song {
            Album    = "",
            Artist   = "artist",
            Title    = "title",
            Year     = 2018,
            Filename = @"Tracks\noalbumart.flac"
        };

        byte[]? actual           = manager.findAlbumArt(song);
        byte[]  expectedAlbumArt = File.ReadAllBytes(@"Tracks\expected.png");
        actual.Should().BeEquivalentTo(expectedAlbumArt);
    }

    [Fact]
    public void albumArtFileShouldNotExistWhenPaused() {
        A.CallTo(() => winampController.status).Returns(Status.Paused);
        manager.findAlbumArt(song).Should().BeNull();
    }

    [Fact]
    public void albumArtFileShouldNotExistWhenStopped() {
        A.CallTo(() => winampController.status).Returns(Status.Stopped);
        manager.findAlbumArt(song).Should().BeNull();
    }

    [Fact]
    public void saveCustomPlaceholderAlbumArtFileWhenStopped() {
        A.CallTo(() => winampController.status).Returns(Status.Stopped);
        File.Copy(@"Tracks\expected.png", "stoppedAlbumArt.png");
        filesToDeleteOnDisposal.Add("stoppedAlbumArt.png");

        song = new Song {
            Album    = "",
            Artist   = "artist",
            Title    = "title",
            Year     = 2018,
            Filename = @"Tracks\noalbumart.flac"
        };

        File.Exists("emptyAlbumArt.png").Should().BeFalse();
        byte[] expectedAlbumArt = File.ReadAllBytes(@"Tracks\expected.png");
        manager.findAlbumArt(song).Should().BeEquivalentTo(expectedAlbumArt);
    }

    [Fact]
    public void copyAlbumArtFromSidecarFiles() {
        song = new Song {
            Album    = "My Album",
            Artist   = "artist",
            Title    = "title",
            Year     = 2023,
            Filename = @"Tracks\noalbumart.flac"
        };

        var albumArtFilenames = new List<string> {
            "My Album.bmp", // highest priority
            "My Album.gif",
            "My Album.jpeg",
            "My Album.jpg",
            "My Album.png",
            "NFO file.nfo",
            "cover.bmp",
            "cover.gif",
            "cover.jpeg",
            "cover.jpg",
            "cover.png",
            "folder.bmp",
            "folder.gif",
            "folder.jpeg",
            "folder.jpg",
            "folder.png",
            "front.bmp",
            "front.gif",
            "front.jpeg",
            "front.jpg",
            "front.png",
            "albumart.bmp",
            "albumart.gif",
            "albumart.jpeg",
            "albumart.jpg",
            "albumart.png"
        };

        foreach (string filename in albumArtFilenames) {
            string absoluteFile = Path.GetFullPath($"Tracks\\{filename}");
            File.WriteAllText(absoluteFile, filename, utf8);
            filesToDeleteOnDisposal.Add(absoluteFile);

            if (Path.GetExtension(filename) == ".nfo") {
                string albumArtFileFromNfo = Path.ChangeExtension(filename, "jpg");
                absoluteFile = Path.GetFullPath($"Tracks\\{albumArtFileFromNfo}");
                File.WriteAllText(absoluteFile, albumArtFileFromNfo, utf8);
                filesToDeleteOnDisposal.Add(absoluteFile);
            }
        }

        foreach (string filename in albumArtFilenames) {
            byte[]? actual       = manager.findAlbumArt(song);
            string? actualString = actual != null ? utf8.GetString(actual) : null;
            string  expected     = Path.GetExtension(filename) == ".nfo" ? Path.ChangeExtension(filename, "jpg") : filename;
            actualString.Should().Be(expected);

            File.Delete($"Tracks\\{filename}");
        }

        manager.findAlbumArt(song).Should().BeNull();
    }

    [Fact]
    public void uncaughtException() {
        manager.error -= onManagerError;

        NowPlayingException? actual = null;
        manager.error += (_, exception) => actual = exception;

        A.CallTo(() => settings.textTemplate).Returns("this is an illegal mustache template {{#}}");
        settings.settingsUpdated += Raise.WithEmpty(); // clear mustache template cache

        manager.update();

        actual.Should().NotBeNull();
        actual!.song.Should().BeSameAs(song);
    }

}