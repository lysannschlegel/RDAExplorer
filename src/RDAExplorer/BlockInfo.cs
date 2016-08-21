using System;

namespace RDAExplorer
{
    public struct BlockInfo
    {
        public uint flags;
        public uint fileCount;
        public ulong directorySize;
        public ulong decompressedSize;
        public ulong nextBlock;

        public BlockInfo Clone()
        {
            return new BlockInfo()
            {
                flags = this.flags,
                fileCount = this.fileCount,
                directorySize = this.directorySize,
                decompressedSize = this.decompressedSize,
                nextBlock = this.nextBlock,
            };
        }

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
