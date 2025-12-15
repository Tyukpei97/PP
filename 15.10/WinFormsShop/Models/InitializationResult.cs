namespace WinFormsShop.Models;

public class InitializationResult
{
    public bool DatabaseCreated { get; set; }
    public bool AdminCreated { get; set; }
    public string AdminLogin { get; set; } = "admin";
    public string AdminPassword { get; set; } = "admin12345";
}
