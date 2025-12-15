using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InteractiveBuildingCrowdSimulator.App.ViewModels;

namespace InteractiveBuildingCrowdSimulator.App.Views;

public partial class EditorView : UserControl
{
    private bool _isDragging;
    private Point _lastPoint;

    public EditorView()
    {
        InitializeComponent();
    }

    private void MapCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not EditorViewModel vm)
        {
            return;
        }

        var position = e.GetPosition(MapCanvas);
        vm.HandleCanvasClick(position);

        if (vm.SelectedTool == EditorTool.Select && vm.SelectedArea is not null && vm.SelectedArea.Bounds.Contains(position))
        {
            _isDragging = true;
            _lastPoint = position;
            MapCanvas.CaptureMouse();
        }
    }

    private void MapCanvas_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        if (DataContext is not EditorViewModel vm)
        {
            return;
        }

        var position = e.GetPosition(MapCanvas);
        var delta = position - _lastPoint;
        vm.MoveSelected(delta);
        _lastPoint = position;
    }

    private void MapCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        MapCanvas.ReleaseMouseCapture();
    }
}
