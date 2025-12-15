using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsShop.Services;

namespace WinFormsShop.Forms;

public class RegistrationForm : Form
{
    private readonly UserService _userService;
    private TextBox _loginBox = null!;
    private TextBox _emailBox = null!;
    private TextBox _passwordBox = null!;
    private Button _registerButton = null!;

    public RegistrationForm(UserService userService)
    {
        _userService = userService;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Регистрация";
        Width = 420;
        Height = 320;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var title = new Label
        {
            Text = "Создание учетной записи",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 15)
        };

        var loginLabel = new Label { Text = "Логин", Location = new Point(20, 60), AutoSize = true };
        _loginBox = new TextBox { Location = new Point(170, 55), Width = 200 };

        var emailLabel = new Label { Text = "Эл. почта", Location = new Point(20, 100), AutoSize = true };
        _emailBox = new TextBox { Location = new Point(170, 95), Width = 200 };

        var passwordLabel = new Label { Text = "Пароль", Location = new Point(20, 140), AutoSize = true };
        _passwordBox = new TextBox { Location = new Point(170, 135), Width = 200, UseSystemPasswordChar = true };

        _registerButton = new Button
        {
            Text = "Зарегистрироваться",
            Location = new Point(170, 180),
            Width = 200
        };
        _registerButton.Click += async (_, _) => await RegisterAsync();

        var cancelButton = new Button
        {
            Text = "Отмена",
            Location = new Point(170, 220),
            Width = 200
        };
        cancelButton.Click += (_, _) => Close();

        Controls.AddRange(new Control[]
        {
            title, loginLabel, _loginBox,
            emailLabel, _emailBox,
            passwordLabel, _passwordBox,
            _registerButton, cancelButton
        });
    }

    private async Task RegisterAsync()
    {
        try
        {
            _registerButton.Enabled = false;
            var (success, error) = await _userService.RegisterAsync(_loginBox.Text.Trim(), _emailBox.Text.Trim(), _passwordBox.Text);
            if (!success)
            {
                MessageBox.Show(error ?? "Не удалось зарегистрировать пользователя.", "Ошибка", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Регистрация успешна. Теперь можете войти.", "Готово",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _registerButton.Enabled = true;
        }
    }
}
