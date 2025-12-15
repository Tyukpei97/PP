using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NfaVisualDebugger.UI.Rendering
{
    public class EdgeGeometryConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3 || values[0] is not Point from || values[1] is not Point to)
            {
                return Geometry.Empty;
            }

            var parallelIndex = values[2] is int i ? i : 0;

            if (Distance(from, to) < 1)
            {
                return BuildSelfLoop(from, parallelIndex);
            }

            return BuildCurve(from, to, parallelIndex);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        private static Geometry BuildSelfLoop(Point origin, int parallelIndex)
        {
            var radius = 30 + parallelIndex * 8;
            var start = new Point(origin.X - radius, origin.Y);
            var end = new Point(origin.X, origin.Y - radius);
            var arc1 = new ArcSegment(end, new Size(radius, radius), 0, false, SweepDirection.Clockwise, true);
            var arc2 = new ArcSegment(start, new Size(radius, radius), 0, false, SweepDirection.Clockwise, true);
            var figure = new PathFigure { StartPoint = start, IsClosed = false };
            figure.Segments.Add(arc1);
            figure.Segments.Add(arc2);
            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }

        private static Geometry BuildCurve(Point from, Point to, int parallelIndex)
        {
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
            var control = new Point(mid.X + perp.X, mid.Y + perp.Y);

            var segment = new QuadraticBezierSegment(control, to, true);
            var figure = new PathFigure { StartPoint = from, IsClosed = false };
            figure.Segments.Add(segment);
            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }

        private static double Distance(Point a, Point b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
