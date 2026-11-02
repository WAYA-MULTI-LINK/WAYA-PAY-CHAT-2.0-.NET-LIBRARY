using System.Net;
using System.Text.Json;
using Wayaquick.Tests.Helpers;
using WayaQuick.Models.Identity;
using Xunit;
using Xunit.Abstractions;

namespace Wayaquick.Tests.Identity;

public sealed class IdentityVerifyBvnTests(ITestOutputHelper output)
{
    [Fact]
    public async Task Initiate_ThrowsArgumentException_WhenBvnInvalid()
    {
        using var client = Factory.Client(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        await Assert.ThrowsAsync<ArgumentException>(() => client.Identity.InitiateBvnAsync("1234567890"));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Identity.InitiateBvnAsync("123456789012"));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Identity.InitiateBvnAsync("2250080903A"));
        output.WriteLine("DONE: Initiate_ThrowsArgumentException_WhenBvnInvalid");
    }

    [Fact]
    public async Task Initiate_SendsPostToCorrectPathAndReturnsResult()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """
            {
                "success": true,
                "code": "00",
                "message": "OTP sent",
                "data": {
                    "reference": "9f1c4e2a-3b77-4d58-9a21-6c0f8e5b7d44",
                    "phoneNumber": "234****0000",
                    "expiresAt": "2026-10-13T14:32:10.482"
                }
            }
            """);
        using var client = Factory.Client(handler);

        var result = await client.Identity.InitiateBvnAsync("22222222222");

        Assert.Equal("9f1c4e2a-3b77-4d58-9a21-6c0f8e5b7d44", result.Reference);
        Assert.Equal("234****0000", result.PhoneNumber);
        Assert.Equal("2026-10-13T14:32:10.482", result.ExpiresAt);

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/identity-verification/bvn/initiate", handler.LastRequest.RequestUri!.AbsolutePath);

        using var doc = JsonDocument.Parse(handler.LastBody!);
        Assert.Equal("22222222222", doc.RootElement.GetProperty("bvn").GetString());
        output.WriteLine("DONE: Initiate_SendsPostToCorrectPathAndReturnsResult");
    }

    [Fact]
    public async Task Complete_ThrowsArgumentException_WhenFieldsMissing()
    {
        using var client = Factory.Client(new StubHandler(HttpStatusCode.OK, """{"success":true}"""));

        await Assert.ThrowsAsync<ArgumentException>(() => client.Identity.CompleteBvnAsync("", "123456"));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Identity.CompleteBvnAsync("ref", ""));
        output.WriteLine("DONE: Complete_ThrowsArgumentException_WhenFieldsMissing");
    }

    [Fact]
    public async Task Complete_SendsPostToCorrectPathAndReturnsKycRecord()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """
            {
                "success": true,
                "code": "00",
                "message": "BVN verification completed",
                "data": {
                    "bvn": "22222222222",
                    "firstName": "Test",
                    "lastName": "User",
                    "dateOfBirth": "01-Jan-1990",
                    "phoneNumber1": "2340000000000",
                    "gender": "Male",
                    "nationality": "Nigeria",
                    "watchListed": "SANDBOX"
                }
            }
            """);
        using var client = Factory.Client(handler);

        var result = await client.Identity.CompleteBvnAsync(new BvnCompleteRequestModel
        {
            Reference = "9f1c4e2a-3b77-4d58-9a21-6c0f8e5b7d44",
            Otp = "123456"
        });

        Assert.Equal("22222222222", result.Bvn);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.Equal("SANDBOX", result.WatchListed);

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/identity-verification/bvn/complete", handler.LastRequest.RequestUri!.AbsolutePath);

        using var doc = JsonDocument.Parse(handler.LastBody!);
        Assert.Equal("9f1c4e2a-3b77-4d58-9a21-6c0f8e5b7d44", doc.RootElement.GetProperty("reference").GetString());
        Assert.Equal("123456", doc.RootElement.GetProperty("otp").GetString());
        output.WriteLine("DONE: Complete_SendsPostToCorrectPathAndReturnsKycRecord");
    }

    [Fact]
    public async Task Legacy_VerifyBvn_StillWorks()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK,
            """{"success":true,"code":"00","data":{"bvn":"22500809037","firstName":"AUGUSTINE","watchListed":"False"}}""");
        using var client = Factory.Client(handler);

#pragma warning disable CS0618 // Type or member is obsolete
        var result = await client.Identity.VerifyBvnAsync("22500809037");
#pragma warning restore CS0618

        Assert.Equal("22500809037", result.Bvn);
        Assert.Equal("AUGUSTINE", result.FirstName);
        Assert.EndsWith("/identity-verification/bvn", handler.LastRequest!.RequestUri!.AbsolutePath);
    }
}
