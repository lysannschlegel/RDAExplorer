namespace RDAExplorer.FileDBTool.Commands
{
    class GenFileDBCommand : ManyConsole.ConsoleCommand
    {
        public GenFileDBCommand()
        {
            IsCommand("gen.file.db", "Generate file.db from RDA files");
            AllowsAnyAdditionalArguments("(RDA files)|(directory containing RDA files)");
        }

        public override int Run(string[] remainingArguments)
        {
            // writing to memory first since the console stream doesn't support seeking, but Generate requires a seekable stream
            using (var fileDBStream = new System.IO.MemoryStream()) {
                Generate(remainingArguments, fileDBStream);

                using (var outputStream = System.Console.OpenStandardOutput()) {
                    fileDBStream.Position = 0;
                    fileDBStream.CopyTo(outputStream);
                }
            }

            return 0;
        }

        public static void Generate(string[] archiveFileNames, System.IO.Stream outputStream)
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

            using (var fileDBWriter = new AnnoRDA.FileDB.Writer.FileSystemWriter(outputStream, true)) {
                fileDBWriter.WriteFileSystem(fileSystem, archiveFiles);
            }
        }

        private static bool PathIsDirectory(string path)
        {
            return (System.IO.File.GetAttributes(path) & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
        }
    }
}
