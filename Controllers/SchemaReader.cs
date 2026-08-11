using System;
using System.Data;
using System.Data.OleDb;

public class SchemaReader
{
    public static List<string> LireChamps(string cheminBD, string table)
    {
        List<string> lst = new List<string> ();
        string connStr =
            $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={cheminBD};";

        string tableName = table;


        using var conn = new OleDbConnection(connStr);
        conn.Open();

        // Restrictions : catalog, owner, table, column
        string?[] restrictions = new string?[] { null, null, tableName, null };

        DataTable columns = conn.GetSchema("Columns", restrictions);

        Console.WriteLine($"SchemaReader LireChamps Table : {tableName}");

        foreach (DataRow col in columns.Rows)
        {
            
            string colName = col["COLUMN_NAME"]?.ToString() ?? "(null)";
            string colType = col["DATA_TYPE"]?.ToString() ?? "(null)";
            lst.Add(colName);

            //Console.WriteLine($"   Champ : {colName} (Type = {colType})");
            
        }
        //Console.WriteLine(string.Join(", ", lst));
        return lst;
    }
}
