namespace NotificationService.Infrastructure.Messaging;

public sealed class NetgsmOptions
{
    public string UserCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string MsgHeader { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.netgsm.com.tr";
    public string Encoding { get; set; } = "TR";
    public string AppName { get; set; } = string.Empty;
    public bool UseOtpEndpoint { get; set; }
}
