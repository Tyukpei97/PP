using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsShop.Forms;
using WinFormsShop.Services;

namespace WinFormsShop;

internal static class Program
{
    [STAThread]
    private static async Task Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            await AppInitializer.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось инициализировать приложение: {ex.Message}", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(new LoginForm());
    }
}
