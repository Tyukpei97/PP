using System.Windows;
using InteractiveBuildingCrowdSimulator.App.ViewModels;

namespace InteractiveBuildingCrowdSimulator.App.Views;

public partial class AgentDetailsDialog : Window
{
    public AgentDetailsDialog(AgentDetailsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
