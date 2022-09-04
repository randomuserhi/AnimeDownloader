using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoDownloader
{
    public partial class UpdateForm : Form
    {
        public UpdateForm(string changeLog)
        {
            InitializeComponent();

            ChangeLog.Text = changeLog;
        }

        private void DownloadUpdate_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/randomuserhi/AnimeDownloader/releases");
        }
    }
}
