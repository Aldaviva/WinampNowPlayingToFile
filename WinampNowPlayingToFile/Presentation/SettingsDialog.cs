using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Mustache;
using WinampNowPlayingToFile.Facade;

namespace WinampNowPlayingToFile.Presentation
{
    public partial class SettingsDialog : Form
    {
        private static readonly FormatCompiler TemplateCompiler = new FormatCompiler();

        private readonly Settings.ISettings settings;

        private static readonly Song ExampleSong = new Song
        {
            Album = "The Joshua Tree",
            Artist = "U2",
            Filename = "Exit.flac",
            Title = "Exit",
            Year = 1987
        };

        public SettingsDialog(Settings.ISettings settings)
        {
            this.settings = settings;
            InitializeComponent();
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            nowPlayingFilenameEditor.InitialDirectory = Path.GetDirectoryName(settings.NowPlayingFilename);
            nowPlayingFilenameEditor.FileName = settings.NowPlayingFilename;

            writeToFileFilename.Text = settings.NowPlayingFilename;

            templateEditor.Text = settings.NowPlayingTemplate;
            templateEditor.Select(templateEditor.TextLength, 0);
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
            templatePreview.Text = TemplateCompiler.Compile(templateEditor.Text).Render(ExampleSong);
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

        private void OkButton_Click(object sender, EventArgs e)
        {
            settings.NowPlayingFilename = nowPlayingFilenameEditor.FileName;
            settings.NowPlayingTemplate = templateEditor.Text;
            settings.Save();
            Close();
        }
    }
}
