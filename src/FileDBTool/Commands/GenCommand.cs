namespace RDAExplorer.FileDBTool.Commands
{
    class GenFileDBCommand : ManyConsole.ConsoleCommand
    {
        private string outputFileName;

        public GenFileDBCommand()
        {
            IsCommand("gen", "Generate file.db from RDA files");
            HasLongDescription("Generate file.db to standard output from RDA files given as parameters.");

            AllowsAnyAdditionalArguments("[data0.rda [data1.rda [...]]]");

            HasRequiredOption("output|o=", "The output file name", (value) => { this.outputFileName = value; });
        }

        public override int Run(string[] remainingArguments)
        {
            var fileSystem = new AnnoRDA.FileSystem();
            var fileLoader = new AnnoRDA.Loader.ContainerFileLoader();

            foreach (var rdaFileName in remainingArguments) {
                var containerFileSystem = fileLoader.Load(rdaFileName);
                fileSystem.Merge(containerFileSystem, System.Threading.CancellationToken.None);
            }

            using (var outputStream = new System.IO.FileStream(this.outputFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write)) {
                using (var writer = new RDAExplorer.FileDBTool.Writer.FileDBWriter(outputStream, false)) {
                    writer.WriteFileDB(fileSystem, remainingArguments);
                }
            }

            return 0;
        }
    }
}
