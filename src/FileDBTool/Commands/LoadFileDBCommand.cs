using System.IO;

using AnnoRDA.FileDB.Reader;
using AnnoRDA.FileDB.Structs;

namespace RDAExplorer.FileDBTool.Commands
{
    class LoadFileDBCommand : ManyConsole.ConsoleCommand
    {
        public LoadFileDBCommand()
        {
            IsCommand("load", "Load file.db");
            HasLongDescription("Load contents of a file.db.");

            HasAdditionalArguments(1, " file.db");
        }

        public override int Run(string[] remainingArguments)
        {
            using (var fileStream = new FileStream(remainingArguments[0], FileMode.Open, FileAccess.Read)) {
                using (var reader = new FileSystemReader(fileStream)) {
                    reader.ReadFileSystem();
                }
            }
            return 0;
        }
    }
}
