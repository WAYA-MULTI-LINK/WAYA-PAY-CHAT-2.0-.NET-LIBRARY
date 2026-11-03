using Wayaquick.Tests.Helpers;
using WayaQuick.Models.Identity;
using Xunit;
using Xunit.Abstractions;

namespace Wayaquick.Tests.Live;

[Trait("Category", "Live")]
public sealed class LiveIdentityTests(ITestOutputHelper output)
{
    [Fact]
    public async Task InitiateBvn_ReturnsReference()
    {
        using var client = LiveFactory.Client();

        var result = await client.Identity.InitiateBvnAsync("22500809037");

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Reference));
        output.WriteLine("DONE: InitiateBvn_ReturnsReference");
    }

    [Fact]
    public async Task VerifyBvn_ReturnsIdentityData()
    {
        using var client = LiveFactory.Client();

#pragma warning disable CS0618
        var result = await client.Identity.VerifyBvnAsync(new BvnIdentityRequestModel
        {
            Bvn = "22500809037",
        });
#pragma warning restore CS0618

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.FirstName));
        Assert.False(string.IsNullOrWhiteSpace(result.LastName));
        Assert.Equal("22500809037", result.Bvn);
        output.WriteLine("DONE: VerifyBvn_ReturnsIdentityData");
    }
}
