using System;

namespace RDAExplorer.Misc
{
    public static class DateTimeExtension
    {
        public static DateTime FromTimeStamp(ulong timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
        }

        public static ulong ToTimeStamp(this DateTime dateTime)
        {
            return (ulong)(TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
