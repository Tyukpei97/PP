using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsShop.Models;
using WinFormsShop.Services;

namespace WinFormsShop.Forms;

public class CheckoutForm : Form
{
    private readonly Cart _cart;
    private readonly OrderService _orderService;
    private readonly EmailService _emailService;
    private readonly SmtpSettingsService _smtpSettingsService;
    private readonly User _user;

    private TextBox _addressBox = null!;
    private ComboBox _paymentBox = null!;
    private Label _totalLabel = null!;
    private Button _confirmButton = null!;

    public CheckoutForm(Cart cart, OrderService orderService, EmailService emailService, SmtpSettingsService smtpSettingsService, User user)
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
        Text = "Оформление заказа";
        Width = 520;
        Height = 420;
        StartPosition = FormStartPosition.CenterParent;

        _totalLabel = new Label
        {
            Text = $"Итого: {_cart.Total():0.00} ₽",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true
        };

        var addressLabel = new Label { Text = "Адрес доставки", Location = new Point(20, 60), AutoSize = true };
        _addressBox = new TextBox { Location = new Point(20, 80), Width = 450, Height = 100, Multiline = true };

        var paymentLabel = new Label { Text = "Способ оплаты", Location = new Point(20, 190), AutoSize = true };
        _paymentBox = new ComboBox { Location = new Point(20, 210), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
        _paymentBox.Items.AddRange(new object[]
        {
            "Карта онлайн",
            "Наличными при получении",
            "СБП",
            "Бесконтактная оплата (Apple Pay / Google Pay)"
        });
        _paymentBox.SelectedIndex = 0;

        _confirmButton = new Button { Text = "Подтвердить заказ", Location = new Point(20, 260), Width = 200 };
        _confirmButton.Click += async (_, _) => await CreateOrderAsync();

        var cancelButton = new Button { Text = "Отмена", Location = new Point(230, 260), Width = 120 };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[]
        {
            _totalLabel, addressLabel, _addressBox, paymentLabel, _paymentBox, _confirmButton, cancelButton
        });
    }

    private async Task CreateOrderAsync()
    {
        if (string.IsNullOrWhiteSpace(_addressBox.Text))
        {
            MessageBox.Show("Введите адрес доставки.", "Валидация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _confirmButton.Enabled = false;
            var order = await _orderService.CreateOrderAsync(_user, _cart, _addressBox.Text.Trim(),
                _paymentBox.SelectedItem?.ToString() ?? "Не указано");
            _cart.Clear();
            DialogResult = DialogResult.OK;

            _ = Task.Run(async () =>
            {
                var (sent, error) = await _emailService.SendOrderEmailAsync(order, _user);
                if (!sent && !string.IsNullOrWhiteSpace(error))
                {
                    BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show($"Заказ сохранен, но письмо не отправлено: {error}", "Предупреждение",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }));
                }
            });

            MessageBox.Show("Заказ оформлен! Подробности отправлены на почту (если настроен SMTP).", "Готово",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось оформить заказ: {ex.Message}", "Ошибка", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _confirmButton.Enabled = true;
        }
    }
}
