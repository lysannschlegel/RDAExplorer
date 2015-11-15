using RDAExplorer.Misc;
using System;
using System.IO;

namespace RDAExplorer
{
    public class RDAMemoryResidentHelper : IDisposable
    {
        public ulong Offset;
        public ulong DataSize;
        public ulong Compressed;
        public MemoryStream Data;
        public BlockInfo Info;

        public RDAMemoryResidentHelper(ulong offset, ulong datasize, ulong compressed, Stream datasource, BlockInfo info, FileHeader.Version version)
        {
            Offset = offset;
            DataSize = datasize;
            Compressed = compressed;
            Info = info;
            Data = new MemoryStream();
            byte[] numArray = new byte[compressed];
            datasource.Position = (long)offset;
            datasource.Read(numArray, 0, (int)compressed);
            if ((info.flags & 2) == 2)
                numArray = BinaryExtension.Decrypt(numArray, BinaryExtension.GetDecryptionSeed(version));
            if ((info.flags & 1) == 1)
                numArray = ZLib.ZLib.Uncompress(numArray, (int)datasize);
            Data.Write(numArray, 0, numArray.Length);
        }

        public void Dispose()
        {
            Data.Close();
        }
    }
}
