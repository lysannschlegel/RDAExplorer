using System.Collections.Generic;
using System.Linq;

namespace RDAExplorer.FileDBTool.Commands
{
    class GenFileDBCommand : ManyConsole.ConsoleCommand
    {
        private string outputFileName;

        public GenFileDBCommand()
        {
            IsCommand("gen.file.db", "Generate file.db from RDA files");

            AllowsAnyAdditionalArguments("(RDA files)|(directory containing RDA files)");

            HasRequiredOption("output|o=", "Path to output file", (value) => { this.outputFileName = value; });
        }

        public override int Run(string[] remainingArguments)
        {
            Generate(remainingArguments, this.outputFileName, fileDBStreamAction: null);

            return 0;
        }

        public static void Generate(string[] archiveFileNames, string outputFileName, System.Action<System.IO.Stream> fileDBStreamAction)
        {
            var archiveFiles = new AnnoRDA.FileDB.Writer.ArchiveFileMap();
            AnnoRDA.FileSystem fileSystem;

            if (archiveFileNames.Length == 1 && PathIsDirectory(archiveFileNames[0])) {
                var directoryLoader = new AnnoRDA.Loader.ContainerDirectoryLoader();
                var loadResult = directoryLoader.Load(archiveFileNames[0], System.Threading.CancellationToken.None);

                fileSystem = loadResult.FileSystem;
                foreach (string path in loadResult.ContainerPaths) {
                    archiveFiles.Add(path, path);
                }
            } else {
                fileSystem = new AnnoRDA.FileSystem();
                var fileLoader = new AnnoRDA.Loader.ContainerFileLoader();
                foreach (var rdaFileName in archiveFileNames) {
                    var loadedFS = fileLoader.Load(rdaFileName);
                    fileSystem.OverwriteWith(loadedFS, null, System.Threading.CancellationToken.None);

                    archiveFiles.Add(rdaFileName, rdaFileName);
                }
            }

            using (var fileDBStream = new System.IO.FileStream(outputFileName, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite)) {
                using (var fileDBWriter = new AnnoRDA.FileDB.Writer.FileSystemWriter(fileDBStream, true)) {
                    fileDBWriter.WriteFileSystem(fileSystem, archiveFiles);
                }

                if (fileDBStreamAction != null) {
                    fileDBStream.Position = 0;
                    fileDBStreamAction(fileDBStream);
                }
            }
        }

        private static bool PathIsDirectory(string path)
        {
            return (System.IO.File.GetAttributes(path) & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
        }
    }
}
