using System;
using System.IO;

namespace WinFormsShop.Services;

public static class AppDataPaths
{
    public static string BaseDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinFormsShop");

    public static string DatabasePath => Path.Combine(BaseDirectory, "shop.db");
    public static string ImagesDirectory => Path.Combine(BaseDirectory, "Images");
    public static string ConfigDirectory => Path.Combine(BaseDirectory, "Config");
    public static string SmtpConfigPath => Path.Combine(ConfigDirectory, "smtp.cfg");

    public static void EnsureCreated()
    {
        Directory.CreateDirectory(BaseDirectory);
        Directory.CreateDirectory(ImagesDirectory);
        Directory.CreateDirectory(ConfigDirectory);
    }
}
