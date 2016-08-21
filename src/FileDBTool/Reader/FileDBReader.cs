using RDAExplorer.FileDBTool.Structs;

namespace RDAExplorer.FileDBTool.Reader
{
    class FileDBReader : System.IDisposable
    {
        private readonly BinaryReader reader;

        public FileDBReader(System.IO.Stream stream, bool leaveOpen)
        {
            if (!stream.CanRead) {
                throw new System.ArgumentException("stream must be readable");
            }
            if (!stream.CanSeek) {
                throw new System.ArgumentException("stream must be seekable");
            }

            this.reader = new BinaryReader(stream, System.Text.Encoding.Unicode, leaveOpen);
        }

        public FileDBReader(System.IO.Stream stream)
        : this(stream, false)
        {}

        public void Dispose()
        {
            reader.Dispose();
        }

        public Node ReadFile()
        {
            var tagSectionReader = new TagsSectionReader(this.reader);
            long tagsSectionPosition = tagSectionReader.FindTagsSection();
            this.reader.BaseStream.Position = tagsSectionPosition;
            Tags tags = tagSectionReader.ReadTagsSection();

            var contentReader = new ContentReader(this.reader, tags);
            this.reader.BaseStream.Position = 0;
            Node content = contentReader.ReadContent();

            Tag tag = contentReader.ReadTag();
            if (tag.Type != Tag.TagType.StructureEnd) {
                throw new System.FormatException(System.String.Format("Unexpected tag type at end of content; expected: {0}, was: {1}", Tag.TagType.StructureEnd, tag.Type));
            }
            if (this.reader.BaseStream.Position != tagsSectionPosition) {
                throw new System.FormatException(System.String.Format("Unexpected position at end of content; expected: {0}, was: {1}", tagSectionReader, this.reader.BaseStream.Position));
            }

            return content;
        }
    }
}
