using System;
using FluentAssertions;
using Microsoft.Win32;
using WinampNowPlayingToFile.Settings;
using Xunit;

namespace Test.Settings
{
    public class RegistrySettingsTest : IDisposable
    {
        private readonly RegistrySettings settings;

        public RegistrySettingsTest()
        {
            settings = new RegistrySettings { Key = @"Software\WinampNowPlayingToFile-test" };
            CleanUp();
        }

        public void Dispose()
        {
            CleanUp();
        }

        private void CleanUp()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(settings.Key);
            }
            catch (ArgumentException)
            {
                //key did not exist, which we want
            }
        }

        [Fact]
        public void Load()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(settings.Key))
            {
                if (key != null)
                {
                    key.SetValue("TextFilename", "a");
                    key.SetValue("AlbumArtFilename", "b");
                    key.SetValue("TextTemplate", "c");
                }
            }

            settings.Load();

            settings.TextFilename.Should().Be("a");
            settings.AlbumArtFilename.Should().Be("b");
            settings.TextTemplate.Should().Be("c");
        }

        [Fact]
        public void Save()
        {
            settings.AlbumArtFilename = "1";
            settings.TextFilename = "2";
            settings.TextTemplate = "3";
            settings.Save();

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(settings.Key))
            {
                key.Should().NotBeNull();
                // ReSharper disable once PossibleNullReferenceException
                key.GetValue("AlbumArtFilename").Should().Be("1");
                key.GetValue("TextFilename").Should().Be("2");
                key.GetValue("TextTemplate").Should().Be("3");
            }
        }

        [Fact]
        public void LeaveDefaultsLoadedWhenRegistryKeysAreMissing()
        {
            settings.LoadDefaults();
            string defaultAlbumArtFilename = settings.AlbumArtFilename;
            string defaultTextTemplate = settings.TextTemplate;
            string defaultTextFilename = settings.TextFilename;

            settings.Load();
            settings.AlbumArtFilename.Should().Be(defaultAlbumArtFilename);
            settings.TextTemplate.Should().Be(defaultTextTemplate);
            settings.TextFilename.Should().Be(defaultTextFilename);
        }
    }
}