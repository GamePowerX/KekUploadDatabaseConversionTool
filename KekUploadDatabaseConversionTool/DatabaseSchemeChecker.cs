using System.Collections;
using System.Text.RegularExpressions;

namespace KekUploadDatabaseConversionTool;

public class DatabaseSchemeChecker
{
    public DatabaseSchemeChecker(Dictionary<string, List<TableInfo.Column>> databaseScheme)
    {
        DatabaseScheme = databaseScheme;
    }

    public Dictionary<string, List<TableInfo.Column>> DatabaseScheme { get; set; }

    public bool IsDatabaseSchemeValid()
    {
        return TableInfo.Compare(DatabaseScheme);
    }
    
    public bool IsDatabaseSchemeValidWithoutIdMapping()
    {
        return TableInfo.CompareWithoutIdMapping(DatabaseScheme);
    }
}

public class TableInfo
{
    public static TableCollection ExpectedTables { get; } = new(new Table("files", new Column("id", "text"),
        new Column("ext", "text"), new Column("hash", "text")), new Table("id_mapping", new Column("stream_id", "text"), new Column("id", "text"), new Column("name", "text")));
    
    public static TableCollection ExpectedTablesWithoutIdMapping { get; } = new(new Table("files", new Column("id", "text"),
        new Column("ext", "text"), new Column("hash", "text")));

    public static bool Compare(Dictionary<string, List<Column>> databaseScheme)
    {
        if(ExpectedTables.Any(x => !databaseScheme.ContainsKey(x.Name)))
        {
            return false;
        }
        
        foreach(var table in databaseScheme)
        {
            foreach (var column in table.Value.Where(column => column.Type.Equals("character") || column.Type.Equals("character varying")))
            {
                databaseScheme[table.Key][databaseScheme[table.Key].IndexOf(column)].Type = "text";
            }
        }
        
        // now check if the structure is the same
        var fileTable = ExpectedTables["files"];
        if(fileTable == null)
        {
            return false;
        }
        var columns = fileTable.Columns;
        if (databaseScheme[fileTable.Name].Count != columns.Length)
        {
            return false;
        }
        var idMappingTable = ExpectedTables["id_mapping"];
        if(idMappingTable == null)
        {
            return false;
        }
        var idMappingColumns = idMappingTable.Columns;
        foreach (var column in columns)
        {
            if (!databaseScheme[fileTable.Name].Contains(column))
            {
                return false;
            }
            // check if the type is the same
            var column1 = databaseScheme[fileTable.Name][databaseScheme[fileTable.Name].IndexOf(column)];
            if (column1.Type != column.Type)
            {
                return false;
            }
        }
        foreach (var column in idMappingColumns)
        {
            if (!databaseScheme[idMappingTable.Name].Contains(column))
            {
                return false;
            }
            // check if the type is the same
            var column1 = databaseScheme[idMappingTable.Name][databaseScheme[idMappingTable.Name].IndexOf(column)];
            if (column1.Type != column.Type)
            {
                return false;
            }
        }
        return true;
    }
    
    public static bool CompareWithoutIdMapping(Dictionary<string, List<Column>> databaseScheme)
    {
        if(ExpectedTablesWithoutIdMapping.Any(x => !databaseScheme.ContainsKey(x.Name)))
        {
            return false;
        }
        
        foreach(var table in databaseScheme)
        {
            foreach (var column in table.Value.Where(column => column.Type.Equals("character") || column.Type.Equals("character varying")))
            {
                databaseScheme[table.Key][databaseScheme[table.Key].IndexOf(column)].Type = "text";
            }
        }
        /*
        foreach (var table in ExpectedTablesWithoutIdMapping)
        {
            foreach(var column in table.Columns)
            { 
                var column1 = databaseScheme[table.Name][databaseScheme[table.Name].IndexOf(column)];
                
                if (regex.IsMatch(column1.Name) || regex2.IsMatch(column1.Name))
                {
                    databaseScheme[table.Name][databaseScheme[table.Name].IndexOf(column1)].Type = "text";
                }
            }
        }*/
        
        // now check if the structure is the same
        var fileTable = ExpectedTablesWithoutIdMapping["files"];
        if(fileTable == null)
        {
            return false;
        }
        var columns = fileTable.Columns;
        if (databaseScheme[fileTable.Name].Count != columns.Length)
        {
            return false;
        }
        foreach (var column in columns)
        {
            if (!databaseScheme[fileTable.Name].Contains(column))
            {
                return false;
            }
            // check if the type is the same
            var column1 = databaseScheme[fileTable.Name][databaseScheme[fileTable.Name].IndexOf(column)];
            if (column1.Type != column.Type)
            {
                return false;
            }
        }
        return true;
    }

    public class TableCollection : IEnumerable<Table>
    {
        public TableCollection(params Table[] tables)
        {
            Tables = tables;
        }

        private Table[] Tables { get; }
        public int Count => Tables.Length;
        public Table this[int index] => Tables[index];
        public Table? this[string name] => Tables.FirstOrDefault(x => x.Name == name);

        public IEnumerator<Table> GetEnumerator()
        {
            return ((IEnumerable<Table>) Tables).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Table
    {
        public Table(string name, params Column[] columns)
        {
            Name = name;
            Columns = columns;
        }

        public string Name { get; }
        public Column[] Columns { get; }
    }

    public class Column
    {
        public Column(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public string Type { get; set; }
        
        public override bool Equals(object? obj)
        {
            if (obj is Column column)
            {
                return column.Name == Name && column.Type == Type;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type);
        }
    }
}