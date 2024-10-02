#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinampNowPlayingToFile.Facade;
using WinampNowPlayingToFile.Facade.Templating;
using WinampNowPlayingToFile.Settings;
using Timer = System.Timers.Timer;

namespace WinampNowPlayingToFile.Presentation;

public partial class SettingsDialog: Form {

    private readonly ISettings        upstreamSettings;
    private readonly ISettings        workingSettings;
    private readonly WinampController winampController;
    private readonly Timer            renderTextTimer = new() { Enabled = true, Interval = 1000 };

    private static readonly UnfuckedTemplateCompiler TEMPLATE_COMPILER = new UnfuckedMustacheCompiler();

    private static readonly Song EXAMPLE_SONG = new() {
        Album    = "The Joshua Tree",
        Artist   = "U2",
        Filename = "C:\\Exit.mp3",
        Title    = "Exit",
        Year     = 1987
    };

    private static readonly IReadOnlyDictionary<string, object> EXAMPLE_SONG_EXTRA_METADATA = new ReadOnlyDictionary<string, object>(new Dictionary<string, object> {
        { "albumartist", "U2" },
        { "bitrate", 320 },
        { "bpm", 123.0 },
        { "category", "Rock" },
        { "composer", "U2" },
        { "disc", 1 },
        { "elapsed", TimeSpan.FromMilliseconds(251422 / 2.0) },
        { "family", "MPEG Layer 3 Audio File" },
        { "filebasename", "Exit.mp3" },
        { "filebasenamewithoutextension", "Exit" },
        { "gain", "+0.92 dB" },
        { "genre", "Rock" },
        { "key", "E minor" },
        { "length", TimeSpan.FromMilliseconds(251422) }, //4:11
        { "lossless", false },
        { "media", "LP" },
        { "playbackstate", "playing" },
        { "producer", "Brian Eno, Daniel Lanois" },
        { "publisher", "Island Records" },
        { "rating", 2 },
        { "replaygain_album_gain", "-3.03 dB" },
        { "replaygain_album_peak", 1.022630334 },
        { "replaygain_track_gain", "-0.77 dB" },
        { "replaygain_track_peak", 1.006227493 },
        { "stereo", true },
        { "track", 10 },
        { "type", "audio" },
        { "vbr", false }
    });

    private int textFileIndex;

    public SettingsDialog(ISettings upstreamSettings, WinampControllerImpl winampController) {
        this.upstreamSettings = upstreamSettings;
        this.winampController = winampController;
        workingSettings       = new InMemorySettings();
        workingSettings.load(upstreamSettings);
        InitializeComponent();

        // Make buttons have animated state transitions, like every other program in the OS
        // https://stackoverflow.com/q/53456865/979493
        foreach (ButtonBase flatStylableControl in Controls.OfType<ButtonBase>()) {
            flatStylableControl.FlatStyle = FlatStyle.System;
        }
    }

    private void onSettingsDialogLoad(object sender, EventArgs e) {
        loadTextFileSettings();

        albumArtFilenameEditor.InitialDirectory = Path.GetDirectoryName(workingSettings.albumArtFilename);
        albumArtFilenameEditor.FileName         = workingSettings.albumArtFilename;
        albumArtFilename.Text                   = workingSettings.albumArtFilename;

        preserveTextFileWhenNotPlaying.Checked = workingSettings.preserveTextFileWhenNotPlaying;
        preserveAlbumArtWhenNotPlaying.Checked = workingSettings.preserveAlbumArtFileWhenNotPlaying;

        foreach (string filename in workingSettings.textFilenames) {
            textFileMenu.Items.Add(Path.GetFileName(filename));
        }

        winampController.songChanged += delegate { renderPreview(); };
        renderTextTimer.Elapsed      += delegate { renderPreview(); };

        applyButton.Enabled = false;
    }

