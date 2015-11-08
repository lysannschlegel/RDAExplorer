using System.Collections.Generic;
using System.IO;

namespace RDAExplorer.Misc
{
    public class BinaryExtension
    {
        public static byte[] Decrypt(byte[] buffer)
        {
            if (buffer.Length == 0)
                return buffer;
            BinaryReader binaryReader = new BinaryReader(new MemoryStream(buffer));
            List<short> list = new List<short>();
            do
            {
                list.Add(binaryReader.ReadInt16());
            }
            while (binaryReader.BaseStream.Position + 2L <= binaryReader.BaseStream.Length);
            binaryReader.Close();
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            int num1 = 666666;
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
