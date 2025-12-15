using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NfaVisualDebugger.UI.Rendering
{
    public class EdgeLabelPointConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3 || values[0] is not Point from || values[1] is not Point to)
            {
                return targetType == typeof(double) ? 0d : new Point();
            }

            var parallelIndex = values[2] is int i ? i : 0;

            if (Distance(from, to) < 1)
            {
                return new Point(from.X - 10, from.Y - 40 - parallelIndex * 8);
            }

            var mid = new Point((from.X + to.X) / 2, (from.Y + to.Y) / 2);
            var dx = to.X - from.X;
            var dy = to.Y - from.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 1)
            {
                length = 1;
            }

            var offset = 20 + 10 * parallelIndex;
            var perp = new Vector(-dy / length * offset, dx / length * offset);
            var point = new Point(mid.X + perp.X, mid.Y + perp.Y);

            if (targetType == typeof(double))
            {
                return string.Equals(parameter as string, "Y", StringComparison.OrdinalIgnoreCase) ? point.Y : point.X;
            }

            return point;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        private static double Distance(Point a, Point b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
