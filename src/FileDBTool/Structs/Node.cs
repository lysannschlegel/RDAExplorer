using System.Collections.Generic;

namespace RDAExplorer.FileDBTool.Structs
{
    class Node
    {
        public Tag Tag { get; }
        public List<Node> Children { get; }
        public byte[] Value { get; }

        public Node(Tag tag)
        {
            this.Tag = tag;
            this.Children = new List<Node>();
            this.Value = null;
        }
        public Node(Tag tag, byte[] value)
        {
            this.Tag = tag;
            this.Children = null;
            this.Value = value;
        }

        public void AddChild(Node child)
        {
            this.Children.Add(child);
        }
    }

    static class AttributeValueConverter
    {
        public static string ValueToString(this Node node)
        {
            if (node.Value.Length < 2) {
                return "";
            } else {
                return System.Text.Encoding.Unicode.GetString(node.Value);
            }
        }

        public static uint ValueToUInt32(this Node node)
        {
            return System.BitConverter.ToUInt32(node.Value, 0);
        }

        public static ulong ValueToUInt64(this Node node)
        {
            return System.BitConverter.ToUInt64(node.Value, 0);
        }
    }
}
