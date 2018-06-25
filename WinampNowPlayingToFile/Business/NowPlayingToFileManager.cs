using System.Linq;
using System.Text;
using Daniel15.Sharpamp;
using Mustache;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Settings;
using File = System.IO.File;

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
            SaveText(RenderText());
            SaveImage(ExtractAlbumArt());
        }

        internal string RenderText()
        {
            return winampController.Status == Status.Playing ? GetTemplate().Render(winampController.CurrentSong) : string.Empty;
        }

        private void SaveText(string nowPlayingText)
        {
            File.WriteAllText(settings.TextFilename, nowPlayingText, Encoding.UTF8);
        }

        private Generator GetTemplate()
        {
            return cachedTemplate ?? (cachedTemplate = TemplateCompiler.Compile(settings.TextTemplate));
        }

        private byte[] ExtractAlbumArt()
        {
            return winampController.Status == Status.Playing
                ? TagLib.File.Create(winampController.CurrentSong.Filename).Tag.Pictures.ElementAtOrDefault(0)?.Data.Data
                : null;
        }

        private void SaveImage(byte[] imageData)
        {
            string filename = settings.AlbumArtFilename;
            if (imageData != null)
            {
                File.WriteAllBytes(filename, imageData);
            }
            else
            {
                File.Delete(filename);
            }
        }

        public virtual void OnQuit()
        {
            SaveText(string.Empty);
            SaveImage(null);
        }
    }
}