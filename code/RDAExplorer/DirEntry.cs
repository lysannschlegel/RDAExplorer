using System.Runtime.InteropServices;

namespace RDAExplorer
{
    public struct DirEntry
    {
        public string filename;
        public ulong offset;
        public ulong compressed;
        public ulong filesize;
        public ulong timestamp;
        public ulong unknown;

        public static uint GetFilenameSize()
        {
            return 520;
        }

        public static uint GetSize(FileHeader.Version version)
        {
            switch (version)
            {
                case FileHeader.Version.Version_2_0: return GetFilenameSize() + 20;
                case FileHeader.Version.Version_2_2: return GetFilenameSize() + 40;
            }
            return 0;
        }
    }
}
