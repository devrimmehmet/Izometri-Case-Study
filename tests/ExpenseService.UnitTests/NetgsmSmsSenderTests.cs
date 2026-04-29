using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure.Messaging;

namespace ExpenseService.Tests;

public sealed class NetgsmSmsSenderTests
{
    [Fact]
    public async Task SendAsync_posts_rest_v2_json_payload_with_basic_auth()
    {
        var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent(new { code = "00", jobid = "12345", description = "success" })
        });
        var sender = CreateSender(handler, new NetgsmOptions
        {
            UserCode = "8503000000",
            Password = "secret",
            MsgHeader = "IZOMETRI",
            BaseUrl = "https://api.netgsm.test",
            Encoding = "TR"
        });

        await sender.SendAsync("5551234567", "Türkçe test mesajı", CancellationToken.None);

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Post, handler.Request.Method);
        Assert.Equal("https://api.netgsm.test/sms/rest/v2/send", handler.Request.RequestUri!.ToString());
        Assert.Equal("Basic", handler.Request.Headers.Authorization!.Scheme);

        var payload = handler.ReadBody();
        Assert.Equal("IZOMETRI", payload.RootElement.GetProperty("msgheader").GetString());
        Assert.Equal("TR", payload.RootElement.GetProperty("encoding").GetString());
        var firstMessage = payload.RootElement.GetProperty("messages")[0];
        Assert.Equal("Türkçe test mesajı", firstMessage.GetProperty("msg").GetString());
        Assert.Equal("5551234567", firstMessage.GetProperty("no").GetString());
    }

    [Fact]
    public async Task SendAsync_uses_otp_endpoint_when_configured()
    {
        var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent(new { code = "00", jobid = "otp-1", description = "success" })
        });
        var sender = CreateSender(handler, new NetgsmOptions
        {
            UserCode = "8503000000",
            Password = "secret",
            MsgHeader = "IZOMETRI",
            BaseUrl = "https://api.netgsm.test",
            UseOtpEndpoint = true
        });

        await sender.SendAsync("5551234567", "OTP test", CancellationToken.None);

        Assert.Equal("https://api.netgsm.test/sms/rest/v2/otp", handler.Request!.RequestUri!.ToString());
        var payload = handler.ReadBody();
        Assert.Equal("OTP test", payload.RootElement.GetProperty("msg").GetString());
        Assert.Equal("5551234567", payload.RootElement.GetProperty("no").GetString());
    }

    [Fact]
    public async Task SendAsync_throws_when_netgsm_returns_error_code()
    {
        var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent(new { code = "40", description = "invalidHeader" })
        });
        var sender = CreateSender(handler, new NetgsmOptions
        {
            UserCode = "8503000000",
            Password = "secret",
            MsgHeader = "INVALID",
            BaseUrl = "https://api.netgsm.test"
        });

        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => sender.SendAsync("5551234567", "test", CancellationToken.None));

        Assert.Contains("invalidHeader", exception.Message);
    }

    private static NetgsmSmsSender CreateSender(CapturingHandler handler, NetgsmOptions options)
    {
        return new NetgsmSmsSender(
            new HttpClient(handler),
            Options.Create(options),
            NullLogger<NetgsmSmsSender>.Instance);
    }

    private static StringContent JsonContent(object value)
    {
        return new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public CapturingHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public HttpRequestMessage? Request { get; private set; }
        public string? RequestBody { get; private set; }

        public JsonDocument ReadBody()
        {
            Assert.NotNull(Request);
            Assert.NotNull(RequestBody);
            return JsonDocument.Parse(RequestBody);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return _response;
        }
    }
}
