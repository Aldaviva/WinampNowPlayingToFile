using System.IO;
using System.Text;
using Daniel15.Sharpamp;
using Mustache;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Settings;
using Song = WinampNowPlayingToFile.Facade.Song;

namespace WinampNowPlayingToFile.Business
{
    public class NowPlayingToFileManager
    {
        private static readonly FormatCompiler TemplateCompiler = new FormatCompiler();

        public ISettings Settings { get; }

        private Generator cachedTemplate;
        private readonly WinampController winampController;

        public NowPlayingToFileManager(WinampController winampController, ISettings settings)
        {
            this.winampController = winampController;
            Settings = settings;

            this.winampController.SongChanged += delegate { Update(); };
            this.winampController.StatusChanged += delegate { Update(); };
        }

        public void Update()
        {
            Save(Render(winampController.CurrentSong));
        }

        internal string Render(Song song)
        {
            return winampController.Status == Status.Playing ? GetTemplate().Render(song) : string.Empty;
        }

        private void Save(string nowPlayingText)
        {
            File.WriteAllText(Settings.NowPlayingFilename, nowPlayingText, Encoding.UTF8);
        }

        private Generator GetTemplate()
        {
            return cachedTemplate ?? (cachedTemplate = TemplateCompiler.Compile(Settings.NowPlayingTemplate));
        }

        public void OnQuit()
        {
            Save(string.Empty);
        }
    }
}