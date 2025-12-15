using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InteractiveBuildingCrowdSimulator.App.ViewModels;

namespace InteractiveBuildingCrowdSimulator.App.Views;

public partial class SimulationView : UserControl
{
    public SimulationView()
    {
        InitializeComponent();
    }

    private void SimulationCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not SimulationViewModel vm)
        {
            return;
        }

        var zoom = ZoomSlider.Value;
        var position = e.GetPosition(SimulationCanvas);
        var world = new Point(position.X / zoom, position.Y / zoom);
        vm.SelectAgentByPosition(world, 1.0 / zoom + 0.2);
    }
}
