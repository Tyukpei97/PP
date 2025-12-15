using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShapeEditor.WinForms.Models;
using ShapeEditor.WinForms.Services;
using ShapeEditor.WinForms.UI;

namespace ShapeEditor.WinForms
{
    public sealed class MainForm : Form
    {
        private const float HandleSize = 8f;
        private const float HitTolerance = 6f;

        private readonly List<ShapeBase> _shapes;
        private readonly CanvasPanel _canvas;

        private ToolStripStatusLabel _statusLabel;

        private ToolMode _toolMode;
        private ShapeBase? _selectedShape;
        private ShapeHandle _activeHandle;

        private bool _isMouseDown;
        private PointF _mouseDownPoint;
        private PointF _lastMousePoint;

        private ShapeBase? _draftShape;

        private Color _currentStrokeColor;
        private Color _currentFillColor;
        private bool _currentIsFilled;
        private float _currentStrokeWidth;

        private string? _currentFilePath;
        private bool _isDirty;

        private ToolStripButton _btnSelect;
        private ToolStripButton _btnRect;
        private ToolStripButton _btnEllipse;
        private ToolStripButton _btnLine;

        private ToolStripButton _btnFillToggle;
        private ToolStripButton _btnStrokeColor;
        private ToolStripButton _btnFillColor;

        private ToolStripComboBox _cmbStrokeWidth;

        private ToolStripButton _btnDelete;
        private ToolStripButton _btnBringForward;
        private ToolStripButton _btnSendBackward;

        public MainForm()
        {
            Text = "Задание 1 — Редактор фигур";
            Width = 1100;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;

            _shapes = new List<ShapeBase>();

            _currentStrokeColor = Color.Black;
            _currentFillColor = Color.Gainsboro;
            _currentIsFilled = true;
            _currentStrokeWidth = 2f;

            var menuStrip = BuildMenu();
            var toolStrip = BuildToolStrip();
            var statusStrip = BuildStatusStrip();

            _canvas = new CanvasPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            _canvas.Paint += CanvasOnPaint;
            _canvas.MouseDown += CanvasOnMouseDown;
            _canvas.MouseMove += CanvasOnMouseMove;
            _canvas.MouseUp += CanvasOnMouseUp;

            Controls.Add(_canvas);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);
            Controls.Add(statusStrip);

            MainMenuStrip = menuStrip;

            _toolMode = ToolMode.Select;
            SetActiveTool(_btnSelect);

            UpdateTitle();
            UpdateStatus("Готово. Выберите инструмент и рисуйте.");
        }

        private MenuStrip BuildMenu()
        {
            var menu = new MenuStrip();

            var file = new ToolStripMenuItem("Файл");
            var itemNew = new ToolStripMenuItem("Новый", null, (_, _) => NewDocument())
            {
                ShortcutKeys = Keys.Control | Keys.N
            };
            var itemOpen = new ToolStripMenuItem("Открыть...", null, (_, _) => OpenDocument())
            {
                ShortcutKeys = Keys.Control | Keys.O
            };
            var itemSave = new ToolStripMenuItem("Сохранить", null, (_, _) => SaveDocument())
            {
                ShortcutKeys = Keys.Control | Keys.S
            };
            var itemSaveAs = new ToolStripMenuItem("Сохранить как...", null, (_, _) => SaveDocumentAs());
            var itemExit = new ToolStripMenuItem("Выход", null, (_, _) => Close());

            file.DropDownItems.Add(itemNew);
            file.DropDownItems.Add(itemOpen);
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(itemSave);
            file.DropDownItems.Add(itemSaveAs);
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(itemExit);

            var edit = new ToolStripMenuItem("Правка");
            var itemDelete = new ToolStripMenuItem("Удалить выделенное", null, (_, _) => DeleteSelected())
            {
                ShortcutKeys = Keys.Delete
            };
            var itemBringF = new ToolStripMenuItem("Вперёд (на слой выше)", null, (_, _) => BringForward())
            {
                ShortcutKeys = Keys.Control | Keys.Up
            };
            var itemSendB = new ToolStripMenuItem("Назад (на слой ниже)", null, (_, _) => SendBackward())
            {
                ShortcutKeys = Keys.Control | Keys.Down
            };

            edit.DropDownItems.Add(itemDelete);
            edit.DropDownItems.Add(new ToolStripSeparator());
            edit.DropDownItems.Add(itemBringF);
            edit.DropDownItems.Add(itemSendB);

            var help = new ToolStripMenuItem("Справка");
            var itemAbout = new ToolStripMenuItem("Горячие клавиши", null, (_, _) => ShowHelp());
            help.DropDownItems.Add(itemAbout);

            menu.Items.Add(file);
            menu.Items.Add(edit);
            menu.Items.Add(help);

            return menu;
        }

