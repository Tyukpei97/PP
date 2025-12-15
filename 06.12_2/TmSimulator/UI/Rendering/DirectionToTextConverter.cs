using System;
using System.Globalization;
using System.Windows.Data;
using TmSimulator.Core.Machine;

namespace TmSimulator.UI.Rendering;

public class DirectionToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Direction direction) return string.Empty;
        return direction switch
        {
            Direction.Left => "Влево",
            Direction.Right => "Вправо",
            _ => "Стоять"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
