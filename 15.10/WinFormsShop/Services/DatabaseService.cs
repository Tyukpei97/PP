using Microsoft.Data.Sqlite;

namespace WinFormsShop.Services;

public static class DatabaseService
{
    private static string ConnectionString => $"Data Source={AppDataPaths.DatabasePath}";

    public static SqliteConnection CreateConnection() => new(ConnectionString);
}
