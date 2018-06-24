using FluentAssertions;
using WinampNowPlayingToFile;
using Xunit;

namespace Test
{
    public class NowPlayingToFilePluginTest
    {
        [Fact]
        public void pluginName()
        {
            new NowPlayingToFilePlugin().Name.Should().Be("Now Playing to File");
        }
    }
}