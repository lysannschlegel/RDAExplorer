using System;

namespace RDAExplorer.Misc
{
    public class DateTimeExtension
    {
        public static DateTime FromTimeStamp(int timestamp)
        {
            return new DateTime(1970, 1, 1).AddSeconds(timestamp);
        }
    }
}
