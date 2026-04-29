using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure.Messaging;

namespace ExpenseService.Tests;

public sealed class LiveDeliveryAndHealthTests
{
    private const string LiveTestSwitch = "RUN_LIVE_DELIVERY_TESTS";

    [Fact]
    public async Task Docker_compose_services_are_healthy()
    {
        if (!LiveTestsEnabled())
        {
            return;
        }

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

        await AssertHealthyAsync(httpClient, "http://localhost:5001/health");
        await AssertHealthyAsync(httpClient, "http://localhost:5002/health");
        await AssertHealthyAsync(httpClient, "http://localhost:8025/api/v1/info");
        await AssertHealthyAsync(httpClient, "http://localhost:15673/api/overview", "izometri", "Izometri2026!");
        await AssertTcpOpenAsync("localhost", 15433);
        await AssertTcpOpenAsync("localhost", 15434);
        await AssertTcpOpenAsync("localhost", 5673);
    }

    [Fact]
    public async Task Send_test_email_to_devrimmehmet_gmail()
    {
        if (!LiveTestsEnabled())
        {
            return;
        }

        var subject = $"testtir {Guid.NewGuid():N}";
        var sender = new SmtpEmailSender(
            Options.Create(new SmtpOptions
            {
                FromName = "Izometri Expense",
                FromEmail = "no-reply@izometri.local",
                Host = Environment.GetEnvironmentVariable("LIVE_MAIL_HOST") ?? "localhost",
                Port = ReadInt("LIVE_MAIL_PORT", 1025),
                UserName = Environment.GetEnvironmentVariable("LIVE_MAIL_USERNAME") ?? string.Empty,
                Password = Environment.GetEnvironmentVariable("LIVE_MAIL_PASSWORD") ?? string.Empty,
                UseSsl = ReadBool("LIVE_MAIL_USE_SSL", false),
                IgnoreCertificateErrors = ReadBool("LIVE_MAIL_IGNORE_CERTIFICATE_ERRORS", false),
                TimeoutSeconds = 15
            }),
            NullLogger<SmtpEmailSender>.Instance);

        await sender.SendAsync("Devrimmehmet@gmail.com", subject, "testtir", CancellationToken.None);

        if (string.Equals(Environment.GetEnvironmentVariable("LIVE_MAIL_VERIFY_MAILPIT"), "0", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        var response = await httpClient.GetFromJsonAsync<MailpitMessagesResponse>("http://localhost:8025/api/v1/messages");
        Assert.NotNull(response);
        Assert.Contains(response.Messages, message =>
            string.Equals(message.Subject, subject, StringComparison.Ordinal) &&
            message.To.Any(to => string.Equals(to.Address, "Devrimmehmet@gmail.com", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task Send_test_sms_to_configured_phone()
    {
        if (!LiveTestsEnabled() ||
            !string.Equals(Environment.GetEnvironmentVariable("LIVE_NETGSM_SEND_SMS"), "1", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var options = new NetgsmOptions
        {
            UserCode = Environment.GetEnvironmentVariable("LIVE_NETGSM_USERCODE") ?? string.Empty,
            Password = Environment.GetEnvironmentVariable("LIVE_NETGSM_PASSWORD") ?? string.Empty,
            MsgHeader = Environment.GetEnvironmentVariable("LIVE_NETGSM_MSGHEADER") ?? string.Empty,
            BaseUrl = Environment.GetEnvironmentVariable("LIVE_NETGSM_BASEURL") ?? "https://api.netgsm.com.tr",
            Encoding = "TR",
            UseOtpEndpoint = ReadBool("LIVE_NETGSM_USE_OTP_ENDPOINT", false)
        };

        if (string.IsNullOrWhiteSpace(options.UserCode) ||
            string.IsNullOrWhiteSpace(options.Password) ||
            string.IsNullOrWhiteSpace(options.MsgHeader))
        {
            throw new InvalidOperationException("LIVE_NETGSM_USERCODE, LIVE_NETGSM_PASSWORD and LIVE_NETGSM_MSGHEADER are required for the live SMS test.");
        }

        var sender = new NetgsmSmsSender(
            new HttpClient { Timeout = TimeSpan.FromSeconds(30) },
            Options.Create(options),
            NullLogger<NetgsmSmsSender>.Instance);

        await sender.SendAsync("5438194976", "test sms", CancellationToken.None);
    }

    private static bool LiveTestsEnabled()
    {
        return string.Equals(Environment.GetEnvironmentVariable(LiveTestSwitch), "1", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task AssertHealthyAsync(HttpClient httpClient, string url, string? userName = null, string? password = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrWhiteSpace(userName))
        {
            var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));
            request.Headers.Authorization = new("Basic", token);
        }

        using var response = await httpClient.SendAsync(request);
        Assert.True(response.IsSuccessStatusCode, $"{url} returned {(int)response.StatusCode} {response.ReasonPhrase}");
    }

    private static async Task AssertTcpOpenAsync(string host, int port)
    {
        using var tcpClient = new System.Net.Sockets.TcpClient();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await tcpClient.ConnectAsync(host, port, timeout.Token);
        Assert.True(tcpClient.Connected, $"{host}:{port} is not reachable.");
    }

    private static int ReadInt(string key, int defaultValue)
    {
        return int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : defaultValue;
    }

    private static bool ReadBool(string key, bool defaultValue)
    {
        return bool.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : defaultValue;
    }

    private sealed record MailpitMessagesResponse(
        [property: JsonPropertyName("messages")] IReadOnlyCollection<MailpitMessage> Messages);

    private sealed record MailpitMessage(
        [property: JsonPropertyName("Subject")] string Subject,
        [property: JsonPropertyName("To")] IReadOnlyCollection<MailpitAddress> To);

    private sealed record MailpitAddress(
        [property: JsonPropertyName("Address")] string Address);
}
