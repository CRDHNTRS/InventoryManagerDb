using System.Data.SQLite;

public static class Db
{
    private static string connectionString = "Data Source=Inventory.db";

    public static SQLiteConnection GetConnection()
    {
        return new SQLiteConnection(connectionString);
    }

    public static void Initialize()
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"CREATE TABLE IF NOT EXISTS Products (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Category TEXT NOT NULL,
                        Quantity INTEGER NOT NULL,
                        Price REAL NOT NULL
                      )";

        using var cmd = new SQLiteCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }
}