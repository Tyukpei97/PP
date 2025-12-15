using System;
using System.Globalization;
using System.Windows.Data;

namespace TmSimulator.UI.Rendering;

public class BoolToStartConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? "старт" : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
