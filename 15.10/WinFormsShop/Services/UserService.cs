using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using WinFormsShop.Models;

namespace WinFormsShop.Services;

public class UserService
{
    public async Task<(bool Success, string? Error)> RegisterAsync(string login, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            return (false, "Логин обязателен.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            return (false, "Введите корректный email.");
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return (false, "Пароль должен быть не короче 8 символов.");
        }

        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();

        await using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(1) FROM users WHERE login = @login;";
            checkCmd.Parameters.AddWithValue("@login", login);
            var exists = (long)(await checkCmd.ExecuteScalarAsync() ?? 0);
            if (exists > 0)
            {
                return (false, "Пользователь с таким логином уже существует.");
            }
        }

        var hashed = PasswordHasher.HashPassword(password);
        var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        await using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText =
            @"INSERT INTO users(login, password_hash, email, role, created_at)
              VALUES(@login, @password_hash, @email, @role, @created_at);";
        insertCmd.Parameters.AddWithValue("@login", login);
        insertCmd.Parameters.AddWithValue("@password_hash", hashed);
        insertCmd.Parameters.AddWithValue("@email", email);
        insertCmd.Parameters.AddWithValue("@role", "user");
        insertCmd.Parameters.AddWithValue("@created_at", now);
        await insertCmd.ExecuteNonQueryAsync();
        return (true, null);
    }

    public async Task<User?> AuthenticateAsync(string login, string password)
    {
        await using var connection = DatabaseService.CreateConnection();
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT id, login, password_hash, email, role, created_at
                            FROM users WHERE login = @login;";
        cmd.Parameters.AddWithValue("@login", login);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var storedHash = reader.GetString(2);
        if (!PasswordHasher.Verify(password, storedHash))
        {
            return null;
        }

        return new User
        {
            Id = reader.GetInt32(0),
            Login = reader.GetString(1),
            PasswordHash = storedHash,
            Email = reader.GetString(3),
            Role = reader.GetString(4),
            CreatedAt = DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
        };
    }
}
