using ManyConsole;

namespace RDAExplorer.FileDBTool
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var commands = GetCommands();
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, System.Console.Out, System.Console.Error);
        }

        public static System.Collections.Generic.IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }
    }
}
