using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WinFormsShop.Models;
using WinFormsShop.Services;

namespace WinFormsShop.Forms;

public class CartForm : Form
{
    private readonly Cart _cart;
    private readonly OrderService _orderService;
    private readonly EmailService _emailService;
    private readonly SmtpSettingsService _smtpSettingsService;
    private readonly User _user;

    private DataGridView _cartGrid = null!;
    private Label _totalLabel = null!;
    private NumericUpDown _quantity = null!;

    public CartForm(Cart cart, OrderService orderService, EmailService emailService, SmtpSettingsService smtpSettingsService, User user)
    {
        _cart = cart;
        _orderService = orderService;
        _emailService = emailService;
        _smtpSettingsService = smtpSettingsService;
        _user = user;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Корзина";
        Width = 720;
        Height = 500;
        StartPosition = FormStartPosition.CenterParent;

        _cartGrid = new DataGridView
        {
            Location = new Point(10, 10),
            Width = 680,
            Height = 320,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        _cartGrid.Columns.Add("Name", "Товар");
        _cartGrid.Columns.Add("Price", "Цена");
        _cartGrid.Columns.Add("Quantity", "Кол-во");
        _cartGrid.Columns.Add("Subtotal", "Сумма");
        _cartGrid.Columns.Add("ProductId", "Id");
        _cartGrid.Columns["ProductId"].Visible = false;

        var quantityLabel = new Label { Text = "Новое количество", Location = new Point(10, 340), AutoSize = true };
        _quantity = new NumericUpDown { Location = new Point(130, 335), Width = 80, Minimum = 1, Maximum = 100, Value = 1 };

        var updateButton = new Button { Text = "Изменить", Location = new Point(220, 333), Width = 100 };
        updateButton.Click += (_, _) => UpdateQuantity();

        var removeButton = new Button { Text = "Удалить", Location = new Point(330, 333), Width = 100 };
        removeButton.Click += (_, _) => RemoveItem();

        var checkoutButton = new Button { Text = "Оформить заказ", Location = new Point(440, 333), Width = 120 };
        checkoutButton.Click += (_, _) => OpenCheckout();

        var closeButton = new Button { Text = "Закрыть", Location = new Point(570, 333), Width = 120 };
        closeButton.Click += (_, _) => Close();

        _totalLabel = new Label
        {
            Text = "Итого: 0 ₽",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Location = new Point(10, 380),
            AutoSize = true
        };

        Controls.AddRange(new Control[]
        {
            _cartGrid, quantityLabel, _quantity, updateButton, removeButton, checkoutButton, closeButton, _totalLabel
        });

        Shown += (_, _) => RenderCart();
    }

    private void RenderCart()
    {
        _cartGrid.Rows.Clear();
        foreach (var item in _cart.Items)
        {
            _cartGrid.Rows.Add(item.Product.Name, $"{item.Product.Price:0.00} ₽", item.Quantity,
                $"{item.Subtotal:0.00} ₽", item.Product.Id);
        }

        _totalLabel.Text = $"Итого: {_cart.Total():0.00} ₽";
    }

    private void UpdateQuantity()
    {
        if (_cartGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите товар.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var id = (int)_cartGrid.SelectedRows[0].Cells["ProductId"].Value;
        _cart.UpdateQuantity(id, (int)_quantity.Value);
        RenderCart();
    }

    private void RemoveItem()
    {
        if (_cartGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите товар.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var id = (int)_cartGrid.SelectedRows[0].Cells["ProductId"].Value;
        _cart.Remove(id);
        RenderCart();
    }

    private void OpenCheckout()
    {
        if (_cart.Items.Count == 0)
        {
            MessageBox.Show("Корзина пуста.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var checkout = new CheckoutForm(_cart, _orderService, _emailService, _smtpSettingsService, _user);
        if (checkout.ShowDialog() == DialogResult.OK)
        {
            RenderCart();
        }
    }
}
