using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CefSharp;
using CefSharp.WinForms;

namespace AutoDownloader
{
    public class AutoDownloader_9Animeid
    {
        public enum Type
        {
            dubbed,
            subbed
        }

        public struct DownloadProgress
        {
            public string id;
            public string fileName;
            public string url;
            public bool completed;
            public bool cancelled;

            public long speed;
            public int percentage;
        }

        public struct Link
        {
            public string anime;
            public string episode;
            public int index;
            public string episodeUrl;
            public Type type;

            public override string ToString()
            {
                return (type == Type.subbed ? "[Sub] " : "[Dub] ") + (index + 1) + (episode != string.Empty ? " - " + episode : string.Empty);
            }
        }

        private class Scripts
        {
            public static string LoadSubbedMp4UploadVideo =
                @"
                    var version = 'sub';
                    var temp = document.getElementsByTagName('li');
                    for (var i = 0; i < temp.length; i++)
                    {
                        if (temp[i].innerHTML == 'Mp4upload')
                        {
                            var parent = temp[i].closest('div');
                            if (parent != null && parent.getAttribute('data-type') + '' == version) 
                            { 
                                temp[i].click(); 
                                break;
                            }
                        }
                    }
                ";

            public static string LoadDubbedMp4UploadVideo =
                @"
                    var version = 'dub';
                    var temp = document.getElementsByTagName('li');
                    for (var i = 0; i < temp.length; i++)
                    {
                        if (temp[i].innerHTML == 'Mp4upload')
                        {
                            var parent = temp[i].closest('div');
                            if (parent != null && parent.getAttribute('data-type') + '' == version) 
                            { 
                                temp[i].click(); 
                                break;
                            }
                        }
                    }
                ";

            public static string RedirectMp4UploadLink =
                @"
                    document.getElementById('todl').click();
                ";

            public static string StartMp4UploadDownload =
                @"
                    document.getElementById('downloadbtn').click();
                ";

            public static string SwitchEpisode(int index)
            {
                string script =
                @"
                    episode = document.getElementsByClassName('ep-range')[0].querySelectorAll('li');
                    episode[" + index + @"].querySelector('span').click();
                ";
                return script;
            }
        }

        static string urlPrefix = "https://9anime.id/watch/";

        ChromiumWebBrowser fetcher;
        ChromiumWebBrowser downloader;
        DownloadManager downloads;

        Dictionary<string, Link> subbed = new Dictionary<string, Link>();
        Dictionary<string, Link> dubbed = new Dictionary<string, Link>();

        LinkedList<Link> queue = new LinkedList<Link>();

        public Form form;

        public string savePath = String.Empty;

        public AutoDownloader_9Animeid(Form form, ChromiumWebBrowser fetcher, ChromiumWebBrowser downloader)
        {
            this.form = form;

            fetcher.LifeSpanHandler = new PopupManager();

            downloader.LifeSpanHandler = new PopupManager();
            downloads = new DownloadManager(form, this);
            downloader.DownloadHandler = downloads;

            this.fetcher = fetcher;
            this.downloader = downloader;
        }

        public Task active;
        public void CheckQueue()
        {
            if (active == null || active.IsCompleted) active = Enqueue();
        }

