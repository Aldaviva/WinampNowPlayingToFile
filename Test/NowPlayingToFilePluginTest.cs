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
            plugin = new NowPlayingToFilePlugin { Manager = manager };
        }

        [Fact]
        public void PluginName()
        {
            plugin.Name.Should().StartWith("Now Playing to File v");
        }

        [Fact]
        public void Initialize()
        {
            plugin.Init(IntPtr.Zero);
            plugin.Initialize();
        }

        [Fact]
        public void Quit()
        {
            A.CallTo(() => manager.OnQuit()).DoesNothing();

            plugin.Quit();

            A.CallTo(() => manager.OnQuit()).MustHaveHappened();
        }
    }
}