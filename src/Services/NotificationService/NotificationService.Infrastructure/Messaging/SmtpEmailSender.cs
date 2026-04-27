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
        if (string.IsNullOrWhiteSpace(toEmail) || (string.IsNullOrWhiteSpace(_options.Host) && !_options.UsePickupFolder))
        {
            _logger.LogDebug("SMTP not configured or recipient empty, skipping email to {ToEmail}.", toEmail);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, ResolveFromEmail()));

        foreach (var address in toEmail.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            message.To.Add(MailboxAddress.Parse(address));
        }

        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        if (_options.UsePickupFolder)
        {
            Directory.CreateDirectory(_options.PickupFolderPath);
            var filePath = Path.Combine(_options.PickupFolderPath, $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.eml");
            await message.WriteToAsync(filePath, cancellationToken);
            _logger.LogInformation("Email written to pickup folder. To: {To}, Subject: {Subject}, Path: {Path}", toEmail, subject, filePath);
            return;
        }

        using var client = new SmtpClient();
        client.Timeout = Math.Max(_options.TimeoutSeconds, 1) * 1000;
        if (_options.AllowInvalidCertificate || _options.IgnoreCertificateErrors)
        {
            client.ServerCertificateValidationCallback = (_, _, _, _) => true;
        }

        await client.ConnectAsync(_options.Host, _options.Port,
            _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
            cancellationToken);
        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        _logger.LogInformation("Email sent. To: {To}, Subject: {Subject}", toEmail, subject);
    }

    private string ResolveFromEmail()
    {
        if (!string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            return _options.FromEmail;
        }

        return _options.FromAddress;
    }
}
