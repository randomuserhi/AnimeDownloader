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
        }

        private bool GetUpdate(UpdateData data)
        {
            try
            {
                if (!Directory.Exists("update")) Directory.CreateDirectory("update");

                using (WebClient wc = new WebClient())
                {
                    Log("[Update] Grabbing update from: " + data.url);
                    wc.DownloadFile(data.url, @"update\update.temp");
                    Log("[Update] Extracting update...");
                    using (ZipFile archive = new ZipFile(Path.Combine(System.Windows.Forms.Application.StartupPath, @"update\update.temp")))
                    {
                        archive.ExtractAll(Path.Combine(System.Windows.Forms.Application.StartupPath, @"update"), ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                File.Delete(@"update.temp");
                Log("[Update] Finished and restarting...");
                return true;
            }
            catch (Exception err)
            {
                Log("Failed to download update: " + err.Message);
                return false;
            }
        }

        private void DownloadUpdateEnable(bool enabled)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<bool>(DownloadUpdateEnable), new object[] { enabled });
                return;
            }

            DownloadUpdate.Enabled = enabled;
            Continue.Enabled = enabled;
            DontShow.Enabled = enabled;
            DownloadUpdate.Text = enabled ? "Update now" : "Downloading, do not close...";
        }

        Task<bool> updateTask;
        private void DownloadUpdate_Click(object sender, EventArgs e)
        {
            if (updateTask == null)
            {
                updateTask = Task<bool>.Run(() => GetUpdate(data));
                updateTask.ContinueWith((result) =>
                {
                    if (result.Result)
                    {
                        ProcessStartInfo updater = new ProcessStartInfo("updater.exe");
                        updater.WindowStyle = ProcessWindowStyle.Normal;
                        updater.Arguments = System.Windows.Forms.Application.StartupPath;
                        Process.Start(updater);
                        System.Windows.Forms.Application.Exit();
                    }
                    else DownloadUpdateEnable(true);
                });
            }
            DownloadUpdateEnable(false);
        }

        private void UpdateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (updateTask != null && !updateTask.IsCompleted)
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
