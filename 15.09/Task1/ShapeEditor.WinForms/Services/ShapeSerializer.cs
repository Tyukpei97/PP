using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using ShapeEditor.WinForms.Models;

namespace ShapeEditor.WinForms.Services
{
    public static class ShapeSerializer
    {
        public static void SaveToFile(string filePath, IReadOnlyList<ShapeBase> shapes)
        {
            var document = new ShapeDocument();
            foreach (var shape in shapes)
            {
                document.Shapes.Add(ShapeDto.FromShape(shape));
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(document, options);
            File.WriteAllText(filePath, json);
        }

        public static List<ShapeBase> LoadFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);

            var document = JsonSerializer.Deserialize<ShapeDocument>(json);
            if (document == null)
            {
                throw new InvalidDataException("Не удалось прочитать документ.");
            }

            var shapes = new List<ShapeBase>();
            foreach (var dto in document.Shapes)
            {
                shapes.Add(dto.ToShape());
            }

            return shapes;
        }

        private sealed class ShapeDocument
        {
            public List<ShapeDto> Shapes { get; set; } = new List<ShapeDto>();
        }

        private sealed class ShapeDto
        {
            public string Type { get; set; } = string.Empty;

            public int StrokeArgb { get; set; }

            public int FillArgb { get; set; }

            public bool IsFilled { get; set; }

            public float StrokeWidth { get; set; }

            public float X1 { get; set; }

            public float Y1 { get; set; }

            public float X2 { get; set; }

            public float Y2 { get; set; }

            public static ShapeDto FromShape(ShapeBase shape)
            {
                var dto = new ShapeDto
                {
                    StrokeArgb = shape.StrokeColor.ToArgb(),
                    FillArgb = shape.FillColor.ToArgb(),
                    IsFilled = shape.IsFilled,
                    StrokeWidth = shape.StrokeWidth
                };

                switch (shape)
                {
                    case RectangleShape rect:
                        dto.Type = "Rectangle";
                        dto.X1 = rect.Bounds.Left;
                        dto.Y1 = rect.Bounds.Top;
                        dto.X2 = rect.Bounds.Right;
                        dto.Y2 = rect.Bounds.Bottom;
                        break;

                    case EllipseShape ellipse:
                        dto.Type = "Ellipse";
                        dto.X1 = ellipse.Bounds.Left;
                        dto.Y1 = ellipse.Bounds.Top;
                        dto.X2 = ellipse.Bounds.Right;
                        dto.Y2 = ellipse.Bounds.Bottom;
                        break;

                    case LineShape line:
                        dto.Type = "Line";
                        dto.X1 = line.Start.X;
                        dto.Y1 = line.Start.Y;
                        dto.X2 = line.End.X;
                        dto.Y2 = line.End.Y;
                        dto.IsFilled = false;
                        break;

                    default:
                        throw new NotSupportedException($"Фигура типа '{shape.GetType().Name}' не поддерживается сериализацией.");
                }

                return dto;
            }

            public ShapeBase ToShape()
            {
                ShapeBase shape;
                switch (Type)
                {
                    case "Rectangle":
                        shape = new RectangleShape(new RectangleF(X1, Y1, X2 - X1, Y2 - Y1));
                        break;

                    case "Ellipse":
                        shape = new EllipseShape(new RectangleF(X1, Y1, X2 - X1, Y2 - Y1));
                        break;

                    case "Line":
                        shape = new LineShape(new PointF(X1, Y1), new PointF(X2, Y2));
                        break;

                    default:
                        throw new NotSupportedException($"Неизвестный тип фигуры: '{Type}'.");
                }

                shape.StrokeColor = Color.FromArgb(StrokeArgb);
                shape.FillColor = Color.FromArgb(FillArgb);
                shape.IsFilled = IsFilled;
                shape.StrokeWidth = StrokeWidth;

                return shape;
            }
        }
    }
}
