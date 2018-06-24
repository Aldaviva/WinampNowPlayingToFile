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

namespace Test
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
            Filename = "filename.ogg"
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
        }

        [Fact]
        public void RenderTemplateWithAlbum()
        {
            string actual = manager.RenderText(song);

            actual.Should().Be("artist \u2013 title \u2013 album");
        }

        [Fact]
        public void RenderTemplateWithoutAlbum()
        {
            var songWithoutAlbum = new Song
            {
                Album = "",
                Artist = "artist",
                Title = "title",
                Year = 2018,
                Filename = "filename.ogg"
            };

            string actual = manager.RenderText(songWithoutAlbum);

            actual.Should().Be("artist \u2013 title");
        }

        [Fact]
        public void RenderTemplateWhenPaused()
        {
            A.CallTo(() => winampController.Status).Returns(Status.Paused);

            manager.RenderText(song).Should().BeEmpty();
        }

        [Fact]
        public void RenderTemplateWhenStopped()
        {
            A.CallTo(() => winampController.Status).Returns(Status.Stopped);

            manager.RenderText(song).Should().BeEmpty();
        }

        [Fact]
        public void WriteToFileOnSongChange()
        {
            winampController.SongChanged += Raise.FreeForm.With(winampController, new SongChangedEventArgs(new Song()));

            string actual = File.ReadAllText(textFilename, Encoding.UTF8);
            actual.Should().Be("artist \u2013 title \u2013 album");
        }

        [Fact]
        public void WriteToFileOnStatusChange()
        {
            winampController.StatusChanged += Raise.FreeForm.With(winampController, new StatusChangedEventArgs(Status.Playing));

            string actual = File.ReadAllText(textFilename, Encoding.UTF8);
            actual.Should().Be("artist \u2013 title \u2013 album");
        }

        [Fact]
        public void DontCrashOnId3V1V24WithNoArtwork()
        {
            Song bloodRave = new Song
            {
                Filename = @"D:\Music\Big Beat\The Crystal Method\Blood Rave.mp3",
                Artist = "The Crystal Method",
                Title = "Blood Rave",
                Album = "Mixes and Soundtracks"
            };
            A.CallTo(() => winampController.CurrentSong).Returns(bloodRave);

            manager.Update();
        }
    }
}