using System;
using System.Collections.Generic;
using System.IO;

namespace RDAExplorer.Misc
{
    public class BinaryExtension
    {
        public static int GetDecryptionSeed(FileHeader.Version version)
        {
            switch (version)
            {
                case FileHeader.Version.Invalid: throw new ArgumentException("Invalid file version", "version");
                case FileHeader.Version.Version_2_0: return 0xA2C2A;    // 0x800A2C2A would also work, but the number doesn't fit into int
                case FileHeader.Version.Version_2_2: return 0x71C71C71; // 0xF1C71C71 would also work, but the number doesn't fit into int
            }
            throw new ArgumentException("Files of version " + Enum.GetName(version.GetType(), version) + " cannot be decrypted yet.", "version");
        }

        public static byte[] Decrypt(byte[] buffer, int seed)
        {
            if (buffer.Length == 0)
                return buffer;

            if (seed == 0) {
                throw new ArgumentException("Invalid decryption seed");
            }

            BinaryReader binaryReader = new BinaryReader(new MemoryStream(buffer));
            List<short> list = new List<short>();
            do {
                list.Add(binaryReader.ReadInt16());
            } while (binaryReader.BaseStream.Position + 2L <= binaryReader.BaseStream.Length);
            binaryReader.Close();

            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

            int num1 = seed;
            for (int index = 0; index < list.Count; ++index)
            {
                num1 = num1 * 214013 + 2531011;
                short num2 = (short)(num1 >> 16 & short.MaxValue);
                short num3 = (short)(list[index] ^ num2);
                binaryWriter.Write(num3);
            }

            if (buffer.Length % 2 != 0)
                binaryWriter.Write(buffer[buffer.Length - 1]);

            memoryStream.Position = 0L;
            byte[] outBuffer = new byte[memoryStream.Length];
            memoryStream.Read(outBuffer, 0, (int)memoryStream.Length);
            return outBuffer;
        }
    }
}