        private ToolStrip BuildToolStrip()
        {
            var strip = new ToolStrip
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden
            };

            _btnSelect = new ToolStripButton("Выбор")
            {
                CheckOnClick = true,
                Checked = true
            };
            _btnRect = new ToolStripButton("Прямоугольник")
            {
                CheckOnClick = true
            };
            _btnEllipse = new ToolStripButton("Эллипс")
            {
                CheckOnClick = true
            };
            _btnLine = new ToolStripButton("Линия")
            {
                CheckOnClick = true
            };

            _btnSelect.Click += (_, _) =>
            {
                _toolMode = ToolMode.Select;
                SetActiveTool(_btnSelect);
                UpdateStatus("Режим выбора: выделяйте, перемещайте и изменяйте размер.");
            };

            _btnRect.Click += (_, _) =>
            {
                _toolMode = ToolMode.DrawRectangle;
                SetActiveTool(_btnRect);
                UpdateStatus("Рисование прямоугольника: зажмите ЛКМ и тяните.");
            };

            _btnEllipse.Click += (_, _) =>
            {
                _toolMode = ToolMode.DrawEllipse;
                SetActiveTool(_btnEllipse);
                UpdateStatus("Рисование эллипса: зажмите ЛКМ и тяните.");
            };

            _btnLine.Click += (_, _) =>
            {
                _toolMode = ToolMode.DrawLine;
                SetActiveTool(_btnLine);
                UpdateStatus("Рисование линии: зажмите ЛКМ и тяните.");
            };

            _btnStrokeColor = new ToolStripButton("Цвет линии");
            _btnStrokeColor.Click += (_, _) => PickStrokeColor();

            _btnFillColor = new ToolStripButton("Цвет заливки");
            _btnFillColor.Click += (_, _) => PickFillColor();

            _btnFillToggle = new ToolStripButton("Заливка")
            {
                CheckOnClick = true,
                Checked = _currentIsFilled
            };
            _btnFillToggle.CheckedChanged += (_, _) =>
            {
                _currentIsFilled = _btnFillToggle.Checked;
                if (_selectedShape != null && _selectedShape is not LineShape)
                {
                    _selectedShape.IsFilled = _currentIsFilled;
                    MarkDirty();
                    _canvas.Invalidate();
                }
            };

