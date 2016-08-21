using System.Collections.Generic;
using System.Linq;

namespace RDAExplorer.FileDBTool.Structs
{
    class Tags
    {
        private readonly Dictionary<ushort, Tag> tags;

        public Tags(Dictionary<ushort, Tag> structures, Dictionary<ushort, Tag> attributes)
        {
            this.tags = new Dictionary<ushort, Tag>();

            this.tags.Add(0x0000, new Tag(0x0000, "StructureEnd"));
            this.tags.Add(0x0001, new Tag(0x0001, "List"));
            foreach (KeyValuePair<ushort, Tag> entry in structures) {
                this.tags.Add(entry.Key, entry.Value);
            }

            this.tags.Add(0x8000, new Tag(0x8000, "String"));
            foreach (KeyValuePair<ushort, Tag> entry in attributes) {
                this.tags.Add(entry.Key, entry.Value);
            }
        }

        public bool TryGetTagById(ushort tagId, out Tag value)
        {
            return this.tags.TryGetValue(tagId, out value);
        }

        public bool TryGetTagByName(string name, out Tag value)
        {
            value = this.tags.FirstOrDefault(kv => kv.Value.Name == name).Value;
            return value != null;
        }

        public IEnumerable<Tag> GetAllCustomTags()
        {
            ICollection<ushort> defaultTagIDs = this.GetAllDefaultTagIDs().ToList();
            return this.tags.Where(kv => !defaultTagIDs.Contains(kv.Key))
                            .Select(kv => kv.Value);
        }
        private IEnumerable<ushort> GetAllDefaultTagIDs()
        {
            yield return 0x0000;
            yield return 0x0001;
            yield return 0x8000;
        }
    }
}
