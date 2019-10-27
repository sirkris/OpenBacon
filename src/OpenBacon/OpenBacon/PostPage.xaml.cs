using Reddit;
using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OpenBacon
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PostPage : ContentPage
    {
        private RedditAPI Reddit { get; set; }
        private Subreddit Subreddit { get; set; }
        private Post Post { get; set; }

        public string Sort { get; private set; } = "Top";

        public PostPage(RedditAPI reddit, Subreddit subreddit, Post post)
        {
            InitializeComponent();

            Reddit = reddit;
            Subreddit = subreddit;
            Post = post;

            RefreshToolbar();
            RefreshPage();
        }

        private void RefreshToolbar()
        {
            Post = Post.About();

            if ((Post.UpVotes + Post.DownVotes) > 0)
            {
                ToolbarItem_Score.Text = Post.Score.ToString("N0");
            }
            else
            {
                ToolbarItem_Score.Text = "No Score";
            }

            // TODO - Uncomment once the Reddit.NET UpvoteRatio bug is fixed (Issue #92).  --Kris
            //ToolbarItem_Score.Text += " (" + (Post.Listing.UpvoteRatio * 100).ToString() + "%)";
            
            ToolbarItem_Upvote.IconImageSource = (Post.Listing.Likes.HasValue && Post.Listing.Likes.Value ? "upvoteselected" : "upvote");
            ToolbarItem_Downvote.IconImageSource = (Post.Listing.Likes.HasValue && !Post.Listing.Likes.Value ? "downvoteselected" : "downvote");

            // Populate the sort listview.  --Kris
            ListView_Sort.ItemsSource = new string[5]
            {
                "Top",
                "New",
                "Controversial",
                "Old",
                "Q&A"
            };

            LoadSort(true);
        }

        private void LoadSort(string sort, bool update = true)
        {
            if (update)
            {
                Sort = (!sort.Equals("Q&A", StringComparison.OrdinalIgnoreCase ) ? "qa" : Sort);
                ButtonSort.Text = sort;

                ButtonSort.FontSize = (sort.Equals("Controversial", StringComparison.OrdinalIgnoreCase)
                    ? Device.GetNamedSize(NamedSize.Micro, typeof(Button))
                    : Device.GetNamedSize(NamedSize.Small, typeof(Button)));
            }

            PopulateComments(forceRefresh: true);
        }

        private void LoadSort(bool update = false)
        {
            LoadSort(Sort, update);
        }

        private List<Comment> GetComments(Comment parent = null)
        {
            switch (Sort.ToLower())
            {
                default:
                    throw new Exception("Unrecognized sort : " + Sort);
                case "top":
                    return (parent == null ? Post.Comments.Top : parent.Comments.Top);
                case "new":
                    return (parent == null ? Post.Comments.Top : parent.Comments.New);
                case "controversial":
                    return (parent == null ? Post.Comments.Top : parent.Comments.Controversial);
                case "old":
                    return (parent == null ? Post.Comments.Top : parent.Comments.Old);
                case "qa":
                    return (parent == null ? Post.Comments.Top : parent.Comments.QA);
            }
        }

        private void PopulateComments(bool append = false)
        {
            if (!append)
            {
                StackLayout_Comments.Children.Clear();
            }

            foreach (Comment comment in GetComments())
            {
                PopulateCommentTree(comment);
            }
        }

        private void PopulateCommentTree(Comment comment, bool recurse = true, int depth = 0)
        {
            // TODO - Display comment and indent as needed.  --Kris

            // Load any child comments or display link to load more.  --Kris
            if (recurse)
            {
                foreach (Comment reply in GetComments(comment))
                {
                    PopulateCommentTree(reply, false, (depth + 1));
                }
            }
            else if (!GetComments(comment).Count.Equals(0))
            {
                // TODO - Display load more comments link.  --Kris
            }
        }

        private void Upvote()
        {
            if (!Post.Listing.Likes.HasValue || !Post.Listing.Likes.Value)
            {
                try
                {
                    Post.Upvote();
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    Post.Unvote();
                }
                catch (Exception) { }
            }

            RefreshToolbar();
        }

        private void Downvote()
        {
            if (!Post.Listing.Likes.HasValue || Post.Listing.Likes.Value)
            {
                try
                {
                    Post.Downvote();
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    Post.Unvote();
                }
                catch (Exception) { }
            }

            RefreshToolbar();
        }

        private void RefreshPage()
        {
            // Title
            if (!string.IsNullOrWhiteSpace(Post.Listing.LinkFlairText))
            {
                Label_PostFlair.Text = Post.Listing.LinkFlairText;
            }

            Label_Title.Text = Post.Title;

            // Metadata
            Grid metaGrid = new Grid { Padding = 0, RowSpacing = 0, ColumnSpacing = 0, Margin = 0 };
            metaGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            metaGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            metaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            metaGrid.Children.Add(
                new Label
                {
                    Text = "submitted at " + Post.Created.ToLocalTime().ToString("g") + " (" + Common.GetDateTimeSpan(Post.Created) + " ago)",
                    TextColor = Color.FromHex("#888"),
                    FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                    VerticalOptions = LayoutOptions.Start,
                    Margin = 0
                }, 0, 0);

            metaGrid.Children.Add(
                new Label
                {
                    Text = " to r/" + Post.Subreddit,
                    TextColor = Color.FromHex("#888"),
                    FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                    VerticalOptions = LayoutOptions.Start,
                    Margin = 0
                }, 0, 1);


            if (!string.IsNullOrWhiteSpace(Post.Listing.AuthorFlairText))
            {
                Grid authorGrid = new Grid { Padding = 0, RowSpacing = 0, ColumnSpacing = 2, Margin = 0 };
                authorGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                authorGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                authorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

                authorGrid.Children.Add(
                    new Label
                    {
                        Text = " by " + Post.Author,
                        TextColor = Color.FromHex("#888"),
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                        VerticalOptions = LayoutOptions.Start,
                        Margin = 0
                    }, 0, 0);

                authorGrid.Children.Add(
                    new Label
                    {
                        Text = Post.Listing.AuthorFlairText,
                        TextColor = Color.FromHex("#555"),
                        BackgroundColor = Color.FromHex("#F5F5F5"),
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                        VerticalOptions = LayoutOptions.Start,
                        Margin = 0
                    }, 1, 0);

                metaGrid.Children.Add(authorGrid, 0, 2);
            }
            else
            {
                metaGrid.Children.Add(
                    new Label
                    {
                        Text = " by " + Post.Author,
                        TextColor = Color.FromHex("#888"),
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                        VerticalOptions = LayoutOptions.Start,
                        Margin = 0
                    }, 0, 2);
            }

            if (!string.IsNullOrWhiteSpace(Post.Listing.Domain) 
                || !Post.Listing.IsSelf)
            {
                LinkPost linkPost = null;
                if (!Post.Listing.IsSelf)
                {
                    linkPost = new LinkPost(Reddit.Models, Post.Listing);
                }
                
                metaGrid.Children.Add(
                    new Label
                    {
                        Text = (!Post.Listing.IsSelf 
                                ? "URL: " + (linkPost.URL.Length >= 60 ? linkPost.URL.Substring(0, 57) + "..." : linkPost.URL) + " "
                                : "") 
                            + (!string.IsNullOrWhiteSpace(Post.Listing.Domain) 
                                ? "(" + Post.Listing.Domain + ")" 
                                : ""),
                        TextColor = Color.FromHex("#888"),
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                        VerticalOptions = LayoutOptions.Start,
                        Margin = 0
                    }, 0, 3);
            }

            StackLayout_MetaData.Children.Add(metaGrid);

            // Preview
            if (Post.Listing.IsSelf)
            {
                WebView_Preview.Source = new HtmlWebViewSource
                {
                    Html = "<div style=\"font-family: Verdana; color: #222; background-color: #FAFAFA; font-size: 1em;\">"
                        + (new SelfPost(Reddit.Models, Post.Listing)).SelfTextHTML
                        + "</div>"
                };
            }
            else
            {
                string url = (new LinkPost(Reddit.Models, Post.Listing)).URL;
                if (url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                    || url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                    || url.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                    || url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                    || url.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)
                    || url.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    StackLayout_Preview.Children.Clear();
                    StackLayout_Preview.Children.Add(
                        new Image
                        {
                            Source = url,
                            VerticalOptions = LayoutOptions.StartAndExpand,
                            Margin = 0
                        }
                    );
                }
                // YouTube videos don't render correctly in WebView on Android.  --Kris
                else if (Post.Listing.Domain.Equals("v.redd.it", StringComparison.OrdinalIgnoreCase)
                    || Post.Listing.Domain.Equals("youtu.be", StringComparison.OrdinalIgnoreCase)
                    || Post.Listing.Domain.Equals("youtube.com", StringComparison.OrdinalIgnoreCase))
                {
                    StackLayout_Preview.Children.Clear();
                    StackLayout_Preview.Children.Add(
                        new Image
                        {
                            Source = (new LinkPost(Reddit.Models, Post.Listing)).Thumbnail,
                            VerticalOptions = LayoutOptions.StartAndExpand,
                            Margin = 0
                        }
                    );
                }
                else
                {
                    WebView_Preview.Source = new UrlWebViewSource { Url = url };
                }
            }
        }

        private void ClearPopups(string skip = "")
        {
            if (string.IsNullOrEmpty(skip))
            {
                skip = "";
            }

            if (!skip.Equals("Sort"))
            {
                Popup_Sort.IsVisible = false;
            }
        }

        private void ToolbarItemUpvote_Clicked(object sender, EventArgs e)
        {
            Upvote();
        }

        private void ToolbarItemDownvote_Clicked(object sender, EventArgs e)
        {
            Downvote();
        }

        private void ToolbarItemComments_Clicked(object sender, EventArgs e)
        {
            ScrollView_Post.ScrollToAsync(0, Frame_Comments.Y, true);
        }

        private void ToolbarItemNewComment_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        private void ButtonSort_Clicked(object sender, EventArgs e)
        {
            ClearPopups("Sort");
            Popup_Sort.IsVisible = !Popup_Sort.IsVisible;
        }

        private void ListView_Sort_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            LoadSort(((ListView)sender).SelectedItem.ToString());
            Popup_Sort.IsVisible = false;
        }

        private void Popup_Sort_OutClick(object sender, EventArgs e)
        {
            Popup_Sort.IsVisible = false;
        }
    }
}
