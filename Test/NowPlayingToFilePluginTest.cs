using System;
using FakeItEasy;
using FluentAssertions;
using WinampNowPlayingToFile;
using WinampNowPlayingToFile.Business;
using Xunit;

namespace Test
{
    public class NowPlayingToFilePluginTest
    {
        private readonly NowPlayingToFilePlugin plugin;
        private readonly NowPlayingToFileManager manager;

        public NowPlayingToFilePluginTest()
        {
            manager = A.Fake<NowPlayingToFileManager>();
            plugin  = new NowPlayingToFilePlugin { manager = manager };
        }

        [Fact]
        public void pluginName() {
            plugin.Name.Should().StartWith("Now Playing to File v");
        }

        [Fact]
        public void initialize() {
            plugin.Init(IntPtr.Zero);
            plugin.Initialize();
        }

        [Fact]
        public void quit() {
            A.CallTo(() => manager.onQuit()).DoesNothing();

            plugin.Quit();

            A.CallTo(() => manager.onQuit()).MustHaveHappened();
        }
    }
}