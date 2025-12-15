using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WinFormsShop.Models;

namespace WinFormsShop.Services;

public class SmtpSettingsService
{
    public async Task SaveAsync(SmtpSettings settings)
    {
        AppDataPaths.EnsureCreated();
        var lines = new List<string>
        {
            $"Host={settings.Host}",
            $"Port={settings.Port}",
            $"EnableSsl={settings.EnableSsl}",
            $"Username={settings.Username}",
            $"Password={settings.Password}",
            $"FromEmail={settings.FromEmail}"
        };
        var raw = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));
        var protectedBytes = ProtectedData.Protect(raw, null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(AppDataPaths.SmtpConfigPath, protectedBytes);
    }

    public async Task<SmtpSettings?> LoadAsync()
    {
        try
        {
            if (File.Exists(AppDataPaths.SmtpConfigPath))
            {
                var protectedBytes = await File.ReadAllBytesAsync(AppDataPaths.SmtpConfigPath);
                var raw = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                var text = Encoding.UTF8.GetString(raw);
                var dict = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                               .Select(line => line.Split('=', 2))
                               .Where(parts => parts.Length == 2)
                               .ToDictionary(parts => parts[0], parts => parts[1]);

                return new SmtpSettings
                {
                    Host = dict.GetValueOrDefault("Host", string.Empty),
                    Port = int.TryParse(dict.GetValueOrDefault("Port"), out var port) ? port : 587,
                    EnableSsl = bool.TryParse(dict.GetValueOrDefault("EnableSsl"), out var ssl) && ssl,
                    Username = dict.GetValueOrDefault("Username", string.Empty),
                    Password = dict.GetValueOrDefault("Password", string.Empty),
                    FromEmail = dict.GetValueOrDefault("FromEmail", string.Empty)
                };
            }
        }
        catch
        {
            // corrupted config, fallback to env variables
        }

        var host = Environment.GetEnvironmentVariable("SMTP_HOST");
        if (string.IsNullOrWhiteSpace(host))
        {
            return null;
        }

        return new SmtpSettings
        {
            Host = host,
            Port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var portEnv) ? portEnv : 587,
            EnableSsl = !string.Equals(Environment.GetEnvironmentVariable("SMTP_SSL"), "false", StringComparison.OrdinalIgnoreCase),
            Username = Environment.GetEnvironmentVariable("SMTP_USER") ?? string.Empty,
            Password = Environment.GetEnvironmentVariable("SMTP_PASS") ?? string.Empty,
            FromEmail = Environment.GetEnvironmentVariable("SMTP_FROM") ?? Environment.GetEnvironmentVariable("SMTP_USER") ?? string.Empty
        };
    }
}
