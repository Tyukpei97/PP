using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShapeEditor.WinForms.Models
{
    public abstract class ShapeBase
    {
        public Color StrokeColor { get; set; } = Color.Black;

        public Color FillColor { get; set; } = Color.Gainsboro;

        public bool IsFilled { get; set; } = true;

        public float StrokeWidth { get; set; } = 2f;

        public abstract void Draw(Graphics graphics);

        public abstract RectangleF GetBounds();

        public abstract bool HitTest(PointF point, float tolerance);

        public abstract void Move(float dx, float dy);

        public abstract void Resize(ShapeHandle handle, PointF delta);

        public virtual IReadOnlyDictionary<ShapeHandle, RectangleF> GetHandleRects(float handleSize)
        {
            return new Dictionary<ShapeHandle, RectangleF>();
        }

        public ShapeHandle GetHandleAt(PointF point, float handleSize)
        {
            foreach (var pair in GetHandleRects(handleSize))
            {
                if (pair.Value.Contains(point))
                {
                    return pair.Key;
                }
            }

            return ShapeHandle.None;
        }

        public void DrawSelection(Graphics graphics, float handleSize)
        {
            using var pen = new Pen(Color.DodgerBlue, 1f)
            {
                DashStyle = DashStyle.Dash
            };

            var bounds = GetBounds();
            graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width, bounds.Height);

            using var brush = new SolidBrush(Color.White);
            using var outline = new Pen(Color.DodgerBlue, 1f);

            foreach (var rect in GetHandleRects(handleSize).Values)
            {
                graphics.FillRectangle(brush, rect);
                graphics.DrawRectangle(outline, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        protected static RectangleF NormalizeRect(PointF start, PointF end)
        {
            float x1 = Math.Min(start.X, end.X);
            float y1 = Math.Min(start.Y, end.Y);
            float x2 = Math.Max(start.X, end.X);
            float y2 = Math.Max(start.Y, end.Y);

            return new RectangleF(x1, y1, x2 - x1, y2 - y1);
        }

        protected static RectangleF EnsureMinSize(RectangleF rect, float minSize)
        {
            float width = Math.Max(rect.Width, minSize);
            float height = Math.Max(rect.Height, minSize);

            return new RectangleF(rect.X, rect.Y, width, height);
        }

        protected static RectangleF MakeHandleRect(PointF center, float size)
        {
            float half = size / 2f;
            return new RectangleF(center.X - half, center.Y - half, size, size);
        }
    }
}
