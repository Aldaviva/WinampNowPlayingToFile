using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Daniel15.Sharpamp;
using Mustache;
using TagLib;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Settings;
using File = System.IO.File;

namespace WinampNowPlayingToFile.Business
{
    public class NowPlayingToFileManager
    {
        private static readonly FormatCompiler TemplateCompiler = new FormatCompiler();

        private static byte[] DefaultAlbumArt
        {
            get {
                try
                {
                    return File.ReadAllBytes("emptyAlbumArt.png");
                }
                catch
                {
                    return Resources.black_png;
                }
            }
        }

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
            File.WriteAllText(settings.TextFilename, nowPlayingText, new UTF8Encoding(false));
        }

        private Generator GetTemplate()
        {
            return cachedTemplate ?? (cachedTemplate = TemplateCompiler.Compile(settings.TextTemplate));
        }

        internal byte[] ExtractAlbumArt()
        {
            try
            {
                return winampController.Status == Status.Playing
                    ? TagLib.File.Create(winampController.CurrentSong.Filename)
                          .Tag
                          .Pictures
                          .ElementAtOrDefault(0)?
                          .Data
                          .Data ?? DefaultAlbumArt
                    : null;
            }
            catch (Exception e) when(e is UnsupportedFormatException || e is CorruptFileException)
            {
                return null;
            }
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