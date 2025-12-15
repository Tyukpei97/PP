using System.Drawing.Drawing2D;
using System.Globalization;

namespace FunctionGraphingCalculator;

public partial class Form1 : Form
{
    private readonly List<List<(double X, double Y)>> _plotSegments = new();
    private ExpressionEvaluator? _evaluator;
    private bool _hasPlot;
    private double _centerX;
    private double _centerY;
    private double _scale = 60.0; // pixels per world unit
    private double _minY;
    private double _maxY;
    private bool _isPanning;
    private Point _lastMouse;
    private bool _canvasHovered;

    public Form1()
    {
        InitializeComponent();
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        KeyPreview = true;
        MouseWheel += Canvas_MouseWheel;
        UpdateStatus("Enter a function and press Plot.", false);
    }

    private void BtnPlot_Click(object? sender, EventArgs e)
    {
        PlotFunction();
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        _plotSegments.Clear();
        _hasPlot = false;
        _evaluator = null;
        canvas.Invalidate();
        UpdateStatus("Cleared plot area.", false);
    }

    private void BtnZoomIn_Click(object? sender, EventArgs e)
    {
        ZoomAtPoint(new Point(canvas.Width / 2, canvas.Height / 2), 1.2);
    }

    private void BtnZoomOut_Click(object? sender, EventArgs e)
    {
        ZoomAtPoint(new Point(canvas.Width / 2, canvas.Height / 2), 1.0 / 1.2);
    }

