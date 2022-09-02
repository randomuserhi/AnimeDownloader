using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CefSharp;
using System.Runtime.InteropServices;

namespace AutoDownloader
{
    /*
     * Lots of complex shit needs to be done since controls cant be grabbed from video embed due to:
     * https://stackoverflow.com/questions/49141848/cefsharp-browser-video-wont-play
     * 
     * Instead utilize the fact that for mp4upload the embed links are the same as the video links:
     * https://www.mp4upload.com/embed-80yewjxsm8to.html?autostart=true
     * https://www.mp4upload.com/80yewjxsm8to
     * 
     * Also this code ONLY supports 9anime.id
     */

    public partial class Form : System.Windows.Forms.Form
    {
        AutoDownloader_9Animeid manager;

        Timer loop = new Timer();
        Task ongoing = null;

        public Form()
        {
            InitializeComponent();

            CurrentProgress.Minimum = 0;
            CurrentProgress.Maximum = 100;

            CurrentLabel.Text = string.Empty;

            manager = new AutoDownloader_9Animeid(this, fetcher, downloader);

            Type.Items.Add("Dubbed");
            Type.Items.Add("Subbed");
            Type.SelectedIndex = 0;

            loop.Interval = 100;
            loop.Tick += (object sender, EventArgs e) => {
                manager.CheckQueue();
            };

            browser.LifeSpanHandler = new PopupManager();
            browser.Load("https://9anime.id/");
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        private const int WM_VSCROLL = 277;
        private const int SB_PAGEBOTTOM = 7;
        internal static void ScrollToBottom(RichTextBox richTextBox)
        {
            SendMessage(richTextBox.Handle, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);
            richTextBox.SelectionStart = richTextBox.Text.Length;
        }

        private LinkedList<string> logs = new LinkedList<string>();
        private int currentLog = 0;
        public void Log(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Log), new object[] { value });
                return;
            }

            logs.AddLast(value);
            if (logs.Count > 150) logs.RemoveFirst();

            StringBuilder sb = new StringBuilder();
            foreach(string log in logs)
            {
                if (log != string.Empty) sb.AppendLine(log);
            }
            Debug.Text = sb.ToString();
            ScrollToBottom(Debug);
        }

        private void EpisodeControl(bool enabled)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<bool>(EpisodeControl), new object[] { enabled });
                return;
            }

            GetEpisodes.Enabled = enabled;
            AddEpisodes.Enabled = enabled;
            Episodes.Enabled = enabled;
            Type.Enabled = enabled;
            SelectAll.Enabled = enabled;
        }

        public void DownloadControl(bool enabled)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<bool>(DownloadControl), new object[] { enabled });
                return;
            }

            Cancel.Enabled = enabled;
        }

        public void SetEpisodes(AutoDownloader_9Animeid.Link[] links)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<AutoDownloader_9Animeid.Link[]>(SetEpisodes), new object[] { links });
                return;
            }

            Episodes.Items.Clear();
            for (int i = 0; i < links.Length; i++)
            {
                Episodes.Items.Add(links[i]);
            }
        }

        public void ClearProgress()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(ClearProgress), new object[] { });
                return;
            }

            CurrentProgress.Value = 0;
            CurrentLabel.Text = String.Empty;
        }

        public void SetProgress(AutoDownloader_9Animeid.DownloadProgress progress)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<AutoDownloader_9Animeid.DownloadProgress>(SetProgress), new object[] { progress });
                return;
            }

            CurrentProgress.Value = progress.percentage;
            CurrentLabel.Text = "bytes: " + progress.speed + "\n\nFilename: " + progress.fileName + "\n\nURL: " + progress.url + "\n\nOriginal file name: " + progress.id;
        }

        private void GetEpisodes_Click(object sender, EventArgs e)
        {
            manager.GetEpisodes(browser.Address, (AutoDownloader_9Animeid.Type)Type.SelectedIndex).ContinueWith(result =>
            {
                EpisodeControl(true);
                SetEpisodes(result.Result);
            });

            EpisodeControl(false);
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {
            Episodes.SelectedIndices.Clear();
            for (int i = 0; i < Episodes.Items.Count; i++)
            {
                Episodes.SelectedIndices.Add(i);
            }
        }

        private void AddEpisodes_Click(object sender, EventArgs e)
        {
            AutoDownloader_9Animeid.Link[] links = new AutoDownloader_9Animeid.Link[Episodes.SelectedIndices.Count];
            for (int i = 0; i < links.Length; i++)
            {
                links[i] = (AutoDownloader_9Animeid.Link)Episodes.Items[Episodes.SelectedIndices[i]];
            }
            for (int i = 0; i < links.Length; i++)
            {
                Episodes.Items.Remove(links[i]);
                Downloads.Items.Add(links[i]);
            }
            manager.AddEpisodes(links, (AutoDownloader_9Animeid.Type)Type.SelectedIndex);
        }

        private void RemoveEpisodes_Click(object sender, EventArgs e)
        {
            AutoDownloader_9Animeid.Link[] links = new AutoDownloader_9Animeid.Link[Downloads.SelectedIndices.Count];
            for (int i = 0; i < links.Length; i++)
            {
                links[i] = (AutoDownloader_9Animeid.Link)Downloads.Items[Downloads.SelectedIndices[i]];
            }
            for (int i = 0; i < links.Length; i++)
            {
                Downloads.Items.Remove(links[i]);
            }
            manager.RemoveEpisodes(links);
        }

        private int lastType = 0;
        private void Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lastType != Type.SelectedIndex)
            {
                lastType = Type.SelectedIndex;
                Episodes.Items.Clear();
            }
        }

        private void Download_Click(object sender, EventArgs e)
        {
            if (Download.Text == "Start Queue")
            {
                if (manager.savePath == string.Empty)
                {
                    if (MessageBox.Show("No Download path specified, are you sure you want to continue? Your episodes will save in the same directory as the application.", "No Download path", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        return;
                    }
                }

                Download.Text = "Pause Queue";

                loop.Start();

                Downloads.Enabled = false;
                Downloads.SelectedIndices.Clear();
            }
            else
            {
                Download.Text = "Start Queue";
                loop.Stop();

                Downloads.Enabled = true;
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            manager.Cancel();
        }

        private void BrowseSavePath_Click(object sender, EventArgs e)
        {
            var dlg = new FolderPicker();
            dlg.InputPath = @"c:\windows\system32";
            if (dlg.ShowDialog(IntPtr.Zero) == true)
            {
                SavePathLabel.Text = dlg.ResultPath;
                manager.savePath = SavePathLabel.Text;
            }
        }
    }
}
