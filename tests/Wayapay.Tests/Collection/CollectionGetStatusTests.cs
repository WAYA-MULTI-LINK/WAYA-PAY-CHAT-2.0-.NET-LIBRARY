using System.Net;
using WayaPay.Models.collection;
using Wayapay.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Wayapay.Tests.Collection;

public sealed class CollectionGetStatusTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ReturnsStatus_OnSuccess()
    {
        using var client = Factory.Client(Factory.OkStub(
            """
            {
                "refNo": "1779662251460508970",
                "tranId": "aff8a062-4aa8-413b-9d2d-834b2e1dff82",
                "merchantId": "MER_TEST",
                "amount": "1500.00",
                "customerEmail": "john@example.com",
                "amountPaid": "1500.00",
                "fee": "15.00",
                "currencyCode": "NGN",
                "status": "SUCCESSFUL",
                "settlementStatus": "PENDING",
                "channel": "CARD",
                "processedBy": "ISW",
                "description": "Order #4523",
                "environment": "LIVE",
                "tranDate": "2026-06-04T10:00:32"
            }
            """));

        var result = await client.Collection.GetStatusAsync("1779662251460508970");

        Assert.Equal("1779662251460508970", result.RefNo);
        Assert.Equal("aff8a062-4aa8-413b-9d2d-834b2e1dff82", result.TranId);
        Assert.Equal("1500.00", result.AmountPaid);
        Assert.Equal("SUCCESSFUL", result.Status);
        Assert.Equal("CARD", result.Channel);
        output.WriteLine("DONE: ReturnsStatus_OnSuccess");
    }

    [Fact]
    public void ParsesStatus_ToTerminalSucceededOutcome()
    {
        var model = new CollectionStatusModel { Status = "SUCCESSFUL" };

        var status = model.ParsedStatus();

        Assert.Equal(CollectionStatus.Successful, status);
        Assert.True(status.IsTerminal());
        Assert.Equal(CollectionOutcome.Succeeded, status.Outcome());
        output.WriteLine("DONE: ParsesStatus_ToTerminalSucceededOutcome");
    }

    [Theory]
    [InlineData("PENDING", CollectionOutcome.InFlight, false)]
    [InlineData("PARTIAL", CollectionOutcome.InFlight, false)]
    [InlineData("REFUNDED", CollectionOutcome.Refunded, true)]
    [InlineData("DECLINED", CollectionOutcome.NotDebited, true)]
    [InlineData("BANK_ERROR", CollectionOutcome.Indeterminate, true)]
    [InlineData("something-new", CollectionOutcome.Indeterminate, false)]
    public void MapsStatus_ToOutcomeAndTerminality(string raw, CollectionOutcome expected, bool terminal)
    {
        var status = raw.ToCollectionStatus();

        Assert.Equal(expected, status.Outcome());
        Assert.Equal(terminal, status.IsTerminal());
        output.WriteLine($"DONE: MapsStatus_ToOutcomeAndTerminality({raw})");
    }

    [Fact]
    public async Task SendsGetRequest_ToCorrectPath_WithEncodedRefNo()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":{"refNo":"REF/01","status":"PENDING"}}""");
        using var client = Factory.Client(handler);

        await client.Collection.GetStatusAsync("REF/01");

        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.EndsWith("/payment-collect/status/REF%2F01", handler.LastRequest.RequestUri!.AbsolutePath);
        output.WriteLine("DONE: SendsGetRequest_ToCorrectPath_WithEncodedRefNo");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ThrowsArgumentException_WhenRefNoMissing(string refNo)
    {
        using var client = Factory.Client(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Collection.GetStatusAsync(refNo));

        Assert.Contains("refNo", ex.Message);
        output.WriteLine("DONE: ThrowsArgumentException_WhenRefNoMissing");
    }

    [Fact]
    public async Task ThrowsHttpRequestException_OnNotFound()
    {
        using var client = Factory.Client(
            Factory.ErrorStub("404", "No transaction with that reference", HttpStatusCode.NotFound));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.Collection.GetStatusAsync("UNKNOWN-REF"));

        Assert.Contains("No transaction with that reference", ex.Message);
        output.WriteLine("DONE: ThrowsHttpRequestException_OnNotFound");
    }
}
