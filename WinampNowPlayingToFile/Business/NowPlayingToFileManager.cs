using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Daniel15.Sharpamp;
using Mustache;
using TagLib;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Settings;
using File = System.IO.File;

namespace WinampNowPlayingToFile.Business {

    public class NowPlayingToFileManager {

        private static readonly FormatCompiler TemplateCompiler = new FormatCompiler();

        private static byte[] DefaultAlbumArt {
            get {
                try {
                    return File.ReadAllBytes("emptyAlbumArt.png");
                } catch {
                    return Resources.black_png;
                }
            }
        }

        private readonly WinampController winampController;
        private readonly ISettings settings;

        private Generator cachedTemplate;

        public NowPlayingToFileManager(ISettings settings, WinampController winampController) {
            this.winampController = winampController;
            this.settings = settings;

            this.winampController.SongChanged += delegate { Update(); };
            this.winampController.StatusChanged += delegate {
                Update();
            }; //these two events can get fired together, which seems to trigger a large throttle in OBS. Maybe we can apply our own shorter throttle to prevent the staggered rerender?
            this.settings.SettingsUpdated += delegate {
                cachedTemplate = null;
                Update();
            };
        }

        public void Update() {
            try {
                SaveText(RenderText());
                SaveImage(ExtractAlbumArt());
            } catch (Exception e) {
                MessageBox.Show("Unhandled exception while updating song info on song change:\n" + e, "Now Playing To File error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal string RenderText() {
            return winampController.Status == Status.Playing ? GetTemplate().Render(winampController.CurrentSong) : string.Empty;
        }

        private void SaveText(string nowPlayingText) {
            File.WriteAllText(settings.TextFilename, nowPlayingText, new UTF8Encoding(false));
        }

        private Generator GetTemplate() {
            return cachedTemplate ?? (cachedTemplate = TemplateCompiler.Compile(settings.TextTemplate));
        }

        internal byte[] ExtractAlbumArt() {
            try {
                return winampController.Status == Status.Playing
                    ? TagLib.File.Create(winampController.CurrentSong.Filename)
                          .Tag
                          .Pictures
                          .ElementAtOrDefault(0)?
                          .Data
                          .Data ?? DefaultAlbumArt
                    : null;
            } catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException) {
                /*
                 * Probably just a race:
                 * 1. Stop playing a song
                 * 2. Delete that song or its parent directory
                 * 3. Start playing a new song
                 * 4. Status property changed, and StatusChanged event fired
                 * 5. CurrentSong property updated with new filename and other metadata, and SongChanged event fired
                 *
                 * It seems like the StatusChanged event is fired before the CurrentSong property is updated, so this plugin tries to extract album art from the deleted file.
                 * We can ignore this because the SongChanged event will be fired immediately afterwards, so we will get the correct artwork from that.
                 */
                return null;
            } catch (Exception e) when (e is UnsupportedFormatException || e is CorruptFileException) {
                /*
                 * TagLib cannot read the metadata from the given file. This can happen with MIDI music, for instance.
                 */
                return null;
            }
        }

        private void SaveImage(byte[] imageData) {
            string filename = settings.AlbumArtFilename;
            if (imageData != null) {
                File.WriteAllBytes(filename, imageData);
            } else {
                File.Delete(filename);
            }
        }

        public virtual void OnQuit() {
            SaveText(string.Empty);
            SaveImage(null);
        }

    }

}