namespace NotificationService.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "izometri";
    public string Password { get; set; } = "Izometri2026!";
}