        public Link current;
        private async Task Enqueue()
        {
            if (downloads.files.Count == 0)
            {
                if (queue.Count != 0)
                {
                    Link l = queue.First();
                    current = l;

                    queue.RemoveFirst();

                    form.Downloads.Items.Clear();
                    foreach (Link li in queue)
                    {
                        form.Downloads.Items.Add(li);
                    }

                    form.Log("[Enqueue] Loading URL...");
                    await downloader.LoadUrlAsync(l.episodeUrl);

                    form.Log("[Enqueue] Attempting to load Mp4Upload video...\n[Enqueue] Attempt 1...");
                    string script = string.Empty;
                    switch (l.type)
                    {
                        case Type.subbed:
                            script = Scripts.LoadSubbedMp4UploadVideo;
                            break;
                        case Type.dubbed:
                            script = Scripts.LoadDubbedMp4UploadVideo;
                            break;
                        default:
                            Remove(l);
                            return;
                    }
                    downloader.ExecuteScriptAsync(script);

                    string html = await downloader.GetSourceAsync();

                    string linkPrefix = "https://www.mp4upload.com/embed-";
                    int mp4Upload = html.IndexOf(linkPrefix);
                    for (int i = 0; i < 9 && mp4Upload < 0; i++)
                    {
                        form.Log("[Enqueue] Attempt " + (i + 2) + "...");
                        html = await downloader.GetSourceAsync();

                        mp4Upload = html.IndexOf(linkPrefix);
                        await Task.Delay(1000);
                    }
                    if (mp4Upload < 0)
                    {
                        form.Log("[Enqueue] Failed to load Mp4Upload video.");
                        Remove(l);
                        return;
                    }
                    mp4Upload += linkPrefix.Length;

                    StringBuilder downloadLink = new StringBuilder("https://www.mp4upload.com/");
                    for (char c = html[mp4Upload]; c != '.'; c = html[++mp4Upload])
                    {
                        downloadLink.Append(c);
                    }

                    form.Log("[Enqueue] Found Mp4 video, " + downloadLink.ToString() + "\n[Enqueue] Attempting to load Mp4Upload embed...\n[Enqueue] Attempt 1...");
                    await downloader.LoadUrlAsync(downloadLink.ToString());

                    downloader.ExecuteScriptAsync(Scripts.RedirectMp4UploadLink);
                    html = await downloader.GetSourceAsync();

                    string loadedTest = "Embed code";
                    mp4Upload = html.IndexOf(loadedTest);
                    for (int i = 0; i < 9 && mp4Upload < 0; i++)
                    {
                        form.Log("[Enqueue] Attempt " + (i + 2) + "...");
                        downloader.ExecuteScriptAsync(Scripts.RedirectMp4UploadLink);
                        html = await downloader.GetSourceAsync();

                        mp4Upload = html.IndexOf(loadedTest);
                        await Task.Delay(1000);
                    }
                    if (mp4Upload < 0)
                    {
                        form.Log("[Enqueue] Failed to load Mp4Upload embed.");
                        Remove(l);
                        return;
                    }

                    form.Log("[Enqueue] Attempting to start download...");
                    for (int i = 0; i < 10 && downloads.files.Count == 0; i++)
                    {
                        form.Log("[Enqueue] Attempt " + (i + 1) + "...");
                        downloader.ExecuteScriptAsync(Scripts.StartMp4UploadDownload);
                        await Task.Delay(1000);
                    }
                    if (downloads.files.Count != 0)
                    {
                        form.Log("[Enqueue] Download started successfully!");
                    }
                    else
                    {
                        form.Log("[Enqueue] Failed to start download.");
                    }
                }
            }
            else
            {
                form.Downloads.Items.Clear();
                foreach (Link l in queue)
                {
                    form.Downloads.Items.Add(l);
                }
            }
        }

        private void Remove(Link l)
        {
            switch (l.type)
            {
                case Type.subbed:
                    subbed.Remove(l.episodeUrl);
                    break;
                case Type.dubbed:
                    dubbed.Remove(l.episodeUrl);
                    break;
                default:
                    return;
            }
        }

        public DownloadProgress GetCurrentProgress()
        {
            if (downloads.files.Count == 1)
            {
                string key = downloads.files.Keys.First();
                return downloads.files[key];
            }
            else return new DownloadProgress();
        }

        public void Cancel()
        {
            if (downloads.files.Count == 1)
            {
                string key = downloads.files.Keys.First();
                DownloadProgress l = downloads.files[key];
                l.cancelled = true;
                downloads.files[key] = l;

                Remove(current);
            }
        }

        public void AddEpisodes(Link[] links, Type type)
        {
            Dictionary<string, Link> active = null;
            switch (type)
            {
                case Type.subbed:
                    active = subbed;
                    break;
                case Type.dubbed:
                    active = dubbed;
                    break;
                default:
                    return;
            }
            if (active == null) return;

            for(int i = 0; i < links.Length; i++)
            {
                active.Add(links[i].episodeUrl, links[i]);
                queue.AddLast(links[i]);
            }
        }

        public void RemoveEpisodes(Link[] links)
        {
            for (int i = 0; i < links.Length; i++)
            {
                Link l = links[i];
                Remove(l);
                queue.Remove(l);
            }
        }

