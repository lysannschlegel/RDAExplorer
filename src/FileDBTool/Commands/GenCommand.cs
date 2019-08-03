using System;

namespace RDAExplorer.FileDBTool.Commands
{
    class GenCommand : ManyConsole.ConsoleCommand
    {
        private string outputFileDBName;
        private string outputChecksumDBName;

        public GenCommand()
        {
            IsCommand("gen", "Generate file.db from RDA files");

            AllowsAnyAdditionalArguments("(RDA files)|(directory containing RDA files)");

            HasRequiredOption("file.db|f=", "Path to output file.db", (value) => { this.outputFileDBName = value; });
            HasOption("checksum.db|c=", "Path to optional output checksum.db", (value) => { this.outputChecksumDBName = value; });
        }

        public override int Run(string[] remainingArguments)
        {
            using (var outputStream = new System.IO.FileStream(this.outputFileDBName, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite)) {
                GenFileDBCommand.Generate(remainingArguments, outputStream);

                if (this.outputChecksumDBName != null) {
                    outputStream.Position = 0;
                    GenChecksumDBCommand.Generate(outputStream, this.outputChecksumDBName);
                }
            }

            return 0;
        }
    }
}
