using WayaPay;
using Xunit;

namespace Wayapay.Tests.Client;

public sealed class GenerateReferenceTests
{
    [Fact]
    public void StartsWithGivenPrefix()
    {
        var reference = WayaPayClient.GenerateReference("PAYOUT");
        Assert.StartsWith("PAYOUT-", reference);
    }

    [Fact]
    public void DefaultPrefix_IsWP()
    {
        var reference = WayaPayClient.GenerateReference();
        Assert.StartsWith("WP-", reference);
    }

    [Fact]
    public void TwoConsecutiveCalls_AreUnique()
    {
        var a = WayaPayClient.GenerateReference();
        var b = WayaPayClient.GenerateReference();
        Assert.NotEqual(a, b);
    }
}
