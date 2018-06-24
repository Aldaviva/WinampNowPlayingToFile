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

        private readonly WinampController winampController;
        private readonly ISettings settings;

        private Generator cachedTemplate;

        public NowPlayingToFileManager(ISettings settings, WinampController winampController)
        {
            this.winampController = winampController;
            this.settings = settings;

            this.winampController.SongChanged += delegate { Update(); };
            this.winampController.StatusChanged += delegate { Update(); };
            this.settings.SettingsUpdated += delegate
            {
                cachedTemplate = null;
                Update();
            };
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
            File.WriteAllText(settings.NowPlayingFilename, nowPlayingText, Encoding.UTF8);
        }

        private Generator GetTemplate()
        {
            return cachedTemplate ?? (cachedTemplate = TemplateCompiler.Compile(settings.NowPlayingTemplate));
        }

        public void OnQuit()
        {
            Save(string.Empty);
        }
    }
}