using System.Windows;

namespace NfaVisualDebugger.UI.Dialogs
{
    public partial class InputDialog : Window
    {
        public string ResultText { get; private set; } = string.Empty;

        public InputDialog(string title, string initial)
        {
            InitializeComponent();
            Title = title;
            InputBox.Text = initial;
            InputBox.Focus();
            InputBox.SelectAll();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            ResultText = InputBox.Text;
            DialogResult = true;
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
