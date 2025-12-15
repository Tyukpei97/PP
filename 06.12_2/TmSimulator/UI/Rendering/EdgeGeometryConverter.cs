using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TmSimulator.UI.Rendering;

public class EdgeGeometryConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 4 ||
            values[0] is not double x1 ||
            values[1] is not double y1 ||
            values[2] is not double x2 ||
            values[3] is not double y2)
        {
            return Geometry.Empty;
        }

        var line = new LineGeometry
        {
            StartPoint = new System.Windows.Point(x1, y1),
            EndPoint = new System.Windows.Point(x2, y2)
        };

        return line;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
