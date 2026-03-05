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
using WinampNowPlayingToFile.Business;
using WinampNowPlayingToFile.Data;
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
    private static readonly string                   DEFAULT_DIRECTORY = Environment.GetEnvironmentVariable("TEMP") ?? string.Empty;

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
        { "directory", "C:" },
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
        { "rating_stars", "★★" },
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

        // ReSharper disable once VirtualMemberCallInConstructor - works great
        Text = $"{NowPlayingToFilePlugin.NAME_WITHOUT_VERSION} plug-in configuration";
        // Make buttons have animated state transitions, like every other program in the OS
        // https://stackoverflow.com/q/53456865/979493
        foreach (ButtonBase flatStylableControl in Controls.OfType<ButtonBase>()) {
            flatStylableControl.FlatStyle = FlatStyle.System;
        }
    }

    private void onSettingsDialogLoad(object sender, EventArgs e) {
        albumArtFilenameEditor.InitialDirectory = workingSettings.albumArtFilename is { Length: not 0 } file ? Path.GetDirectoryName(file) ?? string.Empty : string.Empty;
        albumArtFilenameEditor.FileName         = workingSettings.albumArtFilename ?? string.Empty;
        albumArtFilename.Text                   = workingSettings.albumArtFilename ?? string.Empty;

        preserveTextFileWhenNotPlaying.Checked = workingSettings.preserveTextFileWhenNotPlaying;
        preserveAlbumArtWhenNotPlaying.Checked = workingSettings.preserveAlbumArtFileWhenNotPlaying;

        foreach (string filename in workingSettings.textFilenames) {
            textFileMenu.Items.Add(Path.GetFullPath(filename));
        }
        loadTextFileSettings();
        textFileMenu.SelectedIndex = textFileIndex;

        winampController.songChanged += delegate { renderPreview(); };
        renderTextTimer.Elapsed      += delegate { renderPreview(); };

        applyButton.Enabled = false;
    }

    private void loadTextFileSettings() {
        string initialDirectory;
        try {
            initialDirectory = Path.GetDirectoryName(workingSettings.textFilenames[textFileIndex]) ?? DEFAULT_DIRECTORY;
        } catch (ArgumentException) {
            initialDirectory = DEFAULT_DIRECTORY;
        }
        textFilenameEditor.InitialDirectory = initialDirectory;
        textFilenameEditor.FileName         = workingSettings.textFilenames[textFileIndex];

        templateEditor.Text = workingSettings.textTemplates[textFileIndex];
        templateEditor.Select(0, 0);

        textFilename.Text = workingSettings.textFilenames[textFileIndex];
    }

    private void onTextFileMenuSelectionChanged(object? sender = null, EventArgs? e = null) {
        try {
            saveWorking();
            textFileIndex = textFileMenu.SelectedIndex;
            loadTextFileSettings();
        } catch (Exception ex) when (ex is not OutOfMemoryException) {
            textFileMenu.SelectedIndex = textFileIndex;
        }
    }

    private void addTextFile(object sender, EventArgs e) {
        try {
            saveWorking();
        } catch (Exception ex) when (ex is not OutOfMemoryException) {
            return;
        }
        textFileIndex++;
        workingSettings.textFilenames.Insert(textFileIndex, string.Empty);
        workingSettings.textTemplates.Insert(textFileIndex, string.Empty);
        textFileMenu.Items.Insert(textFileIndex, string.Empty);
        loadTextFileSettings();
        textFileMenu.SelectedIndex = textFileIndex;
    }

    private void removeTextFile(object sender, EventArgs e) {
        if (workingSettings.textFilenames.Count > 1) {
            workingSettings.textFilenames.RemoveAt(textFileIndex);
            workingSettings.textTemplates.RemoveAt(textFileIndex);
            textFileMenu.Items.RemoveAt(textFileIndex);
            textFileIndex = Math.Max(0, textFileIndex - 1);
            loadTextFileSettings();
            textFileMenu.SelectedIndex = textFileIndex;
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
            filenameEditor.FileName = Path.GetFileName(filenameTextBox.Text) ?? string.Empty;
        } catch (ArgumentException) {
            filenameEditor.FileName = string.Empty;
        }

        try {
            filenameEditor.InitialDirectory = Path.GetDirectoryName(filenameTextBox.Text) ?? DEFAULT_DIRECTORY;
        } catch (ArgumentException) {
            filenameEditor.InitialDirectory = DEFAULT_DIRECTORY;
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
        Song previewSong = isSongPlaying() ? winampController.currentSong! : EXAMPLE_SONG;

        try {
            templatePreview.Text = compileTemplate().render(previewSong);
        } catch (KeyNotFoundException e) {
            templatePreview.Text = $"Template key not found: {e.Message}";
        } catch (FormatException e) {
            templatePreview.Text = $"Template format error: {e.Message}";
        }
    }

    private bool isSongPlaying() => !string.IsNullOrEmpty(winampController.currentSong?.Title);

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
        } else if (e.ClickedItem is not ToolStripMenuItem { HasDropDown: true }) {
            string textToInsert;
            if (e.ClickedItem == newLineToolStripMenuItem) {
                textToInsert = "#newline";
            } else if (e.ClickedItem == ifToolStripMenuItem) {
                textToInsert = "#if Album}} - {{Album}}{{/if";
            } else if (e.ClickedItem == ifElseToolStripMenuItem) {
                textToInsert = "#if Album}} - {{Album}}{{#else}} - (no album){{/if";
            } else if (e.ClickedItem == ratingStarsCustomToolStripMenuItem) {
                textToInsert = "#each Rating_Stars}}⭐{{/each";
            } else if (e.ClickedItem == jsonObjectToolStripMenuItem) {
                textToInsert        = "#json artist album title year filename playbackState";
                templateEditor.Text = string.Empty;
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

    private void onCheckboxChange(object sender, EventArgs e) => onFormDirty();

    private void onClickPreserveTextFileDetailsLink(object sender, LinkLabelLinkClickedEventArgs e) => MessageBox.Show(this,
        """
        This checkbox lets you control the text file contents when Winamp is paused, stopped, or exited.

        When unchecked, the text file will be truncated to 0 bytes when Winamp isn't playing.

        When checked, the text file will preserve the rendered template text from the track that was most recently played.
        """, $"{preserveTextFileWhenNotPlaying.Text} - {NowPlayingToFilePlugin.NAME_WITHOUT_VERSION}", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void onClickPreserveAlbumArtDetailsLink(object sender, LinkLabelLinkClickedEventArgs e) => MessageBox.Show(this,
        $"""
        This checkbox lets you control the album art file when Winamp is paused, stopped, or exited.

        When unchecked, the image file will be replaced with a copy of "{Path.GetFullPath("stoppedAlbumArt.png")}" when Winamp isn't playing, or it will be deleted if that optional custom file doesn't exist.

        When checked, the image file will preserve the album art from the track that was most recently played.

        Separately, you may also supply a fallback image to use when the current track doesn't have album art by saving an image to "{Path.GetFullPath("emptyAlbumArt.png")}".
        """, $"{preserveAlbumArtWhenNotPlaying.Text} - {NowPlayingToFilePlugin.NAME_WITHOUT_VERSION}", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void onClickOk(object sender, EventArgs args) {
        try {
            saveUpstream();
            Close();
        } catch (Exception e) when (e is not OutOfMemoryException) {
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

            try {
                validateWritableFile(textFilename.Text);
            } catch (NowPlayingException.FileAccessException e) {
                MessageBox.Show($"Invalid text filename:\n\n{e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            if (!string.IsNullOrEmpty(albumArtFilename.Text)) {
                try {
                    validateWritableFile(albumArtFilename.Text);
                } catch (NowPlayingException.FileAccessException e) {
                    MessageBox.Show($"Invalid album art filename:\n\n{e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }

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

    ///<exception cref="NowPlayingException.FileAccessException" />
    private static void validateWritableFile(string filePath) {
        Stream? fileStream   = null;
        bool    existingFile = File.Exists(filePath);
        try {
            fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            fileStream.Dispose();
            fileStream = null;
            if (!existingFile) {
                File.Delete(filePath);
            }
        } catch (UnauthorizedAccessException e) {
            throw new NowPlayingException.FileAccessException("Permission to write to file denied", e);
        } catch (ArgumentException e) {
            throw new NowPlayingException.FileAccessException("Filename is empty or contains illegal characters", e);
        } catch (DirectoryNotFoundException e) {
            throw new NowPlayingException.FileAccessException("Nonexistent directory", e);
        } catch (IOException e) {
            throw new NowPlayingException.FileAccessException("File IO error", e);
        } catch (Exception e) when (e is not OutOfMemoryException) {
            throw new NowPlayingException.FileAccessException("Unhandled exception", e);
        } finally {
            fileStream?.Dispose();
        }
    }

    private void onFormDirty() => applyButton.Enabled = true;

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
        string entryName;
        try {
            entryName = Path.GetFullPath(textFilename.Text);
        } catch (ArgumentException) {
            entryName = "new template";
        }
        textFileMenu.Items[textFileIndex] = entryName;
    }

    protected override void OnClosed(EventArgs e) {
        renderTextTimer.Dispose();
        base.OnClosed(e);
    }

}