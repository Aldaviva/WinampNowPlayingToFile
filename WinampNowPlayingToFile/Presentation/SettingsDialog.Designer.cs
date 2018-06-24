namespace WinampNowPlayingToFile.Presentation
{
    partial class SettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.writeToFileLabel = new System.Windows.Forms.Label();
            this.nowPlayingFilenameEditor = new System.Windows.Forms.SaveFileDialog();
            this.writeToFileBrowseButton = new System.Windows.Forms.Button();
            this.writeToFileFilename = new System.Windows.Forms.TextBox();
            this.templateLabel = new System.Windows.Forms.Label();
            this.templateEditor = new System.Windows.Forms.TextBox();
            this.templateInsertButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.templatePreview = new System.Windows.Forms.TextBox();
            this.previewLabel = new System.Windows.Forms.Label();
            this.insertTemplatePlaceholderMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.albumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.artistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filenameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.titleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.yearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.horizontalRule1 = new System.Windows.Forms.Label();
            this.insertTemplatePlaceholderMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // writeToFileLabel
            // 
            this.writeToFileLabel.AutoSize = true;
            this.writeToFileLabel.Location = new System.Drawing.Point(13, 72);
            this.writeToFileLabel.Name = "writeToFileLabel";
            this.writeToFileLabel.Size = new System.Drawing.Size(47, 13);
            this.writeToFileLabel.TabIndex = 5;
            this.writeToFileLabel.Text = "Save as";
            // 
            // nowPlayingFilenameEditor
            // 
            this.nowPlayingFilenameEditor.DefaultExt = "txt";
            this.nowPlayingFilenameEditor.Filter = "Text files|*.txt";
            this.nowPlayingFilenameEditor.Title = "Choose file to save Now Playing text into";
            // 
            // writeToFileBrowseButton
            // 
            this.writeToFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.writeToFileBrowseButton.Location = new System.Drawing.Point(499, 67);
            this.writeToFileBrowseButton.Name = "writeToFileBrowseButton";
            this.writeToFileBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.writeToFileBrowseButton.TabIndex = 7;
            this.writeToFileBrowseButton.Text = "Browse…";
            this.writeToFileBrowseButton.UseVisualStyleBackColor = true;
            this.writeToFileBrowseButton.Click += new System.EventHandler(this.WriteToFileBrowseButtonClick);
            // 
            // writeToFileFilename
            // 
            this.writeToFileFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.writeToFileFilename.Location = new System.Drawing.Point(82, 69);
            this.writeToFileFilename.Name = "writeToFileFilename";
            this.writeToFileFilename.Size = new System.Drawing.Size(411, 20);
            this.writeToFileFilename.TabIndex = 6;
            // 
            // templateLabel
            // 
            this.templateLabel.AutoSize = true;
            this.templateLabel.Location = new System.Drawing.Point(13, 14);
            this.templateLabel.Name = "templateLabel";
            this.templateLabel.Size = new System.Drawing.Size(53, 13);
            this.templateLabel.TabIndex = 0;
            this.templateLabel.Text = "Template";
            // 
            // templateEditor
            // 
            this.templateEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.templateEditor.Location = new System.Drawing.Point(82, 11);
            this.templateEditor.Name = "templateEditor";
            this.templateEditor.Size = new System.Drawing.Size(411, 20);
            this.templateEditor.TabIndex = 1;
            this.templateEditor.TextChanged += new System.EventHandler(this.TemplateEditor_TextChanged);
            // 
            // templateInsertButton
            // 
            this.templateInsertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.templateInsertButton.Location = new System.Drawing.Point(499, 9);
            this.templateInsertButton.Name = "templateInsertButton";
            this.templateInsertButton.Size = new System.Drawing.Size(75, 23);
            this.templateInsertButton.TabIndex = 2;
            this.templateInsertButton.Text = "Insert";
            this.templateInsertButton.UseVisualStyleBackColor = true;
            this.templateInsertButton.Click += new System.EventHandler(this.TemplateInsertButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(418, 157);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 10;
            this.okButton.Text = "Save";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(499, 157);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 11;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // templatePreview
            // 
            this.templatePreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.templatePreview.Location = new System.Drawing.Point(82, 40);
            this.templatePreview.Name = "templatePreview";
            this.templatePreview.ReadOnly = true;
            this.templatePreview.Size = new System.Drawing.Size(492, 20);
            this.templatePreview.TabIndex = 4;
            // 
            // previewLabel
            // 
            this.previewLabel.AutoSize = true;
            this.previewLabel.Location = new System.Drawing.Point(13, 43);
            this.previewLabel.Name = "previewLabel";
            this.previewLabel.Size = new System.Drawing.Size(47, 13);
            this.previewLabel.TabIndex = 3;
            this.previewLabel.Text = "Preview";
            // 
            // insertTemplatePlaceholderMenu
            // 
            this.insertTemplatePlaceholderMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.albumToolStripMenuItem,
            this.artistToolStripMenuItem,
            this.filenameToolStripMenuItem,
            this.titleToolStripMenuItem,
            this.yearToolStripMenuItem,
            this.toolStripSeparator1,
            this.helpToolStripMenuItem});
            this.insertTemplatePlaceholderMenu.Name = "insertTemplatePlaceholderMenu";
            this.insertTemplatePlaceholderMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.insertTemplatePlaceholderMenu.ShowImageMargin = false;
            this.insertTemplatePlaceholderMenu.Size = new System.Drawing.Size(87, 142);
            this.insertTemplatePlaceholderMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.InsertTemplatePlaceholderMenu_ItemClicked);
            // 
            // albumToolStripMenuItem
            // 
            this.albumToolStripMenuItem.Name = "albumToolStripMenuItem";
            this.albumToolStripMenuItem.ShowShortcutKeys = false;
            this.albumToolStripMenuItem.Size = new System.Drawing.Size(86, 22);
            this.albumToolStripMenuItem.Text = "Album";
            // 
            // artistToolStripMenuItem
            // 
            this.artistToolStripMenuItem.Name = "artistToolStripMenuItem";
            this.artistToolStripMenuItem.ShowShortcutKeys = false;
            this.artistToolStripMenuItem.Size = new System.Drawing.Size(86, 22);
            this.artistToolStripMenuItem.Text = "Artist";
            // 
            // filenameToolStripMenuItem
            // 
            this.filenameToolStripMenuItem.Name = "filenameToolStripMenuItem";
            this.filenameToolStripMenuItem.ShowShortcutKeys = false;
            this.filenameToolStripMenuItem.Size = new System.Drawing.Size(86, 22);
            this.filenameToolStripMenuItem.Text = "Filename";
            // 
            // titleToolStripMenuItem
            // 
            this.titleToolStripMenuItem.Name = "titleToolStripMenuItem";
            this.titleToolStripMenuItem.ShowShortcutKeys = false;
            this.titleToolStripMenuItem.Size = new System.Drawing.Size(86, 22);
            this.titleToolStripMenuItem.Text = "Title";
            // 
            // yearToolStripMenuItem
            // 
            this.yearToolStripMenuItem.Name = "yearToolStripMenuItem";
            this.yearToolStripMenuItem.ShowShortcutKeys = false;
            this.yearToolStripMenuItem.Size = new System.Drawing.Size(86, 22);
            this.yearToolStripMenuItem.Text = "Year";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(83, 6);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(86, 22);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(22, 115);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(544, 30);
            this.label1.TabIndex = 9;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // horizontalRule1
            // 
            this.horizontalRule1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.horizontalRule1.Location = new System.Drawing.Point(17, 103);
            this.horizontalRule1.Name = "horizontalRule1";
            this.horizontalRule1.Size = new System.Drawing.Size(555, 2);
            this.horizontalRule1.TabIndex = 8;
            // 
            // SettingsDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(586, 192);
            this.Controls.Add(this.horizontalRule1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.templatePreview);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.templateInsertButton);
            this.Controls.Add(this.templateEditor);
            this.Controls.Add(this.previewLabel);
            this.Controls.Add(this.templateLabel);
            this.Controls.Add(this.writeToFileFilename);
            this.Controls.Add(this.writeToFileBrowseButton);
            this.Controls.Add(this.writeToFileLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsDialog";
            this.Text = "Now Playing to File plug-in configuration";
            this.Load += new System.EventHandler(this.SettingsDialog_Load);
            this.insertTemplatePlaceholderMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label writeToFileLabel;
        private System.Windows.Forms.SaveFileDialog nowPlayingFilenameEditor;
        private System.Windows.Forms.Button writeToFileBrowseButton;
        private System.Windows.Forms.TextBox writeToFileFilename;
        private System.Windows.Forms.Label templateLabel;
        private System.Windows.Forms.TextBox templateEditor;
        private System.Windows.Forms.Button templateInsertButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox templatePreview;
        private System.Windows.Forms.Label previewLabel;
        private System.Windows.Forms.ContextMenuStrip insertTemplatePlaceholderMenu;
        private System.Windows.Forms.ToolStripMenuItem albumToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem artistToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filenameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem titleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem yearToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label horizontalRule1;
    }
}