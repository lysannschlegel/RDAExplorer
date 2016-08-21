using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RDAExplorer
{
    public class RDABlockCreator
    {
        public static List<string> FileType_CompressedExtensions = new List<string>() { ".xml", ".txt", ".cfg", ".ini" };

        public static List<RDAFolder> GenerateOf(RDAFolder root)
        {
            Dictionary<string, RDAFolder> dictionary = new Dictionary<string, RDAFolder>();
            foreach (RDAFile rdaFile in root.GetAllFiles())
            {
                string key = Path.GetExtension(rdaFile.FileName).ToLower();
                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, new RDAFolder(root.Version));
                dictionary[key].Files.Add(rdaFile);
            }

            foreach (KeyValuePair<string, RDAFolder> keyValuePair in dictionary)
                keyValuePair.Value.RDABlockCreator_FileType_IsCompressable = new bool?(FileType_CompressedExtensions.Contains(keyValuePair.Key));

            return Enumerable.ToList(dictionary.Values);
        }
    }
}