        public async Task<Link[]> GetEpisodes(string url, Type type)
        {
            form.Log("[Get] Loading site...");
            await fetcher.LoadUrlAsync(url);

            form.Log("[Get] Attempting to find episode listings...\n[Get] Attempt 1...");
            string html = await fetcher.GetSourceAsync();
            int start = html.IndexOf("ep-range");
            for (int i = 0; i < 9 && start < 0; i++)
            {
                form.Log("[Get] Attempt " + (i + 2) + "...");
                html = await fetcher.GetSourceAsync();
                start = html.IndexOf("ep-range");
                await Task.Delay(1000);
            }
            if (start < 0)
            {
                form.Log("[Get] Failed to find episode listings.");
                return new Link[0];
            }

            form.Log("[Get] Succeeded, checking for sub / dub...");
            switch (type)
            {
                case Type.subbed:
                    if (!html.Contains("data-type=\"sub\""))
                    {
                        form.Log("[Get] No subs available.");
                        return new Link[0];
                    }
                    break;
                case Type.dubbed:
                    if (!html.Contains("data-type=\"dub\""))
                    {
                        form.Log("[Get] No dubs available.");
                        return new Link[0];
                    }
                    break;
                default:
                    form.Log("[Get] No subs or dubs available.");
                    return new Link[0];
            }

            form.Log("[Get] Finding title...");
            string titlePrefix = "<title>Watch ";
            int titleStart = html.IndexOf(titlePrefix) + titlePrefix.Length;
            int titleEnd = html.IndexOf(" Online in HD with English Subbed, Dubbed</title>");
            string anime = html.Substring(titleStart, titleEnd - titleStart);

            for (; html[start++] != '>';) { }
            string cut = html.Substring(start);
            int end = cut.IndexOf("</ul>");
            string episodes = cut.Substring(0, end);

            form.Log("[Get] Grabbing Links...");
            string[] list = episodes.Split(new[] { "</li>" }, StringSplitOptions.RemoveEmptyEntries);
            // -1 to remove the trailing </li> entry
            List<Link> links = new List<Link>();
            for (int i = 0; i < list.Length - 1; i++)
            {
                StringBuilder episode = new StringBuilder();
                int index = list[i].IndexOf("</span>");
                if (index >= 0)
                    for (char c = list[i][--index]; c != '>'; c = list[i][--index])
                    {
                        episode.Insert(0, c);
                    }

                StringBuilder href = new StringBuilder();
                string prefix = "href=\"";
                index = list[i].IndexOf(prefix) + prefix.Length;
                if (index >= 0)
                    for (char c = list[i][index]; c != '"'; c = list[i][++index])
                    {
                        href.Append(c);
                    }
                string episodeUrl = href.ToString();

                switch (type)
                {
                    case Type.subbed:
                        if (subbed.ContainsKey(episodeUrl)) continue;
                        break;
                    case Type.dubbed:
                        if (dubbed.ContainsKey(episodeUrl)) continue;
                        break;
                    default:
                        continue;
                }

                links.Add(new Link()
                {
                    anime = anime,
                    episode = episode.ToString(),
                    index = i,
                    episodeUrl = episodeUrl,
                    type = type
                });
            }
            form.Log("[Get] Finished.");
            return links.ToArray();
        }
    }
    public class DownloadManager : IDownloadHandler
    {
        private Form form;
        private AutoDownloader_9Animeid downloader;
        public Dictionary<string, AutoDownloader_9Animeid.DownloadProgress> files = new Dictionary<string, AutoDownloader_9Animeid.DownloadProgress>();

        public DownloadManager(Form form, AutoDownloader_9Animeid downloader)
        {
            this.downloader = downloader;
            this.form = form;
        }

        public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
        {
            Uri uri = new Uri(url);
            string id = Path.GetFileName(uri.AbsolutePath);

            if (!files.ContainsKey(id))
            {
                AutoDownloader_9Animeid.DownloadProgress progress = new AutoDownloader_9Animeid.DownloadProgress()
                {
                    id = id,
                    fileName = downloader.current.ToString() + ".mp4",
                    url = url,
                    completed = false,
                    cancelled = false
                };
                files.Add(id, progress);

                form.SetProgress(progress);
                form.DownloadControl(true);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    AutoDownloader_9Animeid.Link l = downloader.current;
                    if (downloader.savePath == string.Empty) downloader.savePath = AppDomain.CurrentDomain.BaseDirectory;
                    string DownloadsDirectoryPath = Path.Combine(downloader.savePath, l.anime, (l.type == AutoDownloader_9Animeid.Type.subbed ? @"Sub" : @"Dub"));
                    string fullPath = Path.Combine(DownloadsDirectoryPath, l.ToString() + ".mp4");

                    form.Log("[Web] Started downloading to: " + fullPath);

                    callback.Continue(
                        fullPath,
                        showDialog: false
                    );
                }
            }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            if (downloadItem.IsValid)
            {
                Uri uri = new Uri(downloadItem.Url);
                string id = Path.GetFileName(uri.AbsolutePath);

                if (!files.ContainsKey(id))
                {
                    form.Log("[Web] Unable to find file of id...");
                    return;
                }

                AutoDownloader_9Animeid.DownloadProgress progress = files[id];
                if (progress.cancelled)
                {
                    form.Log("[Web] Cancelling download...");
                    files.Remove(id);
                    form.ClearProgress();
                    callback.Cancel();
                    form.DownloadControl(false);
                    return;
                }

                if (downloadItem.IsInProgress && (downloadItem.PercentComplete != 0))
                {
                    progress.speed = downloadItem.CurrentSpeed;
                    progress.percentage = downloadItem.PercentComplete;
                }

                if (downloadItem.IsComplete)
                {
                    progress.completed = true;
                    progress.speed = 0;
                    progress.percentage = 100;
                }

                files[id] = progress;
                form.SetProgress(progress);
            }
        }
    }

    public class PopupManager : ILifeSpanHandler
    {
        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName,
            WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures,
            IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess,
            out IWebBrowser newBrowser)
        {
            newBrowser = null;
            return true;
        }

        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            return false;
        }

        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }
    }
}
