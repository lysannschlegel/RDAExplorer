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

        void IContentReaderDelegate.OnAttribute(Tag tag, AttributeValue value)
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

        private void DumpAttributeValue(Tag tag, AttributeValue value, bool writeNewline)
        {
            switch (tag.Name)
            {
                case Tags.BuiltinTags.Names.String:
                case FileSystemTags.Attributes.Names.FileName:
                case FileSystemTags.Attributes.Names.LastArchiveFile: {
                    System.Console.Out.Write(value.GetUnicodeString());
                    break;
                }
                case FileSystemTags.Attributes.Names.ArchiveFileIndex:
                case FileSystemTags.Attributes.Names.Flags:
                case FileSystemTags.Attributes.Names.ResidentBufferIndex:
                case FileSystemTags.Attributes.Names.Size: {
                    System.Console.Out.Write(value.GetUInt32());
                    break;
                }
                case FileSystemTags.Attributes.Names.Position:
                case FileSystemTags.Attributes.Names.CompressedSize:
                case FileSystemTags.Attributes.Names.UncompressedSize:
                case FileSystemTags.Attributes.Names.ModificationTime: {
                    System.Console.Out.Write(value.GetUInt64());
                    break;
                }
                case FileSystemTags.Attributes.Names.Buffer:
                {
                    const int numBytesToShow = 10;
                    for (int i = 0; i < value.Bytes.Length && i < numBytesToShow; ++i) {
                        System.Console.Out.Write(System.String.Format("{0:X2} ", value.Bytes[i]));
                    }
                    if (value.Bytes.Length > numBytesToShow) {
                        System.Console.Out.Write("...");
                    }
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
    }
}
