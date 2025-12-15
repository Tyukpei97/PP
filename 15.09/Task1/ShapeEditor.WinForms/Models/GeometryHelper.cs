using System;
using System.Drawing;

namespace ShapeEditor.WinForms.Models
{
    public static class GeometryHelper
    {
        public static float DistancePointToSegment(PointF point, PointF a, PointF b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;

            if (Math.Abs(dx) < 0.0001f && Math.Abs(dy) < 0.0001f)
            {
                return Distance(point, a);
            }

            float t = ((point.X - a.X) * dx + (point.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Clamp(t, 0f, 1f);

            var projection = new PointF(a.X + t * dx, a.Y + t * dy);
            return Distance(point, projection);
        }

        public static float Distance(PointF p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
