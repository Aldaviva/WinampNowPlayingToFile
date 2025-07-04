using FluentAssertions;
using Microsoft.Win32;
using System;
using WinampNowPlayingToFile.Settings;
using Xunit;

namespace Test.Settings;

public class RegistrySettingsTest: IDisposable {

    private readonly RegistrySettings settings;

    public RegistrySettingsTest() {
        settings = new RegistrySettings { keyPath = @"Software\WinampNowPlayingToFile-test" };
        cleanUp();
    }

    public void Dispose() {
        cleanUp();
    }

    private void cleanUp() {
        try {
            Registry.CurrentUser.DeleteSubKeyTree(settings.keyPath);
        } catch (ArgumentException) {
            //key did not exist, which we want
        }
    }

    [Fact]
    public void load() {
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(settings.keyPath)!) {
            key.SetValue("TextFilename", "a");
            key.SetValue("AlbumArtFilename", "b");
            key.SetValue("TextTemplate", "c");
        }

        settings.load();

        // settings.textFilename.Should().Be("a"); //TODO
        settings.albumArtFilename.Should().Be("b");
        // settings.textTemplate.Should().Be("c");//TODO
    }

    [Fact]
    public void save() {
        settings.albumArtFilename = "1";
        // settings.textFilename     = "2"; //TODO
        // settings.textTemplate     = "3";//TODO
        settings.save();

        using RegistryKey key = Registry.CurrentUser.OpenSubKey(settings.keyPath)!;
        key.Should().NotBeNull();
        // ReSharper disable once PossibleNullReferenceException
        key.GetValue("AlbumArtFilename").Should().Be("1");
        key.GetValue("TextFilename").Should().Be("2");
        key.GetValue("TextTemplate").Should().Be("3");
    }

    [Fact]
    public void leaveDefaultsLoadedWhenRegistryKeysAreMissing() {
        settings.loadDefaults();
        string defaultAlbumArtFilename = settings.albumArtFilename;
        // string defaultTextTemplate     = settings.textTemplate; //TODO
        // string defaultTextFilename     = settings.textFilename;//TODO

        settings.load();
        settings.albumArtFilename.Should().Be(defaultAlbumArtFilename);
        // settings.textTemplate.Should().Be(defaultTextTemplate); //TODO
        // settings.textFilename.Should().Be(defaultTextFilename); //TODO
    }

}