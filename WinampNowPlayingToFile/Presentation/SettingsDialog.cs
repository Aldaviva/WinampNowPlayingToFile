using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mustache;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Settings;

namespace WinampNowPlayingToFile.Presentation;

public partial class SettingsDialog: Form {

    private static readonly FormatCompiler TEMPLATE_COMPILER = new();

    private readonly ISettings            settings;
    private readonly WinampControllerImpl winampController;

    private static readonly Song EXAMPLE_SONG = new() {
        Album    = "The Joshua Tree",
        Artist   = "U2",
        Filename = "Exit.flac",
        Title    = "Exit",
        Year     = 1987
    };

    public SettingsDialog(ISettings settings, WinampControllerImpl winampController) {
        this.settings         = settings;
        this.winampController = winampController;
        InitializeComponent();

        // Make buttons have animated state transitions, like every other program in the OS
        // https://stackoverflow.com/q/53456865/979493
        foreach (ButtonBase flatStylableControl in Controls.OfType<ButtonBase>()) {
            flatStylableControl.FlatStyle = FlatStyle.System;
        }
    }

    private void SettingsDialog_Load(object sender, EventArgs e) {
        textFilenameEditor.InitialDirectory = Path.GetDirectoryName(settings.textFilename);
        textFilenameEditor.FileName         = settings.textFilename;

        albumArtFilenameEditor.InitialDirectory = Path.GetDirectoryName(settings.albumArtFilename);
        albumArtFilenameEditor.FileName         = settings.albumArtFilename;

        textFilename.Text     = settings.textFilename;
        albumArtFilename.Text = settings.albumArtFilename;

        templateEditor.Text = settings.textTemplate;
        templateEditor.Select(templateEditor.TextLength, 0);

        winampController.songChanged += delegate { renderPreview(); };

        applyButton.Enabled = false;
    }

    private void writeToFileBrowseButtonClick(object sender, EventArgs e) {
        textFilenameEditor.ShowDialog();
        textFilename.Text = textFilenameEditor.FileName;
    }

    private void CancelButton_Click(object sender, EventArgs e) {
        Close();
    }

    private void TemplateEditor_TextChanged(object sender, EventArgs e) {
        renderPreview();
        onFormDirty();
    }

    private void renderPreview() {
        Song previewSong = string.IsNullOrEmpty(winampController.currentSong.Title)
            ? EXAMPLE_SONG
            : winampController.currentSong;

        try {
            templatePreview.Text = TEMPLATE_COMPILER.Compile(templateEditor.Text).Render(previewSong);
        } catch (KeyNotFoundException e) {
            templatePreview.Text = $"Template key not found: {e.Message}";
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

            string placeholder      = $"{{{{{textToInsert}}}}}";
            string originalTemplate = templateEditor.Text;
            int    selectionEnd     = templateEditor.SelectionStart + templateEditor.SelectionLength;

            StringBuilder newTemplate = new();
            newTemplate.Append(originalTemplate.Substring(0, templateEditor.SelectionStart));
            newTemplate.Append(placeholder);
            newTemplate.Append(originalTemplate.Substring(selectionEnd));

            templateEditor.Text            = newTemplate.ToString();
            templateEditor.SelectionStart  = selectionEnd;
            templateEditor.SelectionLength = 0;
        }
    }

    private void OkButton_Click(object sender, EventArgs args) {
        try {
            save();
            Close();
        } catch (Exception e) when (e is FormatException or KeyNotFoundException) {
            //leave form open, with invalid inputs unsaved
        }
    }

    private void ApplyButton_Click(object sender, EventArgs args) {
        try {
            save();
        } catch (Exception e) when (e is FormatException or KeyNotFoundException) {
            //leave form open, with invalid inputs unsaved
        }
    }

    private void save() {
        try {
            TEMPLATE_COMPILER.Compile(templateEditor.Text).Render(EXAMPLE_SONG);

            settings.textFilename     = textFilenameEditor.FileName;
            settings.albumArtFilename = albumArtFilenameEditor.FileName;
            settings.textTemplate     = templateEditor.Text;
            settings.save();

            applyButton.Enabled = false;
        } catch (Exception e) when (e is FormatException or KeyNotFoundException) {
            MessageBox.Show($"Invalid template:\n\n{e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw;
        }
    }

    private void onFormDirty() {
        applyButton.Enabled = true;
    }

    private void albumArtBrowseButton_Click(object sender, EventArgs e) {
        albumArtFilenameEditor.ShowDialog();
        albumArtFilename.Text = albumArtFilenameEditor.FileName;
    }

    private void textFilenameEditor_FileOk(object sender, CancelEventArgs e) {
        onFormDirty();
    }

    private void albumArtFilenameEditor_FileOk(object sender, CancelEventArgs e) {
        onFormDirty();
    }

}