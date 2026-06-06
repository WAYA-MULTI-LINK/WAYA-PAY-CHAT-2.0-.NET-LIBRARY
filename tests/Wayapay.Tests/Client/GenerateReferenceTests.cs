using WayaPay;
using Xunit;
using Xunit.Abstractions;

namespace Wayapay.Tests.Client;

public sealed class GenerateReferenceTests(ITestOutputHelper output)
{
    [Fact]
    public void StartsWithGivenPrefix()
    {
        var reference = WayaPayClient.GenerateReference("PAYOUT");
        Assert.StartsWith("PAYOUT-", reference);
        output.WriteLine("DONE: StartsWithGivenPrefix");
    }

    [Fact]
    public void DefaultPrefix_IsWP()
    {
        var reference = WayaPayClient.GenerateReference();
        Assert.StartsWith("WP-", reference);
        output.WriteLine("DONE: DefaultPrefix_IsWP");
    }

    [Fact]
    public void TwoConsecutiveCalls_AreUnique()
    {
        var a = WayaPayClient.GenerateReference();
        var b = WayaPayClient.GenerateReference();
        Assert.NotEqual(a, b);
        output.WriteLine("DONE: TwoConsecutiveCalls_AreUnique");
    }
}
