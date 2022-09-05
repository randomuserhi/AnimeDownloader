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
using System.Runtime.InteropServices;
using System.Reflection;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using Ionic.Zip;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AutoDownloader
{
    public partial class UpdateForm : System.Windows.Forms.Form
    {
        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        Form form;
        string hidden;
        UpdateData data;

        public UpdateForm(Form form, string hidden, UpdateData data, bool changeLog = false)
        {
            this.data = data;
            this.hidden = hidden;
            this.form = form;
            InitializeComponent();

            ChangeLog.Text = data.changeLog;
            SetForegroundWindow(Handle.ToInt32());

            if (changeLog)
            {
                DownloadUpdate.Visible = false;
                DownloadBar.Visible = false;
                Text = "Change Log";
            }
        }

        private LinkedList<string> logs = new LinkedList<string>();
        public void Log(string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Log), new object[] { text });
                return;
            }

            if (form.logStream != null) form.logStream.WriteLine(text);
            logs.AddLast(text);
            if (logs.Count > 150) logs.RemoveFirst();

            StringBuilder sb = new StringBuilder();
            foreach (string log in logs)
            {
                if (log != string.Empty) sb.AppendLine(log);
            }
            ChangeLog.Text = sb.ToString();
            Form.ScrollToBottom(ChangeLog);

            DownloadBar.Maximum = 100;
        }

        private void SetProgress(int progress)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int>(SetProgress), new object[] { progress });
                return;
            }

            DownloadBar.Value = progress;
        }

        private void GetUpdate(UpdateData data)
        {
            if (!Directory.Exists("update")) Directory.CreateDirectory("update");

            try
            {
                using (WebClient wc = new WebClient())
                {
                    Log("[Update] Grabbing update from: " + data.url);
                    wc.DownloadProgressChanged += (s, e) =>
                    {
                        SetProgress(e.ProgressPercentage);
                    };
                    wc.DownloadFileAsync(new Uri(data.url), @"update\update.temp");
                    wc.DownloadFileCompleted += (s, e) =>
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                string path = Path.Combine(System.Windows.Forms.Application.StartupPath, @"update\update.temp");
                                Log("[Update] Extracting update...");
                                int attempts = 0;
                                for (; attempts < 10; attempts++)
                                {
                                    try
                                    {
                                        using (ZipFile archive = new ZipFile(path))
                                        {
                                            archive.ExtractAll(Path.Combine(System.Windows.Forms.Application.StartupPath, @"update"), ExtractExistingFileAction.OverwriteSilently);
                                        }

                                        break;
                                    }
                                    catch (Exception err)
                                    {
                                        Log("[Update] Failed to extract, trying again...");
                                        Log("[Update : WARNING] " + err);
                                        Task.Delay(1000).Wait();
                                    }
                                }
                                if (attempts == 10) throw new Exception("Failed to extract file after 10 attempts.");

                                System.IO.File.Delete(path);
                                Log("[Update] Finished and restarting...");
                            }
                            catch (Exception err)
                            {
                                Log("[Update] Failed to extract update.");
                                Log("[Update : FATAL ERROR] " + err.Message);
                                DownloadUpdateEnable(true);
                            }

                            try
                            {
                                ProcessStartInfo updater = new ProcessStartInfo(Path.Combine(System.Windows.Forms.Application.StartupPath, "Updater.exe"));
                                updater.WindowStyle = ProcessWindowStyle.Normal;
                                updater.Arguments = AddQuotesIfRequired(System.Windows.Forms.Application.StartupPath);
                                Process.Start(updater);
                                KillApplication();
                            }
                            catch (Exception err)
                            {
                                Log("[Update] Failed to restart program.");
                                Log("[Update : FATAL ERROR] " + err.Message);
                                DownloadUpdateEnable(true);
                            }
                        });
                    };
                }
            }
            catch (Exception err)
            {
                Log("[Update] Failed to grab update.");
                Log("[Update : FATAL ERROR] " + err.Message);
                DownloadUpdateEnable(true);
            }
        }

        // https://stackoverflow.com/questions/6521546/how-to-handle-spaces-in-file-path-if-the-folder-contains-the-space
        private string AddQuotesIfRequired(string path)
        {
            return !string.IsNullOrWhiteSpace(path) ?
                path.Contains(" ") && (!path.StartsWith("\"") && !path.EndsWith("\"")) ?
                    "\"" + path + "\"" : path :
                    string.Empty;
        }

        private void KillApplication()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(KillApplication), new object[] { });
                return;
            }

            updating = false;
            System.Windows.Forms.Application.Exit();
        }

        private void DownloadUpdateEnable(bool enabled)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<bool>(DownloadUpdateEnable), new object[] { enabled });
                return;
            }

            updating = !enabled;
            DownloadUpdate.Enabled = enabled;
            Continue.Enabled = enabled;
            DontShow.Enabled = enabled;
            DownloadUpdate.Text = enabled ? "Update now" : "Downloading, do not close...";
        }

        bool updating = false;
        private void DownloadUpdate_Click(object sender, EventArgs e)
        {
            if (!updating)
            {
                updating = true;
                GetUpdate(data);
            }
            DownloadUpdateEnable(false);
        }

        private void UpdateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (updating)
            {
                MessageBox.Show("An update is installing, do not close.");
                e.Cancel = true;
            }

            form.manager.settings.showUpdates = !DontShow.Checked;
            if (DontShow.Checked) form.manager.settings.hidden = hidden;
        }

        private void Continue_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
