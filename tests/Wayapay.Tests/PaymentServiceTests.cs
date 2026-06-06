using System.Net;
using WayaPay;
using Xunit;

namespace Wayapay.Tests;

public class PaymentServiceTests
{
    private static WayaPayClient CreateClient(HttpMessageHandler handler) =>
        new(new WayaPayOptions
        {
            MerchantId = "MER_TEST",
            SecretKey = "WAYASECK_TEST_key",
            HttpClient = new HttpClient(handler),
        });

    [Fact]
    public async Task Banks_ListAsync_ReturnsEmptyListOnNullData()
    {
        using var client = CreateClient(new StubHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":null}"""));

        var banks = await client.Banks.ListAsync();

        Assert.Empty(banks);
    }

    [Fact]
    public async Task Banks_ListAsync_DeserializesBankList()
    {
        using var client = CreateClient(new StubHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":[{"code":"044","name":"Access Bank","id":"1","status":true}]}"""));

        var banks = await client.Banks.ListAsync();

        Assert.Single(banks);
        Assert.Equal("044", banks[0].Code);
        Assert.Equal("Access Bank", banks[0].Name);
    }

    [Fact]
    public async Task Payouts_InitiateAsync_ReturnsResult()
    {
        using var client = CreateClient(new StubHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":{"payoutReference":"REF123","status":"PROCESSING"}}"""));

        var result = await client.Payouts.InitiateAsync(new PayoutInput
        {
            Amount = 1000m,
            AccountNumber = "0123456789",
            BankCode = "044",
            AccountName = "Test User",
            Narration = "Test payout",
        });

        Assert.Equal("REF123", result.PayoutReference);
        Assert.Equal("PROCESSING", result.Status);
    }

    [Fact]
    public async Task Collect_CreateAsync_ThrowsWhenExpiryMissing()
    {
        using var client = CreateClient(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        var ex = await Assert.ThrowsAsync<WayaPayException>(() =>
            client.Collection.CreateAsync(new CollectInput
            {
                PaymentLinkName = "Test Link",
                Description = "Test",
                PayableAmount = 100m,
                RedirectLink = "https://example.com",
                LinkCanExpire = true,
            }));

        Assert.Equal(WayaPayErrorType.Validation, ex.Type);
    }

    [Fact]
    public async Task Accounts_VerifyAsync_ThrowsWhenBankCodeMissingForOthers()
    {
        using var client = CreateClient(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        var ex = await Assert.ThrowsAsync<WayaPayException>(() =>
            client.Accounts.VerifyAsync(new VerifyAccountInput
            {
                AccountNumber = "0123456789",
                EnquiryType = "OTHERS",
            }));

        Assert.Equal(WayaPayErrorType.Validation, ex.Type);
    }

    [Fact]
    public async Task Client_ThrowsApiException_OnErrorEnvelope()
    {
        using var client = CreateClient(new StubHandler(HttpStatusCode.BadRequest,
            """{"success":false,"code":"13","message":"Validation failed"}"""));

        var ex = await Assert.ThrowsAsync<WayaPayException>(() =>
            client.Banks.ListAsync());

        Assert.Equal("13", ex.ErrorCode);
        Assert.Equal(WayaPayErrorType.Api, ex.Type);
    }

    [Fact]
    public void GenerateReference_HasCorrectPrefix()
    {
        var reference = WayaPayClient.GenerateReference("PAYOUT");

        Assert.StartsWith("PAYOUT-", reference);
    }
}

file sealed class StubHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _status;
    private readonly string _body;

    public StubHandler(HttpStatusCode status, string body)
    {
        _status = status;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json"),
        };
        return Task.FromResult(response);
    }
}
