using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GraphExec.Core.Graph;
using GraphExec.Core.Types;
using GraphExec.Core.Values;

namespace GraphExec.UI.ViewModels;

public enum EditorMode
{
    None,
    Text,
    Slider
}

public sealed class NodeViewModel : ObservableObject
{
    public string Id { get; }
    public NodeDefinition Definition { get; }
    public List<PortViewModel> Inputs { get; }
    public List<PortViewModel> Outputs { get; }
    public Dictionary<string, string> Settings { get; }

    private double _x;
    private double _y;
    private bool _isSelected;
    private string _valueText = "—";
    private string _errorText = string.Empty;
    private string _badge = string.Empty;
    private double _sliderValue;
    private string _textValue = string.Empty;

    public NodeViewModel(NodeDefinition definition, double x, double y)
    {
        Id = Guid.NewGuid().ToString("N");
        Definition = definition;
        _x = x;
        _y = y;
        Inputs = definition.Inputs.Select((p, i) => new PortViewModel(this, p.Name, p.Type, i, true)).ToList();
        Outputs = definition.Outputs.Select((p, i) => new PortViewModel(this, p.Name, p.Type, i, false)).ToList();
        Settings = new Dictionary<string, string>();
        Mode = ResolveEditorMode(definition);
        if (Mode == EditorMode.Slider) _sliderValue = 0;
        if (Mode == EditorMode.Text) _textValue = "0";
    }

    public EditorMode Mode { get; }

    public double X
    {
        get => _x;
        set => SetField(ref _x, value);
    }

    public double Y
    {
        get => _y;
        set => SetField(ref _y, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public string ValueText
    {
        get => _valueText;
        set => SetField(ref _valueText, value);
    }

    public string ErrorText
    {
        get => _errorText;
        set => SetField(ref _errorText, value);
    }

    public string Badge
    {
        get => _badge;
        set => SetField(ref _badge, value);
    }

    public double SliderValue
    {
        get => _sliderValue;
        set
        {
            if (Math.Abs(_sliderValue - value) > 0.0001)
            {
                _sliderValue = value;
                Settings["value"] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                OnPropertyChanged();
            }
        }
    }

    public string TextValue
    {
        get => _textValue;
        set
        {
            if (_textValue != value)
            {
                _textValue = value;
                Settings["value"] = value;
                OnPropertyChanged();
            }
        }
    }

    public double Width => 240;
    public double HeaderHeight => 32;
    public double PortHeight => 22;

    public double Height
    {
        get
        {
            var rows = Math.Max(Inputs.Count, Outputs.Count);
            return HeaderHeight + rows * PortHeight + (Mode != EditorMode.None ? 40 : 16);
        }
    }

    public Point GetInputAnchor(string portName)
    {
        var idx = Inputs.FindIndex(p => p.Name == portName);
        return new Point(X, Y + HeaderHeight + idx * PortHeight + PortHeight / 2);
    }

    public Point GetOutputAnchor(string portName)
    {
        var idx = Outputs.FindIndex(p => p.Name == portName);
        return new Point(X + Width, Y + HeaderHeight + idx * PortHeight + PortHeight / 2);
    }

    public NodeInstance ToNodeInstance()
    {
        return new NodeInstance(Id, Definition, X, Y, Settings);
    }

    public void ApplyOutcome(EvaluationOutcome? outcome)
    {
        if (outcome == null)
        {
            ValueText = "—";
            ErrorText = string.Empty;
            Badge = string.Empty;
            return;
        }

        if (outcome.HasError)
        {
            ErrorText = outcome.ErrorMessage ?? "Ошибка";
            ValueText = "Ошибка";
            Badge = "⚠";
            return;
        }

        ErrorText = string.Empty;
        Badge = outcome.DisplayHint ?? string.Empty;
        var value = outcome.Outputs.Values.FirstOrDefault();
        ValueText = value?.ToString() ?? "—";
    }

    private static EditorMode ResolveEditorMode(NodeDefinition def)
    {
        if (def.Code == "source.slider")
            return EditorMode.Slider;
        if (def.Code.StartsWith("const.") || def.Code == "source.text" || def.Code == "source.list" || def.Code == "op.gt")
            return EditorMode.Text;
        return EditorMode.None;
    }
}
