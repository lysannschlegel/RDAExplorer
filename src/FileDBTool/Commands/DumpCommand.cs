using System.IO;

using AnnoRDA.FileDB.Reader;
using AnnoRDA.FileDB.Structs;

namespace RDAExplorer.FileDBTool.Commands
{
    class DumpFileDBCommand : ManyConsole.ConsoleCommand, IContentReaderDelegate
    {
        private int level;

        public DumpFileDBCommand()
        {
            IsCommand("dump", "Dump file.db");
            HasLongDescription("Dump contents of a file.db to standard output.");

            HasAdditionalArguments(1, " file.db");
        }

        public override int Run(string[] remainingArguments)
        {
            this.level = 0;

            using (var fileStream = new FileStream(remainingArguments[0], FileMode.Open, FileAccess.Read)) {
                using (var reader = new DBReader(fileStream)) {
                    reader.ReadFile(this);
                }
            }
            return 0;
        }

        void IContentReaderDelegate.OnStructureStart(Tag tag)
        {
            this.DumpPadding();
            this.DumpTag(tag, true);

            this.level += 1;
        }
        void IContentReaderDelegate.OnStructureEnd(Tag tag)
        {
            this.level -= 1;
        }

        void IContentReaderDelegate.OnAttribute(Tag tag, byte[] value)
        {
            this.DumpPadding();
            this.DumpTag(tag, false);
            System.Console.Write(": ");
            this.DumpAttributeValue(tag, value, true);
        }

        private void DumpPadding()
        {
            string padding = new System.String(' ', this.level * 2);
            System.Console.Out.Write(padding);
        }

        private void DumpTag(Tag tag, bool writeNewline)
        {
            System.Console.Out.Write(tag.ID.ToString("X4") + " " + tag.Name);
            if (writeNewline) {
                System.Console.Out.WriteLine("");
            }
        }

        private void DumpAttributeValue(Tag tag, byte[] value, bool writeNewline)
        {
            switch (tag.Name)
            {
                case Tags.BuiltinTags.Names.String:
                case FileSystemTags.Attributes.Names.FileName:
                case FileSystemTags.Attributes.Names.LastArchiveFile:
                {
                    DumpStringValue(value);
                    break;
                }
                case FileSystemTags.Attributes.Names.ArchiveFileIndex:
                case FileSystemTags.Attributes.Names.Flags:
                case FileSystemTags.Attributes.Names.ResidentBufferIndex:
                case FileSystemTags.Attributes.Names.Size:
                {
                    DumpUInt32Value(value);
                    break;
                }
                case FileSystemTags.Attributes.Names.Position:
                case FileSystemTags.Attributes.Names.CompressedSize:
                case FileSystemTags.Attributes.Names.UncompressedSize:
                case FileSystemTags.Attributes.Names.ModificationTime:
                {
                    DumpUInt64Value(value);
                    break;
                }
                case FileSystemTags.Attributes.Names.Buffer:
                {
                    DumpRawBytes(value);
                    break;
                }
                default:
                {
                    throw new System.ArgumentException("node.tag.Name");
                }
            }
            if (writeNewline) {
                System.Console.Out.WriteLine("");
            }
        }

        private void DumpStringValue(byte[] value)
        {
            if (value.Length >= 2) {
                System.Console.Out.Write(System.Text.Encoding.Unicode.GetString(value));
            }
        }
        private void DumpUInt32Value(byte[] value)
        {
            System.Console.Out.Write(System.BitConverter.ToUInt32(value, 0));
        }
        private void DumpUInt64Value(byte[] value)
        {
            System.Console.Out.Write(System.BitConverter.ToUInt64(value, 0));
        }
        private void DumpRawBytes(byte[] value)
        {
            for (int i = 0; i < value.Length && i < 10; ++i) {
                System.Console.Out.Write(System.String.Format("{0:X2} ", value[i]));
            }
            if (value.Length > 10) {
                System.Console.Out.Write("...");
            }
        }
    }
}
