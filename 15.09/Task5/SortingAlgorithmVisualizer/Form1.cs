using System.Linq;

namespace SortingAlgorithmVisualizer;

public partial class Form1 : Form
{
    private enum SortState
    {
        Idle,
        Running,
        Paused
    }

    private readonly Random _random = new();
    private int[] _values = Array.Empty<int>();
    private int _compareA = -1;
    private int _compareB = -1;
    private int _swapA = -1;
    private int _swapB = -1;
    private CancellationTokenSource? _cts;
    private SortState _state = SortState.Idle;

    public Form1()
    {
        InitializeComponent();
        DoubleBuffered = true;
        UpdateSpeedLabel();
        GenerateArray();
    }

    private void btnGenerate_Click(object sender, EventArgs e)
    {
        if (_state != SortState.Idle)
        {
            MessageBox.Show("Stop or pause the current run before randomizing a new array.", "Sorting in progress",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        GenerateArray();
    }

    private async void btnStart_Click(object sender, EventArgs e)
    {
        if (_state == SortState.Running)
        {
            return;
        }

        if (_state == SortState.Paused)
        {
            ResumeSorting();
            return;
        }

        await StartSortingAsync();
    }

    private void btnPauseStop_Click(object sender, EventArgs e)
    {
        if (_state == SortState.Running)
        {
            PauseSorting();
        }
        else if (_state == SortState.Paused)
        {
            StopSorting();
        }
    }

    private void trackBarSpeed_Scroll(object sender, EventArgs e)
    {
        UpdateSpeedLabel();
    }

    private void numericSize_ValueChanged(object sender, EventArgs e)
    {
        if (_state == SortState.Idle)
        {
            GenerateArray();
        }
    }

    private void numericMaxValue_ValueChanged(object sender, EventArgs e)
    {
        if (_state == SortState.Idle)
        {
            GenerateArray();
        }
    }

    private void panelCanvas_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.Clear(panelCanvas.BackColor);
        if (_values.Length == 0)
        {
            return;
        }

        var maxValue = _values.Max();
        if (maxValue == 0)
        {
            return;
        }

        var width = panelCanvas.ClientSize.Width;
        var height = panelCanvas.ClientSize.Height;
        var barWidth = Math.Max(2f, (float)width / _values.Length);
        var usableHeight = height - 16;

        for (int i = 0; i < _values.Length; i++)
        {
            var value = _values[i];
            var barHeight = Math.Max(4f, (float)value / maxValue * usableHeight);
            var x = i * barWidth;
            var y = height - barHeight - 4;

            Brush brush = Brushes.SteelBlue;
            if (i == _swapA || i == _swapB)
            {
                brush = Brushes.IndianRed;
            }
            else if (i == _compareA || i == _compareB)
            {
                brush = Brushes.Orange;
            }

            var rect = new RectangleF(x, y, Math.Max(1.5f, barWidth - 2f), barHeight);
            e.Graphics.FillRectangle(brush, rect);
        }
    }

    private void panelCanvas_Resize(object? sender, EventArgs e)
    {
        panelCanvas.Invalidate();
    }

    private async Task StartSortingAsync()
    {
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        SetState(SortState.Running, "Sorting (Bubble Sort)...");

        try
        {
            await BubbleSortAsync(_cts.Token);
            if (!_cts.IsCancellationRequested)
            {
                ClearHighlights();
                panelCanvas.Invalidate();
                SetState(SortState.Idle, "Sorting completed.");
                MessageBox.Show("Sorting completed!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (OperationCanceledException)
        {
            SetState(SortState.Idle, "Sorting stopped.");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            ClearHighlights();
            panelCanvas.Invalidate();
        }
    }

    private void PauseSorting()
    {
        if (_state != SortState.Running)
        {
            return;
        }

        SetState(SortState.Paused, "Paused. Click Resume to continue or Stop to cancel.");
    }

    private void ResumeSorting()
    {
        if (_state != SortState.Paused)
        {
            return;
        }

        SetState(SortState.Running, "Sorting (Bubble Sort)...");
    }

    private void StopSorting()
    {
        if (_state == SortState.Idle)
        {
            return;
        }

        _cts?.Cancel();
        SetState(SortState.Idle, "Sorting stopped.");
    }

    private void GenerateArray()
    {
        var size = (int)numericSize.Value;
        var maxValue = (int)numericMaxValue.Value;
        _values = new int[size];
        for (int i = 0; i < size; i++)
        {
            _values[i] = _random.Next(5, maxValue + 1);
        }

        ClearHighlights();
        panelCanvas.Invalidate();
        SetState(SortState.Idle, $"Generated {size} values.");
    }

    private void SetState(SortState state, string? status = null)
    {
        _state = state;
        btnGenerate.Enabled = state == SortState.Idle;
        numericSize.Enabled = state == SortState.Idle;

        btnStart.Enabled = state != SortState.Running;
        btnStart.Text = state == SortState.Paused ? "Resume" : "Start";

        btnPauseStop.Enabled = state != SortState.Idle;
        btnPauseStop.Text = state == SortState.Paused ? "Stop" : "Pause / Stop";

        if (!string.IsNullOrWhiteSpace(status))
        {
            labelStatus.Text = $"Status: {status}";
        }
    }

    private async Task BubbleSortAsync(CancellationToken token)
    {
        var length = _values.Length;
        for (int i = 0; i < length - 1; i++)
        {
            for (int j = 0; j < length - i - 1; j++)
            {
                token.ThrowIfCancellationRequested();
                await WaitIfPausedAsync(token);

                Highlight(j, j + 1, false);
                await StepDelayAsync(token);

                if (_values[j] > _values[j + 1])
                {
                    (_values[j], _values[j + 1]) = (_values[j + 1], _values[j]);
                    Highlight(j, j + 1, true);
                    await StepDelayAsync(token);
                }
            }
        }
    }

    private async Task WaitIfPausedAsync(CancellationToken token)
    {
        while (_state == SortState.Paused)
        {
            await Task.Delay(50, token);
        }
    }

    private async Task StepDelayAsync(CancellationToken token)
    {
        await Task.Delay(GetStepDelay(), token);
        await WaitIfPausedAsync(token);
        panelCanvas.Invalidate();
    }

    private void Highlight(int indexA, int indexB, bool isSwap)
    {
        _compareA = indexA;
        _compareB = indexB;
        if (isSwap)
        {
            _swapA = indexA;
            _swapB = indexB;
        }
        else
        {
            _swapA = -1;
            _swapB = -1;
        }

        panelCanvas.Invalidate();
    }

    private void ClearHighlights()
    {
        _compareA = -1;
        _compareB = -1;
        _swapA = -1;
        _swapB = -1;
    }

    private int GetStepDelay()
    {
        return 5 + (101 - trackBarSpeed.Value) * 5;
    }

    private void UpdateSpeedLabel()
    {
        labelSpeed.Text = $"Step delay: {GetStepDelay()} ms";
    }
}
