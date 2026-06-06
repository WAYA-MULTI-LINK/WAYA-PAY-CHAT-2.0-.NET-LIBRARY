using System.Net;
using System.Text.Json;
using Wayapay.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Wayapay.Tests.Payouts;

public sealed class PayoutsInitiateTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ReturnsPayoutResult_OnSuccess()
    {
        using var client = Factory.Client(Factory.OkStub(
            """
            {
                "payoutReference": "MPO-20260604-abc123",
                "merchantReference": "REF-001",
                "status": "PROCESSING",
                "message": "Payout accepted for processing"
            }
            """));

        var result = await client.Payouts.InitiateAsync(Factory.PayoutRequest());

        Assert.Equal("MPO-20260604-abc123", result.PayoutReference);
        Assert.Equal("PROCESSING", result.Status);
        Assert.Equal("REF-001", result.MerchantReference);
        output.WriteLine("DONE: ReturnsPayoutResult_OnSuccess");
    }

    [Fact]
    public async Task SendsCorrectBody()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":{"payoutReference":"X","status":"PROCESSING"}}""");
        using var client = Factory.Client(handler);

        await client.Payouts.InitiateAsync(Factory.PayoutRequest("MY-REF-42"));

        Assert.NotNull(handler.LastBody);
        using var doc = JsonDocument.Parse(handler.LastBody!);
        Assert.Equal("MY-REF-42", doc.RootElement.GetProperty("reference").GetString());
        Assert.Equal("NGN", doc.RootElement.GetProperty("currency").GetString());
        Assert.Equal("0123456789", doc.RootElement.GetProperty("accountNumber").GetString());
        output.WriteLine("DONE: SendsCorrectBody");
    }

    [Fact]
    public async Task SendsPostRequest_ToCorrectPath()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":{"payoutReference":"X","status":"PROCESSING"}}""");
        using var client = Factory.Client(handler);

        await client.Payouts.InitiateAsync(Factory.PayoutRequest());

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/payment-payout/initiate", handler.LastRequest.RequestUri!.AbsolutePath);
        output.WriteLine("DONE: SendsPostRequest_ToCorrectPath");
    }

    [Fact]
    public async Task ThrowsArgumentNullException_WhenInputIsNull()
    {
        using var client = Factory.Client(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.Payouts.InitiateAsync(null!));
        output.WriteLine("DONE: ThrowsArgumentNullException_WhenInputIsNull");
    }
}
