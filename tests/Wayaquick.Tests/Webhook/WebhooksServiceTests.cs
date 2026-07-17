using System.Security.Cryptography;
using System.Text;
using WayaQuick;
using WayaQuick.Models.Webhook;
using Xunit;
using Xunit.Abstractions;

namespace Wayaquick.Tests.Webhook;

public sealed class WebhooksServiceTests(ITestOutputHelper output)
{
    private const string Secret = "WAYASECK_TEST_webhook_secret";

    private const string Body =
        """{"OrderId":"1779662251460508970","Amount":1500.00,"Status":"SUCCESSFUL","productName":"CARD","merchantId":"MER_xyz","recurrentPayment":false}""";

    private static string Sign(string timestamp, string body, string secret = Secret) =>
        Convert.ToBase64String(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{body}")));

    private static string NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

    private static WayaQuickClient ClientWith(string? webhookSecret) =>
        new(new WayaQuickOptions
        {
            MerchantId    = "MER_TEST",
            SecretKey     = "WAYASECK_TEST_key",
            WebhookSecret = webhookSecret,
        });

    [Fact]
    public void ConstructEvent_UsesConfiguredSecret()
    {
        using var client = ClientWith(Secret);
        var ts = NowMs();

        var evt = client.Webhooks.ConstructEvent(Body, ts, Sign(ts, Body));

        Assert.Equal("1779662251460508970", evt.OrderId);
        Assert.True(evt.ShouldFulfil());
        output.WriteLine("DONE: ConstructEvent_UsesConfiguredSecret");
    }

    [Fact]
    public void ConstructEvent_Throws_OnWrongConfiguredSecret()
    {
        using var client = ClientWith("a-different-secret");
        var ts = NowMs();

        Assert.Throws<WayaQuickWebhookException>(() =>
            client.Webhooks.ConstructEvent(Body, ts, Sign(ts, Body)));
        output.WriteLine("DONE: ConstructEvent_Throws_OnWrongConfiguredSecret");
    }

    [Fact]
    public void ConstructEvent_Throws_WhenNoSecretConfigured()
    {
        using var client = ClientWith(null);
        var ts = NowMs();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            client.Webhooks.ConstructEvent(Body, ts, Sign(ts, Body)));

        Assert.Contains("WebhookSecret", ex.Message);
        output.WriteLine("DONE: ConstructEvent_Throws_WhenNoSecretConfigured");
    }

    [Fact]
    public void ConstructEvent_ExplicitSecret_OverridesConfigured()
    {
        // No configured secret, but the explicit-secret overload still works (e.g. TEST vs PRODUCTION routing).
        using var client = ClientWith(null);
        var ts = NowMs();

        var evt = client.Webhooks.ConstructEvent(Body, ts, Sign(ts, Body), Secret);

        Assert.Equal("1779662251460508970", evt.OrderId);
        output.WriteLine("DONE: ConstructEvent_ExplicitSecret_OverridesConfigured");
    }

    [Fact]
    public void VerifySignature_UsesConfiguredSecret()
    {
        using var client = ClientWith(Secret);
        var ts = NowMs();

        Assert.True(client.Webhooks.VerifySignature(Body, ts, Sign(ts, Body)));
        Assert.False(client.Webhooks.VerifySignature(Body, ts, Sign(ts, Body, "wrong")));
        output.WriteLine("DONE: VerifySignature_UsesConfiguredSecret");
    }
}
