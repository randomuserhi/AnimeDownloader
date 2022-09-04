using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CefSharp;
using CefSharp.WinForms;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;

namespace AutoDownloader
{
    public static class Utils
    {
        // https://stackoverflow.com/questions/4359910/how-to-abort-a-task-like-aborting-a-thread-thread-abort-method
        public static T RunWithAbort<T>(this Func<T> func, int milliseconds) => RunWithAbort(func, new TimeSpan(0, 0, 0, 0, milliseconds));
        public static T RunWithAbort<T>(this Func<T> func, TimeSpan delay)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var source = new CancellationTokenSource(delay);
            var item = default(T);
            var handle = IntPtr.Zero;
            var fn = new Action(() =>
            {
                using (source.Token.Register(() => TerminateThread(handle, 0)))
                {
                    item = func();
                }
            });

            handle = CreateThread(IntPtr.Zero, IntPtr.Zero, fn, IntPtr.Zero, 0, out var id);
            WaitForSingleObject(handle, 100 + (int)delay.TotalMilliseconds);
            CloseHandle(handle);
            return item;
        }

        [DllImport("kernel32")]
        private static extern bool TerminateThread(IntPtr hThread, int dwExitCode);

        [DllImport("kernel32")]
        private static extern IntPtr CreateThread(IntPtr lpThreadAttributes, IntPtr dwStackSize, Delegate lpStartAddress, IntPtr lpParameter, int dwCreationFlags, out int lpThreadId);

