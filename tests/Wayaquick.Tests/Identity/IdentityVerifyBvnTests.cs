using System.Net;
using System.Text.Json;
using Wayaquick.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Wayaquick.Tests.Identity;

public sealed class IdentityVerifyBvnTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ThrowsArgumentException_WhenBvnTooShort()
    {
        using var client = Factory.Client(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Identity.VerifyBvnAsync(Factory.BvnRequest("1234567890"))); // 10 digits
        output.WriteLine("DONE: ThrowsArgumentException_WhenBvnTooShort");
    }

    [Fact]
    public async Task ThrowsArgumentException_WhenBvnTooLong()
    {
        using var client = Factory.Client(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Identity.VerifyBvnAsync(Factory.BvnRequest("123456789012"))); // 12 digits
        output.WriteLine("DONE: ThrowsArgumentException_WhenBvnTooLong");
    }

    [Fact]
    public async Task ThrowsArgumentException_WhenBvnContainsLetters()
    {
        using var client = Factory.Client(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Identity.VerifyBvnAsync(Factory.BvnRequest("2250080903A")));
        output.WriteLine("DONE: ThrowsArgumentException_WhenBvnContainsLetters");
    }

    [Fact]
    public async Task ReturnsResult_OnSuccess()
    {
        using var client = Factory.Client(Factory.OkStub(
            """
            {
                "bvn": "22500809037",
                "firstName": "AUGUSTINE",
                "middleName": "CHUKWUEMEKA",
                "lastName": "ASOGWA",
                "dateOfBirth": "15-Aug-2001",
                "gender": "Male",
                "nationality": "Nigeria",
                "watchListed": "False"
            }
            """));

        var result = await client.Identity.VerifyBvnAsync(Factory.BvnRequest());

        Assert.Equal("22500809037", result.Bvn);
        Assert.Equal("AUGUSTINE", result.FirstName);
        Assert.Equal("ASOGWA", result.LastName);
        Assert.Equal("False", result.WatchListed);
        output.WriteLine("DONE: ReturnsResult_OnSuccess");
    }

    [Fact]
    public async Task SendsCorrectBody()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":{"bvn":"22500809037","firstName":"TEST","watchListed":"False"}}""");
        using var client = Factory.Client(handler);

        await client.Identity.VerifyBvnAsync(Factory.BvnRequest("22500809037"));

        Assert.NotNull(handler.LastBody);
        using var doc = JsonDocument.Parse(handler.LastBody!);
        Assert.Equal("22500809037", doc.RootElement.GetProperty("bvn").GetString());
        output.WriteLine("DONE: SendsCorrectBody");
    }

    [Fact]
    public async Task SendsPostRequest_ToCorrectPath()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":{"bvn":"22500809037","firstName":"TEST","watchListed":"False"}}""");
        using var client = Factory.Client(handler);

        await client.Identity.VerifyBvnAsync(Factory.BvnRequest());

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/identity-verification/bvn", handler.LastRequest.RequestUri!.AbsolutePath);
        output.WriteLine("DONE: SendsPostRequest_ToCorrectPath");
    }
}
