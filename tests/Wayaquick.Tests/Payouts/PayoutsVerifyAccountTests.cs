using System.Net;
using Wayaquick.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Wayaquick.Tests.Payouts;

public sealed class PayoutsVerifyAccountTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ThrowsArgumentException_WhenBankCodeMissingForOthers()
    {
        using var client = Factory.Client(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Payouts.VerifyAccountAsync(Factory.VerifyRequest(enquiryType: "OTHERS", bankCode: null)));

        Assert.Contains("BankCode", ex.Message);
        output.WriteLine("DONE: ThrowsArgumentException_WhenBankCodeMissingForOthers");
    }

    [Fact]
    public async Task DoesNotThrow_WhenWayaBankWithNoBankCode()
    {
        using var client = Factory.Client(Factory.OkStub(
            """{"accountNumber":"0123456789","accountName":"JOHN DOE","successful":true}"""));

        var result = await client.Payouts.VerifyAccountAsync(
            Factory.VerifyRequest(enquiryType: "WAYA-BANK", bankCode: null));

        Assert.Equal("JOHN DOE", result.AccountName);
        output.WriteLine("DONE: DoesNotThrow_WhenWayaBankWithNoBankCode");
    }

    [Fact]
    public async Task ReturnsVerifiedAccount_OnSuccess()
    {
        using var client = Factory.Client(Factory.OkStub(
            """
            {
                "successful": true,
                "responseCode": "00",
                "responseMessage": "Approved",
                "accountNumber": "0123456789",
                "accountName": "JOHN DOE",
                "bankCode": "044",
                "bankName": "Access Bank",
                "enquiryType": "OTHERS"
            }
            """));

        var result = await client.Payouts.VerifyAccountAsync(Factory.VerifyRequest());

        Assert.True(result.Successful);
        Assert.Equal("JOHN DOE", result.AccountName);
        Assert.Equal("Access Bank", result.BankName);
        Assert.Equal("00", result.ResponseCode);
        output.WriteLine("DONE: ReturnsVerifiedAccount_OnSuccess");
    }

    [Fact]
    public async Task SendsPostRequest_ToCorrectPath()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":{"accountNumber":"0123456789","accountName":"JOHN DOE","successful":true}}""");
        using var client = Factory.Client(handler);

        await client.Payouts.VerifyAccountAsync(Factory.VerifyRequest());

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/verify-account", handler.LastRequest.RequestUri!.AbsolutePath);
        output.WriteLine("DONE: SendsPostRequest_ToCorrectPath");
    }
}
