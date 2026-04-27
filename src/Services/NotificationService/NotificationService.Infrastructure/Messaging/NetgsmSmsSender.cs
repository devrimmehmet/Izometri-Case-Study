using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Abstractions;

namespace NotificationService.Infrastructure.Messaging;

public sealed class NetgsmSmsSender : ISmsService
{
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

        if (string.IsNullOrWhiteSpace(_options.Password) || string.IsNullOrWhiteSpace(_options.MsgHeader))
        {
            throw new InvalidOperationException("Netgsm Password and MsgHeader configuration values are required for SMS delivery.");
        }

        var endpoint = _options.UseOtpEndpoint ? "sms/rest/v2/otp" : "sms/rest/v2/send";
        var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl)
            ? "https://api.netgsm.com.tr"
            : _options.BaseUrl.TrimEnd('/');
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/{endpoint}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", BuildBasicToken());

        object payload = _options.UseOtpEndpoint
            ? new NetgsmOtpRequest(_options.MsgHeader, message, phoneNumber, EmptyToNull(_options.AppName))
            : new NetgsmSmsRequest(
                _options.MsgHeader,
                string.IsNullOrWhiteSpace(_options.Encoding) ? "TR" : _options.Encoding,
                [new NetgsmSmsMessage(message, phoneNumber)],
                EmptyToNull(_options.AppName));
        request.Content = JsonContent.Create(payload);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<NetgsmResponse>(cancellationToken);
        if (response.IsSuccessStatusCode && result?.Code == "00")
        {
            _logger.LogInformation("SMS sent via Netgsm. Phone: {Phone}, JobId: {JobId}", phoneNumber, result.JobId);
            return;
        }

        var code = result?.Code ?? ((int)response.StatusCode).ToString();
        var description = result?.Description ?? response.ReasonPhrase ?? "Unknown Netgsm error";
        _logger.LogError("Netgsm SMS failed. Phone: {Phone}, Code: {Code}, Description: {Description}", phoneNumber, code, description);
        throw new HttpRequestException($"Netgsm error {code}: {description}");
    }

    private string BuildBasicToken()
    {
        var raw = $"{_options.UserCode}:{_options.Password}";
        return Convert.ToBase64String(Encoding.ASCII.GetBytes(raw));
    }

    private static string? EmptyToNull(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private sealed record NetgsmSmsRequest(
        [property: JsonPropertyName("msgheader")] string MsgHeader,
        [property: JsonPropertyName("encoding")] string Encoding,
        [property: JsonPropertyName("messages")] IReadOnlyCollection<NetgsmSmsMessage> Messages,
        [property: JsonPropertyName("appname")] string? AppName);

    private sealed record NetgsmSmsMessage(
        [property: JsonPropertyName("msg")] string Message,
        [property: JsonPropertyName("no")] string Phone);

    private sealed record NetgsmOtpRequest(
        [property: JsonPropertyName("msgheader")] string MsgHeader,
        [property: JsonPropertyName("msg")] string Message,
        [property: JsonPropertyName("no")] string Phone,
        [property: JsonPropertyName("appname")] string? AppName);

    private sealed record NetgsmResponse(
        [property: JsonPropertyName("code")] string? Code,
        [property: JsonPropertyName("jobid")] string? JobId,
        [property: JsonPropertyName("description")] string? Description);
}
