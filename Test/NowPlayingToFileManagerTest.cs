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
        private readonly string filename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing_test.txt");
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
            A.CallTo(() => settings.NowPlayingFilename).Returns(filename);
            A.CallTo(() => settings.NowPlayingTemplate).Returns(new RegistrySettings().LoadDefaults().NowPlayingTemplate);

            manager = new NowPlayingToFileManager(settings, winampController);
        }

        public void Dispose()
        {
            CleanUp();
        }

        private void CleanUp()
        {
            File.Delete(filename);
        }

        [Fact]
        public void RenderTemplateWithAlbum()
        {
            string actual = manager.Render(song);

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

            string actual = manager.Render(songWithoutAlbum);

            actual.Should().Be("artist \u2013 title");
        }

        [Fact]
        public void RenderTemplateWhenPaused()
        {
            A.CallTo(() => winampController.Status).Returns(Status.Paused);

            manager.Render(song).Should().BeEmpty();
        }

        [Fact]
        public void RenderTemplateWhenStopped()
        {
            A.CallTo(() => winampController.Status).Returns(Status.Stopped);

            manager.Render(song).Should().BeEmpty();
        }

        [Fact]
        public void WriteToFileOnSongChange()
        {
            winampController.SongChanged += Raise.FreeForm.With(winampController, new SongChangedEventArgs(new Song()));

            string actual = File.ReadAllText(filename, Encoding.UTF8);
            actual.Should().Be("artist \u2013 title \u2013 album");
        }

        [Fact]
        public void WriteToFileOnStatusChange()
        {
            winampController.StatusChanged += Raise.FreeForm.With(winampController, new StatusChangedEventArgs(Status.Playing));

            string actual = File.ReadAllText(filename, Encoding.UTF8);
            actual.Should().Be("artist \u2013 title \u2013 album");
        }
    }
}