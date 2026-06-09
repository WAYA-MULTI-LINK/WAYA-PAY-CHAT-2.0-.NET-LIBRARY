using System.Security.Cryptography;
using System.Text;
using WayaPay;
using WayaPay.Models.Webhook;
using Xunit;
using Xunit.Abstractions;

namespace Wayapay.Tests.Webhook;

public sealed class WebhookTests(ITestOutputHelper output)
{
    private const string Secret = "WAYASECK_TEST_webhook_secret";

    private const string Body =
        """{"OrderId":"1779662251460508970","Amount":1500.00,"Description":"Order #4523","Fee":15.00,"Currency":"NGN","Status":"SUCCESSFUL","TranTime":"2026-06-07T14:30:12","TransactionDate":"2026-06-07 14:30:12","productName":"CARD","businessName":"Your Shop Ltd","customer":{"name":"John Doe","email":"john@example.com","phoneNumber":"08012345678","customerId":"CUS_abc"},"merchantId":"MER_xyz","recurrentPayment":false}""";

    private static string Sign(string timestamp, string body, string secret = Secret)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{body}"));
        return Convert.ToBase64String(hash);
    }

    private static string NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

    [Fact]
    public void ConstructEvent_ParsesMixedCasing_OnValidSignature()
    {
        var ts = NowMs();
        var sig = Sign(ts, Body);

        var evt = WayaPayWebhook.ConstructEvent(Body, ts, sig, Secret);

        // PascalCase wire fields
        Assert.Equal("1779662251460508970", evt.OrderId);
        Assert.Equal(1500.00m, evt.Amount);
        Assert.Equal(15.00m, evt.Fee);
        Assert.Equal("SUCCESSFUL", evt.Status);
        // camelCase wire fields
        Assert.Equal("CARD", evt.ProductName);
        Assert.Equal("MER_xyz", evt.MerchantId);
        Assert.False(evt.RecurrentPayment);
        // nested customer
        Assert.NotNull(evt.Customer);
        Assert.Equal("john@example.com", evt.Customer!.Email);
        Assert.Equal("CUS_abc", evt.Customer.CustomerId);
        // omitted optional field -> null
        Assert.Null(evt.BranchCategory);

        Assert.Equal(WebhookStatus.Successful, evt.ParsedStatus());
        Assert.True(evt.ShouldFulfil());
        output.WriteLine("DONE: ConstructEvent_ParsesMixedCasing_OnValidSignature");
    }

    [Fact]
    public void ConstructEvent_Throws_OnWrongSignature()
    {
        var ts = NowMs();
        var sig = Sign(ts, Body, "the-wrong-secret");

        var ex = Assert.Throws<WayaPayWebhookException>(() =>
            WayaPayWebhook.ConstructEvent(Body, ts, sig, Secret));

        Assert.Contains("signature", ex.Message, StringComparison.OrdinalIgnoreCase);
        output.WriteLine("DONE: ConstructEvent_Throws_OnWrongSignature");
    }

    [Fact]
    public void ConstructEvent_Throws_WhenBodyTamperedAfterSigning()
    {
        var ts = NowMs();
        var sig = Sign(ts, Body);
        var tampered = Body.Replace("1500.00", "9999.00");

        Assert.Throws<WayaPayWebhookException>(() =>
            WayaPayWebhook.ConstructEvent(tampered, ts, sig, Secret));
        output.WriteLine("DONE: ConstructEvent_Throws_WhenBodyTamperedAfterSigning");
    }

    [Fact]
    public void ConstructEvent_Throws_OnStaleTimestamp_Replay()
    {
        var staleTs = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 10 * 60 * 1000).ToString();
        var sig = Sign(staleTs, Body); // correctly signed, but old

        var ex = Assert.Throws<WayaPayWebhookException>(() =>
            WayaPayWebhook.ConstructEvent(Body, staleTs, sig, Secret));

        Assert.Contains("tolerance", ex.Message, StringComparison.OrdinalIgnoreCase);
        output.WriteLine("DONE: ConstructEvent_Throws_OnStaleTimestamp_Replay");
    }

    [Fact]
    public void ConstructEvent_AcceptsStaleTimestamp_WhenReplayCheckDisabled()
    {
        var staleTs = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 10 * 60 * 1000).ToString();
        var sig = Sign(staleTs, Body);

        var evt = WayaPayWebhook.ConstructEvent(Body, staleTs, sig, Secret, Timeout.InfiniteTimeSpan);

        Assert.Equal("1779662251460508970", evt.OrderId);
        output.WriteLine("DONE: ConstructEvent_AcceptsStaleTimestamp_WhenReplayCheckDisabled");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-base64-!!!")]
    public void VerifySignature_ReturnsFalse_OnMissingOrMalformedSignature(string? signature)
    {
        Assert.False(WayaPayWebhook.VerifySignature(Body, NowMs(), signature, Secret));
        output.WriteLine("DONE: VerifySignature_ReturnsFalse_OnMissingOrMalformedSignature");
    }

    [Fact]
    public void VerifySignature_ReturnsTrue_OnMatch()
    {
        var ts = NowMs();
        Assert.True(WayaPayWebhook.VerifySignature(Body, ts, Sign(ts, Body), Secret));
        output.WriteLine("DONE: VerifySignature_ReturnsTrue_OnMatch");
    }

    [Theory]
    [InlineData("SUCCESSFUL", WebhookStatus.Successful)]
    [InlineData("PARTIAL", WebhookStatus.Partial)]
    [InlineData("FAILED", WebhookStatus.Failed)]
    [InlineData("WHATEVER", WebhookStatus.Unknown)]
    public void ToWebhookStatus_MapsKnownValues(string raw, WebhookStatus expected)
    {
        Assert.Equal(expected, raw.ToWebhookStatus());
        output.WriteLine($"DONE: ToWebhookStatus_MapsKnownValues({raw})");
    }

    [Fact]
    public void ConstructEvent_Throws_OnInvalidJson_WithValidSignature()
    {
        const string notJson = "this is not json";
        var ts = NowMs();
        var sig = Sign(ts, notJson);

        Assert.Throws<WayaPayWebhookException>(() =>
            WayaPayWebhook.ConstructEvent(notJson, ts, sig, Secret));
        output.WriteLine("DONE: ConstructEvent_Throws_OnInvalidJson_WithValidSignature");
    }
}
