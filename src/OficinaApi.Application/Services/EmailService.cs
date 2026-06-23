using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();

        var email = _configuration["EmailSettings:Email"];
        var password = _configuration["EmailSettings:Password"];
        var host = _configuration["EmailSettings:Host"] ?? "smtp.gmail.com";
        var portStr = _configuration["EmailSettings:Port"];
        var port = string.IsNullOrEmpty(portStr) ? 587 : int.Parse(portStr);

        message.From.Add(new MailboxAddress("Oficina Mecânica", email));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            host,
            port,
            SecureSocketOptions.StartTls
        );

        await client.AuthenticateAsync(
            email,
            password
        );

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
