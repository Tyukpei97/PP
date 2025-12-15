using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Task2;

public partial class Form1 : Form
{
    private readonly System.Windows.Forms.Timer _animationTimer;
    private CancellationTokenSource? _renderCts;
    private int[,]? _iterationBuffer;
    private double _centerX = -0.75;
    private double _centerY = 0.0;
    private double _viewWidth = 3.5;
    private int _maxIterations = 300;
    private int _paletteOffset;
    private bool _isDragging;
    private Point _lastMouse;

    public Form1()
    {
        InitializeComponent();
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;

        _animationTimer = new System.Windows.Forms.Timer { Interval = 45 };
        _animationTimer.Tick += (_, _) =>
        {
            if (_iterationBuffer == null)
            {
                return;
            }

            _paletteOffset = (_paletteOffset + 3) % Math.Max(1, _maxIterations);
            ApplyPalette();
        };
        _animationTimer.Start();

        fractalBox.MouseDown += FractalBox_MouseDown;
        fractalBox.MouseMove += FractalBox_MouseMove;
        fractalBox.MouseUp += FractalBox_MouseUp;
        fractalBox.MouseWheel += FractalBox_MouseWheel;
        MouseWheel += FractalBox_MouseWheel;
        fractalBox.Resize += (_, _) => TriggerRender();
        Shown += async (_, _) => await RenderFractalAsync();
    }

    private void FractalBox_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        _isDragging = true;
        _lastMouse = e.Location;
        fractalBox.Cursor = Cursors.Hand;
    }

    private void FractalBox_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        double viewHeight = GetViewHeight();
        double dx = e.X - _lastMouse.X;
        double dy = e.Y - _lastMouse.Y;

        _centerX -= dx * _viewWidth / fractalBox.Width;
        _centerY += dy * viewHeight / fractalBox.Height;
        _lastMouse = e.Location;

        TriggerRender();
    }

    private void FractalBox_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        _isDragging = false;
        fractalBox.Cursor = Cursors.Default;
    }

    private void FractalBox_MouseWheel(object? sender, MouseEventArgs e)
    {
        Point position = e.Location;
        if (sender is not PictureBox)
        {
            position = fractalBox.PointToClient(PointToScreen(e.Location));
        }

        if (!fractalBox.ClientRectangle.Contains(position))
        {
            return;
        }

        double zoomFactor = e.Delta > 0 ? 0.8 : 1.25;

        // Keep the point under the cursor fixed while zooming.
        (double reBefore, double imBefore) = PixelToPoint(position);
        _viewWidth *= zoomFactor;
        (double reAfter, double imAfter) = PixelToPoint(position);

        _centerX += reBefore - reAfter;
        _centerY += imBefore - imAfter;

        TriggerRender();
    }

    private void TriggerRender()
    {
        _ = RenderFractalAsync();
    }

    private async Task RenderFractalAsync()
    {
        if (fractalBox.Width <= 0 || fractalBox.Height <= 0)
        {
            return;
        }

        _renderCts?.Cancel();
        _renderCts?.Dispose();
        var cts = new CancellationTokenSource();
        _renderCts = cts;
        int width = fractalBox.Width;
        int height = fractalBox.Height;
        double viewHeight = GetViewHeight();
        double minRe = _centerX - _viewWidth / 2;
        double minIm = _centerY - viewHeight / 2;

        _maxIterations = CalculateIterations();
        int[,] localBuffer = new int[width, height];

        try
        {
            await Task.Run(() =>
            {
                Parallel.For(0, height, (y, loopState) =>
                {
                    if (cts.IsCancellationRequested)
                    {
                        loopState.Stop();
                        return;
                    }

                    double cIm = minIm + y * viewHeight / height;
                    for (int x = 0; x < width; x++)
                    {
                        double cRe = minRe + x * _viewWidth / width;
                        double zRe = 0;
                        double zIm = 0;
                        int i = 0;
                        for (; i < _maxIterations; i++)
                        {
                            double zRe2 = zRe * zRe;
                            double zIm2 = zIm * zIm;
                            if (zRe2 + zIm2 > 4.0)
                            {
                                break;
                            }

                            double temp = zRe2 - zIm2 + cRe;
                            zIm = 2 * zRe * zIm + cIm;
                            zRe = temp;
                        }

                        localBuffer[x, y] = i;
                    }
                });
            }, cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (cts.IsCancellationRequested)
        {
            return;
        }

        _iterationBuffer = localBuffer;
        _paletteOffset %= Math.Max(1, _maxIterations);
        UpdateStatus();
        ApplyPalette();
    }

    private void ApplyPalette()
    {
        if (_iterationBuffer == null)
        {
            return;
        }

        int width = _iterationBuffer.GetLength(0);
        int height = _iterationBuffer.GetLength(1);
        int[] colors = new int[width * height];

        Parallel.For(0, height, y =>
        {
            int rowOffset = y * width;
            for (int x = 0; x < width; x++)
            {
                int iteration = _iterationBuffer[x, y];
                colors[rowOffset + x] = ChooseColor(iteration, _maxIterations, _paletteOffset).ToArgb();
            }
        });

        Bitmap bitmap = new(width, height, PixelFormat.Format32bppArgb);
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
        Marshal.Copy(colors, 0, data.Scan0, colors.Length);
        bitmap.UnlockBits(data);

        var previous = fractalBox.Image;
        fractalBox.Image = bitmap;
        previous?.Dispose();
    }

    private int CalculateIterations()
    {
        double zoom = 3.5 / _viewWidth;
        double boost = Math.Log10(zoom + 1);
        int result = 200 + (int)(boost * 120);
        return Math.Clamp(result, 200, 2000);
    }

    private (double re, double im) PixelToPoint(Point p)
    {
        double viewHeight = GetViewHeight();
        double re = _centerX - _viewWidth / 2 + p.X * _viewWidth / fractalBox.Width;
        double im = _centerY + viewHeight / 2 - p.Y * viewHeight / fractalBox.Height;
        return (re, im);
    }

    private double GetViewHeight()
    {
        return _viewWidth * fractalBox.Height / fractalBox.Width;
    }

    private Color ChooseColor(int iteration, int maxIterations, int paletteOffset)
    {
        if (iteration >= maxIterations)
        {
            return Color.Black;
        }

        double t = (double)(iteration + paletteOffset) / maxIterations;
        double hue = (t * 360.0) % 360.0;
        double saturation = 0.75;
        double value = 1.0 - Math.Pow((double)iteration / maxIterations, 0.35);

        return FromHsv(hue, saturation, value);
    }

    private static Color FromHsv(double hue, double saturation, double value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        value = value * 255;
        int v = (int)value;
        int p = (int)(value * (1 - saturation));
        int q = (int)(value * (1 - f * saturation));
        int t = (int)(value * (1 - (1 - f) * saturation));

        return hi switch
        {
            0 => Color.FromArgb(255, v, t, p),
            1 => Color.FromArgb(255, q, v, p),
            2 => Color.FromArgb(255, p, v, t),
            3 => Color.FromArgb(255, p, q, v),
            4 => Color.FromArgb(255, t, p, v),
            _ => Color.FromArgb(255, v, p, q),
        };
    }

    private void UpdateStatus()
    {
        double zoom = 3.5 / _viewWidth;
        statusLabel.Text = $"Центр: {_centerX:F4} + {_centerY:F4}i   |   Ширина окна: {_viewWidth:F3}   |   Зум: {zoom:F2}x   |   Итерации: {_maxIterations}";
    }
}
