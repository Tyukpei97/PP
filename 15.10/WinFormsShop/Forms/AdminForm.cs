using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsShop.Models;
using WinFormsShop.Services;

namespace WinFormsShop.Forms;

public class AdminForm : Form
{
    private readonly ProductService _productService;
    private readonly OrderService _orderService;
    private readonly SmtpSettingsService _smtpSettingsService;

    private DataGridView _productGrid = null!;
    private DataGridView _ordersGrid = null!;
    private ComboBox _statusBox = null!;

    private TextBox _smtpHost = null!;
    private NumericUpDown _smtpPort = null!;
    private CheckBox _smtpSsl = null!;
    private TextBox _smtpUser = null!;
    private TextBox _smtpPass = null!;
    private TextBox _smtpFrom = null!;

    private List<Product> _products = new();

    public AdminForm(ProductService productService, OrderService orderService, SmtpSettingsService smtpSettingsService)
    {
        _productService = productService;
        _orderService = orderService;
        _smtpSettingsService = smtpSettingsService;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Админ-панель";
        Width = 1100;
        Height = 720;
        StartPosition = FormStartPosition.CenterParent;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        var productsPage = new TabPage("Товары");
        var ordersPage = new TabPage("Заказы");
        var smtpPage = new TabPage("Почта (SMTP)");

        InitializeProductsTab(productsPage);
        InitializeOrdersTab(ordersPage);
        InitializeSmtpTab(smtpPage);

        tabs.TabPages.Add(productsPage);
        tabs.TabPages.Add(ordersPage);
        tabs.TabPages.Add(smtpPage);

        Controls.Add(tabs);

        Shown += async (_, _) =>
        {
            await LoadProductsAsync();
            await LoadOrdersAsync();
            await LoadSmtpSettingsAsync();
        };
    }

