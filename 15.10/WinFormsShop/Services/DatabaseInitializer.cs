using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using WinFormsShop.Models;

namespace WinFormsShop.Services;

public static class DatabaseInitializer
{
    private const string AdminLogin = "admin";
    private const string AdminPassword = "admin12345";

    public static async Task<InitializationResult> InitializeAsync()
    {
        AppDataPaths.EnsureCreated();
        var result = new InitializationResult
        {
            DatabaseCreated = !File.Exists(AppDataPaths.DatabasePath),
            AdminLogin = AdminLogin,
            AdminPassword = AdminPassword
        };

        var connectionString = $"Data Source={AppDataPaths.DatabasePath}";
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        await CreateTablesAsync(connection);

        if (result.DatabaseCreated)
        {
            await SeedProductsAsync(connection);
        }

        result.AdminCreated = await EnsureAdminAsync(connection);
        return result;
    }

    private static async Task CreateTablesAsync(SqliteConnection connection)
    {
        var commands = new[]
        {
            @"CREATE TABLE IF NOT EXISTS users(
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                login TEXT UNIQUE NOT NULL,
                password_hash TEXT NOT NULL,
                email TEXT NOT NULL,
                role TEXT NOT NULL,
                created_at TEXT NOT NULL
            );",
            @"CREATE TABLE IF NOT EXISTS products(
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                description TEXT NOT NULL,
                price REAL NOT NULL,
                category TEXT NOT NULL,
                image_path TEXT NULL,
                is_active INTEGER NOT NULL DEFAULT 1
            );",
            @"CREATE TABLE IF NOT EXISTS orders(
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                order_details TEXT NOT NULL,
                total_price REAL NOT NULL,
                delivery_address TEXT NOT NULL,
                payment_method TEXT NOT NULL,
                status TEXT NOT NULL,
                created_at TEXT NOT NULL,
                FOREIGN KEY(user_id) REFERENCES users(id)
            );",
            @"CREATE TABLE IF NOT EXISTS order_items(
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                order_id INTEGER NOT NULL,
                product_id INTEGER NOT NULL,
                product_name_snapshot TEXT NOT NULL,
                unit_price_snapshot REAL NOT NULL,
                quantity INTEGER NOT NULL,
                FOREIGN KEY(order_id) REFERENCES orders(id),
                FOREIGN KEY(product_id) REFERENCES products(id)
            );"
        };

        foreach (var sql in commands)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task<bool> EnsureAdminAsync(SqliteConnection connection)
    {
        const string checkSql = "SELECT COUNT(1) FROM users WHERE role = 'admin';";
        using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = checkSql;
        var existingAdmins = (long)(await checkCmd.ExecuteScalarAsync() ?? 0);
        if (existingAdmins > 0)
        {
            return false;
        }

        var hash = PasswordHasher.HashPassword(AdminPassword);
        var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        const string insertSql = @"INSERT INTO users(login, password_hash, email, role, created_at)
                                   VALUES(@login, @password, @email, @role, @created_at);";
        using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = insertSql;
        insertCmd.Parameters.AddWithValue("@login", AdminLogin);
        insertCmd.Parameters.AddWithValue("@password", hash);
        insertCmd.Parameters.AddWithValue("@email", "admin@example.com");
        insertCmd.Parameters.AddWithValue("@role", "admin");
        insertCmd.Parameters.AddWithValue("@created_at", now);
        await insertCmd.ExecuteNonQueryAsync();
        return true;
    }

    private static async Task SeedProductsAsync(SqliteConnection connection)
    {
        const string countSql = "SELECT COUNT(1) FROM products;";
        using var countCmd = connection.CreateCommand();
        countCmd.CommandText = countSql;
        var count = (long)(await countCmd.ExecuteScalarAsync() ?? 0);
        if (count > 0)
        {
            return;
        }

        var samples = CreateSampleProducts();
        foreach (var product in samples)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO products(name, description, price, category, image_path, is_active)
                                VALUES(@name, @description, @price, @category, @image, @active);";
            cmd.Parameters.AddWithValue("@name", product.Name);
            cmd.Parameters.AddWithValue("@description", product.Description);
            cmd.Parameters.AddWithValue("@price", product.Price);
            cmd.Parameters.AddWithValue("@category", product.Category);
            cmd.Parameters.AddWithValue("@image", (object?)product.ImagePath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@active", product.IsActive ? 1 : 0);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static List<Product> CreateSampleProducts()
    {
        var laptopImage = CreatePlaceholderImage("laptop.png", Color.CadetBlue, "Ноутбук");
        var phoneImage = CreatePlaceholderImage("phone.png", Color.DarkKhaki, "Смартфон");
        var headphonesImage = CreatePlaceholderImage("headphones.png", Color.IndianRed, "Наушники");
        var watchImage = CreatePlaceholderImage("watch.png", Color.MediumSeaGreen, "Часы");

        return new List<Product>
        {
            new()
            {
                Name = "Ноутбук Старт",
                Description = "Базовый ноутбук для учебы и работы.",
                Price = 45999,
                Category = "Ноутбуки",
                ImagePath = laptopImage
            },
            new()
            {
                Name = "Смартфон Про",
                Description = "Современный смартфон с отличной камерой.",
                Price = 39999,
                Category = "Смартфоны",
                ImagePath = phoneImage
            },
            new()
            {
                Name = "Беспроводные наушники",
                Description = "Легкие и удобные, до 20 часов работы.",
                Price = 8999,
                Category = "Аудио",
                ImagePath = headphonesImage
            },
            new()
            {
                Name = "Умные часы",
                Description = "Контроль активности и уведомления на запястье.",
                Price = 12999,
                Category = "Гаджеты",
                ImagePath = watchImage
            }
        };
    }

    private static string CreatePlaceholderImage(string fileName, Color color, string text)
    {
        var path = Path.Combine(AppDataPaths.ImagesDirectory, fileName);
        if (File.Exists(path))
        {
            return path;
        }

        using var bmp = new Bitmap(180, 120);
        using var g = Graphics.FromImage(bmp);
        g.Clear(color);
        using var brush = new SolidBrush(Color.White);
        using var font = new Font("Arial", 14, FontStyle.Bold);
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, brush, (bmp.Width - size.Width) / 2, (bmp.Height - size.Height) / 2);
        bmp.Save(path);
        return path;
    }
}
