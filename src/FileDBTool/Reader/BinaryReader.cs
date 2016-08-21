namespace RDAExplorer.FileDBTool.Reader
{
    class BinaryReader : System.IO.BinaryReader
    {
        public BinaryReader(System.IO.Stream input, System.Text.Encoding encoding, bool leaveOpen)
        : base(input, encoding, leaveOpen)
        {
        }

        new public int Read7BitEncodedInt()
        {
            return base.Read7BitEncodedInt();
        }

        public string ReadZeroTerminatedASCIIString()
        {
            var bytes = new System.Collections.Generic.List<char>();
            while (true) {
                byte c = this.ReadByte();
                if (c == 0) {
                    break;
                } else {
                    bytes.Add((char)c);
                }
            }
            var result = new System.String(bytes.ToArray());
            return result;
        }

        public void SkipNullByte()
        {
            byte b = this.ReadByte();
            if (b != 0) {
                throw new System.FormatException(System.String.Format("Expected 0x00 byte, but was: {0}", b));
            }
        }
    }
}
