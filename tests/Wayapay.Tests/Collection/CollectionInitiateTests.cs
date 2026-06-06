using System.Net;
using System.Text.Json;
using Wayapay.Tests.Helpers;
using Xunit;

namespace Wayapay.Tests.Collection;

public sealed class CollectionInitiateTests
{
    [Fact]
    public async Task ReturnsCheckoutUrl_OnSuccess()
    {
        using var client = Factory.Client(Factory.OkStub(
            """
            {
                "uniqueId": "TXN-001",
                "transactionId": "178056425312256824",
                "email": "test@example.com",
                "amount": "5000.00",
                "checkOutUrl": "https://pay.staging.wayaquick.com/?_tranId=178056425312256824",
                "merchantId": "MER_TEST"
            }
            """));

        var result = await client.Collection.InitiateAsync(Factory.CollectionRequest());

        Assert.Equal("TXN-001", result.UniqueId);
        Assert.Equal("178056425312256824", result.TransactionId);
        Assert.Equal("https://pay.staging.wayaquick.com/?_tranId=178056425312256824", result.CheckOutUrl);
        Assert.Equal("MER_TEST", result.MerchantId);
    }

    [Fact]
    public async Task SendsCorrectBody()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":{"uniqueId":"TXN-BODY-TEST","checkOutUrl":"https://pay.example.com"}}""");
        using var client = Factory.Client(handler);

        await client.Collection.InitiateAsync(Factory.CollectionRequest("TXN-BODY-TEST"));

        Assert.NotNull(handler.LastBody);
        using var doc = JsonDocument.Parse(handler.LastBody!);
        Assert.Equal("TXN-BODY-TEST", doc.RootElement.GetProperty("transactionId").GetString());
        Assert.Equal("NGN", doc.RootElement.GetProperty("currency").GetString());
        Assert.Equal("test@example.com", doc.RootElement.GetProperty("email").GetString());
        Assert.Equal("5000.00", doc.RootElement.GetProperty("amount").GetString());
    }

    [Fact]
    public async Task SendsPostRequest_ToCorrectPath()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":{"uniqueId":"TXN-001","checkOutUrl":"https://pay.example.com"}}""");
        using var client = Factory.Client(handler);

        await client.Collection.InitiateAsync(Factory.CollectionRequest());

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/payment-collect/initiate", handler.LastRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task ThrowsArgumentNullException_WhenInputIsNull()
    {
        using var client = Factory.Client(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.Collection.InitiateAsync(null!));
    }

    [Fact]
    public async Task ThrowsHttpRequestException_OnApiError()
    {
        using var client = Factory.Client(
            Factory.ErrorStub("57", "IP 1.2.3.4 is not whitelisted", HttpStatusCode.Forbidden));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.Collection.InitiateAsync(Factory.CollectionRequest()));

        Assert.Contains("IP 1.2.3.4 is not whitelisted", ex.Message);
    }
}
