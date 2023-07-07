using ManyConsole;
using Npgsql;

namespace KekUploadDatabaseConversionTool;

public class ConnectionStringBuilderCommand : ConsoleCommand
{
    public ConnectionStringBuilderCommand()
    {
        IsCommand("ConnectionStringBuilder", "Build a Connection String");
        HasLongDescription("Can be used to build a Connection String for the Database");
        HasRequiredOption("h|host=", "The Server", p => Server = p);
        HasRequiredOption("d|database=", "The Database Name", p => Database = p);
        HasRequiredOption("U|username=", "The Username", p => Username = p);
        HasRequiredOption("W|password=", "The Password", p => Password = p);
        HasOption("p|port=", "The Port", p => Port = int.TryParse(p, out var port) ? port : null);
        HasOption("t|trusted=", "If the Connection should be trusted", t => Trusted = true);
    }

    public bool? Trusted { get; set; }
    public string? Password { get; set; }
    public string? Username { get; set; }
    public string? Database { get; set; }
    public int? Port { get; set; }
    public string? Server { get; set; }
    

    public override int Run(string[] remainingArguments)
    {
        if(Server == null || Database == null || Username == null || Password == null)
        {
            Console.WriteLine("Please provide all required Arguments!");
            return (int)ReturnCodes.InvalidArguments;
        }

        if(Port is < 1 or > 65535)
        {
            Console.WriteLine("Please provide a valid Port!");
            return (int)ReturnCodes.InvalidArguments;
        }
        
        var builder = new NpgsqlConnectionStringBuilder()
        {
            Host = Server,
            Port = Port ?? 5432,
            Database = Database,
            Username = Username,
            Password = Password,
            TrustServerCertificate = Trusted ?? false
        };
        Console.WriteLine(builder.ConnectionString);
        return (int)ReturnCodes.Success;
    }
}