    private void loadTextFileSettings() {
        textFilenameEditor.InitialDirectory = Path.GetDirectoryName(workingSettings.textFilenames[textFileIndex]);
        textFilenameEditor.FileName         = workingSettings.textFilenames[textFileIndex];

        textFilename.Text = workingSettings.textFilenames[textFileIndex];

        templateEditor.Text = workingSettings.textTemplates[textFileIndex];
        templateEditor.Select(templateEditor.TextLength, 0);
    }

    private void onTextFileMenuSelectionChanged(object sender, EventArgs e) {
        try {
            saveWorking();
            textFileIndex = textFileMenu.SelectedIndex;
            loadTextFileSettings();
        } catch (Exception ex) when (ex is FormatException or KeyNotFoundException) {
            textFileMenu.SelectedIndex = textFileIndex;
        }
    }

    private void addTextFile(object sender, EventArgs e) {
        saveWorking();
        textFileIndex++;
        workingSettings.textFilenames.Insert(textFileIndex, string.Empty);
        workingSettings.textTemplates.Insert(textFileIndex, string.Empty);
        textFileMenu.Items.Insert(textFileIndex, string.Empty);
        textFileMenu.SelectedIndex = textFileIndex;
    }

    private void removeTextFile(object sender, EventArgs e) {
        if (workingSettings.textFilenames.Count >= 2) {
            workingSettings.textFilenames.RemoveAt(textFileIndex);
            workingSettings.textTemplates.RemoveAt(textFileIndex);
            textFileMenu.Items.RemoveAt(textFileIndex);
            textFileIndex = textFileMenu.SelectedIndex;
            // TODO hopefully this selects the following or preceding index automatically
        }
    }

    private void onTextFileBrowseButtonClick(object sender, EventArgs e) {
        onBrowseButtonClick(textFilenameEditor, textFilename);
    }

    private void onAlbumArtBrowseButtonClick(object sender, EventArgs e) {
        onBrowseButtonClick(albumArtFilenameEditor, albumArtFilename);
    }

    private static void onBrowseButtonClick(SaveFileDialog filenameEditor, TextBox filenameTextBox) {
        try {
            filenameEditor.FileName = Path.GetFileName(filenameTextBox.Text) ?? "";
        } catch (ArgumentException) {
            filenameEditor.FileName = "";
        }

        try {
            filenameEditor.InitialDirectory = Path.GetDirectoryName(filenameTextBox.Text) ?? "";
        } catch (ArgumentException) {
            filenameEditor.InitialDirectory = "";
        }

        filenameEditor.ShowDialog();
        filenameTextBox.Text = filenameEditor.FileName;
    }

    private void onCancel(object sender, EventArgs e) {
        Close();
    }

    private void onTemplateChange(object sender, EventArgs e) {
        renderPreview();
        onFormDirty();
    }

    private void renderPreview() {
        Song previewSong = isSongPlaying() ? winampController.currentSong : EXAMPLE_SONG;

        try {
            templatePreview.Text = compileTemplate().render(previewSong);
        } catch (KeyNotFoundException e) {
            templatePreview.Text = $"Template key not found: {e.Message}";
        } catch (FormatException e) {
            templatePreview.Text = $"Template format error: {e.Message}";
        }
    }

    private bool isSongPlaying() => !string.IsNullOrEmpty(winampController.currentSong.Title);

    private UnfuckedGenerator compileTemplate() {
        UnfuckedGenerator generator = TEMPLATE_COMPILER.compile(templateEditor.Text);
        generator.keyNotFound += (_, args) => {
            args.substitute = isSongPlaying()
                ? winampController.fetchMetadataFieldValue(args.key)
                : EXAMPLE_SONG_EXTRA_METADATA.TryGetValue(args.key.ToLowerInvariant(), out object? value)
                    ? value
                    : string.Empty;

            args.handled = true;
        };
        return generator;
    }

    private void showTemplateMenu(object sender, EventArgs e) {
        insertTemplatePlaceholderMenu.Show(templateInsertButton, new Point(0, templateInsertButton.Height));
    }