    private void Canvas_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(Color.White);
        DrawGrid(e.Graphics);
        DrawAxes(e.Graphics);
        DrawFunction(e.Graphics);
    }

    private void Canvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isPanning = true;
            _lastMouse = e.Location;
            canvas.Cursor = Cursors.Hand;
        }
    }

    private void Canvas_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isPanning)
        {
            var dx = e.Location.X - _lastMouse.X;
            var dy = e.Location.Y - _lastMouse.Y;
            _centerX -= dx / _scale;
            _centerY += dy / _scale;
            _lastMouse = e.Location;
            canvas.Invalidate();
            UpdateStatusForView();
        }
    }

    private void Canvas_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isPanning = false;
            canvas.Cursor = Cursors.Default;
        }
    }

    private void Canvas_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (!_canvasHovered && sender != canvas)
        {
            return;
        }

        var originControl = sender as Control ?? canvas;
        Point canvasPoint = originControl == canvas
            ? e.Location
            : canvas.PointToClient(originControl.PointToScreen(e.Location));

        var factor = e.Delta > 0 ? 1.15 : 1.0 / 1.15;
        ZoomAtPoint(canvasPoint, factor);
    }

    private void Canvas_MouseEnter(object? sender, EventArgs e)
    {
        _canvasHovered = true;
        canvas.Focus();
    }

    private void Canvas_MouseLeave(object? sender, EventArgs e)
    {
        _canvasHovered = false;
    }

    private void Canvas_Resize(object? sender, EventArgs e)
    {
        canvas.Invalidate();
    }

    private void PlotFunction()
    {
        if (!TryReadInputs(out var fromX, out var toX, out var samples, out var evaluator))
        {
            return;
        }

        var result = SampleFunction(evaluator, fromX, toX, samples);
        if (!result.HasPoints)
        {
            UpdateStatus("No valid points to plot in the selected range.", true);
            return;
        }

        _plotSegments.Clear();
        _plotSegments.AddRange(result.Segments);
        _minY = result.MinY;
        _maxY = result.MaxY;
        _evaluator = evaluator;
        _hasPlot = true;

        ResetView(fromX, toX, _minY, _maxY);
        canvas.Invalidate();
        UpdateStatus($"Plotted {samples} samples. Center ({_centerX:F2}, {_centerY:F2}), scale {_scale:0.##} px/unit.", false);
    }

    private bool TryReadInputs(out double fromX, out double toX, out int samples, out ExpressionEvaluator evaluator)
    {
        fromX = 0;
        toX = 0;
        samples = 0;
        evaluator = null!;

        if (string.IsNullOrWhiteSpace(txtExpression.Text))
        {
            UpdateStatus("Function expression cannot be empty.", true);
            return false;
        }

        if (!double.TryParse(txtFromX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out fromX))
        {
            UpdateStatus("Invalid From X value. Use digits and optional dot for decimals.", true);
            return false;
        }

        if (!double.TryParse(txtToX.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out toX))
        {
            UpdateStatus("Invalid To X value. Use digits and optional dot for decimals.", true);
            return false;
        }

        if (fromX >= toX)
        {
            UpdateStatus("From X must be smaller than To X.", true);
            return false;
        }

        if (!int.TryParse(txtSamples.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out samples) || samples < 2 || samples > 20000)
        {
            UpdateStatus("Samples must be an integer between 2 and 20000.", true);
            return false;
        }

        try
        {
            evaluator = ExpressionEvaluator.Parse(txtExpression.Text);
        }
        catch (Exception ex)
        {
            UpdateStatus(ex.Message, true);
            return false;
        }

        return true;
    }

    private PlotResult SampleFunction(ExpressionEvaluator evaluator, double fromX, double toX, int samples)
    {
        var segments = new List<List<(double X, double Y)>>();
        var current = new List<(double X, double Y)>();
        var step = (toX - fromX) / (samples - 1);
        double prevY = double.NaN;
        bool prevValid = false;
        double minY = double.PositiveInfinity;
        double maxY = double.NegativeInfinity;

        for (int i = 0; i < samples; i++)
        {
            double x = fromX + step * i;
            double y;
            try
            {
                y = evaluator.Evaluate(x);
            }
            catch
            {
                y = double.NaN;
            }

            if (!double.IsFinite(y))
            {
                if (current.Count > 0)
                {
                    segments.Add(current);
                    current = new List<(double X, double Y)>();
                }
                prevValid = false;
                continue;
            }

            if (prevValid && IsJump(prevY, y, step))
            {
                if (current.Count > 0)
                {
                    segments.Add(current);
                    current = new List<(double X, double Y)>();
                }
            }

            current.Add((x, y));
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
            prevValid = true;
            prevY = y;
        }

        if (current.Count > 0)
        {
            segments.Add(current);
        }

        bool hasPoints = segments.Count > 0 && segments.Any(s => s.Count > 0);
        if (!hasPoints)
        {
            minY = maxY = 0;
        }

        return new PlotResult(segments, minY, maxY, hasPoints);
    }

    private static bool IsJump(double previous, double current, double stepX)
    {
        if (!double.IsFinite(previous) || !double.IsFinite(current))
        {
            return true;
        }

        double diff = Math.Abs(current - previous);
        double slope = diff / Math.Max(stepX, 1e-9);
        double relativeJump = diff / Math.Max(1.0, Math.Abs(previous));

        return diff > 1e6 || slope > 1e6 || relativeJump > 25;
    }

    private void ResetView(double fromX, double toX, double minY, double maxY)
    {
        _centerX = (fromX + toX) / 2.0;
        _centerY = double.IsFinite(minY + maxY) ? (minY + maxY) / 2.0 : 0.0;

        double width = Math.Max(1e-6, toX - fromX);
        double height = Math.Max(1e-6, maxY - minY);
        if (!double.IsFinite(height) || height < 1e-4)
        {
            height = Math.Max(4.0, width * 0.5);
        }

        double scaleX = canvas.Width > 0 ? (canvas.Width - 80) / width : 60.0;
        double scaleY = canvas.Height > 0 ? (canvas.Height - 80) / height : scaleX;
        double targetScale = Math.Min(scaleX, scaleY);
        if (!double.IsFinite(targetScale) || targetScale <= 0)
        {
            targetScale = 60.0;
        }

        _scale = Math.Clamp(targetScale, 5.0, 2000.0);
    }

    private void DrawGrid(Graphics g)
    {
        if (_scale <= 0)
        {
            return;
        }

        var bounds = GetWorldBounds();
        double step = GetNiceStep(80.0 / _scale);
        if (!double.IsFinite(step) || step <= 0)
        {
            return;
        }

        using var gridPen = new Pen(Color.FromArgb(235, 235, 235));
        using var textBrush = new SolidBrush(Color.Gray);

        // Vertical lines
        double startX = Math.Floor(bounds.Left / step) * step;
        for (double x = startX; x <= bounds.Right; x += step)
        {
            var p1 = WorldToScreen(x, bounds.Top);
            var p2 = WorldToScreen(x, bounds.Bottom);
            g.DrawLine(gridPen, p1, p2);
            g.DrawString(x.ToString("0.###", CultureInfo.InvariantCulture), Font, textBrush, p1.X + 2, canvas.Height - 22);
        }

        // Horizontal lines
        double startY = Math.Floor(bounds.Bottom / step) * step;
        for (double y = startY; y <= bounds.Top; y += step)
        {
            var p1 = WorldToScreen(bounds.Left, y);
            var p2 = WorldToScreen(bounds.Right, y);
            g.DrawLine(gridPen, p1, p2);
            g.DrawString(y.ToString("0.###", CultureInfo.InvariantCulture), Font, textBrush, 4, p1.Y - 18);
        }
    }

    private void DrawAxes(Graphics g)
    {
        var bounds = GetWorldBounds();
        using var axisPen = new Pen(Color.FromArgb(160, 160, 160), 2f);

        if (bounds.Left <= 0 && bounds.Right >= 0)
        {
            var top = WorldToScreen(0, bounds.Top);
            var bottom = WorldToScreen(0, bounds.Bottom);
            g.DrawLine(axisPen, top, bottom);
        }

        if (bounds.Bottom <= 0 && bounds.Top >= 0)
        {
            var left = WorldToScreen(bounds.Left, 0);
            var right = WorldToScreen(bounds.Right, 0);
            g.DrawLine(axisPen, left, right);
        }
    }

    private void DrawFunction(Graphics g)
    {
        if (!_hasPlot || _plotSegments.Count == 0)
        {
            return;
        }

        using var plotPen = new Pen(Color.RoyalBlue, 2f);
        foreach (var segment in _plotSegments)
        {
            if (segment.Count < 2)
            {
                continue;
            }

            var points = new PointF[segment.Count];
            for (int i = 0; i < segment.Count; i++)
            {
                points[i] = WorldToScreen(segment[i].X, segment[i].Y);
            }

            g.DrawLines(plotPen, points);
        }
    }

    private void ZoomAtPoint(Point location, double factor)
    {
        if (!double.IsFinite(factor) || factor <= 0)
        {
            return;
        }

        var worldBefore = ScreenToWorld(location);
        _scale = Math.Clamp(_scale * factor, 5.0, 4000.0);
        _centerX = worldBefore.X - (location.X - canvas.Width / 2.0) / _scale;
        _centerY = worldBefore.Y + (location.Y - canvas.Height / 2.0) / _scale;
        canvas.Invalidate();
        UpdateStatusForView();
    }

    private (double Left, double Right, double Bottom, double Top) GetWorldBounds()
    {
        double halfWidth = canvas.Width / _scale / 2.0;
        double halfHeight = canvas.Height / _scale / 2.0;
        return (_centerX - halfWidth, _centerX + halfWidth, _centerY - halfHeight, _centerY + halfHeight);
    }

    private PointF WorldToScreen(double x, double y)
    {
        double screenX = canvas.Width / 2.0 + (x - _centerX) * _scale;
        double screenY = canvas.Height / 2.0 - (y - _centerY) * _scale;
        return new PointF((float)screenX, (float)screenY);
    }

    private PointF ScreenToWorld(Point screen)
    {
        double worldX = _centerX + (screen.X - canvas.Width / 2.0) / _scale;
        double worldY = _centerY - (screen.Y - canvas.Height / 2.0) / _scale;
        return new PointF((float)worldX, (float)worldY);
    }

    private static double GetNiceStep(double rawStep)
    {
        if (rawStep <= 0 || double.IsNaN(rawStep))
        {
            return 1;
        }

        double exponent = Math.Floor(Math.Log10(rawStep));
        double fraction = rawStep / Math.Pow(10, exponent);
        double niceFraction = fraction switch
        {
            < 1.5 => 1,
            < 3 => 2,
            < 7 => 5,
            _ => 10
        };

        return niceFraction * Math.Pow(10, exponent);
    }

    private void UpdateStatus(string message, bool isError)
    {
        lblStatus.ForeColor = isError ? Color.Firebrick : Color.FromArgb(32, 96, 32);
        lblStatus.Text = message;
    }

    private void UpdateStatusForView()
    {
        if (_hasPlot)
        {
            UpdateStatus($"Center ({_centerX:F2}, {_centerY:F2}) | Scale {_scale:0.##} px/unit", false);
        }
    }

    private record struct PlotResult(List<List<(double X, double Y)>> Segments, double MinY, double MaxY, bool HasPoints);
}
