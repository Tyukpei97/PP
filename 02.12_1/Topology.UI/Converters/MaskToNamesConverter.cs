using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Topology.Core.Models;

namespace Topology.UI.Converters;

public class MaskToNamesConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2) return string.Empty;
        var maskObj = values[0];
        var pointsObj = values[1] as IEnumerable<TopologyPoint>;
        if (maskObj is not int mask || pointsObj == null) return string.Empty;

        var names = pointsObj.Where(p => (mask & (1 << p.Id)) != 0).Select(p => p.Name);
        return string.Join(", ", names);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
