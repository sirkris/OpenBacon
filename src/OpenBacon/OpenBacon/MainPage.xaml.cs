using Reddit;
using Reddit.Controllers;
using Reddit.Exceptions;
using Reddit.Exceptions;
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

        private IDictionary<string, Image> UpArrows;
        private IDictionary<string, Label> ScoreLabels;
        private IDictionary<string, Image> DownArrows;
        private IDictionary<string, Frame> Frames;

        private IDictionary<string, TapGestureRecognizer> UpArrowTaps;
        private IDictionary<string, TapGestureRecognizer> DownArrowTaps;
        private IDictionary<string, SwipeGestureRecognizer> FrameLeftSwipes;
        private IDictionary<string, SwipeGestureRecognizer> FrameRightSwipes;

        private IDictionary<string, Frame> BaconMenuFrames;
        private IDictionary<string, TapGestureRecognizer> BaconMenuTaps;

        private int MaxSubNameLength { get; set; } = 15;
        private bool RefreshDisplayed { get; set; } = false;
        private string After { get; set; } = "";

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

            UpArrows = new Dictionary<string, Image>();
            ScoreLabels = new Dictionary<string, Label>();
            DownArrows = new Dictionary<string, Image>();

            UpArrowTaps = new Dictionary<string, TapGestureRecognizer>();
            DownArrowTaps = new Dictionary<string, TapGestureRecognizer>();

            Frames = new Dictionary<string, Frame>();

            FrameLeftSwipes = new Dictionary<string, SwipeGestureRecognizer>();
            FrameRightSwipes = new Dictionary<string, SwipeGestureRecognizer>();

            BaconMenuFrames = new Dictionary<string, Frame>();
            BaconMenuTaps = new Dictionary<string, TapGestureRecognizer>();

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
            StackLayout_BaconMenu.Children.Clear();

            BaconMenuFrames.Clear();
            BaconMenuTaps.Clear();

            // TODO - Use Models.OAuthCredentials to determine if account is linked once Reddit.NET 1.3 is available.  --Kris
            string username = null;
            try
            {
                username = Reddit.Account.Me.Name;
            }
            catch (Exception) { }

            IList<BaconMenuItem> baconMenuItems = new List<BaconMenuItem>
            {
                // TODO - Use Models.OAuthCredentials to determine if account is linked once Reddit.NET 1.3 is available.  --Kris
                (!string.IsNullOrEmpty(username) 
                    ? new BaconMenuItem("Your Profile", "Access u/" + username + "'s profile.", "alien") 
                    : new BaconMenuItem("Connect Account", "Allow OpenBacon access to your Reddit account.", "alien" )),
                new BaconMenuItem("Refresh", "Refreshes the current view.", "refresh"),
                new BaconMenuItem("Search", "Performs a search.", "search"),
                new BaconMenuItem("Open Subreddit", "Loads a subreddit.", "subreddit")
            };

            foreach (BaconMenuItem baconMenuItem in baconMenuItems)
            {
                if (!BaconMenuFrames.ContainsKey(baconMenuItem.Name))
                {
                    BaconMenuFrames.Add(baconMenuItem.Name, new Frame
                    {
                        HasShadow = false, 
                        BackgroundColor = Color.FromHex("#0079D3"),
                        BorderColor = Color.FromHex("#1089E3"),
                        Content = new Grids.BaconMenuItem(baconMenuItem).Grid,
                        Padding = 2,
                        StyleId = baconMenuItem.Name
                    });
                }

                // Tap gesture recognizer.  --Kris
                if (!BaconMenuTaps.ContainsKey(baconMenuItem.Name))
                {
                    BaconMenuTaps.Add(baconMenuItem.Name, new TapGestureRecognizer());
                    BaconMenuTaps[baconMenuItem.Name].Tapped += BaconMenu_FrameTapped;
                }

                BaconMenuFrames[baconMenuItem.Name].GestureRecognizers.Add(BaconMenuTaps[baconMenuItem.Name]);

                StackLayout_BaconMenu.Children.Add(BaconMenuFrames[baconMenuItem.Name]);
            }
        }

        public void LoadSub(string subreddit)
        {
            if (subreddit.Equals("Front Page"))
            {
                subreddit = "";
            }

            Subreddit = Reddit.Subreddit(subreddit);
            ToolbarItem_Subreddits.Text = (!string.IsNullOrWhiteSpace(subreddit)
                ? (Subreddit.Name.Length <= MaxSubNameLength ? Subreddit.Name : Subreddit.Name.Substring(0, (MaxSubNameLength - 3)) + "...")
                : "Front Page");
            
            ToolbarItem_Spacer.Text = (Subreddit.Name.Length <= 10 ? "     " : " ");
            RefreshToolbar();
            LoadSort();
        }

        private void LoadSort(string sort, bool update = true)
        {
            if (update)
            {
                Sort = sort;
                ButtonSort.Text = "Sort: " + Sort;
            }

            PopulatePosts(forceRefresh: true);
        }

        private void LoadSort()
        {
            LoadSort(Sort, false);
        }

        public void Refresh()
        {
            RefreshToolbar();
            PopulatePosts();
        }

        private IList<Post> GetPosts()
        {
            // TODO - Use caching code once Reddit.NET supports setting params (like after and limit) for these.  --Kris
            return GetPosts(null);
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
                    return Subreddit.Posts.GetHot(after: after, limit: 10);
                case "new":
                    return Subreddit.Posts.GetNew(after: after, limit: 10);
                case "rising":
                    return Subreddit.Posts.GetRising(after: after, limit: 10);
                case "top":
                    return Subreddit.Posts.GetTop(after: after, limit: 10);
                case "controversial":
                    return Subreddit.Posts.GetControversial(after: after, limit: 10);
            }
        }

        private void PopulatePosts(string after = null, bool forceRefresh = false, bool append = false)
        {
            if (!append)
            {
                StackLayout_Posts.Children.Clear();

                UpArrows.Clear();
                UpArrowTaps.Clear();
                DownArrows.Clear();
                DownArrowTaps.Clear();

                Frames.Clear();
                FrameLeftSwipes.Clear();
                FrameRightSwipes.Clear();

                ScrollView_Main.ScrollToAsync(0, 0, false);
            }

            try
            {
                foreach (Post post in (string.IsNullOrWhiteSpace(after) && !forceRefresh ? GetPosts() : GetPosts(after)))
                {
                    if (!Frames.ContainsKey(post.Fullname))
                    {
                        Frames.Add(post.Fullname, new Frame
                        {
                            HasShadow = true,
                            Content = new Grids.Post(Reddit, post, ref UpArrows, ref ScoreLabels, ref DownArrows, showUserFlair: false, 
                                showSub: (string.IsNullOrWhiteSpace(Subreddit.Name) || Subreddit.Name.Equals("all", StringComparison.OrdinalIgnoreCase))).Grid,
                            Padding = 2,
                            StyleId = post.Fullname
                        });
                    }

                    if (!FrameLeftSwipes.ContainsKey(post.Fullname))
                    {
                        FrameLeftSwipes.Add(post.Fullname, new SwipeGestureRecognizer { Direction = SwipeDirection.Left });
                        FrameLeftSwipes[post.Fullname].Swiped += FrameSwiped_Left;
                    }

                    if (!FrameRightSwipes.ContainsKey(post.Fullname))
                    {
                        FrameRightSwipes.Add(post.Fullname, new SwipeGestureRecognizer { Direction = SwipeDirection.Right });
                        FrameRightSwipes[post.Fullname].Swiped += FrameSwiped_Right;
                    }

                    Frames[post.Fullname].GestureRecognizers.Add(FrameLeftSwipes[post.Fullname]);
                    Frames[post.Fullname].GestureRecognizers.Add(FrameRightSwipes[post.Fullname]);

                    StackLayout_Posts.Children.Add(Frames[post.Fullname]);

                    After = post.Fullname;
                }
            }
            catch (RedditForbiddenException)
            {
                StackLayout_Posts.Children.Add(new Frame
                {
                    HasShadow = true,
                    Content = new Label { Text = "The requested subreddit does not exist or has been set to private." },
                    Padding = 2
                });
            }

            // Add tap gesture recognizers to upvote and downvote icons.  --Kris
            foreach (KeyValuePair<string, Image> pair in UpArrows)
            {
                if (!UpArrowTaps.ContainsKey(pair.Key))
                {
                    UpArrowTaps.Add(pair.Key, new TapGestureRecognizer());
                    UpArrowTaps[pair.Key].Tapped += ImageUpvote_Clicked;
                    pair.Value.GestureRecognizers.Add(UpArrowTaps[pair.Key]);

                    DownArrowTaps.Add(pair.Key, new TapGestureRecognizer());
                    DownArrowTaps[pair.Key].Tapped += ImageDownvote_Clicked;
                    DownArrows[pair.Key].GestureRecognizers.Add(DownArrowTaps[pair.Key]);
                }
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
            ToolbarItem_Mail.IconImageSource = (Reddit.Account.Messages.Unread.Count.Equals(0) ? "mail" : "mailnewmessages");

            // Populate the subreddits listview with the user's subscriptions.  --Kris
            List<string> subs = new List<string>();
            subs.Add(null);
            foreach (Subreddit subreddit in Subscriptions)
            {
                subs.Add(subreddit.Name);
            }
            subs.Sort();

            subs[0] = "Front Page";

            ListView_Subreddits.ItemsSource = subs.ToArray();

            // Populate the sort listview.  --Kris
            ListView_Sort.ItemsSource = new string[5]
            {
                "Hot",
                "New",
                "Rising",
                "Controversial",
                "Top"
            };

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
                ToolbarItems.Add(new ToolbarItem("refresh", "refresh.png", () =>
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

        private void ClearPopups(string skip = "")
        {
            if (string.IsNullOrEmpty(skip))
            {
                skip = "";
            }

            if (!skip.Equals("BaconButton"))
            {
                Popup_BaconButton.IsVisible = false;
            }
            if (!skip.Equals("SubredditEntry"))
            {
                Popup_SubredditEntry.IsVisible = false;
            }
            if (!skip.Equals("Subreddits"))
            {
                Popup_Subreddits.IsVisible = false;
            }
            if (!skip.Equals("Sort"))
            {
                Popup_Sort.IsVisible = false;
            }
        }

        private void ToolbarItemSubreddits_Clicked(object sender, EventArgs e)
        {
            ClearPopups("Subreddits");
            Popup_Subreddits.IsVisible = !Popup_Subreddits.IsVisible;
        }

        private void ToolbarItemMail_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        private void ToolbarItemRefresh_Clicked(object sender, EventArgs e)
        {
            Refresh();
        }

        private void ToolbarItemSearch_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        private void ToolbarItemLoadSub_Clicked(object sender, EventArgs e)
        {
            ClearPopups("SubredditEntry");
            Popup_SubredditEntry.IsVisible = !Popup_SubredditEntry.IsVisible;
        }

        private void ToolbarItemBaconButton_Clicked(object sender, EventArgs e)
        {
            ClearPopups("BaconButton");
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

        private void FrameSwiped_Right(object sender, EventArgs e)
        {
            Upvote(Subreddit.Post(((Frame)sender).StyleId).About());
        }

        private void ImageUpvote_Clicked(object sender, EventArgs e)
        {
            Upvote(Subreddit.Post(((Image)sender).StyleId).About());
        }

        private void Upvote(Post post)
        {
            if (!post.Listing.Likes.HasValue || !post.Listing.Likes.Value)
            {
                try
                {
                    post.Upvote();
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    post.Unvote();
                }
                catch (Exception) { }
            }

            UpdateVotingGrid(post.About());
        }

        private void FrameSwiped_Left(object sender, EventArgs e)
        {
            Downvote(Subreddit.Post(((Frame)sender).StyleId).About());
        }

        private void ImageDownvote_Clicked(object sender, EventArgs e)
        {
            Downvote(Subreddit.Post(((Image)sender).StyleId).About());
        }

        private void Downvote(Post post)
        {
            if (!post.Listing.Likes.HasValue || post.Listing.Likes.Value)
            {
                try
                {
                    post.Downvote();
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    post.Unvote();
                }
                catch (Exception) { }
            }

            UpdateVotingGrid(post.About());
        }

        private void UpdateVotingGrid(Post post)
        {
            UpArrows[post.Fullname].Source = (post.Listing.Likes.HasValue && post.Listing.Likes.Value ? "upvoteselected.png" : "upvote.png");
            ScoreLabels[post.Fullname].Text = (post.Score < 1000
                            ? post.Score.ToString()
                            : Math.Round(((double)post.Score / 1000), 1).ToString() + "K");
            ScoreLabels[post.Fullname].TextColor = (!post.Listing.Likes.HasValue
                ? Color.Black
                : (post.Listing.Likes.Value ? Color.FromHex("#FF4500") : Color.FromHex("#7193FF")));
            DownArrows[post.Fullname].Source = (post.Listing.Likes.HasValue && !post.Listing.Likes.Value ? "downvoteselected.png" : "downvote.png");
        }

        private void ButtonSort_Clicked(object sender, EventArgs e)
        {
            ClearPopups("Sort");
            Popup_Sort.IsVisible = !Popup_Sort.IsVisible;
        }

        private void ButtonSubredditGo_Clicked(object sender, EventArgs e)
        {
            LoadSub(Entry_Subreddit.Text);
            Popup_SubredditEntry.IsVisible = false;
        }

        private void ListView_Subreddits_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            LoadSub(((ListView)sender).SelectedItem.ToString());
            Popup_Subreddits.IsVisible = false;
        }

        private void ListView_Sort_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            LoadSort(((ListView)sender).SelectedItem.ToString());
            Popup_Sort.IsVisible = false;
        }

        private void Popup_Subreddits_OutClick(object sender, EventArgs e)
        {
            Popup_Subreddits.IsVisible = false;
        }

        private void Popup_Sort_OutClick(object sender, EventArgs e)
        {
            Popup_Sort.IsVisible = false;
        }

        private void Popup_SubredditEntry_OutClick(object sender, EventArgs e)
        {
            Popup_SubredditEntry.IsVisible = false;
        }

        private void BaconMenu_FrameTapped(object sender, EventArgs e)
        {
            switch (((Frame)sender).StyleId)
            {
                default:
                    throw new Exception("Unrecognized menu item : " + ((Frame)sender).StyleId);
                case "Your Profile":
                case "Connect Account":
                    // TODO
                    break;
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

            Popup_BaconButton.IsVisible = false;
        }

        private void Popup_BaconButton_OutClick(object sender, EventArgs e)
        {
            Popup_BaconButton.IsVisible = false;
        }

        private void OnScrolled(object sender, ScrolledEventArgs e)
        {
            // User has scrolled to the bottom, so load more posts.  --Kris
            if (e.ScrollY >= (ScrollView_Main.ContentSize.Height - ScrollView_Main.Height))
            {
                PopulatePosts(after: After, append: true);
            }
        }
    }
}
