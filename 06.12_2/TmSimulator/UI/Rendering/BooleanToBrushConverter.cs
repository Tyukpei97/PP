using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TmSimulator.UI.Rendering;

public class BooleanToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var flag = value is bool b && b;
        var param = parameter as string ?? string.Empty;

        return param switch
        {
            "Halting" => flag ? new SolidColorBrush(Color.FromRgb(255, 99, 71)) : new SolidColorBrush(Color.FromRgb(46, 134, 222)),
            "Head" => flag ? new SolidColorBrush(Color.FromRgb(0, 150, 136)) : new SolidColorBrush(Color.FromRgb(18, 32, 47)),
            _ => flag ? Brushes.LightGreen : Brushes.White
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
