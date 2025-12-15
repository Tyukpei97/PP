using System.Threading.Tasks;
using WinFormsShop.Models;

namespace WinFormsShop.Services;

public static class AppInitializer
{
    public static InitializationResult? Result { get; private set; }

    public static async Task InitializeAsync()
    {
        Result = await DatabaseInitializer.InitializeAsync();
    }
}
