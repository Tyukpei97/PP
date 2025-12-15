using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GraphExec.Core.Graph;
using GraphExec.UI.ViewModels;

namespace GraphExec.UI;

public partial class MainWindow : Window
{
    private Point? _dragStart;
    private NodeViewModel? _dragNode;
    private bool _isPanning;
    private Point _panStart;
    private NodeViewModel? _connectingFrom;
    private string? _connectingPort;
    private double _zoom = 1.0;

    private MainViewModel Vm => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        Vm.Nodes.CollectionChanged += Nodes_CollectionChanged;
        ApplyRussianUi();
    }

    private void ApplyRussianUi()
    {
        Title = "Реактивный граф функций";
        NewButton.Content = "Новый";
        DemoButton.Content = "Демо";
        SaveButton.Content = "Сохранить";
        LoadButton.Content = "Загрузить";
        AutoButton.Content = "Автовыравнивание";
        MacroButton.Content = "Создать макрос";
        HintText.Text = "Перетаскивайте узлы на холст. Колесо мыши — зум, средняя кнопка — панорамирование.";
        PaletteHeader.Text = "Палитра узлов";
    }

    private void Nodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems.OfType<NodeViewModel>())
                item.PropertyChanged += Node_PropertyChanged;
        }
    }

    private void Node_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is NodeViewModel node && (e.PropertyName == nameof(NodeViewModel.SliderValue) || e.PropertyName == nameof(NodeViewModel.TextValue)))
        {
            Vm.Recalculate(new[] { node.Id });
        }
    }

    private void PaletteList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;
        if (PaletteList.SelectedItem is NodeDefinition def)
        {
            DragDrop.DoDragDrop(PaletteList, def, DragDropEffects.Copy);
        }
    }

    private void EditorCanvas_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(NodeDefinition)) is NodeDefinition def)
        {
            var point = e.GetPosition(EditorCanvas);
            point = ScreenToCanvas(point);
            Vm.AddNode(def, point);
        }
    }

    private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is NodeViewModel node)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                Vm.ToggleSelection(node);
                return;
            }
            _dragNode = node;
            _dragStart = e.GetPosition(EditorCanvas);
            border.CaptureMouse();
        }
    }

    private void Node_MouseMove(object sender, MouseEventArgs e)
    {
        if (_dragNode != null && _dragStart is Point start && e.LeftButton == MouseButtonState.Pressed)
        {
            var current = e.GetPosition(EditorCanvas);
            var delta = current - start;
            delta = new Vector(delta.X / _zoom, delta.Y / _zoom);
            _dragNode.X += delta.X;
            _dragNode.Y += delta.Y;
            _dragStart = current;
            foreach (var c in Vm.Connections.Where(c => c.From == _dragNode || c.To == _dragNode))
                c.UpdatePath();
        }
    }

    private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border)
            border.ReleaseMouseCapture();
        _dragNode = null;
        _dragStart = null;
    }

    private void EditorCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        var delta = e.Delta > 0 ? 0.1 : -0.1;
        _zoom = Math.Clamp(_zoom + delta, 0.4, 2.5);
        ZoomTransform.ScaleX = _zoom;
        ZoomTransform.ScaleY = _zoom;
    }

    private void EditorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
        {
            _isPanning = true;
            _panStart = e.GetPosition(this);
            Cursor = Cursors.Hand;
        }
    }

    private void EditorCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isPanning && e.MiddleButton == MouseButtonState.Pressed)
        {
            var pos = e.GetPosition(this);
            var delta = pos - _panStart;
            PanTransform.X += delta.X;
            PanTransform.Y += delta.Y;
            _panStart = pos;
        }
    }

    private void EditorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _isPanning = false;
        Cursor = Cursors.Arrow;
    }

    private void OutputPort_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is PortViewModel port)
        {
            _connectingFrom = port.Owner;
            _connectingPort = port.Name;
            Vm.Status = $"Соединение: выберите вход для {port.Name}";
        }
    }

    private void InputPort_Click(object sender, RoutedEventArgs e)
    {
        if (_connectingFrom == null || _connectingPort == null)
            return;
        if (sender is Button btn && btn.Tag is PortViewModel port)
        {
            if (port.Owner == _connectingFrom)
                return;
            if (Vm.TryAddConnection(_connectingFrom, _connectingPort, port.Owner, port.Name, out var msg))
                Vm.Status = "Связь добавлена";
            else
                Vm.Status = msg;
        }
        _connectingFrom = null;
        _connectingPort = null;
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        Vm.Clear();
    }

    private void Demo_Click(object sender, RoutedEventArgs e)
    {
        Vm.LoadDemo();
    }

    private void Auto_Click(object sender, RoutedEventArgs e)
    {
        Vm.AutoLayout();
    }

    private void Macro_Click(object sender, RoutedEventArgs e)
    {
        var name = Prompt("Название макроса", "Введите имя макроузла:");
        if (string.IsNullOrWhiteSpace(name))
            return;
        Vm.CreateMacro(name);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON|*.json",
            FileName = "graph.json"
        };
        if (dialog.ShowDialog() == true)
        {
            var json = Vm.ExportJson();
            System.IO.File.WriteAllText(dialog.FileName, json);
        }
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON|*.json"
        };
        if (dialog.ShowDialog() == true)
        {
            var json = System.IO.File.ReadAllText(dialog.FileName);
            Vm.ImportJson(json);
        }
    }

    private Point ScreenToCanvas(Point p)
    {
        return new Point((p.X - PanTransform.X) / _zoom, (p.Y - PanTransform.Y) / _zoom);
    }

    private string? Prompt(string title, string hint)
    {
        var window = new Window
        {
            Title = title,
            Width = 320,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = Brushes.White,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
        };
        var panel = new StackPanel { Margin = new Thickness(10) };
        panel.Children.Add(new TextBlock { Text = hint, Margin = new Thickness(0, 0, 0, 6) });
        var box = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
        panel.Children.Add(box);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var ok = new Button { Content = "ОК", Width = 70, Margin = new Thickness(0, 0, 6, 0) };
        var cancel = new Button { Content = "Отмена", Width = 70 };
        ok.Click += (_, _) => { window.DialogResult = true; window.Close(); };
        cancel.Click += (_, _) => { window.DialogResult = false; window.Close(); };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        panel.Children.Add(buttons);
        window.Content = panel;
        var result = window.ShowDialog();
        return result == true ? box.Text : null;
    }
}
