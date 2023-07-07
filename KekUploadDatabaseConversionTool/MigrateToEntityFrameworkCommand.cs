using ManyConsole;
using Npgsql;

namespace KekUploadDatabaseConversionTool;

public class MigrateToEntityFrameworkCommand : ConsoleCommand
{

    public MigrateToEntityFrameworkCommand()
    {
        IsCommand("MigrateToEntityFramework", "Migrate the Database to Entity Framework");
        HasLongDescription("Can be used to migrate the old Database to Entity Framework");
        HasRequiredOption("c|connectionstring=", "The Connection String to the Database", p => ConnectionString = p);
    }

    private string? ConnectionString { get; set; }

    public override int Run(string[] remainingArguments)
    {
        if(ConnectionString == null)
        {
            Console.WriteLine("Please enter a valid Connection String!");
            return (int)ReturnCodes.InvalidArguments;
        }
        var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        // select all tables
        var tables = new List<string>();
        using var command = new NpgsqlCommand("select table_name from information_schema.tables where table_schema = 'public' and table_type = 'BASE TABLE';", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }
        reader.Close();
        if(tables.Contains("__EFMigrationsHistory"))
        {
            // select where migration id = 20230706105428_Initial
            using var command2 = new NpgsqlCommand("select \"MigrationId\" from \"__EFMigrationsHistory\" where \"MigrationId\" = '20230706105428_Initial';", connection);
            using var reader2 = command2.ExecuteReader();
            if(reader2.Read())
            {
                Console.WriteLine("The Database was already migrated to Entity Framework!");
                reader2.Close();
                return (int)ReturnCodes.Success;
            }
            reader2.Close();
            Console.WriteLine("The migration table already exists, but the initial migration was not found! Deleting the migration table and recreating it!");
            // drop migration table
            using var command3 = new NpgsqlCommand("drop table \"__EFMigrationsHistory\";", connection);
            command3.ExecuteNonQuery();
        }

        if (tables.Contains("UploadItems"))
        {
            Console.WriteLine("The UploadItems table already exists!");
            Console.WriteLine("It could be that the Database was already migrated to Entity Framework!");
            Console.WriteLine("If you are sure that the Database was not migrated, please delete the UploadItems table and try again!");
            Console.WriteLine("If the Database was migrated, please insert \"20230706105428_Initial\" with Product Version \"7.0.8\" into the MigrationId column of the __EFMigrationsHistory table! Else future migrations will fail!");
            return (int)ReturnCodes.Warning;
        }
        // create migration table
        using var command4 = new NpgsqlCommand("create table if not exists \"__EFMigrationsHistory\" (\"MigrationId\" varchar(150) not null, \"ProductVersion\" varchar(32) not null, primary key (\"MigrationId\"));", connection);
        command4.ExecuteNonQuery();
        // select all columns
        var columns = new Dictionary<string, List<TableInfo.Column>>();
        foreach (var table in tables)
        {
            using var command2 = new NpgsqlCommand("select column_name, data_type from information_schema.columns where table_schema = 'public' and table_name = '" + table + "';", connection);
            using var reader2 = command2.ExecuteReader();
            var list = new List<TableInfo.Column>();
            while (reader2.Read())
            {
                list.Add(new TableInfo.Column(reader2.GetString(0), reader2.GetString(1)));
            }
            columns.Add(table, list);
            reader2.Close();
        }

        var checker = new DatabaseSchemeChecker(columns);
        var result = checker.IsDatabaseSchemeValid();
        var resultWithoutIdMappings = checker.IsDatabaseSchemeValidWithoutIdMapping();

        if (!resultWithoutIdMappings)
        {
            Console.WriteLine("The Database Scheme is not valid!");
            return (int)ReturnCodes.Failure;
        }
        
        // create new table named UploadItems or delete if it already exists
        if (tables.Contains("UploadItems"))
        {
            using var command2 = new NpgsqlCommand("drop table \"UploadItems\";", connection);
            command2.ExecuteNonQuery();
        }

        using var command5 =
            new NpgsqlCommand(
                "create table \"UploadItems\" ( \"Id\" text not null constraint \"PK_UploadItems\" primary key, \"UploadStreamId\" text not null, \"Extension\" text not null, \"Name\" text, \"Hash\" text not null);",
                connection);
        
        command5.ExecuteNonQuery();
        
        // copy all data from files to UploadItems
        using var command6 = new NpgsqlCommand("insert into \"UploadItems\" (\"Id\", \"Extension\", \"Hash\", \"UploadStreamId\") select id, ext, hash, hash from files;", connection);
        command6.ExecuteNonQuery();
        
        // copy all data from idMappings to UploadItems
        if(result)
        {
            using var command7 = new NpgsqlCommand("update \"UploadItems\" set \"Name\" = idm.name, \"UploadStreamId\" = idm.stream_id from id_mapping idm where \"Id\" = idm.id;", connection);
            command7.ExecuteNonQuery();
        }else Console.WriteLine("Old Version of Database detected! The Name will be null for all UploadItems! And the UploadStreamId will be the same as the Hash!");

        // insert initial migration
        using var command8 = new NpgsqlCommand("insert into \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") values ('20230706105428_Initial', '7.0.8');", connection);
        command8.ExecuteNonQuery();
        
        Console.WriteLine("The Database was successfully migrated to Entity Framework!");

        return (int)ReturnCodes.Success;
    }
}