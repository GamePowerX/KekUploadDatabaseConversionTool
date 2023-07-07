using ManyConsole;

namespace KekUploadDatabaseConversionTool;

internal static class Program
{
    public const string Version = "1.0.0";
    public static int Main(string[] args)
    {
        var commands = GetCommands();
        return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
    }

    private static IEnumerable<ConsoleCommand> GetCommands()
    {
        return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
    }
}

public enum ReturnCodes
{
    Success = 0,
    Failure = 2,
    Error = 1,
    InvalidArguments = 0xA0,
    Warning = 4
}
