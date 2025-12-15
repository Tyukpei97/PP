using System;
using System.ComponentModel;
using System.Windows;

namespace GraphExec.UI.ViewModels;

public sealed class ConnectionViewModel : ObservableObject
{
    public NodeViewModel From { get; }
    public NodeViewModel To { get; }
    public string FromPort { get; }
    public string ToPort { get; }

    private string _pathData = string.Empty;
    private bool _energized;

    public ConnectionViewModel(NodeViewModel from, string fromPort, NodeViewModel to, string toPort)
    {
        From = from;
        To = to;
        FromPort = fromPort;
        ToPort = toPort;
        UpdatePath();
        From.PropertyChanged += OnNodeChanged;
        To.PropertyChanged += OnNodeChanged;
    }

    private void OnNodeChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeViewModel.X) || e.PropertyName == nameof(NodeViewModel.Y))
            UpdatePath();
    }

    public string PathData
    {
        get => _pathData;
        private set => SetField(ref _pathData, value);
    }

    public bool Energized
    {
        get => _energized;
        set => SetField(ref _energized, value);
    }

    public void UpdatePath()
    {
        var start = From.GetOutputAnchor(FromPort);
        var end = To.GetInputAnchor(ToPort);
        var c1 = new Point(start.X + 60, start.Y);
        var c2 = new Point(end.X - 60, end.Y);
        PathData = $"M {start.X},{start.Y} C {c1.X},{c1.Y} {c2.X},{c2.Y} {end.X},{end.Y}";
    }
}
