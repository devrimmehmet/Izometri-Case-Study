using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Application.Abstractions;

namespace NotificationService.Infrastructure.Messaging;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly SmtpOptions _options;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(_options.Host))
        {
            _logger.LogDebug("SMTP not configured or recipient empty, skipping email to {ToEmail}.", toEmail);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));

        foreach (var address in toEmail.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            message.To.Add(MailboxAddress.Parse(address));
        }

        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Host, _options.Port,
            _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
            cancellationToken);
        await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        _logger.LogInformation("Email sent. To: {To}, Subject: {Subject}", toEmail, subject);
    }
}