    private void onTemplateMenuSelection(object sender, ToolStripItemClickedEventArgs e) {
        if (e.ClickedItem == helpToolStripMenuItem) {
            Process.Start("https://github.com/jehugaleahsa/mustache-sharp/blob/v1.0/README.md#placeholders");
        } else if (e.ClickedItem != otherToolStripMenuItem) {
            string textToInsert;
            if (e.ClickedItem == newLineToolStripMenuItem) {
                textToInsert = "#newline";
            } else if (e.ClickedItem == ifToolStripMenuItem) {
                textToInsert = "#if Album}} - {{Album}}{{/if";
            } else if (e.ClickedItem == ifElseToolStripMenuItem) {
                textToInsert = "#if Album}} - {{Album}}{{#else}} - (no album){{/if";
            } else if (e.ClickedItem.Tag is string tag && !string.IsNullOrWhiteSpace(tag)) {
                textToInsert = tag;
            } else {
                textToInsert = e.ClickedItem.Text;
            }

            string placeholder      = $$$"""{{{{{textToInsert}}}}}""";
            string originalTemplate = templateEditor.Text;
            int    selectionStart   = templateEditor.SelectionStart;
            int    selectionEnd     = selectionStart + templateEditor.SelectionLength;

            StringBuilder newTemplate = new();
            newTemplate.Append(originalTemplate.Substring(0, selectionStart));
            newTemplate.Append(placeholder);
            newTemplate.Append(originalTemplate.Substring(selectionEnd));

            templateEditor.Text            = newTemplate.ToString();
            templateEditor.SelectionLength = 0;
            templateEditor.SelectionStart  = selectionStart + placeholder.Length;
            templateEditor.Focus();
        }
    }

    private void onClickOk(object sender, EventArgs args) {
        try {
            saveUpstream();
            Close();
        } catch (Exception e) when (e is FormatException or KeyNotFoundException) {
            //leave form open, with invalid inputs unsaved
        }
    }

    private void onClickApply(object sender, EventArgs args) {
        try {
            saveUpstream();
        } catch (Exception e) when (e is FormatException or KeyNotFoundException) {
            //leave form open, with invalid inputs unsaved
        }
    }

    private void saveWorking() {
        try {
            compileTemplate().render(EXAMPLE_SONG);

            workingSettings.textFilenames[textFileIndex]       = textFilename.Text;
            workingSettings.albumArtFilename                   = albumArtFilename.Text;
            workingSettings.textTemplates[textFileIndex]       = templateEditor.Text;
            workingSettings.preserveAlbumArtFileWhenNotPlaying = preserveAlbumArtWhenNotPlaying.Checked;
            workingSettings.preserveTextFileWhenNotPlaying     = preserveTextFileWhenNotPlaying.Checked;
        } catch (FormatException e) {
            MessageBox.Show($"Invalid template:\n\n{e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw;
        }
    }

    private void saveUpstream() {
        IEnumerable<string> oldtextFilenames = upstreamSettings.textFilenames.Select(Path.GetFullPath).ToList();

        saveWorking();

        upstreamSettings.load(workingSettings);
        upstreamSettings.save();

        applyButton.Enabled = false;

        foreach (string removedFilename in oldtextFilenames.Except(upstreamSettings.textFilenames.Select(Path.GetFullPath), StringComparer.OrdinalIgnoreCase)) {
            try {
                File.Delete(removedFilename);
            } catch (Exception e) when (e is not OutOfMemoryException) {
                // continue
            }
        }
    }

    private void onFormDirty() {
        applyButton.Enabled = true;
    }

    private void onSubmitFilename(object sender, CancelEventArgs e) {
        onFormDirty();
        if (sender == textBrowseButton) {
            updateTextFileMenuEntryName();
        }
    }

    private void onFilenameChange(object sender, EventArgs e) {
        onFormDirty();
        if (sender == textFilename) {
            updateTextFileMenuEntryName();
        }
    }

    private void updateTextFileMenuEntryName() {
        textFileMenu.Items[textFileIndex] = Path.GetFileName(textFilename.Text);
    }

    protected override void OnClosed(EventArgs e) {
        renderTextTimer.Dispose();
        base.OnClosed(e);
    }

}