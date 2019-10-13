using Reddit;
using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OpenBacon
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public RedditAPI Reddit { get; private set; }
        public Subreddit Subreddit { get; private set; }
        public string Sort { get; private set; } = "Hot";

        private int MaxSubNameLength { get; set; } = 15;
        private bool RefreshDisplayed { get; set; } = false;

        private TapGestureRecognizer BaconButtonOutClick { get; set; }
        
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
            PopulateBaconMenu();
            UpdateSortButton();

            BaconButtonOutClick = new TapGestureRecognizer();
            BaconButtonOutClick.Tapped += Popup_BaconButton_OutClick;
        }

        private void UpdateSortButton()
        {
            ButtonSort.Text = "Sort: " + Sort;
        }

        private void PopulateBaconMenu()
        {
            ListView_BaconButton.ItemsSource = new List<BaconMenuItem>
            {
                new BaconMenuItem("Refresh", "Refreshes the current view.", "OpenBaconRefresh"),
                new BaconMenuItem("Search", "Performs a search.", "OpenBaconSearch"),
                new BaconMenuItem("Open Subreddit", "Loads a subreddit.", "OpenBaconSubreddit")
            };
        }

        public void LoadSub(string subreddit)
        {
            Subreddit = Reddit.Subreddit(subreddit);
            ToolbarItem_Subreddits.Text = (!string.IsNullOrWhiteSpace(subreddit)
                ? (Subreddit.Name.Length <= MaxSubNameLength ? Subreddit.Name : Subreddit.Name.Substring(0, (MaxSubNameLength - 3)) + "...")
                : "Front Page");

            ToolbarItem_Spacer.Text = (Subreddit.Name.Length <= 10 ? "     " : " ");
            RefreshToolbar();
            PopulatePosts();
        }

        private IList<Post> GetPosts()
        {
            switch (Sort.ToLower())
            {
                default:
                    throw new Exception("Unrecognized sort : " + Sort);
                case "hot":
                    return Subreddit.Posts.Hot;
                case "new":
                    return Subreddit.Posts.New;
                case "rising":
                    return Subreddit.Posts.Rising;
                case "top":
                    return Subreddit.Posts.Top;
                case "controversial":
                    return Subreddit.Posts.Controversial;
            }
        }

        private IList<Post> GetPosts(string after = null)
        {
            switch (Sort.ToLower())
            {
                default:
                    throw new Exception("Unrecognized sort : " + Sort);
                case "hot":
                    return Subreddit.Posts.GetHot(after: after, limit: 25);
                case "new":
                    return Subreddit.Posts.GetNew(after: after, limit: 25);
                case "rising":
                    return Subreddit.Posts.GetRising(after: after, limit: 25);
                case "top":
                    return Subreddit.Posts.GetTop(after: after, limit: 25);
                case "controversial":
                    return Subreddit.Posts.GetControversial(after: after, limit: 25);
            }
        }

        private void PopulatePosts(string after = null, bool forceRefresh = false)
        {
            StackLayout_Posts.Children.Clear();
            foreach (Post post in (string.IsNullOrWhiteSpace(after) && !forceRefresh ? GetPosts() : GetPosts(after)))
            {
                StackLayout_Posts.Children.Add(new Frame
                {
                    HasShadow = true,
                    Content = new Grids.Post(Reddit, post).Grid,
                    Padding = 2
                });
            }
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
            // Check for new messages and display the appropriate icon.  --Kris
            ToolbarItem_Mail.IconImageSource = (Reddit.Account.Messages.Unread.Count.Equals(0) ? "OpenBaconMail" : "OpenBaconMailNewMessages");

            // Populate the subreddits listview with the user's subscriptions.  --Kris
            List<string> subs = new List<string>();
            foreach (Subreddit subreddit in Subscriptions)
            {
                subs.Add(subreddit.Name);
            }
            subs.Sort();

            ListView_Subreddits.ItemsSource = subs.ToArray();

            // Squeeze in more toolbar items for higher screen resolutions.  --Kris
            AdaptToResolution();

            // Populate the Bacon Menu.  --Kris
            PopulateBaconMenu();
        }

        private void AdaptToResolution()
        {
            if (DeviceDisplay.MainDisplayInfo.Width >= 1920)
            {
                AddRefreshToToolbar();
                MaxSubNameLength = 23;
            }
            else if (DeviceDisplay.MainDisplayInfo.Width >= 1440)
            {
                AddRefreshToToolbar();
                MaxSubNameLength = 20;
            }
            else if (DeviceDisplay.MainDisplayInfo.Width >= 1080)
            {
                AddRefreshToToolbar();
                MaxSubNameLength = 18;
            }
            else if (DeviceDisplay.MainDisplayInfo.Width >= 720)
            {
                AddRefreshToToolbar();
                MaxSubNameLength = 14;
            }
        }

        private void AddRefreshToToolbar()
        {
            if (!RefreshDisplayed)
            {
                ToolbarItems.Add(new ToolbarItem("Refresh", "OpenBaconRefresh.png", () =>
                {
                    ToolbarItemRefresh_Clicked(this, null);
                }, ToolbarItemOrder.Primary, 35));

                RefreshDisplayed = true;
            }
        }

        public string GetVersion()
        {
            string res = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return (string.IsNullOrWhiteSpace(res) || !res.Contains(".") ? res : res.Substring(0, res.LastIndexOf(".")) +
                (res.EndsWith(".1") ? "+develop" : res.EndsWith(".2") ? "+beta" : ""));
        }

        private void ToolbarItemSubreddits_Clicked(object sender, EventArgs e)
        {
            Popup_BaconButton.IsVisible = false;
            Popup_Subreddits.IsVisible = !Popup_Subreddits.IsVisible;
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

        private void ToolbarItemLoadSub_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        private void ToolbarItemBaconButton_Clicked(object sender, EventArgs e)
        {
            Popup_Subreddits.IsVisible = false;
            Popup_BaconButton.IsVisible = !Popup_BaconButton.IsVisible;

            if (Popup_BaconButton.IsVisible)
            {
                StackLayout_Main.GestureRecognizers.Add(BaconButtonOutClick);
            }
            else
            {
                StackLayout_Main.GestureRecognizers.Remove(BaconButtonOutClick);
            }
        }

        private void ButtonSort_Clicked(object sender, EventArgs e)
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

        private void ListView_BaconButton_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            switch (((BaconMenuItem)((ListView)sender).SelectedItem).Name)
            {
                default:
                    throw new Exception("Unknown item tapped : " + ((BaconMenuItem)((ListView)sender).SelectedItem).Name);
                case "Refresh":
                    ToolbarItemRefresh_Clicked(sender, e);
                    break;
                case "Search":
                    ToolbarItemSearch_Clicked(sender, e);
                    break;
                case "Open Subreddit":
                    ToolbarItemLoadSub_Clicked(sender, e);
                    break;
            }

            ((ListView)sender).SelectedItem = null;
            Popup_BaconButton.IsVisible = false;
        }

        private void Popup_BaconButton_OutClick(object sender, EventArgs e)
        {
            Popup_BaconButton.IsVisible = false;
        }
    }
}
