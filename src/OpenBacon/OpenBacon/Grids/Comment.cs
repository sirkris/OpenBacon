using Reddit;
using Controllers = Reddit.Controllers;
using System;
using System.Collections.Generic;
using Xam.Forms.Markdown;
using Xamarin.Forms;

namespace OpenBacon.Grids
{
    public class Comment
    {
        public Grid Grid { get; private set; }

        public Comment(RedditAPI reddit, Controllers.Post post, Controllers.Comment comment, bool loadUser = false, bool showUserFlair = true)
        {
            Grid = new Grid { Padding = 0 };

            Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5, GridUnitType.Auto) });

            Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Controllers.User user = null;
            if (loadUser
                && (string.IsNullOrWhiteSpace(post.Author)
                    || !post.Author.Equals("[deleted]", StringComparison.OrdinalIgnoreCase)))
            {
                user = UsersCache.GetUser(reddit, post.Author, (!string.IsNullOrWhiteSpace(post.Subreddit) ? post.Subreddit : null));
            }

            Grid metaGrid = new Grid { Padding = 0, VerticalOptions = LayoutOptions.Start, ColumnSpacing = 2 };

            metaGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            metaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            metaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            Label author = new Label
            {
                Text = (!string.IsNullOrWhiteSpace(comment.Author) 
                    ? (comment.Author.Equals(post.Author) && !comment.Author.Equals("[deleted]") ? "[S] " : "") + comment.Author 
                    : "[deleted]"),
                FontAttributes = (!string.IsNullOrWhiteSpace(comment.Author) && !comment.Author.Equals("[deleted]") ? FontAttributes.Bold : FontAttributes.None),
                TextColor = (comment.Author.Equals(post.Author) ? Color.Blue : Color.FromHex("#369")),
                FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Margin = 0
            };

            if (showUserFlair && !string.IsNullOrWhiteSpace(comment.Listing.AuthorFlairText))
            {
                Grid authorGrid = new Grid
                {
                    Padding = 0,
                    VerticalOptions = LayoutOptions.Start,
                    ColumnSpacing = 2,
                    RowDefinitions = new RowDefinitionCollection
                        {
                            new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }
                        },
                    ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }
                        }
                };

                authorGrid.Children.Add(author, 0, 0);
                authorGrid.Children.Add(
                    new Label
                    {
                        Text = comment.Listing.AuthorFlairText,
                        TextColor = Color.FromHex("#555"),
                        BackgroundColor = Color.FromHex("#F5F5F5"),
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                        VerticalOptions = LayoutOptions.Start,
                        Margin = 0
                    }, 1, 0);

                metaGrid.Children.Add(authorGrid, 0, 0);
            }
            else
            {
                metaGrid.Children.Add(author, 0, 0);
            }

            metaGrid.Children.Add(
                new Label
                {
                    Text = comment.Score.ToString() + " points " + Common.GetDateTimeSpan(comment.Created) + " ago",
                    TextColor = Color.FromHex("#888"),
                    FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End,
                    Margin = 0
                }, 1, 0);

            Grid.Children.Add(metaGrid, 0, 0);

            /*Grid.Children.Add(
                new AutoWebView
                {
                    Source = new HtmlWebViewSource
                    {
                        Html = "<html><body>" + comment.BodyHTML + "</body></html>"
                    }, 
                    VerticalOptions = LayoutOptions.StartAndExpand
                }, 0, 1);*/

            Grid.Children.Add(
                new MarkdownView
                {
                    Markdown = comment.Body
                }, 0, 1);
        }
    }
}
