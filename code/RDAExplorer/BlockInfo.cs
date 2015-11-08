using System.Runtime.InteropServices;

namespace RDAExplorer
{
    public struct BlockInfo
    {
        public uint flags;
        public uint fileCount;
        public ulong directorySize;
        public ulong decompressedSize;
        public ulong nextBlock;

        public static uint GetSize(FileHeader.Version version)
        {
            switch (version)
            {
                case FileHeader.Version.Version_2_0: return 20;
                case FileHeader.Version.Version_2_2: return 32;
            }
            return 0;
        }
    }
}
