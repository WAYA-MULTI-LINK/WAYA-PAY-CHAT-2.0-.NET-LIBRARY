using System.Net;
using Wayaquick.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Wayaquick.Tests.Client;

public sealed class ClientHttpBehaviorTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ThrowsHttpRequestException_OnErrorEnvelope()
    {
        using var client = Factory.Client(
            Factory.ErrorStub("57", "IP not whitelisted", HttpStatusCode.Forbidden));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.Payouts.ListBanksAsync());

        Assert.Contains("IP not whitelisted", ex.Message);
        output.WriteLine("DONE: ThrowsHttpRequestException_OnErrorEnvelope");
    }

    [Fact]
    public async Task ThrowsHttpRequestException_OnUnauthorized()
    {
        using var client = Factory.Client(
            Factory.ErrorStub("01", "Missing or malformed Authorization header", HttpStatusCode.Unauthorized));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.Payouts.ListBanksAsync());
        output.WriteLine("DONE: ThrowsHttpRequestException_OnUnauthorized");
    }

    [Fact]
    public async Task ThrowsInvalidOperationException_OnNonJsonResponse()
    {
        using var client = Factory.Client(
            new StubHandler(HttpStatusCode.InternalServerError, "<html>Internal Server Error</html>"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.Payouts.ListBanksAsync());
        output.WriteLine("DONE: ThrowsInvalidOperationException_OnNonJsonResponse");
    }

    [Fact]
    public async Task SendsXMerchantIdHeader()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":[]}""");
        using var client = Factory.Client(handler);

        await client.Payouts.ListBanksAsync();

        Assert.True(handler.LastRequest!.Headers.TryGetValues("X-Merchant-Id", out var values));
        Assert.Equal("MER_TEST", values!.First());
        output.WriteLine("DONE: SendsXMerchantIdHeader");
    }

    [Fact]
    public async Task SendsBearerAuthorizationHeader()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":[]}""");
        using var client = Factory.Client(handler);

        await client.Payouts.ListBanksAsync();

        Assert.Equal("Bearer", handler.LastRequest!.Headers.Authorization!.Scheme);
        Assert.Equal("WAYASECK_TEST_key", handler.LastRequest.Headers.Authorization.Parameter);
        output.WriteLine("DONE: SendsBearerAuthorizationHeader");
    }
}
