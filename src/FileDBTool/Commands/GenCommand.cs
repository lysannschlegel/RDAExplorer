using System.Collections.Generic;
using System.Linq;

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
            GenFileDBCommand.Generate(remainingArguments, this.outputFileDBName, this.GenerateChecksumDB);

            return 0;
        }

        private void GenerateChecksumDB(System.IO.Stream fileDBStream)
        {
            if (this.outputChecksumDBName != null) {
                GenChecksumDBCommand.Generate(fileDBStream, this.outputChecksumDBName);
            }
        }
    }
}
