using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RDAExplorer
{
    public struct FileHeader
    {
        public string magic;
        public Version version;
        public byte[] unkown;
        public ulong firstBlockOffset;

        public static FileHeader Create(Version version)
        {
            string magic = GetMagic(version);
            uint unknownSize = GetUnknownSize(version);
            return new FileHeader()
            {
                magic = magic,
                version = version,
                unkown = new byte[unknownSize],
                firstBlockOffset = 0,
            };
        }

        public enum Version
        {
            Invalid,
            Version_2_0,
            Version_2_2,
        }

        public static string GetMagic(Version version)
        {
            switch (version)
            {
                case Version.Version_2_0: return "Resource File V2.0";
                case Version.Version_2_2: return "Resource File V2.2";
            }
            return null;
        }

        public static Encoding GetMagicEncoding(Version version)
        {
            switch (version)
            {
                case Version.Version_2_0: return Encoding.Unicode;
                case Version.Version_2_2: return Encoding.UTF8;
            }
            return null;
        }

        public static uint GetSize(Version version)
        {
            uint magicSize = GetMagicSize(version);
            uint unknownSize = GetUnknownSize(version);
            uint firstBlockOffsetSize = GetUIntSize(version);
            return magicSize + unknownSize + firstBlockOffsetSize;
        }

        public static uint GetMagicSize(Version version)
        {
            string magic = GetMagic(version);
            Encoding encoding = GetMagicEncoding(version);
            return (uint)encoding.GetByteCount(magic);
        }

        public static byte[] GetMagicBytes(Version version)
        {
            string magic = GetMagic(version);
            Encoding encoding = GetMagicEncoding(version);
            return encoding.GetBytes(magic);
        }

        public static uint GetUnknownSize(Version version)
        {
            switch (version)
            {
                case Version.Version_2_0: return 1008;
                case Version.Version_2_2: return 766;
            }
            return 0;
        }

        public static uint GetUIntSize(Version version)
        {
            switch (version)
            {
                case Version.Version_2_0: return 4;
                case Version.Version_2_2: return 8;
            }
            return 0;
        }

        public static ulong ReadUIntVersionAware(BinaryReader reader, Version version)
        {
            switch (version)
            {
                case Version.Version_2_0: return reader.ReadUInt32();
                case Version.Version_2_2: return reader.ReadUInt64();
            }
            throw new System.Exception("Unsupported RDA version");
        }

        public static void WriteUIntVersionAware(BinaryWriter writer, ulong value, Version version)
        {
            switch (version)
            {
                case Version.Version_2_0: writer.Write((System.UInt32)value); return;
                case Version.Version_2_2: writer.Write((System.UInt64)value); return;
            }
            throw new System.Exception("Unsupported RDA version");
        }
    }
}
