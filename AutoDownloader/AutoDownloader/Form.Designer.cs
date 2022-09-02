﻿namespace AutoDownloader
{
    partial class Form
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
            this.browser = new CefSharp.WinForms.ChromiumWebBrowser();
            this.Debug = new System.Windows.Forms.RichTextBox();
            this.fetcher = new CefSharp.WinForms.ChromiumWebBrowser();
            this.downloader = new CefSharp.WinForms.ChromiumWebBrowser();
            this.GetEpisodes = new System.Windows.Forms.Button();
            this.Episodes = new System.Windows.Forms.ListBox();
            this.AddEpisodes = new System.Windows.Forms.Button();
            this.Type = new System.Windows.Forms.ComboBox();
            this.BrowseSavePath = new System.Windows.Forms.Button();
            this.Downloads = new System.Windows.Forms.ListBox();
            this.CurrentProgress = new System.Windows.Forms.ProgressBar();
            this.CurrentLabel = new System.Windows.Forms.Label();
            this.Cancel = new System.Windows.Forms.Button();
            this.RemoveEpisodes = new System.Windows.Forms.Button();
            this.SavePathLabel = new System.Windows.Forms.TextBox();
            this.SelectAll = new System.Windows.Forms.Button();
            this.Download = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // browser
            // 
            this.browser.ActivateBrowserOnCreation = false;
            this.browser.Location = new System.Drawing.Point(12, 12);
            this.browser.Name = "browser";
            this.browser.Size = new System.Drawing.Size(583, 657);
            this.browser.TabIndex = 0;
            // 
            // Debug
            // 
            this.Debug.Location = new System.Drawing.Point(1020, 516);
            this.Debug.Name = "Debug";
            this.Debug.ReadOnly = true;
            this.Debug.Size = new System.Drawing.Size(450, 153);
            this.Debug.TabIndex = 2;
            this.Debug.Text = "";
            // 
            // fetcher
            // 
            this.fetcher.ActivateBrowserOnCreation = false;
            this.fetcher.Enabled = false;
            this.fetcher.Location = new System.Drawing.Point(1020, 388);
            this.fetcher.Name = "fetcher";
            this.fetcher.Size = new System.Drawing.Size(231, 122);
            this.fetcher.TabIndex = 3;
            // 
            // downloader
            // 
            this.downloader.ActivateBrowserOnCreation = false;
            this.downloader.Enabled = false;
            this.downloader.Location = new System.Drawing.Point(1257, 388);
            this.downloader.Name = "downloader";
            this.downloader.Size = new System.Drawing.Size(213, 122);
            this.downloader.TabIndex = 4;
            // 
            // GetEpisodes
            // 
            this.GetEpisodes.Location = new System.Drawing.Point(601, 12);
            this.GetEpisodes.Name = "GetEpisodes";
            this.GetEpisodes.Size = new System.Drawing.Size(179, 36);
            this.GetEpisodes.TabIndex = 5;
            this.GetEpisodes.Text = "Get Episodes";
            this.GetEpisodes.UseVisualStyleBackColor = true;
            this.GetEpisodes.Click += new System.EventHandler(this.GetEpisodes_Click);
            // 
            // Episodes
            // 
            this.Episodes.FormattingEnabled = true;
            this.Episodes.Location = new System.Drawing.Point(601, 81);
            this.Episodes.Name = "Episodes";
            this.Episodes.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.Episodes.Size = new System.Drawing.Size(179, 459);
            this.Episodes.TabIndex = 6;
            // 
            // AddEpisodes
            // 
            this.AddEpisodes.Location = new System.Drawing.Point(601, 591);
            this.AddEpisodes.Name = "AddEpisodes";
            this.AddEpisodes.Size = new System.Drawing.Size(179, 36);
            this.AddEpisodes.TabIndex = 7;
            this.AddEpisodes.Text = "Add Selected";
            this.AddEpisodes.UseVisualStyleBackColor = true;
            this.AddEpisodes.Click += new System.EventHandler(this.AddEpisodes_Click);
            // 
            // Type
            // 
            this.Type.FormattingEnabled = true;
            this.Type.Location = new System.Drawing.Point(601, 54);
            this.Type.Name = "Type";
            this.Type.Size = new System.Drawing.Size(179, 21);
            this.Type.TabIndex = 8;
            this.Type.Text = "Dubbed";
            this.Type.SelectedIndexChanged += new System.EventHandler(this.Type_SelectedIndexChanged);
            // 
            // BrowseSavePath
            // 
            this.BrowseSavePath.Location = new System.Drawing.Point(882, 18);
            this.BrowseSavePath.Name = "BrowseSavePath";
            this.BrowseSavePath.Size = new System.Drawing.Size(75, 25);
            this.BrowseSavePath.TabIndex = 10;
            this.BrowseSavePath.Text = "Browse";
            this.BrowseSavePath.UseVisualStyleBackColor = true;
            this.BrowseSavePath.Click += new System.EventHandler(this.BrowseSavePath_Click);
            // 
            // Downloads
            // 
            this.Downloads.FormattingEnabled = true;
            this.Downloads.Location = new System.Drawing.Point(786, 54);
            this.Downloads.Name = "Downloads";
            this.Downloads.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.Downloads.Size = new System.Drawing.Size(228, 615);
            this.Downloads.TabIndex = 11;
            // 
            // CurrentProgress
            // 
            this.CurrentProgress.Location = new System.Drawing.Point(1020, 83);
            this.CurrentProgress.Name = "CurrentProgress";
            this.CurrentProgress.Size = new System.Drawing.Size(450, 23);
            this.CurrentProgress.TabIndex = 12;
            // 
            // CurrentLabel
            // 
            this.CurrentLabel.AutoSize = true;
            this.CurrentLabel.Location = new System.Drawing.Point(1020, 138);
            this.CurrentLabel.MaximumSize = new System.Drawing.Size(450, 0);
            this.CurrentLabel.Name = "CurrentLabel";
            this.CurrentLabel.Size = new System.Drawing.Size(420, 39);
            this.CurrentLabel.TabIndex = 13;
            this.CurrentLabel.Text = "Placeholder Placeholder Placeholder Placeholder Placeholder Placeholder Placehold" +
    "er Placeholder Placeholder Placeholder Placeholder Placeholder Placeholder Place" +
    "holder Placeholder";
            // 
            // Cancel
            // 
            this.Cancel.Enabled = false;
            this.Cancel.Location = new System.Drawing.Point(1020, 112);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(81, 23);
            this.Cancel.TabIndex = 14;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // RemoveEpisodes
            // 
            this.RemoveEpisodes.Location = new System.Drawing.Point(601, 633);
            this.RemoveEpisodes.Name = "RemoveEpisodes";
            this.RemoveEpisodes.Size = new System.Drawing.Size(179, 36);
            this.RemoveEpisodes.TabIndex = 15;
            this.RemoveEpisodes.Text = "Remove Selected";
            this.RemoveEpisodes.UseVisualStyleBackColor = true;
            this.RemoveEpisodes.Click += new System.EventHandler(this.RemoveEpisodes_Click);
            // 
            // SavePathLabel
            // 
            this.SavePathLabel.Enabled = false;
            this.SavePathLabel.Location = new System.Drawing.Point(963, 21);
            this.SavePathLabel.Name = "SavePathLabel";
            this.SavePathLabel.Size = new System.Drawing.Size(507, 20);
            this.SavePathLabel.TabIndex = 16;
            // 
            // SelectAll
            // 
            this.SelectAll.Location = new System.Drawing.Point(601, 549);
            this.SelectAll.Name = "SelectAll";
            this.SelectAll.Size = new System.Drawing.Size(179, 36);
            this.SelectAll.TabIndex = 17;
            this.SelectAll.Text = "Select All";
            this.SelectAll.UseVisualStyleBackColor = true;
            this.SelectAll.Click += new System.EventHandler(this.SelectAll_Click);
            // 
            // Download
            // 
            this.Download.Location = new System.Drawing.Point(1020, 54);
            this.Download.Name = "Download";
            this.Download.Size = new System.Drawing.Size(81, 23);
            this.Download.TabIndex = 18;
            this.Download.Text = "Start Queue";
            this.Download.UseVisualStyleBackColor = true;
            this.Download.Click += new System.EventHandler(this.Download_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(786, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Download Folder:";
            // 
            // Form
            // 
            this.ClientSize = new System.Drawing.Size(1482, 681);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Download);
            this.Controls.Add(this.SelectAll);
            this.Controls.Add(this.SavePathLabel);
            this.Controls.Add(this.RemoveEpisodes);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.CurrentLabel);
            this.Controls.Add(this.CurrentProgress);
            this.Controls.Add(this.Downloads);
            this.Controls.Add(this.BrowseSavePath);
            this.Controls.Add(this.Type);
            this.Controls.Add(this.AddEpisodes);
            this.Controls.Add(this.Episodes);
            this.Controls.Add(this.GetEpisodes);
            this.Controls.Add(this.downloader);
            this.Controls.Add(this.fetcher);
            this.Controls.Add(this.Debug);
            this.Controls.Add(this.browser);
            this.Name = "Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CefSharp.WinForms.ChromiumWebBrowser browser;
        private System.Windows.Forms.RichTextBox Debug;
        private CefSharp.WinForms.ChromiumWebBrowser fetcher;
        private CefSharp.WinForms.ChromiumWebBrowser downloader;
        private System.Windows.Forms.Button GetEpisodes;
        private System.Windows.Forms.ListBox Episodes;
        private System.Windows.Forms.Button AddEpisodes;
        private System.Windows.Forms.ComboBox Type;
        private System.Windows.Forms.Button BrowseSavePath;
        public System.Windows.Forms.ListBox Downloads;
        private System.Windows.Forms.ProgressBar CurrentProgress;
        private System.Windows.Forms.Label CurrentLabel;
        public System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button RemoveEpisodes;
        private System.Windows.Forms.TextBox SavePathLabel;
        private System.Windows.Forms.Button SelectAll;
        private System.Windows.Forms.Button Download;
        private System.Windows.Forms.Label label1;
    }
}
