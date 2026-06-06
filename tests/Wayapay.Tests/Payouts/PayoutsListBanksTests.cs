using System.Net;
using Wayapay.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Wayapay.Tests.Payouts;

public sealed class PayoutsListBanksTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ReturnsEmptyList_WhenDataIsNull()
    {
        using var client = Factory.Client(new StubHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":null}"""));

        var banks = await client.Payouts.ListBanksAsync();

        Assert.Empty(banks);
        output.WriteLine("DONE: ReturnsEmptyList_WhenDataIsNull");
    }

    [Fact]
    public async Task DeserializesBankList()
    {
        using var client = Factory.Client(Factory.OkStub(
            """[{"code":"058","name":"Guaranty Trust Bank","id":"058","status":true},{"code":"011","name":"First Bank of Nigeria","id":"011","status":true}]"""));

        var banks = await client.Payouts.ListBanksAsync();

        Assert.Equal(2, banks.Count);
        Assert.Equal("058", banks[0].Code);
        Assert.Equal("Guaranty Trust Bank", banks[0].Name);
        Assert.True(banks[0].Status);
        Assert.Equal("011", banks[1].Code);
        output.WriteLine("DONE: DeserializesBankList");
    }

    [Fact]
    public async Task SendsGetRequest_ToCorrectPath()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":[]}""");
        using var client = Factory.Client(handler);

        await client.Payouts.ListBanksAsync();

        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.EndsWith("/get-bank-list", handler.LastRequest.RequestUri!.AbsolutePath);
        output.WriteLine("DONE: SendsGetRequest_ToCorrectPath");
    }
}
