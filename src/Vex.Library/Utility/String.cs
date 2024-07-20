using System;
using System.IO;

namespace Vex.Library
{
    public static class Util
    {
        public static string DurationToReadableTime(TimeSpan duration)
        {
            if (duration.TotalMilliseconds < 1)
            {
                return "N/A";
            }
            // Check for small durations
            else if (duration.TotalMilliseconds < 1000)
            {
                // Only use milliseconds
                return $"{(int)duration.TotalMilliseconds}ms";
            }

            // Otherwise convert from time 
            var hours = (int)duration.TotalHours;
            duration -= TimeSpan.FromHours(hours);

            var minutes = (int)duration.TotalMinutes;
            duration -= TimeSpan.FromMinutes(minutes);

            var seconds = (int)duration.TotalSeconds;
            _ = TimeSpan.FromSeconds(seconds);

            // Check positive values and format
            var hoursFmt = (hours > 0) ? $"{hours}h " : "";
            var minutesFmt = (minutes > 0) ? $"{minutes}m " : "";
            var secondsFmt = (seconds > 0) ? $"{seconds}s " : "";

            // Return it
            return $"{hoursFmt}{minutesFmt}{secondsFmt}";
        }

        public static string GetFileNamePurgeExtensions(string path)
        {
            if (path == null)
                return null;

            ReadOnlySpan<char> result = GetFileNamePurgeExtensions(path.AsSpan());
            if (path.Length == result.Length)
                return path;

            return result.ToString();
        }

        /// <summary>
        /// Returns the characters between the last separator and last (.) in the path.
        /// </summary>
        public static ReadOnlySpan<char> GetFileNamePurgeExtensions(ReadOnlySpan<char> path)
        {
            ReadOnlySpan<char> fileName = Path.GetFileName(path);
            int lastPeriod = fileName.IndexOf('.');
            return lastPeriod < 0 ?
                fileName : // No extension was found
                fileName[..lastPeriod];
        }
    }
}
