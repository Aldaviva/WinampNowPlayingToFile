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
            SaveText(RenderText(winampController.CurrentSong));
            ExtractAlbumArt();
        }

        private void ExtractAlbumArt()
        {
            IPicture artwork = TagLib.File.Create(winampController.CurrentSong.Filename)
                .Tag.Pictures.ElementAtOrDefault(0);

            if (artwork != null && winampController.Status == Status.Playing)
            {
                File.WriteAllBytes(settings.AlbumArtFilename, artwork.Data.Data);
            }
            else
            {
                File.Delete(settings.AlbumArtFilename);
            }
        }

        internal string RenderText(Song song)
        {
            return winampController.Status == Status.Playing ? GetTemplate().Render(song) : string.Empty;
        }

        private void SaveText(string nowPlayingText)
        {
            File.WriteAllText(settings.TextFilename, nowPlayingText, Encoding.UTF8);
        }

        private Generator GetTemplate()
        {
            return cachedTemplate ?? (cachedTemplate = TemplateCompiler.Compile(settings.TextTemplate));
        }

        public void OnQuit()
        {
            SaveText(string.Empty);
            File.Delete(settings.AlbumArtFilename);
        }
    }
}