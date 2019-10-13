using Reddit;
using Reddit.Controllers;
using Things = Reddit.Things;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBacon
{
    public static class UsersCache
    {
        private static Dictionary<string, User> Users { get; set; } = new Dictionary<string, User>();
        private static Dictionary<string, DateTime?> LastUpdated { get; set; } = new Dictionary<string, DateTime?>();

        public static User GetUser(RedditAPI reddit, string username, string subreddit, bool forceRefresh = false)
        {
            if (!Users.ContainsKey(username))
            {
                Users.Add(username, null);
                LastUpdated.Add(username, null);
            }

            if (forceRefresh || !LastUpdated[username].HasValue || LastUpdated[username].Value.AddMinutes(15) < DateTime.UtcNow)
            {
                Users[username] = reddit.User(username).About();
                
                LastUpdated[username] = DateTime.UtcNow;
            }

            return Users[username];
        }

        public static User GetUser(RedditAPI reddit, string username, bool forceRefresh = false)
        {
            return GetUser(reddit, username, null, forceRefresh);
        }
    }
}
