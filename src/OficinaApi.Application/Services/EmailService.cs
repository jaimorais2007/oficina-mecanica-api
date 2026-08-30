using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using OficinaApi.Domain.Interfaces;

namespace OficinaApi.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var email = _configuration["EmailSettings:Email"] ?? string.Empty;
        var password = _configuration["EmailSettings:Password"] ?? string.Empty;
        var host = _configuration["EmailSettings:Host"] ?? "smtp.gmail.com";
        var portStr = _configuration["EmailSettings:Port"];
        var port = string.IsNullOrEmpty(portStr) ? 587 : int.Parse(portStr);

        try
        {
            var message = new MimeMessage();

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

            _logger.LogInformation(
                "Email successfully sent to {Recipient}. Subject: {Subject}",
                to,
                subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email to {Recipient}. Subject: {Subject}. Host: {Host}, Port: {Port}. ErrorType: {ErrorType}. Message: {ErrorMessage}",
                to,
                subject,
                host,
                port,
                ex.GetType().Name,
                ex.Message);

            throw;
        }
    }
}
