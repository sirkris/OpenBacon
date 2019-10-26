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
    }
}
