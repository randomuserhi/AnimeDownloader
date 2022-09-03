using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using AndroidAutoDownloader.Droid.Browser;

using AndroidAutoDownloader;
using Android.App;
using Android.Widget;

[assembly: ExportRenderer(typeof(WebView), typeof(AndroidWebView))]
namespace AndroidAutoDownloader.Droid.Browser
{
    public class AndroidWebView : WebViewRenderer
    {
        public AndroidWebView(Context ctx) : base(ctx)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Settings.UserAgentString = "Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.9.0.4) Gecko/20100101 Firefox/4.0";
            }

            Control.Download += DownloadEvent;
        }

        private void DownloadEvent(object sender, Android.Webkit.DownloadEventArgs e) // CODE BREAKS HERE CAUSE MP4Upload only triggers download on redirect from its own site
        {
            string url = e.Url;
            DownloadManager.Request request = new DownloadManager.Request(Android.Net.Uri.Parse(url));
            request.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
            request.SetDestinationInExternalPublicDir(Android.OS.Environment.DirectoryDownloads, "CPPPrimer");
            DownloadManager dm = (DownloadManager)Android.App.Application.Context.GetSystemService("download");
            dm.Enqueue(request);
            Toast.MakeText(Android.App.Application.Context, e.Url, ToastLength.Long).Show();
        }
    }
}