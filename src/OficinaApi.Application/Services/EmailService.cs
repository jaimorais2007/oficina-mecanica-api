using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.Services;

public class EmailService : IEmailService
{
    public async Task SendAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Oficina Mecânica", "noreplyoficinamecanica@gmail.com"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            "smtp.gmail.com",
            587,
            SecureSocketOptions.StartTls
        );

        await client.AuthenticateAsync(
            "noreplyoficinamecanica@gmail.com",
            "owpy skvg jhvc rjgz"
        );

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
