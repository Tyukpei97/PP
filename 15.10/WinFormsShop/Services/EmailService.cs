using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using WinFormsShop.Models;

namespace WinFormsShop.Services;

public class EmailService
{
    private readonly SmtpSettingsService _settingsService;

    public EmailService(SmtpSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<(bool Sent, string? Error)> SendOrderEmailAsync(Order order, User user)
    {
        var settings = await _settingsService.LoadAsync();
        if (settings == null || string.IsNullOrWhiteSpace(settings.Host))
        {
            return (false, "SMTP не настроен.");
        }

        try
        {
            await Task.Run(() =>
            {
                using var client = new SmtpClient(settings.Host, settings.Port)
                {
                    EnableSsl = settings.EnableSsl
                };

                if (!string.IsNullOrWhiteSpace(settings.Username))
                {
                    client.Credentials = new NetworkCredential(settings.Username, settings.Password);
                }

                var body = new StringBuilder();
                body.AppendLine("Спасибо за заказ!");
                body.AppendLine(order.OrderDetails);
                body.AppendLine($"Статус: {order.Status}");

                var message = new MailMessage
                {
                    From = new MailAddress(string.IsNullOrWhiteSpace(settings.FromEmail) ? settings.Username : settings.FromEmail),
                    Subject = "Подтверждение заказа",
                    Body = body.ToString()
                };
                message.To.Add(user.Email);
                client.Send(message);
            });
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
