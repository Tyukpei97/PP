using System.Windows;
using System.Windows.Input;
using TmSimulator.UI.ViewModels;
using TmSimulator.UI.Views;

namespace TmSimulator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private MachineWorkspaceViewModel? GetActiveWorkspace()
    {
        if (MachineTabs?.SelectedContent is MachineWorkspaceView view && view.DataContext is MachineWorkspaceViewModel vm)
        {
            return vm;
        }
        return null;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        var vm = GetActiveWorkspace();
        if (vm == null) return;

        if (e.Key == Key.Space)
        {
            if (vm.IsRunning)
                vm.PauseCommand.Execute(null);
            else
                vm.StartCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.F10)
        {
            vm.StepCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.R && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            vm.ResetCommand.Execute(null);
            e.Handled = true;
        }
    }
}
