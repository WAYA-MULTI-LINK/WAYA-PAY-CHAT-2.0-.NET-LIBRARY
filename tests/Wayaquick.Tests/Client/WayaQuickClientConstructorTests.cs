using WayaQuick;
using Xunit;
using Xunit.Abstractions;

namespace Wayaquick.Tests.Client;

public sealed class WayaQuickClientConstructorTests(ITestOutputHelper output)
{
    [Fact]
    public void Throws_WhenMerchantIdIsEmpty()
    {
        var ex = Assert.Throws<ArgumentException>(() => new WayaQuickClient(new WayaQuickOptions
        {
            MerchantId = "",
            SecretKey  = "WAYASECK_TEST_key",
        }));
        Assert.Contains("MerchantId", ex.Message);
        output.WriteLine("DONE: Throws_WhenMerchantIdIsEmpty");
    }

    [Fact]
    public void Throws_WhenSecretKeyIsEmpty()
    {
        var ex = Assert.Throws<ArgumentException>(() => new WayaQuickClient(new WayaQuickOptions
        {
            MerchantId = "MER_TEST",
            SecretKey  = "",
        }));
        Assert.Contains("SecretKey", ex.Message);
        output.WriteLine("DONE: Throws_WhenSecretKeyIsEmpty");
    }

    [Fact]
    public void Constructs_WithValidOptions()
    {
        using var client = new WayaQuickClient(new WayaQuickOptions
        {
            MerchantId = "MER_TEST",
            SecretKey  = "WAYASECK_TEST_key",
        });
        Assert.NotNull(client);
        output.WriteLine("DONE: Constructs_WithValidOptions");
    }
}
