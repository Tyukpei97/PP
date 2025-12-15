using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Puzzle15Game;

public partial class Form1 : Form
{
    private const int GridSize = 4;
    private readonly Button[,] tiles = new Button[GridSize, GridSize];
    private readonly int[,] board = new int[GridSize, GridSize];
    private readonly Random random = new();
    private int emptyRow;
    private int emptyCol;
    private int moves;

    public Form1()
    {
        InitializeComponent();
        BuildBoardButtons();
        StartNewGame();
    }

    private void BuildBoardButtons()
    {
        boardTable.SuspendLayout();
        boardTable.Controls.Clear();

        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                var button = new Button
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.LightSteelBlue,
                    ForeColor = Color.Black,
                    TabStop = false,
                    Tag = (row, col)
                };
                button.FlatAppearance.BorderColor = Color.WhiteSmoke;
                button.FlatAppearance.BorderSize = 1;
                button.Click += Tile_Click;
                tiles[row, col] = button;
                boardTable.Controls.Add(button, col, row);
            }
        }

        boardTable.ResumeLayout();
    }

    private void StartNewGame()
    {
        moves = 0;
        ShuffleBoard();
        UpdateTiles();
        statusLabel.Text = "Moves: 0";
    }

    private void ShuffleBoard()
    {
        var values = Enumerable.Range(0, GridSize * GridSize).ToArray();

        do
        {
            Shuffle(values);
        }
        while (!IsSolvable(values) || IsSolved(values));

        ApplyValuesToBoard(values);
    }

    private void Shuffle(int[] values)
    {
        for (int i = values.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }

    private void ApplyValuesToBoard(int[] values)
    {
        for (int index = 0; index < values.Length; index++)
        {
            int row = index / GridSize;
            int col = index % GridSize;
            board[row, col] = values[index];

            if (values[index] == 0)
            {
                emptyRow = row;
                emptyCol = col;
            }
        }
    }

    private void UpdateTiles()
    {
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                int value = board[row, col];
                var button = tiles[row, col];

                button.Text = value == 0 ? string.Empty : value.ToString();
                button.Enabled = value != 0;
                button.BackColor = value == 0 ? Color.WhiteSmoke : Color.LightSteelBlue;
            }
        }

        statusLabel.Text = $"Moves: {moves}";
    }

    private void Tile_Click(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.Tag is not ValueTuple<int, int> position)
        {
            return;
        }

        var (row, col) = position;

        if (board[row, col] == 0)
        {
            return;
        }

        if (IsAdjacent(row, col, emptyRow, emptyCol))
        {
            MoveTile(row, col);
        }
    }

    private void MoveTile(int row, int col)
    {
        board[emptyRow, emptyCol] = board[row, col];
        board[row, col] = 0;
        emptyRow = row;
        emptyCol = col;
        moves++;

        UpdateTiles();

        if (IsSolved())
        {
            statusLabel.Text = $"Solved in {moves} moves!";
            MessageBox.Show($"You win! Moves: {moves}", "Congratulations", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private bool IsSolvable(int[] values)
    {
        int inversions = 0;

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == 0)
            {
                continue;
            }

            for (int j = i + 1; j < values.Length; j++)
            {
                if (values[j] != 0 && values[i] > values[j])
                {
                    inversions++;
                }
            }
        }

        int blankIndex = Array.IndexOf(values, 0);
        int blankRowFromBottom = GridSize - (blankIndex / GridSize); // 1-based

        bool blankOnEvenRowFromBottom = blankRowFromBottom % 2 == 0;
        bool inversionsEven = inversions % 2 == 0;

        return blankOnEvenRowFromBottom != inversionsEven;
    }

    private bool IsSolved(int[] values)
    {
        for (int i = 0; i < values.Length - 1; i++)
        {
            if (values[i] != i + 1)
            {
                return false;
            }
        }

        return values[^1] == 0;
    }

    private bool IsSolved()
    {
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                if (row == GridSize - 1 && col == GridSize - 1)
                {
                    break;
                }

                int expected = row * GridSize + col + 1;

                if (board[row, col] != expected)
                {
                    return false;
                }
            }
        }

        return board[GridSize - 1, GridSize - 1] == 0;
    }

    private static bool IsAdjacent(int row1, int col1, int row2, int col2)
    {
        return (row1 == row2 && Math.Abs(col1 - col2) == 1) ||
               (col1 == col2 && Math.Abs(row1 - row2) == 1);
    }

    private void NewGameButton_Click(object? sender, EventArgs e)
    {
        StartNewGame();
    }
}
