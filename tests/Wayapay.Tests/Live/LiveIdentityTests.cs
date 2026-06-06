using Wayapay.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Wayapay.Tests.Live;

[Trait("Category", "Live")]
public sealed class LiveIdentityTests(ITestOutputHelper output)
{
    [Fact]
    public async Task VerifyBvn_ReturnsIdentityData()
    {
        using var client = LiveFactory.Client();

        var result = await client.Identity.VerifyBvnAsync(new()
        {
            Bvn = "22500809037",
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.FirstName));
        Assert.False(string.IsNullOrWhiteSpace(result.LastName));
        Assert.Equal("22500809037", result.Bvn);
        output.WriteLine("DONE: VerifyBvn_ReturnsIdentityData");
    }
}