        [DllImport("kernel32")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32")]
        private static extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);
    }

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
            public string filePath;
            public string savePath;
            public string location;
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
            public bool filler;

            public override string ToString()
            {
                return (type == Type.subbed ? "[Sub] " : "[Dub] ") + (index + 1) + " - " + anime + " " + (filler ? " **Filler** " : string.Empty) + (episode != string.Empty ? " - " + episode : string.Empty);
            }

            public static bool operator ==(Link a, Link b)
            {
                return (a.type == b.type) && (a.anime == b.anime) && (a.episode == b.episode) && (a.index == b.index);
            }

            public static bool operator !=(Link a, Link b)
            {
                return (a.type != b.type) || (a.anime != b.anime) || (a.episode != b.episode) || (a.index != b.index);
            }

            public override bool Equals(object o)
            {
                return base.Equals(o);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
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

        //static string urlPrefix = "https://9anime.id/watch/";

        ChromiumWebBrowser fetcher;
        ChromiumWebBrowser downloader;
        DownloadManager downloads;

        public Dictionary<string, Link> subbed = new Dictionary<string, Link>();
        public Dictionary<string, Link> dubbed = new Dictionary<string, Link>();

        public LinkedList<Link> queue = new LinkedList<Link>();

        public Form form;

        public string savePath = string.Empty;

        public AutoDownloader_9Animeid(Form form, ChromiumWebBrowser fetcher, ChromiumWebBrowser downloader)
        {
            this.form = form;

            fetcher.LifeSpanHandler = new PopupManager();

            downloader.LifeSpanHandler = new PopupManager();
            downloads = new DownloadManager(form, this);
            downloader.DownloadHandler = downloads;

            this.fetcher = fetcher;
            this.downloader = downloader;

            LoadSettings();
        }

        static string settingsLocation = "settings.txt";
        private void LoadSettings()
        {
            form.Log("[Manager] Loading settings.txt ...");
            if (!File.Exists(settingsLocation))
            {
                form.Log("[Manager] settings.txt does not exist.");
                return;
            }
            try
            {
                form.Log("[Manager] Reading settings.txt ...");
                string[] lines = File.ReadAllLines(settingsLocation);

                if (!Directory.Exists(lines[0])) lines[0] = string.Empty;
                savePath = lines[0];
                form.SavePathLabel.Text = savePath;
                form.subbedDubbed = int.Parse(lines[1]);
                form.SubbedButton.Enabled = form.subbedDubbed == 0;
                form.DubbedButton.Enabled = form.subbedDubbed == 1;
                form.checkUpdates = int.Parse(lines[2]) == 1;
                if (savePath == string.Empty) return;
                CheckAnimes();
            }
            catch (Exception err)
            {
                form.Log("[Manager] Unable to load old settings.txt (May have been invalidated by an update), removing...");
                form.Log("[Manager : WARNING] " + err.Message);
                File.Delete(settingsLocation);
            }
        }
        public void CheckAnimes()
        {
            subbed.Clear();
            dubbed.Clear();

            form.Log("[Manager] Checking anime folder...");
            string[] directories = Directory.GetDirectories(savePath);
            List<Link> foundLinks = new List<Link>();
            for (int i = 0; i < directories.Length; i++)
            {
                foundLinks.AddRange(LoadEpisodes(subbed, Type.subbed, Path.Combine(savePath, directories[i], @"Sub\autodownloader.ini")));
                foundLinks.AddRange(LoadEpisodes(dubbed, Type.dubbed, Path.Combine(savePath, directories[i], @"Dub\autodownloader.ini")));
            }
            form.RestoreEpisodesFromListings(foundLinks);
            form.Log("[Manager] Finished.");
        }
        private List<Link> LoadEpisodes(Dictionary<string, Link> database, Type type, string filePath)
        {
            List<Link> foundLinks = new List<Link>();
            if (File.Exists(filePath))
            {
                string[] data = VerifyMetaFile(filePath);
                if (data == null)
                {
                    form.Log("[Manager] autodownloader.ini failed verification..." + filePath);
                    return foundLinks;
                }
                try
                {
                    for (int j = 2; j < data.Length; j++)
                    {
                        try
                        {
                            if (!database.ContainsKey(data[j]))
                            {
                                string[] components = data[j].Split('?');
                                Link l = new Link()
                                {
                                    anime = data[1],
                                    episodeUrl = components[0],
                                    index = int.Parse(components[1]),
                                    filler = components[2] == "true",
                                    episode = components[3],
                                    type = type
                                };
                                foundLinks.Add(l);
                                if (!database.ContainsKey(components[0]))
                                    database.Add(components[0], l);
                            }
                        }
                        catch (Exception err)
                        {
                            form.Log("[Manager] Failed to load episode for " + filePath);
                            form.Log("[Manager : FATAL ERROR] " + err.Message);
                        }
                    }
                }
                catch (Exception err)
                {
                    form.Log("[Manager] Failed to read meta data for " + filePath);
                    form.Log("[Manager : FATAL ERROR] " + err.Message);
                }
            }
            return foundLinks;
        }
        private string[] VerifyMetaFile(string filePath)
        {
            string version = "1.0.0";

            StringBuilder edits = new StringBuilder();
            string[] lines = File.ReadAllLines(filePath);
            string[] header = lines[0].Split('?');
            if (header.Length != 2)
            {
                form.Log("[Manager] autodownloader.ini has an invalid header, assuming it is an old version...");
                form.Log("[Manager : WARNING] Reconstruction of autodownloader.ini may fail resulting in dat for the given folder to be invalid.");
                try
                {
                    edits.AppendLine(version + "?");
                    FileInfo fileInfo = new FileInfo(filePath);
                    edits.AppendLine(fileInfo.Directory.Parent.Name);
                }
                catch (Exception err)
                {
                    form.Log("[Manager] Failed to reconstruct autodownloader.ini for " + filePath);
                    form.Log("[Manager : FATAL ERROR] " + err.Message);

                    return null;
                }
            }
            else if (header[0] != version)
            {
                form.Log("[Manager] autodownloader.ini is an old version...");
                try
                {
                    edits.AppendLine(version + "?");
                }
                catch (Exception err)
                {
                    form.Log("[Manager] Failed to update autodownloader.ini for " + filePath);
                    form.Log("[Manager : FATAL ERROR] " + err.Message);

                    return null;
                }
            }

            if (edits.Length > 0)
            {
                for (int i = 0; i < lines.Length; i++) edits.AppendLine(lines[i]);
                File.WriteAllText(filePath, edits.ToString());
            }

            return File.ReadAllLines(filePath);
        }

        public void SaveSettings()
        {
            form.Log("[Manager] Saving settings.txt ...");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(savePath);
            sb.AppendLine("" + form.subbedDubbed);
            sb.AppendLine((form.checkUpdates ? 1 : 0).ToString());
            File.WriteAllText(settingsLocation, sb.ToString());
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
                    LoadUrlAsyncResponse resp = await downloader.LoadUrlAsync(l.episodeUrl);
                    if (!resp.Success)
                    {
                        form.Log("[Enqueue] Browser timed out, aborting...");
                        return;
                    }

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
                    resp = await downloader.LoadUrlAsync(downloadLink.ToString());
                    if (!resp.Success)
                    {
                        form.Log("[Enqueue] Browser timed out, aborting...");
                        return;
                    }

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

        public string currentAnime = string.Empty;
        public Link[] allLinks = new Link[0];
        public async Task<Link[]> GetEpisodes(CancellationToken ct, string url, Type type)
        {
            try
            {
                // Were we already canceled?
                ct.ThrowIfCancellationRequested();

                form.Log("[Get] Loading site...");
                LoadUrlAsyncResponse resp = await fetcher.LoadUrlAsync(url);
                if (!resp.Success)
                {
                    form.Log("[Get] Browser timed out, aborting...");
                    return null;
                }
                ct.ThrowIfCancellationRequested();

                string tag = "<div class=\"episodes number\">";

                form.Log("[Get] Attempting to find episode listings...\n[Get] Attempt 1...");
                string html = await fetcher.GetSourceAsync();
                if (!html.Contains(tag)) tag = "<div class=\"episodes name\">";
                int start = html.IndexOf(tag);
                //File.WriteAllText(@"D:\test.txt", html);
                for (int i = 0; i < 9 && start < 0; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    form.Log("[Get] Attempt " + (i + 2) + "...");
                    html = await fetcher.GetSourceAsync();
                    start = html.IndexOf(tag);
                    await Task.Delay(1000);
                }
                if (start < 0)
                {
                    form.Log("[Get] Failed to find episode listings.");
                    return null;
                }

                form.Log("[Get] Succeeded, checking for sub / dub...");
                switch (type)
                {
                    case Type.subbed:
                        if (!html.Contains("data-type=\"sub\""))
                        {
                            form.Log("[Get] No subs available.");
                            return null;
                        }
                        break;
                    case Type.dubbed:
                        if (!html.Contains("data-type=\"dub\""))
                        {
                            form.Log("[Get] No dubs available.");
                            return null;
                        }
                        break;
                    default:
                        form.Log("[Get] No subs or dubs available.");
                        return null;
                }

                form.Log("[Get] Finding title...");
                string titlePrefix = "<title>Watch ";
                int titleStart = html.IndexOf(titlePrefix) + titlePrefix.Length;
                int titleEnd = html.IndexOf(" Online in HD with English Subbed, Dubbed</title>");
                string anime = html.Substring(titleStart, titleEnd - titleStart);
                var invalids = System.IO.Path.GetInvalidFileNameChars();
                anime = String.Join("", anime.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

                for (; html[start++] != '>';) { }
                string cut = html.Substring(start);
                int end = cut.IndexOf("</div>");
                string subString = cut.Substring(0, end);
                string[] sets = subString.Split(new[] { "</ul>" }, StringSplitOptions.RemoveEmptyEntries);

                form.Log("[Get] Found " + (sets.Length - 1) + " sets...");

                List<Link> links = new List<Link>();
                List<Link> allLinks = new List<Link>();
                int episodeIndex = 0;
                for (int i = 0; i < sets.Length - 1; i++)
                {
                    string episodes = sets[i];

                    form.Log("[Get] Grabbing Links of set " + (i + 1) + "...");
                    string[] list = episodes.Split(new[] { "</li>" }, StringSplitOptions.RemoveEmptyEntries);
                    // -1 to remove the trailing </li> entry
                    for (int j = 0; j < list.Length - 1; j++, episodeIndex++)
                    {
                        StringBuilder episode = new StringBuilder();
                        int index = list[j].IndexOf("</span>");
                        if (index >= 0)
                            for (char c = list[j][--index]; c != '>'; c = list[j][--index])
                            {
                                episode.Insert(0, c);
                            }

                        StringBuilder href = new StringBuilder();
                        string prefix = "href=\"";
                        index = list[j].IndexOf(prefix) + prefix.Length;
                        if (index >= 0)
                            for (char c = list[j][index]; c != '"'; c = list[j][++index])
                            {
                                href.Append(c);
                            }
                        string episodeUrl = href.ToString();

                        Link l = new Link()
                        {
                            anime = anime,
                            episode = String.Join("", episode.ToString().Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.'),
                            index = episodeIndex,
                            episodeUrl = episodeUrl,
                            filler = list[j].Contains("** Filler Episode **"),
                            type = type
                        };
                        allLinks.Add(l);

                        switch (type)
                        {
                            case Type.subbed:
                                if (subbed.ContainsKey(episodeUrl) || !list[j].Contains("data-sub=\"1\"")) continue;
                                break;
                            case Type.dubbed:
                                if (dubbed.ContainsKey(episodeUrl) || !list[j].Contains("data-dub=\"1\"")) continue;
                                break;
                            default:
                                continue;
                        }
                        links.Add(l);
                    }
                }
                form.Log("[Get] Finished.");
                currentAnime = anime;
                this.allLinks = allLinks.ToArray();
                return links.ToArray();
            }
            catch(OperationCanceledException)
            {
                form.Log("[Get] Cancelling...");
                return null;
            }
        }
    }
    public class DownloadManager : IDownloadHandler
    {
        public object mutex = new object();

        private Form form;
        private AutoDownloader_9Animeid manager;
        public Dictionary<string, AutoDownloader_9Animeid.DownloadProgress> files = new Dictionary<string, AutoDownloader_9Animeid.DownloadProgress>();

        public DownloadManager(Form form, AutoDownloader_9Animeid downloader)
        {
            this.manager = downloader;
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
                    fileName = manager.current.ToString() + ".mp4",
                    savePath = manager.savePath,
                    url = url,
                    completed = false,
                    cancelled = false
                };
                files.Add(id, progress);

                form.SetProgress(progress);

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
                    Uri uri = new Uri(downloadItem.Url);
                    string id = Path.GetFileName(uri.AbsolutePath);

                    AutoDownloader_9Animeid.Link l = manager.current;
                    AutoDownloader_9Animeid.DownloadProgress progress = files[id];

                    if (progress.savePath == string.Empty) progress.savePath = AppDomain.CurrentDomain.BaseDirectory;
                    string DownloadsDirectoryPath = Path.Combine(progress.savePath, l.anime, (l.type == AutoDownloader_9Animeid.Type.subbed ? @"Sub" : @"Dub"));
                    string fullPath = Path.Combine(DownloadsDirectoryPath, l.ToString() + ".temp");

                    progress.filePath = fullPath;
                    files[id] = progress;

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
                    form.Log("[Web] Unable to find file of id, clearing...");
                    form.ClearProgress();
                    callback.Cancel();
                    return;
                }

                AutoDownloader_9Animeid.DownloadProgress progress = files[id];
                if (progress.cancelled)
                {
                    form.Log("[Web] Cancelling download...");
                    if (progress.savePath == manager.savePath) form.RestoreEpisode(manager.current);
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
                    form.Log("[Web] Download completed.");

                    AutoDownloader_9Animeid.Link l = manager.current;
                    File.AppendAllText(Path.Combine(progress.savePath, l.anime, (l.type == AutoDownloader_9Animeid.Type.subbed ? @"Sub" : @"Dub"), "autodownloader.ini"), l.episodeUrl + "?" + l.index + "?" + l.filler + "?" + l.episode + "\n");

                    FileInfo fileInfo = new FileInfo(progress.filePath);
                    fileInfo.MoveTo(Path.Combine(fileInfo.Directory.FullName, l.ToString() + ".mp4"));

                    form.DownloadControl(false);

                    progress.completed = true;
                    progress.speed = 0;
                    progress.percentage = 0;

                    files.Remove(id);
                }
                else files[id] = progress;

                form.SetProgress(progress);
                form.DownloadControl(true);
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
