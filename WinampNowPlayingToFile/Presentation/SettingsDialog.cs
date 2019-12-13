using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Mustache;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Settings;

namespace WinampNowPlayingToFile.Presentation {

    public partial class SettingsDialog: Form {

        private static readonly FormatCompiler TemplateCompiler = new FormatCompiler();

        private readonly ISettings settings;
        private readonly WinampControllerImpl winampController;

        private static readonly Song ExampleSong = new Song {
            Album = "The Joshua Tree",
            Artist = "U2",
            Filename = "Exit.flac",
            Title = "Exit",
            Year = 1987
        };

        public SettingsDialog(ISettings settings, WinampControllerImpl winampController) {
            this.settings = settings;
            this.winampController = winampController;
            InitializeComponent();

            // Make buttons have animated state transitions, like every other program in the OS
            // https://stackoverflow.com/q/53456865/979493
            foreach (Control control in Controls) {
                if (control is ButtonBase flatStylableControl) {
                    flatStylableControl.FlatStyle = FlatStyle.System;
                }
            }
        }

        private void SettingsDialog_Load(object sender, EventArgs e) {
            textFilenameEditor.InitialDirectory = Path.GetDirectoryName(settings.TextFilename);
            textFilenameEditor.FileName = settings.TextFilename;

            albumArtFilenameEditor.InitialDirectory = Path.GetDirectoryName(settings.AlbumArtFilename);
            albumArtFilenameEditor.FileName = settings.AlbumArtFilename;

            textFilename.Text = settings.TextFilename;
            albumArtFilename.Text = settings.AlbumArtFilename;

            templateEditor.Text = settings.TextTemplate;
            templateEditor.Select(templateEditor.TextLength, 0);

            winampController.SongChanged += delegate { RenderPreview(); };

            applyButton.Enabled = false;
        }

        private void WriteToFileBrowseButtonClick(object sender, EventArgs e) {
            textFilenameEditor.ShowDialog();
            textFilename.Text = textFilenameEditor.FileName;
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            Close();
        }

        private void TemplateEditor_TextChanged(object sender, EventArgs e) {
            RenderPreview();
            OnFormDirty();
        }

        private void RenderPreview() {
            Song previewSong = string.IsNullOrEmpty(winampController.CurrentSong.Title)
                ? ExampleSong
                : winampController.CurrentSong;

            try {
                templatePreview.Text = TemplateCompiler.Compile(templateEditor.Text).Render(previewSong);
            } catch (FormatException e) {
                templatePreview.Text = $"Template format error: {e.Message}";
            }
        }

        private void TemplateInsertButton_Click(object sender, EventArgs e) {
            insertTemplatePlaceholderMenu.Show(templateInsertButton, new Point(0, templateInsertButton.Height));
        }

        private void InsertTemplatePlaceholderMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if (e.ClickedItem == helpToolStripMenuItem) {
                Process.Start("https://handlebarsjs.com/");
            } else {
                string textToInsert;
                if (e.ClickedItem == newLineToolStripMenuItem) {
                    textToInsert = "#newline";
                } else if (e.ClickedItem == ifToolStripMenuItem) {
                    textToInsert = "#if Album}} - {{Album}}{{/if";
                } else if (e.ClickedItem == ifElseToolStripMenuItem) {
                    textToInsert = "#if Album}} - {{Album}}{{#else}} - (no album){{/if";
                } else {
                    textToInsert = e.ClickedItem.Text;
                }

                string placeholder = $"{{{{{textToInsert}}}}}";
                string originalTemplate = templateEditor.Text;
                int selectionEnd = templateEditor.SelectionStart + templateEditor.SelectionLength;

                var newTemplate = new StringBuilder();
                newTemplate.Append(originalTemplate.Substring(0, templateEditor.SelectionStart));
                newTemplate.Append(placeholder);
                newTemplate.Append(originalTemplate.Substring(selectionEnd));

                templateEditor.Text = newTemplate.ToString();
                templateEditor.SelectionStart = selectionEnd;
                templateEditor.SelectionLength = 0;
            }
        }

        private void OkButton_Click(object sender, EventArgs e) {
            try {
                Save();
                Close();
            } catch (FormatException) {
                //leave form open, with invalid inputs unsaved
            }
        }

        private void ApplyButton_Click(object sender, EventArgs e) {
            try {
                Save();
            } catch (FormatException) {
                //leave form open, with invalid inputs unsaved
            }
        }

        private void Save() {
            try {
                TemplateCompiler.Compile(templateEditor.Text);

                settings.TextFilename = textFilenameEditor.FileName;
                settings.AlbumArtFilename = albumArtFilenameEditor.FileName;
                settings.TextTemplate = templateEditor.Text;
                settings.Save();

                applyButton.Enabled = false;
            } catch (FormatException e) {
                MessageBox.Show($"Invalid template:\n\n{e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void OnFormDirty() {
            applyButton.Enabled = true;
        }

        private void albumArtBrowseButton_Click(object sender, EventArgs e) {
            albumArtFilenameEditor.ShowDialog();
            albumArtFilename.Text = albumArtFilenameEditor.FileName;
        }

        private void textFilenameEditor_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            OnFormDirty();
        }

        private void albumArtFilenameEditor_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            OnFormDirty();
        }

    }

}