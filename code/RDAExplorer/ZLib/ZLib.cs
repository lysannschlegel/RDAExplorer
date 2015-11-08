using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace RDAExplorer.ZLib
{
    public class ZLib
    {
        [DllImport("zlib.DLL")]
        private static extern int uncompress(byte[] des, ref int destLen, byte[] src, int srcLen);

        [DllImport("zlib.DLL")]
        private static extern int compress(byte[] des, ref int destLen, byte[] src, int srcLen);

        public static byte[] Uncompress(byte[] input, int uncompressedSize)
        {
            byte[] des = new byte[uncompressedSize];
            Console.WriteLine("\tDecompressing returned " + uncompress(des, ref uncompressedSize, input, input.Length));
            return des;
        }

        public static byte[] Uncompress(byte[] input, int uncompressedSize, out int result)
        {
            byte[] des = new byte[uncompressedSize];
            result = uncompress(des, ref uncompressedSize, input, input.Length);
            Console.WriteLine("\tDecompressing returned " + result);
            return des;
        }

        public static byte[] Compress(byte[] input)
        {
            int length = input.Length;
            byte[] des = new byte[input.Length];
            compress(des, ref length, input, input.Length);
            return Enumerable.ToList(des).GetRange(0, length).ToArray();
        }
    }
}
