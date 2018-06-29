using System;
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

namespace Test.Business
{
    public class NowPlayingToFileManagerTest : IDisposable
    {
        private readonly WinampController winampController = A.Fake<WinampController>();
        private readonly NowPlayingToFileManager manager;
        private readonly string textFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing_test.txt");
        private readonly string albumArtFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing_test.png");
        private readonly ISettings settings = A.Fake<ISettings>();

        private readonly Song song = new Song
        {
            Album = "album",
            Artist = "artist",
            Title = "title",
            Year = 2018,
            Filename = @"Tracks\empty.flac"
        };

        public NowPlayingToFileManagerTest()
        {
            CleanUp();

            A.CallTo(() => winampController.Status).Returns(Status.Playing);
            A.CallTo(() => winampController.CurrentSong).Returns(song);
            A.CallTo(() => settings.TextFilename).Returns(textFilename);
            A.CallTo(() => settings.TextTemplate).Returns(new RegistrySettings().LoadDefaults().TextTemplate);
            A.CallTo(() => settings.AlbumArtFilename).Returns(albumArtFilename);

            manager = new NowPlayingToFileManager(settings, winampController);
        }

        public void Dispose()
        {
            CleanUp();
        }

        private void CleanUp()
        {
            File.Delete(textFilename);
            File.Delete(albumArtFilename);
        }

        [Fact]
        public void RenderTemplateWithAlbum()
        {
            string actual = manager.RenderText();

            actual.Should().Be("artist \u2013 title \u2013 album");
        }

        [Fact]
        public void RenderTemplateWithoutAlbum()
        {
            A.CallTo(() => winampController.CurrentSong).Returns(new Song
            {
                Album = "",
                Artist = "artist",
                Title = "title",
                Year = 2018,
                Filename = "empty.ogg"
            });

            string actual = manager.RenderText();

            actual.Should().Be("artist \u2013 title");
        }

        [Fact]
        public void RenderTemplateWhenPaused()
        {
            A.CallTo(() => winampController.Status).Returns(Status.Paused);

            manager.RenderText().Should().BeEmpty();
        }

        [Fact]
        public void RenderTemplateWhenStopped()
        {
            A.CallTo(() => winampController.Status).Returns(Status.Stopped);

            manager.RenderText().Should().BeEmpty();
        }

        [Fact]
        public void WriteToFileOnSongChange()
        {
            winampController.SongChanged += Raise.FreeForm.With(winampController, new SongChangedEventArgs(new Song()));

            string actualText = File.ReadAllText(textFilename, Encoding.UTF8);
            actualText.Should().Be("artist \u2013 title \u2013 album");

            byte[] actualAlbumArt = File.ReadAllBytes(albumArtFilename);
            byte[] expectedAlbumArt = File.ReadAllBytes(@"Tracks\albumart.png");
            actualAlbumArt.Should().BeEquivalentTo(expectedAlbumArt, "album art");
        }

        [Fact]
        public void WriteToFileOnStatusChange()
        {
            winampController.StatusChanged += Raise.FreeForm.With(winampController, new StatusChangedEventArgs(Status.Playing));

            string actualText = File.ReadAllText(textFilename, Encoding.UTF8);
            actualText.Should().Be("artist \u2013 title \u2013 album");

            byte[] actualAlbumArt = File.ReadAllBytes(albumArtFilename);
            byte[] expectedAlbumArt = File.ReadAllBytes(@"Tracks\albumart.png");
            actualAlbumArt.Should().BeEquivalentTo(expectedAlbumArt, "album art");
        }

        [Fact]
        public void DontCrashOnId3V1V24WithNoArtwork()
        {
            A.CallTo(() => winampController.CurrentSong).Returns(new Song
            {
                Filename = @"D:\Music\Big Beat\The Crystal Method\Blood Rave.mp3",
                Artist = "The Crystal Method",
                Title = "Blood Rave",
                Album = "Mixes and Soundtracks"
            });

            manager.Update();
        }

        [Fact]
        public void ClearFilesOnQuit()
        {
            File.WriteAllText(textFilename, "test");
            File.WriteAllText(albumArtFilename, "test");

            manager.OnQuit();

            File.ReadAllText(textFilename).Should().BeEmpty();
            File.Exists(albumArtFilename).Should().BeFalse();
        }

        [Fact]
        public void RecompileTemplateWhenSettingsChange()
        {
            manager.Update();

            A.CallTo(() => settings.TextTemplate).Returns("{{Artist}}");
            settings.SettingsUpdated += Raise.WithEmpty();

            File.ReadAllText(textFilename).Should().Be("artist");
        }

        [Fact]
        public void ExtractDefaultAlbumArtWhenPlayingAndTrackHasNoAlbumArt()
        {
            A.CallTo(() => winampController.CurrentSong).Returns(new Song
            {
                Album = "",
                Artist = "artist",
                Title = "title",
                Year = 2018,
                Filename = @"Tracks\noalbumart.flac"
            });

            byte[] actual = manager.ExtractAlbumArt();
            byte[] expected = WinampNowPlayingToFile.Resources.black_png;
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void AlbumArtFileShouldNotExistWhenPaused()
        {
            A.CallTo(() => winampController.Status).Returns(Status.Paused);
            manager.ExtractAlbumArt().Should().BeNull();
        }

        [Fact]
        public void AlbumArtFileShouldNotExistWhenStopped()
        {
            A.CallTo(() => winampController.Status).Returns(Status.Stopped);
            manager.ExtractAlbumArt().Should().BeNull();
        }
    }
}