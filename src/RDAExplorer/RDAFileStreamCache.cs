using System;
using System.Collections.Generic;
using System.IO;

namespace RDAExplorer
{
    public class RDAFileStreamCache
    {
        public static Dictionary<string, FileStream> Cache = new Dictionary<string, FileStream>();

        public static FileStream Open(string file)
        {
            try
            {
                if (!Cache.ContainsKey(file))
                    Cache.Add(file, new FileStream(file, FileMode.Open));
                return Cache[file];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
