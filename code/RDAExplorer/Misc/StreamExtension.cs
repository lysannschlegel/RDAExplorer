using System;
using System.IO;

namespace RDAExplorer.Misc
{
    static class StreamExtension
    {
        // http://stackoverflow.com/a/13022108
        public static void CopyLimited(this Stream input, Stream output, ulong bytes)
        {
            byte[] buffer = new byte[4096];
            int read;
            while (bytes > 0 &&
                   (read = input.Read(buffer, 0, (int)Math.Min((ulong)buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= (ulong)read;
            }
        }

        public static void WriteBytes(this Stream output, ulong count, byte value)
        {
            byte[] buffer = new byte[Math.Min(count, 4096)];
            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = value;
            }

            while (count > 0)
            {
                uint writeCount = (uint)Math.Min(count, 4096);
                output.Write(buffer, 0, (int)writeCount);
                count -= writeCount;
            }
        }
    }
}
