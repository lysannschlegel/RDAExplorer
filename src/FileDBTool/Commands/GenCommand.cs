using System.Collections.Generic;
using System.Linq;

namespace RDAExplorer.FileDBTool.Commands
{
    class GenFileDBCommand : ManyConsole.ConsoleCommand
    {
        private string outputFileName;

        public GenFileDBCommand()
        {
            IsCommand("gen", "Generate file.db from RDA files");
            HasLongDescription("Generate file.db to standard output from RDA files given as parameters.");

            AllowsAnyAdditionalArguments("(RDA files)|(directory containing RDA files)");

            HasRequiredOption("output|o=", "The output file name", (value) => { this.outputFileName = value; });
        }

        public override int Run(string[] remainingArguments)
        {
            var archiveFiles = new AnnoRDA.FileDB.Writer.ArchiveFileMap();
            AnnoRDA.FileSystem fileSystem;

            if (remainingArguments.Length == 1 && this.PathIsDirectory(remainingArguments[0])) {
                var directoryLoader = new AnnoRDA.Loader.ContainerDirectoryLoader();
                var loadTask = directoryLoader.Load(remainingArguments[0], System.Threading.CancellationToken.None);
                loadTask.RunSynchronously();

                fileSystem = loadTask.Result.FileSystem;
                foreach (string path in loadTask.Result.ContainerPaths) {
                    archiveFiles.Add(path, path);
                }
            } else {
                fileSystem = new AnnoRDA.FileSystem();
                var fileLoader = new AnnoRDA.Loader.ContainerFileLoader();
                foreach (var rdaFileName in remainingArguments) {
                    var loadTask = fileLoader.Load(rdaFileName);
                    loadTask.RunSynchronously();
                    var overwritetask = fileSystem.OverwriteWith(loadTask.Result, System.Threading.CancellationToken.None);
                    overwritetask.RunSynchronously();

                    archiveFiles.Add(rdaFileName, rdaFileName);
                }
            }

            using (var outputStream = new System.IO.FileStream(this.outputFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write)) {
                using (var writer = new AnnoRDA.FileDB.Writer.FileDBWriter(outputStream, false)) {
                    var writeTask = writer.WriteFileDB(fileSystem, archiveFiles);
                    writeTask.RunSynchronously();
                }
            }

            return 0;
        }

        private bool PathIsDirectory(string path)
        {
            return (System.IO.File.GetAttributes(path) & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
        }
    }
}
