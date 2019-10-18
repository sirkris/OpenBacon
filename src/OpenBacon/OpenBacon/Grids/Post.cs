using Reddit;
using Controllers = Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OpenBacon.Grids
{
    public class Post
    {
        public Grid Grid { get; private set; }
        
        public Post(RedditAPI reddit, Controllers.Post post, ref IDictionary<string, Image> upArrows, ref IDictionary<string, Label> scoreLabels, 
            ref IDictionary<string, Image> downArrows, bool loadUser = false, bool showSub = false, bool showUserFlair = true)
        {
            Grid = new Grid { Padding = 0 };

            Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            //Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            //Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(7, GridUnitType.Star) });

            Controllers.User user = null;
            if (loadUser
                && (string.IsNullOrWhiteSpace(post.Author)
                    || !post.Author.Equals("[deleted]", StringComparison.OrdinalIgnoreCase)))
            {
                user = UsersCache.GetUser(reddit, post.Author, (!string.IsNullOrWhiteSpace(post.Subreddit) ? post.Subreddit : null));
            }

            Grid votingGrid = new Grid { Padding = 0, VerticalOptions = LayoutOptions.StartAndExpand };
            votingGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            votingGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            votingGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            votingGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            if (!upArrows.ContainsKey(post.Fullname))
            {
                upArrows.Add(post.Fullname, new Image
                {
                    Source = (!post.Listing.Likes.HasValue || !post.Listing.Likes.Value ? "upvote" : "upvoteselected") + ".png",
                    VerticalOptions = LayoutOptions.End,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = 2,
                    StyleId = post.Fullname
                });
            }
            votingGrid.Children.Add(upArrows[post.Fullname], 0, 0);

            if (!scoreLabels.ContainsKey(post.Fullname))
            {
                scoreLabels.Add(post.Fullname, new Label
                {
                    Text = (post.Score < 1000
                            ? post.Score.ToString()
                            : Math.Round(((double)post.Score / 1000), 1).ToString() + "K"),
                    TextColor = (!post.Listing.Likes.HasValue
                            ? Color.Black
                            : (post.Listing.Likes.Value ? Color.FromHex("#FF4500") : Color.FromHex("#7193FF"))),
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = 2
                });
            }
            votingGrid.Children.Add(scoreLabels[post.Fullname], 0, 1);

            if (!downArrows.ContainsKey(post.Fullname))
            {
                downArrows.Add(post.Fullname, new Image
                {
                    Source = (!post.Listing.Likes.HasValue || post.Listing.Likes.Value ? "downvote" : "downvoteselected") + ".png",
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = 2,
                    StyleId = post.Fullname
                });
            }
            votingGrid.Children.Add(downArrows[post.Fullname], 0, 2);

            Grid.Children.Add(votingGrid, 0, 1, 0, (post.Listing.Approved && !string.IsNullOrWhiteSpace(post.Listing.ApprovedBy) ? 4 : 3));

            Grid.Children.Add(
                new Image
                {
                    Source = post.Listing.Thumbnail, 
                    VerticalOptions = LayoutOptions.Start,
                    Margin = 5
                }, 1, 2, 0, (post.Listing.Approved && !string.IsNullOrWhiteSpace(post.Listing.ApprovedBy) ? 4 : 3));

            Grid.Children.Add(
                new Label
                {
                    Text = (post.Title.Length <= 150 ? post.Title : post.Title.Substring(0, 147) + "..."),
                    FontAttributes = FontAttributes.Bold,
                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                    VerticalOptions = LayoutOptions.StartAndExpand
                }, 2, 0);
            
            Grid authorGrid = new Grid { Padding = 0 };
            authorGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            authorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            authorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            authorGrid.Children.Add(
                new Label
                {
                    Text = "submitted " + GetDateTimeSpan(post.Created) + " ago" + (showSub ? " to r/" + post.Subreddit : " by " + post.Author),
                    TextColor = Color.FromHex("#888"),
                    FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                    VerticalOptions = LayoutOptions.Start,
                    Margin = 0
                }, 0, 0);

            if (!string.IsNullOrWhiteSpace(post.Listing.Domain))
            {
                authorGrid.Children.Add(
                    new Label
                    {
                        Text = "(" + post.Listing.Domain + ")",
                        TextColor = Color.FromHex("#888"),
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                        VerticalOptions = LayoutOptions.Start,
                        Margin = 0
                    }, 1, 0);
            }

            if (!showSub 
                && showUserFlair
                && !string.IsNullOrWhiteSpace(post.Listing.AuthorFlairText))
            {
                authorGrid.Children.Add(
                    new Label
                    {
                        Text = post.Listing.AuthorFlairText,
                        TextColor = Color.FromHex("#555"),
                        BackgroundColor = Color.FromHex("#F5F5F5"),
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                        VerticalOptions = LayoutOptions.Start,
                        Margin = 0
                    }, 2, 0);
            }

            int currentRow = 1;

            Grid.Children.Add(authorGrid, 2, currentRow);
            currentRow++;
            
            if (post.Listing.Approved && !string.IsNullOrWhiteSpace(post.Listing.ApprovedBy))
            {
                Grid.Children.Add(
                    new Label
                    {
                        Text = "approved by " + post.Listing.ApprovedBy + " at " + post.Listing.ApprovedAtUTC.ToLocalTime().ToString("g"),
                        TextColor = Color.FromHex("#282"),
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                        VerticalOptions = LayoutOptions.Start,
                        Margin = 0
                    }, 2, currentRow);
                currentRow++;
            }

            Grid.Children.Add(
                new Label
                {
                    Text = post.Listing.NumComments.ToString() + " comments",
                    TextColor = Color.FromHex("#888"),
                    FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.End,
                    Margin = 0
                }, 2, currentRow);
        }

        private string GetDateTimeSpan(DateTime posted)
        {
            if (posted.AddMinutes(5) > DateTime.UtcNow)
            {
                return "moments";
            }
            else if (posted.AddHours(1) > DateTime.UtcNow)
            {
                int diff = ((int)(DateTime.UtcNow - posted).TotalMinutes);
                return diff.ToString() + " minute" + (!diff.Equals(1) ? "s" : "");
            }
            else if (posted.AddDays(1) > DateTime.UtcNow)
            {
                int diff = ((int)(DateTime.UtcNow - posted).TotalHours);
                return diff.ToString() + " hour" + (!diff.Equals(1) ? "s" : "");
            }
            else
            {
                int diff = ((int)(DateTime.UtcNow - posted).TotalDays);
                return diff.ToString() + " day" + (!diff.Equals(1) ? "s" : "");
            }
        }
    }
}
