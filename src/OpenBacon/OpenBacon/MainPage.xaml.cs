using Reddit;
using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OpenBacon
{
    public partial class MainPage : ContentPage
    {
        public RedditAPI Reddit { get; private set; }
        public Subreddit Subreddit { get; private set; }

        public MainPage(RedditAPI reddit, string subreddit = "")
        {
            InitializeComponent();
            Reddit = reddit;
            LoadSub(subreddit);
        }

        public void LoadSub(string subreddit)
        {
            Subreddit = Reddit.Subreddit(subreddit);
        }

        public string GetVersion()
        {
            string res = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return (string.IsNullOrWhiteSpace(res) || !res.Contains(".") ? res : res.Substring(0, res.LastIndexOf(".")) +
                (res.EndsWith(".1") ? "+develop" : res.EndsWith(".2") ? "+beta" : ""));
        }
    }
}
