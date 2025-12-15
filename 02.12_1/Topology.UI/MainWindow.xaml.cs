using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using Topology.Core.Models;
using Topology.UI.ViewModels;

namespace Topology.UI;

public partial class MainWindow : Window
{
    private bool _isDragging;
    private int _dragPointId = -1;
    private Point _dragOffset;
    private bool _historyCaptured;

    private MainViewModel Vm => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Vm.Space.Points.CollectionChanged += (_, _) => RedrawCanvas();
        Vm.Space.OpenSets.CollectionChanged += (_, _) =>
        {
            HookOpenSetEvents();
            RedrawCanvas();
        };
        HookOpenSetEvents();
        RedrawCanvas();
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.Source is FrameworkElement fe && fe.Tag is TopologyPoint point)
        {
            Vm.ToggleSelectPoint(point.Id);
            RedrawCanvas();
            e.Handled = true;
            return;
        }

        var pos = e.GetPosition(DrawCanvas);
        Vm.AddPointAt(pos.X, pos.Y);
        RedrawCanvas();
    }

    private void Point_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is TopologyPoint point)
        {
            _isDragging = true;
            _dragPointId = point.Id;
            var pos = e.GetPosition(DrawCanvas);
            _dragOffset = new Point(pos.X - point.X, pos.Y - point.Y);
            _historyCaptured = false;
            DrawCanvas.CaptureMouse();
            Vm.ToggleSelectPoint(point.Id);
            e.Handled = true;
        }
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _dragPointId < 0) return;
        if (!_historyCaptured)
        {
            Vm.BeginPointMove();
            _historyCaptured = true;
        }

        var pos = e.GetPosition(DrawCanvas);
        Vm.MovePoint(_dragPointId, pos.X - _dragOffset.X, pos.Y - _dragOffset.Y);
        RedrawCanvas();
    }

    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        _dragPointId = -1;
        DrawCanvas.ReleaseMouseCapture();
    }

    private void DeletePoint_Click(object sender, RoutedEventArgs e)
    {
        Vm.DeleteSelectedPoints();
        RedrawCanvas();
    }

    private void HookOpenSetEvents()
    {
        foreach (var set in Vm.Space.OpenSets)
        {
            set.PropertyChanged -= OpenSetPropertyChanged;
            set.PropertyChanged += OpenSetPropertyChanged;
        }
    }

    private void OpenSetPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RedrawCanvas();
    }

    private void RedrawCanvas()
    {
        if (DrawCanvas == null) return;
        DrawCanvas.Children.Clear();

        foreach (var set in Vm.Space.OpenSets.Where(s => s.IsVisible))
        {
            var color = TryParseColor(set.ColorHex ?? "#88AADD");
            var fill = new SolidColorBrush(color) { Opacity = set.Opacity };
            foreach (var point in Vm.Space.Points.Where(p => (set.Mask & (1 << p.Id)) != 0))
            {
                var halo = new Ellipse
                {
                    Width = 68,
                    Height = 68,
                    Fill = fill,
                    StrokeThickness = 0,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(halo, point.X - 34);
                Canvas.SetTop(halo, point.Y - 34);
                DrawCanvas.Children.Add(halo);
            }
        }

        foreach (var point in Vm.Space.Points)
        {
            var isSelected = Vm.SelectedPoints.Contains(point.Id);
            var ellipse = new Ellipse
            {
                Width = 22,
                Height = 22,
                Stroke = Brushes.Black,
                StrokeThickness = isSelected ? 2.5 : 1,
                Fill = isSelected ? Brushes.Gold : Brushes.White,
                Tag = point
            };
            ellipse.MouseLeftButtonDown += Point_MouseDown;

            var text = new System.Windows.Controls.TextBlock
            {
                Text = point.Name,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                Tag = point
            };
            text.MouseLeftButtonDown += Point_MouseDown;

            Canvas.SetLeft(ellipse, point.X - 11);
            Canvas.SetTop(ellipse, point.Y - 11);
            Canvas.SetLeft(text, point.X + 12);
            Canvas.SetTop(text, point.Y - 6);

            var tooltip = Vm.GetTooltip(point);
            ellipse.ToolTip = tooltip;
            text.ToolTip = tooltip;

            DrawCanvas.Children.Add(ellipse);
            DrawCanvas.Children.Add(text);
        }
    }

    private static Color TryParseColor(string hex)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }
        catch
        {
            return Colors.LightBlue;
        }
    }
}
