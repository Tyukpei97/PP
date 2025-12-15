using System.Windows.Forms;

namespace ShapeEditor.WinForms
{
    static partial class ApplicationConfiguration
    {
        public static void ConfigureApplication()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }
    }
}
