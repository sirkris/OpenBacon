using Reddit;
using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBacon
{
    public static class Common
    {
        public static string GetDateTimeSpan(DateTime posted)
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

        public static string UpdateCommentStats(Comment comment, int? scoreMod = null)
        {
            return (scoreMod ?? comment.Score).ToString() + " point" + (!comment.Score.Equals(1) ? "s" : "") 
                + " " + GetDateTimeSpan(comment.Created) + " ago";
        }

        public static bool Upvote(Post post)
        {
            if (!post.Listing.Likes.HasValue || !post.Listing.Likes.Value)
            {
                try
                {
                    post.Upvote();
                }
                catch (Exception) { }

                return true;
            }
            else
            {
                try
                {
                    post.Unvote();
                }
                catch (Exception) { }

                return false;
            }
        }

        public static bool Upvote(Comment comment)
        {
            if (!comment.Listing.Likes.HasValue || !comment.Listing.Likes.Value)
            {
                try
                {
                    comment.Upvote();
                }
                catch (Exception) { }

                return true;
            }
            else
            {
                try
                {
                    comment.Unvote();
                }
                catch (Exception) { }

                return false;
            }
        }

        public static bool Downvote(Post post)
        {
            if (!post.Listing.Likes.HasValue || post.Listing.Likes.Value)
            {
                try
                {
                    post.Downvote();
                }
                catch (Exception) { }

                return true;
            }
            else
            {
                try
                {
                    post.Unvote();
                }
                catch (Exception) { }
            }

            return false;
        }

        public static bool Downvote(Comment comment)
        {
            if (!comment.Listing.Likes.HasValue || comment.Listing.Likes.Value)
            {
                try
                {
                    comment.Downvote();
                }
                catch (Exception) { }

                return true;
            }
            else
            {
                try
                {
                    comment.Unvote();
                }
                catch (Exception) { }
            }

            return false;
        }
    }
}
