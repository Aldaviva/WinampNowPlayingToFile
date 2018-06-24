using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Mustache;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Settings;

namespace WinampNowPlayingToFile.Presentation
{
    public partial class SettingsDialog : Form
    {
        private static readonly FormatCompiler TemplateCompiler = new FormatCompiler();

        private readonly ISettings settings;
        private readonly WinampControllerImpl winampController;

        private static readonly Song ExampleSong = new Song
        {
            Album = "The Joshua Tree",
            Artist = "U2",
            Filename = "Exit.flac",
            Title = "Exit",
            Year = 1987
        };

        public SettingsDialog(ISettings settings, WinampControllerImpl winampController)
        {
            this.settings = settings;
            this.winampController = winampController;
            InitializeComponent();
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            nowPlayingFilenameEditor.InitialDirectory = Path.GetDirectoryName(settings.NowPlayingFilename);
            nowPlayingFilenameEditor.FileName = settings.NowPlayingFilename;

            writeToFileFilename.Text = settings.NowPlayingFilename;

            templateEditor.Text = settings.NowPlayingTemplate;
            templateEditor.Select(templateEditor.TextLength, 0);

            winampController.SongChanged += delegate { RenderPreview(); };

            applyButton.Enabled = false;
        }

        private void WriteToFileBrowseButtonClick(object sender, EventArgs e)
        {
            nowPlayingFilenameEditor.ShowDialog();
            writeToFileFilename.Text = nowPlayingFilenameEditor.FileName;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TemplateEditor_TextChanged(object sender, EventArgs e)
        {
            RenderPreview();
            OnFormDirty();
        }

        private void RenderPreview()
        {
            Song previewSong = string.IsNullOrEmpty(winampController.CurrentSong.Title)
                ? ExampleSong
                : winampController.CurrentSong;

            try
            {
                templatePreview.Text = TemplateCompiler.Compile(templateEditor.Text).Render(previewSong);
            }
            catch (FormatException e)
            {
                templatePreview.Text = $"Template format error: {e.Message}";
            }
        }

        private void TemplateInsertButton_Click(object sender, EventArgs e)
        {
            insertTemplatePlaceholderMenu.Show(templateInsertButton, new Point(0, templateInsertButton.Height));
        }

        private void InsertTemplatePlaceholderMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == helpToolStripMenuItem)
            {
                Process.Start("https://handlebarsjs.com/");
            }
            else
            {
                string placeholder = $"{{{{{e.ClickedItem.Text}}}}}";
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

        private void NowPlayingFilenameEditor_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OnFormDirty();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                Save();
                Close();
            }
            catch (FormatException)
            {
                //leave form open, with invalid inputs unsaved
            }
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            try
            {
                Save();
            }
            catch (FormatException)
            {
                //leave form open, with invalid inputs unsaved
            }
        }

        private void Save()
        {
            try
            {
                TemplateCompiler.Compile(templateEditor.Text);

                settings.NowPlayingFilename = nowPlayingFilenameEditor.FileName;
                settings.NowPlayingTemplate = templateEditor.Text;
                settings.Save();
            }
            catch (FormatException e)
            {
                MessageBox.Show($"Invalid template:\n\n{e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void OnFormDirty()
        {
            applyButton.Enabled = true;
        }
    }
}