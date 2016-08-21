namespace RDAExplorer.FileDBTool.Writer
{
    class BinaryWriter : System.IO.BinaryWriter
    {
        public BinaryWriter(System.IO.Stream input, System.Text.Encoding encoding, bool leaveOpen)
        : base(input, encoding, leaveOpen)
        {
        }

        new public void Write7BitEncodedInt(int value)
        {
            base.Write7BitEncodedInt(value);
        }

        public void WriteZeroTerminatedASCIIString(string value)
        {
            char[] chars = value.ToCharArray();
            foreach (char ch in chars) {
                if (ch <= 255) {
                    this.Write((byte)ch);
                }
            }
            this.WriteNullByte();
        }

        public void WriteNullByte()
        {
            this.Write((byte)0x00);
        }
    }
}
