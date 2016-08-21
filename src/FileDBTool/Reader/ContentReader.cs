using RDAExplorer.FileDBTool.Structs;

namespace RDAExplorer.FileDBTool.Reader
{
    class ContentReader
    {
        private readonly BinaryReader reader;
        private readonly Tags tags;

        public ContentReader(BinaryReader reader, Tags tags)
        {
            this.reader = reader;
            this.tags = tags;
        }

        public Tag ReadTag()
        {
            var tagId = this.reader.ReadUInt16();

            Tag tag;
            if (!this.tags.TryGetTagById(tagId, out tag)) {
                throw new System.FormatException(System.String.Format("Unexpected tag found: {0}", tagId));
            } else {
                return tag;
            }
        }

        public Node ReadContent()
        {
            Tag tag = this.ReadTag();
            if (tag.Type != Tag.TagType.StructureStart) {
                throw new System.FormatException(System.String.Format("Root node should be structure, but tag was {0} ({1})", tag.Type, tag.Name));
            }
            Node result = this.ReadStructure(tag);
            return result;
        }

        private Node ReadStructure(Tag tag)
        {
            Node result = new Node(tag);
            while (true) {
                Tag innerTag = this.ReadTag();
                switch (innerTag.Type) {
                    case Tag.TagType.Attribute: {
                        Node child = this.ReadAttribute(innerTag);
                        result.AddChild(child);
                        break;
                    }
                    case Tag.TagType.StructureStart: {
                        Node child = this.ReadStructure(innerTag);
                        result.AddChild(child);
                        break;
                    }
                    case Tag.TagType.StructureEnd: {
                        return result;
                    }
                }
            }
        }

        private Node ReadAttribute(Tag tag)
        {
            var valueLength = this.reader.Read7BitEncodedInt();
            byte[] value = this.reader.ReadBytes(valueLength);
            Node result = new Node(tag, value);
            return result;
        }
    }
}
