using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShapeEditor.WinForms.Models
{
    public sealed class RectangleShape : ShapeBase
    {
        public RectangleF Bounds { get; set; }

        public RectangleShape()
        {
            Bounds = new RectangleF(10, 10, 100, 80);
        }

        public RectangleShape(RectangleF bounds)
        {
            Bounds = bounds;
        }

        public override void Draw(Graphics graphics)
        {
            using var pen = new Pen(StrokeColor, StrokeWidth);

            if (IsFilled)
            {
                using var brush = new SolidBrush(FillColor);
                graphics.FillRectangle(brush, Bounds);
            }

            graphics.DrawRectangle(pen, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
        }

        public override RectangleF GetBounds()
        {
            return Bounds;
        }

        public override bool HitTest(PointF point, float tolerance)
        {
            using var path = new GraphicsPath();
            path.AddRectangle(Bounds);

            if (IsFilled)
            {
                return path.IsVisible(point);
            }

            using var pen = new Pen(StrokeColor, StrokeWidth + tolerance * 2f);
            return path.IsOutlineVisible(point, pen);
        }

        public override void Move(float dx, float dy)
        {
            Bounds = new RectangleF(Bounds.X + dx, Bounds.Y + dy, Bounds.Width, Bounds.Height);
        }

        public override void Resize(ShapeHandle handle, PointF delta)
        {
            if (handle == ShapeHandle.None || handle == ShapeHandle.Move)
            {
                return;
            }

            float x = Bounds.X;
            float y = Bounds.Y;
            float w = Bounds.Width;
            float h = Bounds.Height;

            switch (handle)
            {
                case ShapeHandle.TopLeft:
                    x += delta.X;
                    y += delta.Y;
                    w -= delta.X;
                    h -= delta.Y;
                    break;

                case ShapeHandle.TopRight:
                    y += delta.Y;
                    w += delta.X;
                    h -= delta.Y;
                    break;

                case ShapeHandle.BottomLeft:
                    x += delta.X;
                    w -= delta.X;
                    h += delta.Y;
                    break;

                case ShapeHandle.BottomRight:
                    w += delta.X;
                    h += delta.Y;
                    break;
            }

            var a = new PointF(x, y);
            var b = new PointF(x + w, y + h);

            var normalized = NormalizeRect(a, b);
            normalized = EnsureMinSize(normalized, 10f);

            Bounds = normalized;
        }

        public override IReadOnlyDictionary<ShapeHandle, RectangleF> GetHandleRects(float handleSize)
        {
            var tl = new PointF(Bounds.Left, Bounds.Top);
            var tr = new PointF(Bounds.Right, Bounds.Top);
            var bl = new PointF(Bounds.Left, Bounds.Bottom);
            var br = new PointF(Bounds.Right, Bounds.Bottom);

            return new Dictionary<ShapeHandle, RectangleF>
            {
                [ShapeHandle.TopLeft] = MakeHandleRect(tl, handleSize),
                [ShapeHandle.TopRight] = MakeHandleRect(tr, handleSize),
                [ShapeHandle.BottomLeft] = MakeHandleRect(bl, handleSize),
                [ShapeHandle.BottomRight] = MakeHandleRect(br, handleSize),
            };
        }
    }
}