    private void InitializeProductsTab(TabPage page)
    {
        _productGrid = new DataGridView
        {
            Dock = DockStyle.Top,
            Height = 500,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        _productGrid.Columns.Add("Id", "Id");
        _productGrid.Columns.Add("Name", "Название");
        _productGrid.Columns.Add("Category", "Категория");
        _productGrid.Columns.Add("Price", "Цена");
        _productGrid.Columns.Add("Active", "Активен");
        _productGrid.Columns.Add("Image", "Изображение");
        _productGrid.Columns["Id"].Visible = false;

        var addButton = new Button { Text = "Добавить", Location = new Point(10, 520), Width = 120 };
        addButton.Click += async (_, _) => await AddProductAsync();

        var editButton = new Button { Text = "Изменить", Location = new Point(140, 520), Width = 120 };
        editButton.Click += async (_, _) => await EditProductAsync();

        var deactivateButton = new Button { Text = "Деактивировать", Location = new Point(270, 520), Width = 140 };
        deactivateButton.Click += async (_, _) => await SetActiveAsync(false);

        var activateButton = new Button { Text = "Активировать", Location = new Point(420, 520), Width = 140 };
        activateButton.Click += async (_, _) => await SetActiveAsync(true);

        var deleteButton = new Button { Text = "Удалить", Location = new Point(570, 520), Width = 120 };
        deleteButton.Click += async (_, _) => await DeleteProductAsync();

        page.Controls.Add(_productGrid);
        page.Controls.Add(addButton);
        page.Controls.Add(editButton);
        page.Controls.Add(deactivateButton);
        page.Controls.Add(activateButton);
        page.Controls.Add(deleteButton);
    }

    private void InitializeOrdersTab(TabPage page)
    {
        _ordersGrid = new DataGridView
        {
            Dock = DockStyle.Top,
            Height = 500,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        _ordersGrid.Columns.Add("Id", "Id");
        _ordersGrid.Columns.Add("User", "Пользователь");
        _ordersGrid.Columns.Add("Email", "Эл. почта");
        _ordersGrid.Columns.Add("Status", "Статус");
        _ordersGrid.Columns.Add("Total", "Сумма");
        _ordersGrid.Columns.Add("Created", "Создан");
        _ordersGrid.Columns.Add("Details", "Детали");
        _ordersGrid.Columns["Id"].Visible = false;

        _statusBox = new ComboBox
        {
            Location = new Point(10, 520),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _statusBox.Items.AddRange(new object[]
        {
            "В обработке",
            "Отправлен",
            "Доставлен",
            "Отменен"
        });
        _statusBox.SelectedIndex = 0;

        var statusButton = new Button { Text = "Изменить статус", Location = new Point(220, 518), Width = 150 };
        statusButton.Click += async (_, _) => await UpdateOrderStatusAsync();

        page.Controls.Add(_ordersGrid);
        page.Controls.Add(_statusBox);
        page.Controls.Add(statusButton);
    }

    private void InitializeSmtpTab(TabPage page)
    {
        var hostLabel = new Label { Text = "Хост", Location = new Point(20, 30), AutoSize = true };
        _smtpHost = new TextBox { Location = new Point(150, 25), Width = 300 };

        var portLabel = new Label { Text = "Порт", Location = new Point(20, 70), AutoSize = true };
        _smtpPort = new NumericUpDown { Location = new Point(150, 65), Width = 100, Maximum = 100000, Value = 587 };

        _smtpSsl = new CheckBox { Text = "SSL (шифрование)", Location = new Point(270, 67), AutoSize = true, Checked = true };

        var userLabel = new Label { Text = "Имя пользователя", Location = new Point(20, 110), AutoSize = true };
        _smtpUser = new TextBox { Location = new Point(150, 105), Width = 300 };

        var passLabel = new Label { Text = "Пароль", Location = new Point(20, 150), AutoSize = true };
        _smtpPass = new TextBox { Location = new Point(150, 145), Width = 300, UseSystemPasswordChar = true };

        var fromLabel = new Label { Text = "Отправитель (email)", Location = new Point(20, 190), AutoSize = true };
        _smtpFrom = new TextBox { Location = new Point(150, 185), Width = 300 };

        var saveButton = new Button { Text = "Сохранить", Location = new Point(150, 230), Width = 120 };
        saveButton.Click += async (_, _) => await SaveSmtpAsync();

        page.Controls.AddRange(new Control[]
        {
            hostLabel, _smtpHost, portLabel, _smtpPort, _smtpSsl,
            userLabel, _smtpUser, passLabel, _smtpPass, fromLabel, _smtpFrom, saveButton
        });
    }

    private async Task LoadProductsAsync()
    {
        _products = await _productService.GetProductsAsync(includeInactive: true);
        _productGrid.Rows.Clear();
        foreach (var p in _products)
        {
            _productGrid.Rows.Add(p.Id, p.Name, p.Category, $"{p.Price:0.00} ₽", p.IsActive ? "Да" : "Нет", p.ImagePath);
        }
    }

    private async Task AddProductAsync()
    {
        var product = new Product { IsActive = true };
        using var form = new ProductEditForm(_productService, product);
        if (form.ShowDialog() == DialogResult.OK)
        {
            product = form.Product;
            await _productService.AddAsync(product);
            await LoadProductsAsync();
        }
    }

    private async Task EditProductAsync()
    {
        if (_productGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите товар.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var id = (int)_productGrid.SelectedRows[0].Cells["Id"].Value;
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return;
        }

        using var form = new ProductEditForm(_productService, new Product
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Price = product.Price,
            ImagePath = product.ImagePath,
            IsActive = product.IsActive
        });

        if (form.ShowDialog() == DialogResult.OK)
        {
            await _productService.UpdateAsync(form.Product);
            await LoadProductsAsync();
        }
    }

    private async Task SetActiveAsync(bool active)
    {
        if (_productGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите товар.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var id = (int)_productGrid.SelectedRows[0].Cells["Id"].Value;
        await _productService.SetActiveAsync(id, active);
        await LoadProductsAsync();
    }

    private async Task DeleteProductAsync()
    {
        if (_productGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите товар.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var id = (int)_productGrid.SelectedRows[0].Cells["Id"].Value;
        var confirm = MessageBox.Show("Удалить товар без возможности восстановления?", "Подтверждение",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        await _productService.DeleteAsync(id);
        await LoadProductsAsync();
    }

    private async Task LoadOrdersAsync()
    {
        var orders = await _orderService.GetOrdersWithUsersAsync();
        _ordersGrid.Rows.Clear();
        foreach (var entry in orders)
        {
            var order = entry.Order;
            _ordersGrid.Rows.Add(order.Id, entry.UserLogin, entry.UserEmail, order.Status,
                $"{order.TotalPrice:0.00} ₽", order.CreatedAt.ToLocalTime(), order.OrderDetails);
        }
    }

    private async Task UpdateOrderStatusAsync()
    {
        if (_ordersGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите заказ.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var id = (int)_ordersGrid.SelectedRows[0].Cells["Id"].Value;
        var status = _statusBox.SelectedItem?.ToString() ?? "В обработке";
        await _orderService.UpdateStatusAsync(id, status);
        await LoadOrdersAsync();
    }

    private async Task LoadSmtpSettingsAsync()
    {
        var settings = await _smtpSettingsService.LoadAsync();
        if (settings == null)
        {
            return;
        }

        _smtpHost.Text = settings.Host;
        _smtpPort.Value = settings.Port;
        _smtpSsl.Checked = settings.EnableSsl;
        _smtpUser.Text = settings.Username;
        _smtpPass.Text = settings.Password;
        _smtpFrom.Text = settings.FromEmail;
    }

    private async Task SaveSmtpAsync()
    {
        var settings = new SmtpSettings
        {
            Host = _smtpHost.Text.Trim(),
            Port = (int)_smtpPort.Value,
            EnableSsl = _smtpSsl.Checked,
            Username = _smtpUser.Text.Trim(),
            Password = _smtpPass.Text,
            FromEmail = _smtpFrom.Text.Trim()
        };

        await _smtpSettingsService.SaveAsync(settings);
        MessageBox.Show("SMTP настройки сохранены (шифрование DPAPI).", "Готово",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
