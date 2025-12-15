using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsShop.Models;
using WinFormsShop.Services;

namespace WinFormsShop.Forms;

public class MainForm : Form
{
    private readonly User _user;
    private readonly ProductService _productService;
    private readonly OrderService _orderService;
    private readonly EmailService _emailService;
    private readonly SmtpSettingsService _smtpSettingsService;

    private DataGridView _productsGrid = null!;
    private ComboBox _categoryBox = null!;
    private TextBox _searchBox = null!;
    private NumericUpDown _minPrice = null!;
    private NumericUpDown _maxPrice = null!;
    private NumericUpDown _quantity = null!;
    private Button _addToCartButton = null!;
    private Label _userLabel = null!;

    private List<Product> _currentProducts = new();

    public MainForm(User user, ProductService productService, OrderService orderService, EmailService emailService,
        SmtpSettingsService smtpSettingsService)
    {
        _user = user;
        _productService = productService;
        _orderService = orderService;
        _emailService = emailService;
        _smtpSettingsService = smtpSettingsService;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Каталог магазина";
        Width = 1000;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        _userLabel = new Label
        {
            Text = $"Пользователь: {_user.Login} ({_user.Role})",
            Location = new Point(10, 10),
            AutoSize = true
        };

        var searchLabel = new Label { Text = "Поиск", Location = new Point(10, 40), AutoSize = true };
        _searchBox = new TextBox { Location = new Point(70, 35), Width = 200 };

        var categoryLabel = new Label { Text = "Категория", Location = new Point(280, 40), AutoSize = true };
        _categoryBox = new ComboBox { Location = new Point(360, 35), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
        _categoryBox.Items.Add("Все категории");
        _categoryBox.SelectedIndex = 0;

        var minLabel = new Label { Text = "Цена от", Location = new Point(520, 40), AutoSize = true };
        _minPrice = new NumericUpDown { Location = new Point(580, 35), Width = 80, Maximum = 1_000_000, DecimalPlaces = 0 };

        var maxLabel = new Label { Text = "до", Location = new Point(670, 40), AutoSize = true };
        _maxPrice = new NumericUpDown { Location = new Point(700, 35), Width = 80, Maximum = 1_000_000, DecimalPlaces = 0, Value = 1_000_000 };

        var filterButton = new Button { Text = "Применить", Location = new Point(800, 33), Width = 80 };
        filterButton.Click += async (_, _) => await LoadProductsAsync();

        var resetButton = new Button { Text = "Сброс", Location = new Point(890, 33), Width = 80 };
        resetButton.Click += async (_, _) =>
        {
            _searchBox.Clear();
            _categoryBox.SelectedIndex = 0;
            _minPrice.Value = 0;
            _maxPrice.Value = 1_000_000;
            await LoadProductsAsync();
        };

        _productsGrid = new DataGridView
        {
            Location = new Point(10, 70),
            Width = 960,
            Height = 480,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowTemplate = { Height = 80 },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        _productsGrid.Columns.Add(new DataGridViewImageColumn
        {
            HeaderText = "Изображение",
            Name = "Image",
            ImageLayout = DataGridViewImageCellLayout.Zoom,
            Width = 120
        });
        _productsGrid.Columns.Add("Name", "Название");
        _productsGrid.Columns.Add("Category", "Категория");
        _productsGrid.Columns.Add("Price", "Цена");
        _productsGrid.Columns.Add("Description", "Описание");
        _productsGrid.Columns.Add("Id", "Id");
        _productsGrid.Columns["Id"].Visible = false;

        var quantityLabel = new Label { Text = "Количество", Location = new Point(10, 570), AutoSize = true };
        _quantity = new NumericUpDown { Location = new Point(100, 565), Width = 80, Minimum = 1, Maximum = 100, Value = 1 };

        _addToCartButton = new Button { Text = "Добавить в корзину", Location = new Point(200, 563), Width = 160 };
        _addToCartButton.Click += (_, _) => AddToCart();

        var cartButton = new Button { Text = "Корзина / Оформление", Location = new Point(370, 563), Width = 180 };
        cartButton.Click += (_, _) => OpenCart();

        var logoutButton = new Button { Text = "Выход", Location = new Point(560, 563), Width = 100 };
        logoutButton.Click += (_, _) => Close();

        var adminButton = new Button { Text = "Админ-панель", Location = new Point(670, 563), Width = 140 };
        adminButton.Click += (_, _) => OpenAdmin();
        adminButton.Visible = _user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase);

        var refreshButton = new Button { Text = "Обновить", Location = new Point(820, 563), Width = 120 };
        refreshButton.Click += async (_, _) => await LoadProductsAsync();

        Controls.AddRange(new Control[]
        {
            _userLabel, searchLabel, _searchBox, categoryLabel, _categoryBox,
            minLabel, _minPrice, maxLabel, _maxPrice, filterButton, resetButton,
            _productsGrid, quantityLabel, _quantity, _addToCartButton, cartButton,
            logoutButton, adminButton, refreshButton
        });

        Shown += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _productService.GetCategoriesAsync();
        _categoryBox.Items.Clear();
        _categoryBox.Items.Add("Все категории");
        foreach (var c in categories)
        {
            _categoryBox.Items.Add(c);
        }

        _categoryBox.SelectedIndex = 0;
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var category = _categoryBox.SelectedIndex > 0 ? _categoryBox.SelectedItem?.ToString() : null;
            decimal? min = _minPrice.Value > 0 ? _minPrice.Value : null;
            decimal? max = _maxPrice.Value < _maxPrice.Maximum ? _maxPrice.Value : null;
            var search = _searchBox.Text.Trim();
            _currentProducts = await _productService.GetProductsAsync(category, (decimal?)min, (decimal?)max, search, includeInactive: false);
            RenderProducts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RenderProducts()
    {
        _productsGrid.Rows.Clear();
        foreach (var product in _currentProducts)
        {
            var image = LoadProductImage(product.ImagePath);
            _productsGrid.Rows.Add(image, product.Name, product.Category, $"{product.Price:0.00} ₽", product.Description, product.Id);
        }
    }

    private Image LoadProductImage(string? path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                return Image.FromStream(fs);
            }
        }
        catch
        {
            // ignored, fallback to placeholder below
        }

        var placeholder = new Bitmap(120, 80);
        using var g = Graphics.FromImage(placeholder);
        g.Clear(Color.LightGray);
        using var pen = new Pen(Color.DarkGray);
        g.DrawRectangle(pen, 2, 2, 115, 75);
        g.DrawString("Нет фото", new Font("Segoe UI", 9), Brushes.DimGray, 20, 30);
        return placeholder;
    }

    private void AddToCart()
    {
        if (_productsGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите товар.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var id = (int)_productsGrid.SelectedRows[0].Cells["Id"].Value;
        var product = _currentProducts.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return;
        }

        AppSession.CurrentCart?.AddItem(product, (int)_quantity.Value);
        MessageBox.Show("Товар добавлен в корзину.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OpenCart()
    {
        if (AppSession.CurrentCart == null)
        {
            MessageBox.Show("Сначала войдите в систему.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var cartForm = new CartForm(AppSession.CurrentCart, _orderService, _emailService, _smtpSettingsService, _user);
        cartForm.ShowDialog();
    }

    private void OpenAdmin()
    {
        if (!_user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Доступ только для администратора.", "Отказано", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var admin = new AdminForm(_productService, _orderService, _smtpSettingsService);
        admin.ShowDialog();
        _ = LoadProductsAsync();
    }
}
