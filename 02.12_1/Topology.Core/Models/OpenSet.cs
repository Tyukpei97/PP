using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Topology.Core.Models;

/// <summary>
/// Открытое множество в виде битовой маски.
/// </summary>
public class OpenSet : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private int _mask;
    private string? _colorHex;
    private double _opacity = 0.35;
    private bool _isVisible = true;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Битовая маска точек (до 12 бит).
    /// </summary>
    public int Mask
    {
        get => _mask;
        set { _mask = value; OnPropertyChanged(); }
    }

    public string? ColorHex
    {
        get => _colorHex;
        set { _colorHex = value; OnPropertyChanged(); }
    }

    public double Opacity
    {
        get => _opacity;
        set { _opacity = value; OnPropertyChanged(); }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set { _isVisible = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
