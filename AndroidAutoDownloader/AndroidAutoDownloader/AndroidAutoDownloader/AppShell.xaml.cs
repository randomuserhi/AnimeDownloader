using AndroidAutoDownloader.Tabs;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace AndroidAutoDownloader
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Main), typeof(Main));
        }

    }
}
