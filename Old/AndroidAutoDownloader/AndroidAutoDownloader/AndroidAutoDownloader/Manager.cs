using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using AndroidAutoDownloader.Tabs;
using System.Threading.Tasks;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using Xamarin.Forms.PlatformConfiguration;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AndroidAutoDownloader
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

            public override string ToString()
            {
                return (type == Type.subbed ? "[Sub] " : "[Dub] ") + (index + 1) + (episode != string.Empty ? " - " + episode : string.Empty);
            }
        }

        private class Scripts
        {
            public static string innerHTML =
                @"
                    document.documentElement.innerHTML;
                ";

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

        WebView fetcher;
        WebView downloader;

        Dictionary<string, Link> subbed = new Dictionary<string, Link>();
        Dictionary<string, Link> dubbed = new Dictionary<string, Link>();

        LinkedList<Link> queue = new LinkedList<Link>();

        public Main form;

        public string savePath = "/storage/emulated/0/Documents";

        public AutoDownloader_9Animeid(Main form, WebView fetcher, WebView downloader)
        {
            this.form = form;

            fetcher.IsEnabled = false;
            downloader.IsEnabled = false;

            this.fetcher = fetcher;
            this.downloader = downloader;
        }

        private static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                }), "\\\\\"", m => { return "\""; });
        }

        public async void Download()
        {
            form.Log("[Download] Loading site...");
            downloader.Source = "https://www.mp4upload.com/017l9y5ynd9k";

            await downloader.EvaluateJavaScriptAsync(Scripts.RedirectMp4UploadLink);
            string html = DecodeEncodedNonAsciiCharacters(await downloader.EvaluateJavaScriptAsync(Scripts.innerHTML));

            string loadedTest = "Embed code";
            int mp4Upload = html.IndexOf(loadedTest);
            for (int i = 0; i < 9 && mp4Upload < 0; i++)
            {
                form.Log("[Download] Attempt " + (i + 2) + "...");
                await downloader.EvaluateJavaScriptAsync(Scripts.RedirectMp4UploadLink);
                html = DecodeEncodedNonAsciiCharacters(await downloader.EvaluateJavaScriptAsync(Scripts.innerHTML));

                mp4Upload = html.IndexOf(loadedTest);
                await Task.Delay(1000);
            }
            if (mp4Upload < 0)
            {
                form.Log("[Download] Failed to load Mp4Upload embed.");
                return;
            }

            form.Log("[Download] Attempting to start download...");
            await downloader.EvaluateJavaScriptAsync(Scripts.StartMp4UploadDownload);
            /*for (int i = 0; i < 10; i++)
            {
                form.Log("[Download] Attempt " + (i + 1) + "...");
                await downloader.EvaluateJavaScriptAsync(Scripts.StartMp4UploadDownload);
                await Task.Delay(1000);
            }*/
        }

        public async Task<Link[]> GetEpisodes(string url, Type type)
        {
            form.Log("[Get] Loading site...");
            fetcher.Source = url;

            form.Log("[Get] Attempting to find episode listings...\n[Get] Attempt 1...");
            string html = DecodeEncodedNonAsciiCharacters(await fetcher.EvaluateJavaScriptAsync(Scripts.innerHTML));
            int start = html.IndexOf("ep-range");
            for (int i = 0; i < 9 && start < 0; i++)
            {
                form.Log("[Get] Attempt " + (i + 2) + "...");
                html = DecodeEncodedNonAsciiCharacters(await fetcher.EvaluateJavaScriptAsync(Scripts.innerHTML));
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
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            anime = String.Join("", anime.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

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
                    episode = String.Join("", episode.ToString().Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.'),
                    index = i,
                    episodeUrl = episodeUrl,
                    type = type
                });
            }
            form.Log("[Get] Finished.");
            return links.ToArray();
        }
    }
}
