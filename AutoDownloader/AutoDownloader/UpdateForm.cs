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

namespace AutoDownloader
{
    public partial class UpdateForm : System.Windows.Forms.Form
    {
        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        Form form;

        public UpdateForm(Form form, bool realUpdate = true)
        {
            this.form = form;
            InitializeComponent();

            FileInfo fileInfo = new FileInfo(@"updateInfo-web.temp");
            fileInfo.CopyTo(Path.Combine(fileInfo.Directory.FullName, @"updateInfo.temp"), true);

            ChangeLog.Text = File.ReadAllText(@"updateInfo-web.temp");
            SetForegroundWindow(Handle.ToInt32());

            if (!realUpdate)
            {
                DownloadUpdate.Visible = false;
                DontShow.Visible = false;
                Text = "Change Log";
            }
        }

        private void DownloadUpdate_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/randomuserhi/AnimeDownloader/releases");
        }

        private void UpdateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            form.checkUpdates = !DontShow.Checked;
        }

        private void Continue_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
