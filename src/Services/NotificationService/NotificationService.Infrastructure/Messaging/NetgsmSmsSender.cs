using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Abstractions;

namespace NotificationService.Infrastructure.Messaging;

public sealed class NetgsmSmsSender : ISmsService
{
    private const string ApiUrl = "https://api.netgsm.com.tr/sms/send/get/";
    private readonly HttpClient _httpClient;
    private readonly ILogger<NetgsmSmsSender> _logger;
    private readonly NetgsmOptions _options;

    public NetgsmSmsSender(HttpClient httpClient, IOptions<NetgsmOptions> options, ILogger<NetgsmSmsSender> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(_options.UserCode))
        {
            _logger.LogDebug("Netgsm not configured or phone empty, skipping SMS to {Phone}.", phoneNumber);
            return;
        }

        var url = $"{ApiUrl}?usercode={Uri.EscapeDataString(_options.UserCode)}" +
                  $"&password={Uri.EscapeDataString(_options.Password)}" +
                  $"&gsmno={Uri.EscapeDataString(phoneNumber)}" +
                  $"&message={Uri.EscapeDataString(message)}" +
                  $"&msgheader={Uri.EscapeDataString(_options.MsgHeader)}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        var content = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();

        // Netgsm returns "00 <msgId>", "01 <msgId>", or "02 <msgId>" on success
        if (content.StartsWith("00") || content.StartsWith("01") || content.StartsWith("02"))
        {
            _logger.LogInformation("SMS sent via Netgsm. Phone: {Phone}, Response: {Response}", phoneNumber, content);
        }
        else
        {
            _logger.LogError("Netgsm SMS failed. Phone: {Phone}, Response: {Response}", phoneNumber, content);
            throw new HttpRequestException($"Netgsm error: {content}");
        }
    }
}
