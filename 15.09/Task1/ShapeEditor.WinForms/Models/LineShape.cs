using System.Collections.Generic;
using System.Drawing;

namespace ShapeEditor.WinForms.Models
{
    public sealed class LineShape : ShapeBase
    {
        public PointF Start { get; set; }

        public PointF End { get; set; }

        public LineShape()
        {
            Start = new PointF(30, 30);
            End = new PointF(180, 140);
            IsFilled = false;
        }

        public LineShape(PointF start, PointF end)
        {
            Start = start;
            End = end;
            IsFilled = false;
        }

        public override void Draw(Graphics graphics)
        {
            using var pen = new Pen(StrokeColor, StrokeWidth);
            graphics.DrawLine(pen, Start, End);
        }

        public override RectangleF GetBounds()
        {
            float x1 = System.Math.Min(Start.X, End.X);
            float y1 = System.Math.Min(Start.Y, End.Y);
            float x2 = System.Math.Max(Start.X, End.X);
            float y2 = System.Math.Max(Start.Y, End.Y);

            return new RectangleF(x1, y1, System.Math.Max(1f, x2 - x1), System.Math.Max(1f, y2 - y1));
        }

        public override bool HitTest(PointF point, float tolerance)
        {
            float distance = GeometryHelper.DistancePointToSegment(point, Start, End);
            return distance <= (tolerance + StrokeWidth / 2f);
        }

        public override void Move(float dx, float dy)
        {
            Start = new PointF(Start.X + dx, Start.Y + dy);
            End = new PointF(End.X + dx, End.Y + dy);
        }

        public override void Resize(ShapeHandle handle, PointF delta)
        {
            if (handle == ShapeHandle.StartPoint)
            {
                Start = new PointF(Start.X + delta.X, Start.Y + delta.Y);
            }
            else if (handle == ShapeHandle.EndPoint)
            {
                End = new PointF(End.X + delta.X, End.Y + delta.Y);
            }
        }

        public override IReadOnlyDictionary<ShapeHandle, RectangleF> GetHandleRects(float handleSize)
        {
            return new Dictionary<ShapeHandle, RectangleF>
            {
                [ShapeHandle.StartPoint] = MakeHandleRect(Start, handleSize),
                [ShapeHandle.EndPoint] = MakeHandleRect(End, handleSize),
            };
        }
    }
}
