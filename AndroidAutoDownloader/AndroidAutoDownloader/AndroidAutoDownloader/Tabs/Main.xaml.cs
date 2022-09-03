using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AndroidAutoDownloader.Tabs
{
    public partial class Main : ContentPage
    {
        private AutoDownloader_9Animeid manager;

        public string Browser_URL = string.Empty;

        public Main()
        {
            InitializeComponent();
            Debug.IsReadOnly = true;
            DownloadAll.IsEnabled = false;

            Browser.Navigated += (object sender, WebNavigatedEventArgs args) =>
            {
                Browser_URL = args.Url;
            };

            Type.Items.Add("Dubbed");
            Type.Items.Add("Subbed");
            Type.SelectedIndex = 0;

            manager = new AutoDownloader_9Animeid(this, Fetcher, Downloader);

            Browser.Source = "https://9anime.id/";
        }

        async void GetEpisodes_Click(object sender, EventArgs args)
        {
            //GetEpisodes.IsEnabled = false;

            manager.Download();

            //AutoDownloader_9Animeid.Link[] links = await manager.GetEpisodes(Browser_URL, (AutoDownloader_9Animeid.Type)Type.SelectedIndex);
        }

        void DownloadAll_Click(object sender, EventArgs args)
        {
            DownloadAll.IsEnabled = false;
        }

        private LinkedList<string> logs = new LinkedList<string>();
        public void Log(string text)
        {
            logs.AddLast(text);
            if (logs.Count > 10) logs.RemoveFirst();

            StringBuilder sb = new StringBuilder();
            foreach (string log in logs)
            {
                if (log != string.Empty) sb.AppendLine(log);
            }
            Debug.Text = sb.ToString();
        }
    }
}