            _cmbStrokeWidth = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 70
            };
            _cmbStrokeWidth.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "8", "10", "12" });
            _cmbStrokeWidth.SelectedIndex = 1;
            _cmbStrokeWidth.SelectedIndexChanged += (_, _) =>
            {
                if (!float.TryParse(_cmbStrokeWidth.SelectedItem?.ToString(), out float width))
                {
                    return;
                }

                _currentStrokeWidth = width;
                if (_selectedShape != null)
                {
                    _selectedShape.StrokeWidth = _currentStrokeWidth;
                    MarkDirty();
                    _canvas.Invalidate();
                }
            };

            _btnDelete = new ToolStripButton("Удалить");
            _btnDelete.Click += (_, _) => DeleteSelected();

            _btnBringForward = new ToolStripButton("Вперёд");
            _btnBringForward.Click += (_, _) => BringForward();

            _btnSendBackward = new ToolStripButton("Назад");
            _btnSendBackward.Click += (_, _) => SendBackward();

            strip.Items.Add(_btnSelect);
            strip.Items.Add(_btnRect);
            strip.Items.Add(_btnEllipse);
            strip.Items.Add(_btnLine);

            strip.Items.Add(new ToolStripSeparator());

            strip.Items.Add(_btnStrokeColor);
            strip.Items.Add(_btnFillColor);
            strip.Items.Add(_btnFillToggle);

            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(new ToolStripLabel("Толщина:"));
            strip.Items.Add(_cmbStrokeWidth);

            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(_btnDelete);
            strip.Items.Add(_btnBringForward);
            strip.Items.Add(_btnSendBackward);

            UpdateColorButtons();

            return strip;
        }

        private StatusStrip BuildStatusStrip()
        {
            var strip = new StatusStrip();

            _statusLabel = new ToolStripStatusLabel("Готово");
            strip.Items.Add(_statusLabel);

            return strip;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!ConfirmDiscardIfDirty())
            {
                e.Cancel = true;
                return;
            }

            base.OnFormClosing(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelected();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                _draftShape = null;
                _isMouseDown = false;
                _activeHandle = ShapeHandle.None;
                UpdateStatus("Отменено.");
                _canvas.Invalidate();
                e.Handled = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveDocument();
                e.Handled = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.O)
            {
                OpenDocument();
                e.Handled = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.N)
            {
                NewDocument();
                e.Handled = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.Up)
            {
                BringForward();
                e.Handled = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.Down)
            {
                SendBackward();
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
        }

        private void CanvasOnPaint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var shape in _shapes)
            {
                shape.Draw(e.Graphics);
            }

            _draftShape?.Draw(e.Graphics);

            if (_selectedShape != null)
            {
                _selectedShape.DrawSelection(e.Graphics, HandleSize);
            }
        }

        private void CanvasOnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            _canvas.Focus();

            _isMouseDown = true;
            _mouseDownPoint = e.Location;
            _lastMousePoint = e.Location;

            if (_toolMode == ToolMode.Select)
            {
                SelectAtPoint(e.Location);
                if (_selectedShape != null)
                {
                    _activeHandle = _selectedShape.GetHandleAt(e.Location, HandleSize);
                    if (_activeHandle == ShapeHandle.None && _selectedShape.HitTest(e.Location, HitTolerance))
                    {
                        _activeHandle = ShapeHandle.Move;
                    }
                }
                else
                {
                    _activeHandle = ShapeHandle.None;
                }

                _canvas.Invalidate();
                return;
            }

            _draftShape = CreateShapeForTool(_toolMode, e.Location);
            _canvas.Invalidate();
        }

        private void CanvasOnMouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isMouseDown)
            {
                UpdateCursor(e.Location);
                return;
            }

            var currentPoint = (PointF)e.Location;
            var delta = new PointF(currentPoint.X - _lastMousePoint.X, currentPoint.Y - _lastMousePoint.Y);

            if (_toolMode == ToolMode.Select)
            {
                if (_selectedShape != null)
                {
                    if (_activeHandle == ShapeHandle.Move)
                    {
                        _selectedShape.Move(delta.X, delta.Y);
                        MarkDirty();
                        _canvas.Invalidate();
                    }
                    else if (_activeHandle != ShapeHandle.None)
                    {
                        _selectedShape.Resize(_activeHandle, delta);
                        MarkDirty();
                        _canvas.Invalidate();
                    }
                }

                _lastMousePoint = currentPoint;
                return;
            }

            if (_draftShape != null)
            {
                UpdateDraftShape(_draftShape, _mouseDownPoint, currentPoint);
                _canvas.Invalidate();
            }

            _lastMousePoint = currentPoint;
        }

        private void CanvasOnMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            _isMouseDown = false;

            if (_toolMode != ToolMode.Select && _draftShape != null)
            {
                if (IsDraftValid(_draftShape))
                {
                    _shapes.Add(_draftShape);
                    SetSelectedShape(_draftShape);
                    MarkDirty();
                    UpdateStatus("Фигура добавлена. Можно выделять и редактировать.");
                }
                else
                {
                    UpdateStatus("Слишком маленькая фигура — отменено.");
                }

                _draftShape = null;
                _canvas.Invalidate();
                return;
            }

            _activeHandle = ShapeHandle.None;
            _canvas.Invalidate();
        }

        private void UpdateCursor(PointF point)
        {
            if (_toolMode != ToolMode.Select)
            {
                _canvas.Cursor = Cursors.Cross;
                return;
            }

            var shape = FindTopShape(point);
            if (shape == null)
            {
                _canvas.Cursor = Cursors.Default;
                return;
            }

            if (shape == _selectedShape)
            {
                var handle = shape.GetHandleAt(point, HandleSize);
                _canvas.Cursor = handle switch
                {
                    ShapeHandle.TopLeft or ShapeHandle.BottomRight => Cursors.SizeNWSE,
                    ShapeHandle.TopRight or ShapeHandle.BottomLeft => Cursors.SizeNESW,
                    ShapeHandle.StartPoint or ShapeHandle.EndPoint => Cursors.SizeAll,
                    _ => shape.HitTest(point, HitTolerance) ? Cursors.SizeAll : Cursors.Default
                };
            }
            else
            {
                _canvas.Cursor = shape.HitTest(point, HitTolerance) ? Cursors.Hand : Cursors.Default;
            }
        }

        private void SelectAtPoint(PointF point)
        {
            var shape = FindTopShape(point);
            SetSelectedShape(shape);
        }

        private ShapeBase? FindTopShape(PointF point)
        {
            for (int i = _shapes.Count - 1; i >= 0; i--)
            {
                if (_shapes[i].HitTest(point, HitTolerance))
                {
                    return _shapes[i];
                }
            }

            return null;
        }

        private ShapeBase CreateShapeForTool(ToolMode mode, PointF startPoint)
        {
            ShapeBase shape = mode switch
            {
                ToolMode.DrawRectangle => new RectangleShape(new RectangleF(startPoint.X, startPoint.Y, 1, 1)),
                ToolMode.DrawEllipse => new EllipseShape(new RectangleF(startPoint.X, startPoint.Y, 1, 1)),
                ToolMode.DrawLine => new LineShape(startPoint, startPoint),
                _ => new RectangleShape(new RectangleF(startPoint.X, startPoint.Y, 1, 1))
            };

            shape.StrokeColor = _currentStrokeColor;
            shape.FillColor = _currentFillColor;
            shape.IsFilled = _currentIsFilled;
            shape.StrokeWidth = _currentStrokeWidth;

            if (shape is LineShape)
            {
                shape.IsFilled = false;
            }

            return shape;
        }

        private static void UpdateDraftShape(ShapeBase draft, PointF startPoint, PointF currentPoint)
        {
            if (draft is RectangleShape rect)
            {
                rect.Bounds = Normalize(startPoint, currentPoint);
            }
            else if (draft is EllipseShape ellipse)
            {
                ellipse.Bounds = Normalize(startPoint, currentPoint);
            }
            else if (draft is LineShape line)
            {
                line.Start = startPoint;
                line.End = currentPoint;
            }
        }

        private static RectangleF Normalize(PointF a, PointF b)
        {
            float x1 = Math.Min(a.X, b.X);
            float y1 = Math.Min(a.Y, b.Y);
            float x2 = Math.Max(a.X, b.X);
            float y2 = Math.Max(a.Y, b.Y);

            var rect = new RectangleF(x1, y1, x2 - x1, y2 - y1);

            float min = 5f;
            if (rect.Width < min)
            {
                rect.Width = min;
            }

            if (rect.Height < min)
            {
                rect.Height = min;
            }

            return rect;
        }

        private static bool IsDraftValid(ShapeBase shape)
        {
            if (shape is LineShape line)
            {
                return GeometryHelper.Distance(line.Start, line.End) >= 8f;
            }

            var bounds = shape.GetBounds();
            return bounds.Width >= 8f && bounds.Height >= 8f;
        }

        private void SetActiveTool(ToolStripButton activeButton)
        {
            foreach (var button in new[] { _btnSelect, _btnRect, _btnEllipse, _btnLine })
            {
                button.Checked = button == activeButton;
            }
        }

        private void SetSelectedShape(ShapeBase? shape)
        {
            _selectedShape = shape;

            if (_selectedShape == null)
            {
                UpdateStatus("Ничего не выделено.");
                return;
            }

            _currentStrokeColor = _selectedShape.StrokeColor;
            _currentFillColor = _selectedShape.FillColor;
            _currentIsFilled = _selectedShape.IsFilled;
            _currentStrokeWidth = _selectedShape.StrokeWidth;

            if (_selectedShape is LineShape)
            {
                _btnFillToggle.Enabled = false;
                _btnFillColor.Enabled = false;
            }
            else
            {
                _btnFillToggle.Enabled = true;
                _btnFillColor.Enabled = true;
            }

            _btnFillToggle.Checked = _currentIsFilled;
            _cmbStrokeWidth.SelectedItem = ((int)_currentStrokeWidth).ToString();

            UpdateColorButtons();
            UpdateStatus("Фигура выделена: можно перемещать или менять размер.");
        }

        private void UpdateColorButtons()
        {
            _btnStrokeColor.BackColor = _currentStrokeColor;
            _btnStrokeColor.ForeColor = GetReadableTextColor(_currentStrokeColor);

            _btnFillColor.BackColor = _currentFillColor;
            _btnFillColor.ForeColor = GetReadableTextColor(_currentFillColor);
        }

        private static Color GetReadableTextColor(Color background)
        {
            int brightness = (background.R * 299 + background.G * 587 + background.B * 114) / 1000;
            return brightness > 140 ? Color.Black : Color.White;
        }

        private void PickStrokeColor()
        {
            using var dialog = new ColorDialog
            {
                Color = _currentStrokeColor,
                FullOpen = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _currentStrokeColor = dialog.Color;

            if (_selectedShape != null)
            {
                _selectedShape.StrokeColor = _currentStrokeColor;
                MarkDirty();
            }

            UpdateColorButtons();
            _canvas.Invalidate();
        }

        private void PickFillColor()
        {
            using var dialog = new ColorDialog
            {
                Color = _currentFillColor,
                FullOpen = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _currentFillColor = dialog.Color;

            if (_selectedShape != null && _selectedShape is not LineShape)
            {
                _selectedShape.FillColor = _currentFillColor;
                MarkDirty();
            }

            UpdateColorButtons();
            _canvas.Invalidate();
        }

        private void DeleteSelected()
        {
            if (_selectedShape == null)
            {
                return;
            }

            _shapes.Remove(_selectedShape);
            _selectedShape = null;
            MarkDirty();
            UpdateStatus("Фигура удалена.");
            _canvas.Invalidate();
        }

        private void BringForward()
        {
            if (_selectedShape == null)
            {
                return;
            }

            int index = _shapes.IndexOf(_selectedShape);
            if (index < 0 || index == _shapes.Count - 1)
            {
                return;
            }

            _shapes.RemoveAt(index);
            _shapes.Insert(index + 1, _selectedShape);
            MarkDirty();
            _canvas.Invalidate();
        }

        private void SendBackward()
        {
            if (_selectedShape == null)
            {
                return;
            }

            int index = _shapes.IndexOf(_selectedShape);
            if (index <= 0)
            {
                return;
            }

            _shapes.RemoveAt(index);
            _shapes.Insert(index - 1, _selectedShape);
            MarkDirty();
            _canvas.Invalidate();
        }

        private void ShowHelp()
        {
            string text =
                "Управление:\n" +
                "• Инструменты: Выбор / Прямоугольник / Эллипс / Линия\n" +
                "• Рисование: зажмите ЛКМ и тяните\n" +
                "• Выделение: клик по фигуре\n" +
                "• Перемещение: тяните выделенную фигуру\n" +
                "• Изменение размера: тяните маркеры (квадраты)\n\n" +
                "Горячие клавиши:\n" +
                "• Ctrl+N — новый\n" +
                "• Ctrl+O — открыть\n" +
                "• Ctrl+S — сохранить\n" +
                "• Delete — удалить выделенное\n" +
                "• Esc — отменить рисование\n" +
                "• Ctrl+Up / Ctrl+Down — слой вверх / вниз";

            MessageBox.Show(this, text, "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void NewDocument()
        {
            if (!ConfirmDiscardIfDirty())
            {
                return;
            }

            _shapes.Clear();
            _selectedShape = null;
            _currentFilePath = null;
            _isDirty = false;

            UpdateTitle();
            UpdateStatus("Новый документ.");
            _canvas.Invalidate();
        }

        private void OpenDocument()
        {
            if (!ConfirmDiscardIfDirty())
            {
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Filter = "Документ фигур (*.json)|*.json|Все файлы (*.*)|*.*",
                Title = "Открыть документ"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                var shapes = ShapeSerializer.LoadFromFile(dialog.FileName);
                _shapes.Clear();
                _shapes.AddRange(shapes);

                _selectedShape = null;
                _currentFilePath = dialog.FileName;
                _isDirty = false;

                UpdateTitle();
                UpdateStatus("Документ открыт.");
                _canvas.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка открытия", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveDocument()
        {
            if (string.IsNullOrWhiteSpace(_currentFilePath))
            {
                SaveDocumentAs();
                return;
            }

            try
            {
                ShapeSerializer.SaveToFile(_currentFilePath, _shapes);
                _isDirty = false;
                UpdateTitle();
                UpdateStatus("Сохранено.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveDocumentAs()
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Документ фигур (*.json)|*.json|Все файлы (*.*)|*.*",
                Title = "Сохранить как..."
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _currentFilePath = dialog.FileName;
            SaveDocument();
        }

        private bool ConfirmDiscardIfDirty()
        {
            if (!_isDirty)
            {
                return true;
            }

            var result = MessageBox.Show(
                this,
                "Есть несохранённые изменения. Сохранить перед продолжением?",
                "Подтверждение",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Cancel)
            {
                return false;
            }

            if (result == DialogResult.Yes)
            {
                SaveDocument();
                return !_isDirty;
            }

            return true;
        }

        private void MarkDirty()
        {
            _isDirty = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            string fileName = string.IsNullOrWhiteSpace(_currentFilePath) ? "Без имени" : System.IO.Path.GetFileName(_currentFilePath);
            Text = $"Задание 1 — Редактор фигур — {fileName}{(_isDirty ? " *" : string.Empty)}";
        }

        private void UpdateStatus(string text)
        {
            _statusLabel.Text = text;
        }

        private enum ToolMode
        {
            Select = 0,
            DrawRectangle = 1,
            DrawEllipse = 2,
            DrawLine = 3,
        }
    }
}
