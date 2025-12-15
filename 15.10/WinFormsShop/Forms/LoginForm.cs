using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsShop.Services;

namespace WinFormsShop.Forms;

public class LoginForm : Form
{
    private readonly UserService _userService = new();
    private readonly ProductService _productService = new();
    private readonly OrderService _orderService = new();
    private readonly SmtpSettingsService _smtpSettingsService = new();
    private readonly EmailService _emailService;

    private TextBox _loginBox = null!;
    private TextBox _passwordBox = null!;
    private Label _infoLabel = null!;
    private Button _loginButton = null!;

    public LoginForm()
    {
        _emailService = new EmailService(_smtpSettingsService);
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Вход в магазин";
        Width = 420;
        Height = 280;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var title = new Label
        {
            Text = "Интернет-магазин",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 15)
        };

        var loginLabel = new Label { Text = "Логин", Location = new Point(20, 60), AutoSize = true };
        _loginBox = new TextBox { Location = new Point(120, 55), Width = 250 };

        var passwordLabel = new Label { Text = "Пароль", Location = new Point(20, 100), AutoSize = true };
        _passwordBox = new TextBox { Location = new Point(120, 95), Width = 250, UseSystemPasswordChar = true };

        _loginButton = new Button
        {
            Text = "Войти",
            Location = new Point(120, 140),
            Width = 120
        };
        _loginButton.Click += async (_, _) => await LoginAsync();

        var registerButton = new Button
        {
            Text = "Регистрация",
            Location = new Point(250, 140),
            Width = 120
        };
        registerButton.Click += (_, _) => OpenRegistration();

        var exitButton = new Button
        {
            Text = "Выход",
            Location = new Point(120, 180),
            Width = 250
        };
        exitButton.Click += (_, _) => Close();

        _infoLabel = new Label
        {
            AutoSize = true,
            ForeColor = Color.DarkGreen,
            Location = new Point(20, 220)
        };

        Controls.AddRange(new Control[]
        {
            title, loginLabel, _loginBox, passwordLabel, _passwordBox,
            _loginButton, registerButton, exitButton, _infoLabel
        });

        Load += (_, _) => ShowInitializationInfo();
    }

    private void ShowInitializationInfo()
    {
        if (AppInitializer.Result is { AdminCreated: true } result)
        {
            _infoLabel.Text = $"Создан админ: {result.AdminLogin} / {result.AdminPassword}";
            MessageBox.Show(
                $"База данных создана. Администратор: {result.AdminLogin} / {result.AdminPassword}",
                "Первый запуск",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private async Task LoginAsync()
    {
        try
        {
            _loginButton.Enabled = false;
            var login = _loginBox.Text.Trim();
            var password = _passwordBox.Text;

            var user = await _userService.AuthenticateAsync(login, password);
            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AppSession.SetUser(user);
            Hide();
            using (var main = new MainForm(user, _productService, _orderService, _emailService, _smtpSettingsService))
            {
                main.ShowDialog();
            }

            AppSession.Logout();
            Show();
            _passwordBox.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _loginButton.Enabled = true;
        }
    }

    private void OpenRegistration()
    {
        using var reg = new RegistrationForm(_userService);
        reg.ShowDialog();
    }
}
