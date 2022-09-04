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
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Net;
using System.Security.Policy;
using System.Threading;

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

    //TODO:: Add a way for the program to know what episodes have already been installed on the system (create a json / dump file containing the information)

    public partial class Form : System.Windows.Forms.Form
    {
        AutoDownloader_9Animeid manager;
        private Version version = new Version("1.2.6");

        public class ScrollingText
        {
            public string Text {
                set
                {
                    rollingText = new StringBuilder(value);
                    container.Text = rollingText.ToString();
                    for (int i = 0, j = rollingText.Length; j < 40 || i < 15; i++, j++) rollingText.Append(" ");
                    scrollingText.Interval = 1000;
                }
            }
            StringBuilder rollingText = new StringBuilder();
            System.Windows.Forms.Timer scrollingText = new System.Windows.Forms.Timer();

            TextBox container;

            public ScrollingText(TextBox container)
            {
                this.container = container;
                container.Font = new Font(FontFamily.GenericMonospace, container.Font.Size); ;

                scrollingText.Interval = 100;
                scrollingText.Tick += (object sender, EventArgs e) =>
                {
                    scrollingText.Interval = 100;
                    string temp = rollingText.ToString();
                    if (temp.Length == 0) return;
                    container.Text = temp;
                    for (int i = 0; i < rollingText.Length - 1; i++)
                        rollingText[i] = temp[i + 1];
                    rollingText[temp.Length - 1] = temp[0];
                };
                scrollingText.Start();
            }
        }

        ScrollingText currentAnimeScroll;
        ScrollingText selectionScroll;

        System.Windows.Forms.Timer loop = new System.Windows.Forms.Timer();

        StreamWriter logStream;

        public int subbedDubbed = 0;
        public bool checkUpdates = true;
        public Form()
        {
            InitializeComponent();

            Text = "Auto Downloader " + version;
            File.WriteAllText("mainLog.txt", Text + "\n");
            logStream = File.AppendText("mainLog.txt");

            CurrentProgress.Minimum = 0;
            CurrentProgress.Maximum = 100;

            CurrentLabel.Text = string.Empty;
            currentAnimeLabel.Text = string.Empty;

            manager = new AutoDownloader_9Animeid(this, fetcher, downloader);

            currentAnimeScroll = new ScrollingText(currentAnimeLabel);
            selectionScroll = new ScrollingText(CurrentSelection);

            loop.Interval = 100;
            loop.Tick += (object sender, EventArgs e) => {
                manager.CheckQueue();
            };

            browser.LifeSpanHandler = new PopupManager();
            browser.Load("https://9anime.id/");

            CheckForUpdates();
        }

        private Version GetVersion(string v)
        {
            return new Version(v.Split(new[] { "Auto Downloader " }, StringSplitOptions.RemoveEmptyEntries)[0]);
        }
        public void CheckForUpdates()
        {
            Log("[Manager] Checking for updates...");
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("a", "a");
                try
                {
                    wc.DownloadFile("https://raw.githubusercontent.com/randomuserhi/AnimeDownloader/main/updateInfo.update", @"updateInfo.temp");
                    string[] updateInfo_temp = File.ReadAllLines("updateInfo.temp");
                    if (!File.Exists("updateInfo.update"))
                    {
                        Log("[Manager] Unable to find update info, grabbing from web...");
                        FileInfo fileInfo = new FileInfo(@"updateInfo.temp");
                        fileInfo.CopyTo(Path.Combine(fileInfo.Directory.FullName, @"updateInfo.update"));
                    }

                    if (File.Exists("updateInfo.temp"))
                    {
                        string[] updateInfo_update = File.ReadAllLines("updateInfo.update");
                        Version latest = GetVersion(updateInfo_temp[0]);
                        Version previous = GetVersion(updateInfo_update[0]);
                        if (version < latest && latest != previous)
                        {

                            Log("[Manager] An update is available!");
                            UpdateForm updateForm = new UpdateForm(this, updateInfo_temp);
                            updateForm.ShowDialog();
                        }
                        else if (version < previous)
                        {
                            Log("[Manager] An update is available!");
                            if (!checkUpdates) return;
                            UpdateForm updateForm = new UpdateForm(this, updateInfo_temp);
                            updateForm.ShowDialog();
                        }
                        else if (version == latest)
                        {
                            Log("[Manager] No updates found.");
                            if (!checkUpdates) return;
                            UpdateForm updateForm = new UpdateForm(this, updateInfo_temp, false);
                            updateForm.ShowDialog();
                        }
                        else
                            Log("[Manager] No updates found.");
                    }
                    else
                        Log("[Manager] Unable to find updateInfo.temp .");
                }
                catch (Exception err)
                {
                    Log("[Manager] Failed to check for updates.");
                    Log("[Manager : WARNING] " + err.Message);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            logStream.Close();
            logStream.Dispose();
            logStream = null;
            manager.SaveAll();
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
        public void Log(string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Log), new object[] { text });
                return;
            }

            if (logStream != null) logStream.WriteLine(text);
            logs.AddLast(text);
            if (logs.Count > 150) logs.RemoveFirst();

            StringBuilder sb = new StringBuilder();
            foreach(string log in logs)
            {
                if (log != string.Empty) sb.AppendLine(log);
            }
            Debug.Text = sb.ToString();
            ScrollToBottom(Debug);
        }

        private bool episodeControl = true;
        private void EpisodeControl(bool enabled, bool set = true)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<bool, bool>(EpisodeControl), new object[] { enabled, set });
                return;
            }

            if (set) episodeControl = enabled;
            currentAnimeLabel.Text = currentAnime;

            AddEpisodes.Enabled = enabled;
            Episodes.Enabled = enabled;
            if (enabled)
            {
                currentAnimeScroll.Text = currentAnime;
                SubbedButton.Enabled = subbedDubbed == 0;
                DubbedButton.Enabled = subbedDubbed == 1;
            }
            else
            {
                SubbedButton.Enabled = enabled;
                DubbedButton.Enabled = enabled;
            }
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
            try
            {
                if (InvokeRequired)
                {
                    this.Invoke(new Action<AutoDownloader_9Animeid.DownloadProgress>(SetProgress), new object[] { progress });
                    return;
                }

                CurrentProgress.Value = progress.percentage;
                CurrentLabel.Text = progress.percentage + " %\ncompleted: " + progress.completed + "\nbytes: " + progress.speed + "\n\nFilename: " + progress.fileName + "\n\nLocation: " + progress.filePath + "\n\nURL: " + progress.url + "\n\nOriginal file name: " + progress.id;
            }
            catch (Exception err)
            {
                Log("[Manager] Failed to set progress...");
                Log("[Manager : WARNING] " + err.Message);
            }
        }

        public void EnableGetEpisodes()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(EnableGetEpisodes), new object[] { });
                return;
            }

            GetEpisodes.Text = "Get Episodes";
            GetEpisodes.Enabled = true;
        }

        CancellationTokenSource getEpisodesCT;
        private void GetEpisodes_Click(object sender, EventArgs e)
        {
            if (getEpisodesCT == null)
            {
                getEpisodesCT = new CancellationTokenSource();
                manager.GetEpisodes(getEpisodesCT.Token, browser.Address, (AutoDownloader_9Animeid.Type)subbedDubbed).ContinueWith(result =>
                {
                    if (result.Result != null)
                    {
                        currentAnime = manager.currentAnime;
                        SetEpisodes(result.Result);
                    }

                    EpisodeControl(true);
                    EnableGetEpisodes();
                    getEpisodesCT.Dispose();
                    getEpisodesCT = null;
                });

                EpisodeControl(false);

                GetEpisodes.Text = "Cancel Get Episodes";
            }
            else
            {
                GetEpisodes.Enabled = false;
                GetEpisodes.Text = "Get Episodes";

                getEpisodesCT.Cancel();
            }
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {
            Episodes.SelectedIndices.Clear();
            for (int i = 0; i < Episodes.Items.Count; i++)
            {
                Episodes.SelectedIndices.Add(i);
            }
        }

        public void RestoreEpisodesFromListings(List<AutoDownloader_9Animeid.Link> link)
        {
            if (manager == null) return; //Prevents first boot up call from breaking

            Log("[Manager] Restoring and removing episodes from listings...");

            bool prevState = Downloads.Enabled;
            Downloads.Enabled = false;
            EpisodeControl(false, false);

            Downloads.Items.Clear();
            LinkedList<AutoDownloader_9Animeid.Link> old = new LinkedList<AutoDownloader_9Animeid.Link>(manager.queue);
            manager.queue.Clear();
            foreach (AutoDownloader_9Animeid.Link item in old)
            {
                if (!link.Any(l => l == item))
                {
                    Downloads.Items.Add(item);
                    manager.queue.AddLast(item);

                    switch (item.type)
                    {
                        case AutoDownloader_9Animeid.Type.subbed:
                            manager.subbed.Add(item.episodeUrl, item);
                            break;
                        case AutoDownloader_9Animeid.Type.dubbed:
                            manager.dubbed.Add(item.episodeUrl, item);
                            break;
                    }
                }
                else if (currentAnime == item.anime)
                    RestoreEpisode(item);
            }

            Episodes.Items.Clear();
            for (int i = 0; i < manager.allLinks.Length; i++)
            {
                if (!link.Any(l => l == manager.allLinks[i]) && 
                    !manager.subbed.ContainsKey(manager.allLinks[i].episodeUrl) && 
                    !manager.dubbed.ContainsKey(manager.allLinks[i].episodeUrl))
                    Episodes.Items.Add(manager.allLinks[i]);
            }

            Downloads.Enabled = prevState;
            EpisodeControl(episodeControl);
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
            manager.AddEpisodes(links, (AutoDownloader_9Animeid.Type)subbedDubbed);
        }

        public void RestoreEpisode(AutoDownloader_9Animeid.Link link)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<AutoDownloader_9Animeid.Link>(RestoreEpisode), new object[] { link });
                return;
            }

            if (link.anime == currentAnime)
            {
                if (Episodes.Items.Count == 0) Episodes.Items.Add(link);
                else for (int j = 0; j < Episodes.Items.Count + 1; j++)
                    {
                        if (j < Episodes.Items.Count)
                        {
                            if (((AutoDownloader_9Animeid.Link)Episodes.Items[j]).index > link.index)
                            {
                                Episodes.Items.Insert(j, link);
                                break;
                            }
                        }
                        else
                        {
                            Episodes.Items.Add(link);
                            break;
                        }
                    }
            }
        }

        private string currentAnime = string.Empty;
        private void RemoveEpisodes_Click(object sender, EventArgs e)
        {
            AutoDownloader_9Animeid.Link[] links = new AutoDownloader_9Animeid.Link[Downloads.SelectedIndices.Count];
            for (int i = 0; i < links.Length; i++) links[i] = (AutoDownloader_9Animeid.Link)Downloads.Items[Downloads.SelectedIndices[i]];
            for (int i = 0; i < links.Length; i++)
            {
                Downloads.Items.Remove(links[i]);
                RestoreEpisode(links[i]);
            }

            manager.RemoveEpisodes(links);
        }

        private int lastType = 0;
        private void Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lastType != subbedDubbed)
            {
                lastType = subbedDubbed;
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
            dlg.InputPath = manager.savePath == String.Empty ? Application.StartupPath : manager.savePath;
            if (dlg.ShowDialog(IntPtr.Zero) == true)
            {
                SavePathLabel.Text = dlg.ResultPath;
                manager.savePath = dlg.ResultPath;
                manager.CheckAnimes();
            }

            manager.SaveSettings();
        }

        private void SubbedButton_Click(object sender, EventArgs e)
        {
            SubbedButton.Enabled = false;
            DubbedButton.Enabled = true;
            subbedDubbed = 1;
        }

        private void DubbedButton_Click(object sender, EventArgs e)
        {
            SubbedButton.Enabled = true;
            DubbedButton.Enabled = false;
            subbedDubbed = 0;
        }

        private void Episodes_SelectedValueChanged(object sender, EventArgs e)
        {
            if (Episodes.SelectedItems.Count == 0)
            {
                selectionScroll.Text = string.Empty;
                return;
            }
            selectionScroll.Text = ((AutoDownloader_9Animeid.Link)Episodes.SelectedItems[Episodes.SelectedItems.Count - 1]).ToString();
        }

        private void Downloads_SelectedValueChanged(object sender, EventArgs e)
        {
            if (Downloads.SelectedItems.Count == 0)
            {
                selectionScroll.Text = string.Empty;
                return;
            }
            selectionScroll.Text = ((AutoDownloader_9Animeid.Link)Downloads.SelectedItems[Downloads.SelectedItems.Count - 1]).ToString();
        }
    }
}
