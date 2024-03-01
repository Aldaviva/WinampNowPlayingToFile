﻿#nullable enable

using Mustache;
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
using WinampNowPlayingToFile.Settings;
using Timer = System.Timers.Timer;

namespace WinampNowPlayingToFile.Presentation;

public partial class SettingsDialog: Form {

    private readonly ISettings            settings;
    private readonly WinampControllerImpl winampController;
    private readonly Timer                renderTextTimer = new() { Enabled = true, Interval = 1000 };

    private static readonly FormatCompiler TEMPLATE_COMPILER = new();

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
        // Since the templateSelector holds all textTemplates let's not fire textBox events since those update the templateSelector too
        templateEditor.TextChanged -= onTemplateChange; 
        textFilename.TextChanged -= onFilenameChange; 
        albumArtFilename.TextChanged -= onFilenameChange;

		albumArtFilenameEditor.InitialDirectory = Path.GetDirectoryName(settings.albumArtFilename);
		albumArtFilenameEditor.FileName = settings.albumArtFilename;
		albumArtFilename.Text = settings.albumArtFilename;

		foreach (textTemplate item in settings.textTemplates) {
            templateSelector.Items.Add(item);
		}
        templateSelector.SelectedIndex = 0;

		templateEditor.TextChanged += onTemplateChange;
		textFilename.TextChanged += onFilenameChange;
		albumArtFilename.TextChanged += onFilenameChange;

		winampController.songChanged += delegate { renderPreview(templatePreview); };
        renderTextTimer.Elapsed      += delegate { renderPreview(templatePreview); };

        applyButton.Enabled = false;
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
        renderPreview(sender);
        templateSelector.Items[templateSelector.SelectedIndex] = new textTemplate(textFilename.Text, templateEditor.Text);
		onFormDirty();
    }

    private void renderPreview(object sender) {
        Song previewSong = isSongPlaying() ? winampController.currentSong : EXAMPLE_SONG;

        try {
			templatePreview.Text = compileTemplate().Render(previewSong);
        } catch (KeyNotFoundException e) {
			templatePreview.Text = $"Template key not found: {e.Message}";
        } catch (FormatException e) {
			templatePreview.Text = $"Template format error: {e.Message}";
        }
    }

    private bool isSongPlaying() => !string.IsNullOrEmpty(winampController.currentSong.Title);

    private Generator compileTemplate() {
        Generator generator = TEMPLATE_COMPILER.Compile(templateEditor.Text);
        generator.KeyNotFound += (_, args) => {
            args.Substitute = isSongPlaying()
                ? winampController.fetchMetadataFieldValue(args.Key)
                : EXAMPLE_SONG_EXTRA_METADATA.TryGetValue(args.Key.ToLowerInvariant(), out object? value)
                    ? value
                    : string.Empty;

            args.Handled = true;
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

			templateEditor.Text             = newTemplate.ToString();
			templateEditor.SelectionLength = 0;
			templateEditor.SelectionStart  = selectionStart + placeholder.Length;
			templateEditor.Focus();
        }
    }

    private void onClickOk(object sender, EventArgs args) {
        try {
            save();
            Close();
        } catch (Exception e) when (e is FormatException or KeyNotFoundException) {
            //leave form open, with invalid inputs unsaved
        }
    }

    private void onClickApply(object sender, EventArgs args) {
        try {
            save();
        } catch (Exception e) when (e is FormatException or KeyNotFoundException) {
            //leave form open, with invalid inputs unsaved
        }
    }

    private void save() {
        try {
            compileTemplate().Render(EXAMPLE_SONG);

            // Actually the templateSelector will hold all of the templates now
            settings.textTemplates = templateSelector.Items.Cast<textTemplate>().ToList();

            settings.albumArtFilename      = albumArtFilename.Text;
            settings.save();

            applyButton.Enabled = false;
        } catch (FormatException e) {
            MessageBox.Show($"Invalid template:\n\n{e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw;
        }
    }

    private void onFormDirty() {
        applyButton.Enabled = true;
    }

    private void onSubmitFilename(object sender, CancelEventArgs e) {
        onFormDirty();
    }

    private void onFilenameChange(object sender, EventArgs e) {
		templateSelector.Items[templateSelector.SelectedIndex] = new textTemplate(textFilename.Text, templateEditor.Text);
		onFormDirty();
    }

    protected override void OnClosed(EventArgs e) {
        renderTextTimer.Dispose();
        base.OnClosed(e);
    }

	private void onTemplateSelectorChanged(object sender, EventArgs e) {
        bool applyButtonState = applyButton.Enabled;

        textTemplate selectedTemplate = (textTemplate)templateSelector.SelectedItem;
		textFilenameEditor.InitialDirectory = Path.GetDirectoryName(selectedTemplate.fileName);
		textFilenameEditor.FileName = selectedTemplate.fileName;

		textFilename.Text = selectedTemplate.fileName;

		templateEditor.Text = selectedTemplate.text;

        applyButton.Enabled = applyButtonState; // Restore dirty state
	}

	private void onTemplateAdd(object sender, EventArgs e) {
        textTemplate newTemplate = settings.getDefault(); // Add an index to the new template's file name
		FileInfo newTemplatePath = new FileInfo(newTemplate.fileName);
		string newTemplateFileName = Path.Combine(
            newTemplatePath.DirectoryName, 
            $"{Path.GetFileNameWithoutExtension(newTemplatePath.FullName)}{(settings.textTemplates.Count+1)}{newTemplatePath.Extension}"
        );
        newTemplate.fileName = newTemplateFileName;
		settings.textTemplates.Add(newTemplate);

        templateSelector.Items.Add(settings.textTemplates.Last());
        templateSelector.SelectedIndex = templateSelector.Items.Count - 1;
        onFormDirty();
	}

	private void onTemmplateRemove(object sender, EventArgs e) {
        settings.textTemplates.Remove((textTemplate)templateSelector.SelectedItem);
        templateSelector.Items.Remove(templateSelector.SelectedItem);
		templateSelector.SelectedIndex = templateSelector.Items.Count - 1;
		onFormDirty();
	}
}