namespace NotificationService.Infrastructure.Messaging;

public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public bool AllowInvalidCertificate { get; set; }
    public bool IgnoreCertificateErrors { get; set; }
    public bool UsePickupFolder { get; set; }
    public int TimeoutSeconds { get; set; } = 15;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Izometri Expense";
    public string PickupFolderPath { get; set; } = "App_Data/mail-drop";
}
