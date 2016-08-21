namespace RDAExplorer.FileDBTool.Structs
{
    class Tag
    {
        public enum TagType
        {
            StructureStart,
            StructureEnd,
            Attribute,
        }

        public ushort ID { get; }
        public string Name { get; }
        public TagType Type { get {
            if (ID == 0x0000) {
                return TagType.StructureEnd;
            } else if ((ID & 0x8000) == 0) {
                return TagType.StructureStart;
            } else {
                return TagType.Attribute;
            }
        } }

        public Tag(ushort id, string name)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}
