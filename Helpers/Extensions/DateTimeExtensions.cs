using System;

namespace JiraApp.Helpers.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToFriendlyDate(this DateTime dateTime)
        {
            var now = DateTime.Now;
            var diff = now - dateTime;

            if (diff.TotalDays < 1)
            {
                if (diff.TotalHours < 1)
                {
                    if (diff.TotalMinutes < 1)
                    {
                        return "just now";
                    }
                    else
                    {
                        return $"{(int)diff.TotalMinutes} minute{((int)diff.TotalMinutes == 1 ? "" : "s")} ago";
                    }
                }
                else
                {
                    return $"{(int)diff.TotalHours} hour{((int)diff.TotalHours == 1 ? "" : "s")} ago";
                }
            }
            else if (diff.TotalDays < 7)
            {
                return $"{(int)diff.TotalDays} day{((int)diff.TotalDays == 1 ? "" : "s")} ago";
            }
            else if (diff.TotalDays < 30)
            {
                var weeks = (int)(diff.TotalDays / 7);
                return $"{weeks} week{(weeks == 1 ? "" : "s")} ago";
            }
            else if (diff.TotalDays < 365)
            {
                var months = (int)(diff.TotalDays / 30);
                return $"{months} month{(months == 1 ? "" : "s")} ago";
            }
            else
            {
                var years = (int)(diff.TotalDays / 365);
                return $"{years} year{(years == 1 ? "" : "s")} ago";
            }
        }

        public static string ToRemainingTime(this DateTime dueDate)
        {
            var now = DateTime.Now;
            var diff = dueDate - now;

            if (diff.TotalDays < 0)
            {
                return "Overdue";
            }
            else if (diff.TotalDays < 1)
            {
                if (diff.TotalHours < 1)
                {
                    return "Due soon";
                }
                else
                {
                    return $"{(int)diff.TotalHours} hour{((int)diff.TotalHours == 1 ? "" : "s")} left";
                }
            }
            else if (diff.TotalDays < 7)
            {
                return $"{(int)diff.TotalDays} day{((int)diff.TotalDays == 1 ? "" : "s")} left";
            }
            else if (diff.TotalDays < 30)
            {
                var weeks = (int)(diff.TotalDays / 7);
                return $"{weeks} week{(weeks == 1 ? "" : "s")} left";
            }
            else
            {
                var months = (int)(diff.TotalDays / 30);
                return $"{months} month{(months == 1 ? "" : "s")} left";
            }
        }
    }
}
