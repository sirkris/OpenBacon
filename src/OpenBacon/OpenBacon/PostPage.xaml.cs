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
            Label_Title.Text = Post.Title;
            if (Post.Listing.IsSelf)
            {
                WebView_Preview.Source = new HtmlWebViewSource { Html = (new SelfPost(Reddit.Models, Post.Listing)).SelfTextHTML };
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
