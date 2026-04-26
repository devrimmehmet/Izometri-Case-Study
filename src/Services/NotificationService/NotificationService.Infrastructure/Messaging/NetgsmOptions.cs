namespace NotificationService.Infrastructure.Messaging;

public sealed class NetgsmOptions
{
    public string UserCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string MsgHeader { get; set; } = string.Empty;
}
