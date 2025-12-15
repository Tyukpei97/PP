using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using InteractiveBuildingCrowdSimulator.App.Models;

namespace InteractiveBuildingCrowdSimulator.App.ViewModels;

/// <summary>
/// Сравнение значения перечисления для переключателей.
/// </summary>
public class EnumEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Equals(value, parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Equals(value, true) ? parameter! : Binding.DoNothing;
    }
}

/// <summary>
/// Преобразует дверь и вью-модель в координату для линии между областями.
/// </summary>
public class DoorCoordinateConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not Door door || parameter is not string param)
        {
            return 0d;
        }

        BuildingMap? map = values[1] switch
        {
            EditorViewModel editor => editor.Map,
            SimulationViewModel sim => sim.Map,
            _ => null
        };
        if (map is null)
        {
            return 0d;
        }

        var from = map.FindArea(door.FromAreaId);
        var to = map.FindArea(door.ToAreaId);
        return param switch
        {
            "FromX" => from?.Center.X ?? 0,
            "FromY" => from?.Center.Y ?? 0,
            "ToX" => to?.Center.X ?? 0,
            "ToY" => to?.Center.Y ?? 0,
            _ => 0d
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Преобразует Color в кисть для визуализации агентов.
/// </summary>
public class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is System.Windows.Media.Color color)
        {
            return new System.Windows.Media.SolidColorBrush(color);
        }

        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.SteelBlue);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Преобразует набор точек графика и размеры холста в PointCollection для Polyline.
/// </summary>
public class ChartSeriesConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3 ||
            values[0] is not System.Collections.IEnumerable series ||
            values[1] is not double width ||
            values[2] is not double height ||
            width <= 0 || height <= 0)
        {
            return new System.Windows.Media.PointCollection();
        }

        var points = series.Cast<ChartPoint>().ToList();
        if (points.Count == 0)
        {
            return new System.Windows.Media.PointCollection();
        }

        var maxX = points.Max(p => p.X);
        var maxY = Math.Max(points.Max(p => p.Y), 0.0001);
        var pc = new System.Windows.Media.PointCollection();

        foreach (var p in points)
        {
            var x = maxX <= 0 ? 0 : p.X / maxX * width;
            var y = height - (p.Y / maxY * height);
            pc.Add(new System.Windows.Point(x, y));
        }

        return pc;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
