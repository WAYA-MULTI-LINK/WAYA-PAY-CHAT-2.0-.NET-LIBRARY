using System.Net;
using WayaPay.Models.Payout;
using Wayapay.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Wayapay.Tests.Payouts;

public sealed class PayoutsGetStatusTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ReturnsStatus_OnSuccess()
    {
        using var client = Factory.Client(Factory.OkStub(
            """
            {
                "transactionReference": "PAYOUT-20260604-001",
                "status": "SUCCESS",
                "amount": "500.00",
                "destinationAccountNumber": "0123456789",
                "destinationAccountName": "JOHN DOE",
                "destinationBankName": "GTBank",
                "narration": "TRF FM ... REF:PAYOUT-20260604-001",
                "createdAt": "2026-06-04T10:00:32"
            }
            """));

        var result = await client.Payouts.GetStatusAsync("PAYOUT-20260604-001");

        Assert.Equal("PAYOUT-20260604-001", result.TransactionReference);
        Assert.Equal("SUCCESS", result.Status);
        Assert.Equal("500.00", result.Amount);
        Assert.Equal("JOHN DOE", result.DestinationAccountName);
        Assert.Equal("GTBank", result.DestinationBankName);
        output.WriteLine("DONE: ReturnsStatus_OnSuccess");
    }

    [Fact]
    public void ParsesStatus_ToTerminalSucceededOutcome()
    {
        var model = new PayoutStatusModel { Status = "SUCCESS" };

        var status = model.ParsedStatus();

        Assert.Equal(PayoutStatus.Success, status);
        Assert.True(status.IsTerminal());
        Assert.Equal(PayoutOutcome.Succeeded, status.Outcome());
        output.WriteLine("DONE: ParsesStatus_ToTerminalSucceededOutcome");
    }

    [Theory]
    [InlineData("PENDING", PayoutOutcome.Reconciling, false)]
    [InlineData("SUCCESS", PayoutOutcome.Succeeded, true)]
    [InlineData("REVERSED", PayoutOutcome.Reversed, true)]
    [InlineData("something-new", PayoutOutcome.Reconciling, false)]
    public void MapsStatus_ToOutcomeAndTerminality(string raw, PayoutOutcome expected, bool terminal)
    {
        var status = raw.ToPayoutStatus();

        Assert.Equal(expected, status.Outcome());
        Assert.Equal(terminal, status.IsTerminal());
        output.WriteLine($"DONE: MapsStatus_ToOutcomeAndTerminality({raw})");
    }

    [Fact]
    public async Task SendsGetRequest_ToCorrectPath_WithEncodedReference()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":{"transactionReference":"PAYOUT 01","status":"PENDING"}}""");
        using var client = Factory.Client(handler);

        await client.Payouts.GetStatusAsync("PAYOUT 01");

        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.EndsWith("/payment-payout/status/PAYOUT%2001", handler.LastRequest.RequestUri!.AbsolutePath);
        output.WriteLine("DONE: SendsGetRequest_ToCorrectPath_WithEncodedReference");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ThrowsArgumentException_WhenReferenceMissing(string reference)
    {
        using var client = Factory.Client(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Payouts.GetStatusAsync(reference));

        Assert.Contains("reference", ex.Message);
        output.WriteLine("DONE: ThrowsArgumentException_WhenReferenceMissing");
    }

    [Fact]
    public async Task ThrowsHttpRequestException_OnNotFound()
    {
        using var client = Factory.Client(
            Factory.ErrorStub("404", "No payout with that reference", HttpStatusCode.NotFound));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.Payouts.GetStatusAsync("UNKNOWN-REF"));

        Assert.Contains("No payout with that reference", ex.Message);
        output.WriteLine("DONE: ThrowsHttpRequestException_OnNotFound");
    }
}
