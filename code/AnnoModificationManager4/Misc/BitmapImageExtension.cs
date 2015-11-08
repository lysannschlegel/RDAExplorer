using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace AnnoModificationManager4.Misc
{
    public class BitmapImageExtension
    {
        private static Dictionary<string, BitmapImage> Cache = new Dictionary<string, BitmapImage>();

        public static BitmapImage Load(string file)
        {
            if (Cache.ContainsKey(file))
                return Cache[file];
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(file);
            bitmapImage.EndInit();
            Cache.Add(file, bitmapImage);
            return bitmapImage;
        }
    }
}
