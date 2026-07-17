using WayaQuick;
using Xunit;
using Xunit.Abstractions;

namespace Wayaquick.Tests.Client;

public sealed class GenerateReferenceTests(ITestOutputHelper output)
{
    [Fact]
    public void StartsWithGivenPrefix()
    {
        var reference = WayaQuickClient.GenerateReference("PAYOUT");
        Assert.StartsWith("PAYOUT-", reference);
        output.WriteLine("DONE: StartsWithGivenPrefix");
    }

    [Fact]
    public void DefaultPrefix_IsWP()
    {
        var reference = WayaQuickClient.GenerateReference();
        Assert.StartsWith("WP-", reference);
        output.WriteLine("DONE: DefaultPrefix_IsWP");
    }

    [Fact]
    public void TwoConsecutiveCalls_AreUnique()
    {
        var a = WayaQuickClient.GenerateReference();
        var b = WayaQuickClient.GenerateReference();
        Assert.NotEqual(a, b);
        output.WriteLine("DONE: TwoConsecutiveCalls_AreUnique");
    }
}
