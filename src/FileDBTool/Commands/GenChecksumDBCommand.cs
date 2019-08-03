namespace RDAExplorer.FileDBTool.Commands
{
    class GenChecksumDBCommand : ManyConsole.ConsoleCommand
    {
        public GenChecksumDBCommand()
        {
            IsCommand("gen.checksum.db", "Generate checksum.db from file.db");
            HasAdditionalArguments(2, " file.db checksum.db");
        }

        public override int Run(string[] remainingArguments)
        {
            string inputFileName = remainingArguments[0];
            string outputFileName = remainingArguments[1];

            byte[] checksum;
            using (var fileDBStream = new System.IO.FileStream(inputFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
                checksum = CalcChecksum(fileDBStream);
            }

            WriteOutputFile(checksum, outputFileName);

            return 0;
        }

        public static void Generate(System.IO.Stream inputFileStream, string outputFileName)
        {
            byte[] checksum = CalcChecksum(inputFileStream);
            WriteOutputFile(checksum, outputFileName);
        }

        private static byte[] CalcChecksum(System.IO.Stream inputFileStream)
        {
            return AnnoRDA.ChecksumDB.Generator.ComputeChecksum(inputFileStream);
        }

        private static void WriteOutputFile(byte[] checksum, string outputFileName)
        {
            using (var checksumDBStream = new System.IO.FileStream(outputFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write)) {
                using (var checksumDBWriter = new System.IO.BinaryWriter(checksumDBStream)) {
                    checksumDBWriter.Write(checksum);
                }
            }
        }
    }
}
