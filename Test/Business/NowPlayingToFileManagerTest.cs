﻿#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Daniel15.Sharpamp;
using FakeItEasy;
using FluentAssertions;
using WinampNowPlayingToFile;
using WinampNowPlayingToFile.Business;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Settings;
using Xunit;
using Song = WinampNowPlayingToFile.Facade.Song;
using SongChangedEventArgs = WinampNowPlayingToFile.Facade.SongChangedEventArgs;

namespace Test.Business {

    public class NowPlayingToFileManagerTest: IDisposable {

        private readonly WinampController        winampController = A.Fake<WinampController>();
        private readonly NowPlayingToFileManager manager;
        private readonly string                  textFilename     = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing_test.txt");
        private readonly string                  albumArtFilename = Environment.ExpandEnvironmentVariables(@"%TEMP%\winamp_now_playing_test.png");
        private readonly ISettings               settings         = A.Fake<ISettings>();
        private readonly Encoding                utf8             = new UTF8Encoding(false, true);

        private Song song = new() {
            Album    = "album",
            Artist   = "artist",
            Title    = "title",
            Year     = 2018,
            Filename = @"Tracks\empty.flac"
        };

        private readonly IList<string> filesToDeleteOnDisposal = new List<string>();

        public NowPlayingToFileManagerTest() {
            cleanUp();

            A.CallTo(() => winampController.status).Returns(Status.Playing);
            A.CallTo(() => winampController.currentSong).Returns(song);
            A.CallTo(() => settings.textFilename).Returns(textFilename);
            A.CallTo(() => settings.textTemplate).Returns(new RegistrySettings().loadDefaults().textTemplate);
            A.CallTo(() => settings.albumArtFilename).Returns(albumArtFilename);

            manager = new NowPlayingToFileManager(settings, winampController);
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
            winampController.songChanged += Raise.FreeForm.With(winampController, new SongChangedEventArgs(new Song()));

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
                Filename = @"C:\Users\Ben\Music\Big Beat\The Crystal Method\Blood Rave.mp3",
                Artist   = "The Crystal Method",
                Title    = "Blood Rave",
                Album    = "Mixes and Soundtracks"
            });

            manager.update();
        }

        [Fact]
        public void dontCrashOnUrisWithFileExtensions() {
            A.CallTo(() => winampController.currentSong).Returns(new Song {
                Filename = @"http://135.125.239.164:8080/dance.mp3",
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
        public void extractDefaultAlbumArtWhenPlayingAndTrackHasNoAlbumArt() {
            song = new Song {
                Album    = "",
                Artist   = "artist",
                Title    = "title",
                Year     = 2018,
                Filename = @"Tracks\noalbumart.flac"
            };

            byte[] actual   = manager.findAlbumArt(song);
            byte[] expected = Resources.black_png;
            actual.Should().BeEquivalentTo(expected);
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
                "albumart.png",
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

            manager.findAlbumArt(song).Should().BeEquivalentTo(Resources.black_png);
        }

    }

}