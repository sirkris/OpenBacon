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

        public IList<Subreddit> Subscriptions
        {
            get
            {
                return (!SubscriptionsLastUpdated.HasValue || SubscriptionsLastUpdated.Value.AddMinutes(1) <= DateTime.Now
                    ? GetSubscriptions()
                    : subscriptions);
            }
            private set
            {
                subscriptions = value;
            }
        }
        private IList<Subreddit> subscriptions;
        private DateTime? SubscriptionsLastUpdated;

        public MainPage(RedditAPI reddit, string subreddit = "")
        {
            InitializeComponent();
            Reddit = reddit;
            LoadSub(subreddit);
        }

        public void LoadSub(string subreddit)
        {
            Subreddit = Reddit.Subreddit(subreddit);
            ToolbarItem_Subreddits.Text = (!string.IsNullOrWhiteSpace(subreddit) 
                ? (Subreddit.Name.Length <= 15 ? Subreddit.Name : Subreddit.Name.Substring(0, 12) + "...")
                : "Front Page");

            ToolbarItem_Spacer.Text = (Subreddit.Name.Length <= 10 ? "     " : "  ");

            RefreshToolbar();
        }

        public IList<Subreddit> GetSubscriptions()
        {
            IList<Subreddit> res = new List<Subreddit>();
            string after = "";
            List<Subreddit> subs;
            DateTime now = DateTime.Now;
            do
            {
                subs = Reddit.Account.MySubscribedSubreddits(after: after, limit: 100);
                foreach (Subreddit subreddit in subs)
                {
                    res.Add(subreddit);
                    after = subreddit.Fullname;
                }
            } while (subs.Count > 0 && now.AddMinutes(1) > DateTime.Now);

            Subscriptions = res;
            SubscriptionsLastUpdated = DateTime.Now;

            return res;
        }

        public void RefreshToolbar()
        {
            ToolbarItem_Mail.Icon = (Reddit.Account.Messages.Unread.Count.Equals(0) ? "OpenBacon_Mail" : "OpenBacon_Mail_NewMessages");

            List<string> subs = new List<string>();
            foreach (Subreddit subreddit in Subscriptions)
            {
                subs.Add(subreddit.Name);
            }
            subs.Sort();

            ListView_Subreddits.ItemsSource = subs.ToArray();
        }

        public string GetVersion()
        {
            string res = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return (string.IsNullOrWhiteSpace(res) || !res.Contains(".") ? res : res.Substring(0, res.LastIndexOf(".")) +
                (res.EndsWith(".1") ? "+develop" : res.EndsWith(".2") ? "+beta" : ""));
        }

        private void ToolbarItemSubreddits_Clicked(object sender, EventArgs e)
        {
            Popup_Subreddits.IsVisible = true;
        }

        private void ToolbarItemMail_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        private void ToolbarItemRefresh_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        private void ToolbarItemSearch_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        private void ListView_Subreddits_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            LoadSub(((ListView)sender).SelectedItem.ToString());
            Popup_Subreddits.IsVisible = false;
        }

        private void Popup_Subreddits_OutClick(object sender, EventArgs e)
        {
            Popup_Subreddits.IsVisible = false;
        }
    }
}
