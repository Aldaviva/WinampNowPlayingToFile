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

        private static readonly FormatCompiler TEMPLATE_COMPILER = new FormatCompiler();

        private static byte[] defaultAlbumArt {
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

            this.winampController.songChanged += delegate { update(); };
            this.winampController.statusChanged += delegate { update(); };
            this.settings.settingsUpdated += delegate {
                cachedTemplate = null;
                update();
            };
        }
         
        public void update() {
            try {
                saveText(renderText());
                saveImage(extractAlbumArt());
            } catch (Exception e) {
                MessageBox.Show("Unhandled exception while updating song info on song change:\n" + e, "Now Playing To File error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal string renderText() {
            return winampController.status == Status.Playing ? getTemplate().Render(winampController.currentSong) : string.Empty;
        }

        private void saveText(string nowPlayingText) {
            File.WriteAllText(settings.textFilename, nowPlayingText, new UTF8Encoding(false));
        }

        private Generator getTemplate() {
            return cachedTemplate ?? (cachedTemplate = TEMPLATE_COMPILER.Compile(settings.textTemplate));
        }

        internal byte[] extractAlbumArt() {
            try {
                return winampController.status == Status.Playing
                    ? TagLib.File.Create(winampController.currentSong.Filename)
                          .Tag
                          .Pictures
                          .ElementAtOrDefault(0)?
                          .Data
                          .Data ?? defaultAlbumArt
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

        private void saveImage(byte[] imageData) {
            string filename = settings.albumArtFilename;
            if (imageData != null) {
                File.WriteAllBytes(filename, imageData);
            } else {
                File.Delete(filename);
            }
        }

        public virtual void onQuit() {
            saveText(string.Empty);
            saveImage(null);
        }

    }

}