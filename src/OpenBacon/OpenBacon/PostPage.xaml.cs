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
            // TODO
        }
    }
}
