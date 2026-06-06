using WayaPay;
using Xunit;

namespace Wayapay.Tests.Client;

public sealed class WayaPayClientConstructorTests
{
    [Fact]
    public void Throws_WhenMerchantIdIsEmpty()
    {
        var ex = Assert.Throws<ArgumentException>(() => new WayaPayClient(new WayaPayOptions
        {
            MerchantId = "",
            SecretKey  = "WAYASECK_TEST_key",
        }));
        Assert.Contains("MerchantId", ex.Message);
    }

    [Fact]
    public void Throws_WhenSecretKeyIsEmpty()
    {
        var ex = Assert.Throws<ArgumentException>(() => new WayaPayClient(new WayaPayOptions
        {
            MerchantId = "MER_TEST",
            SecretKey  = "",
        }));
        Assert.Contains("SecretKey", ex.Message);
    }

    [Fact]
    public void UsesProductionUrl_ByDefault()
    {
        using var client = new WayaPayClient(new WayaPayOptions
        {
            MerchantId = "MER_TEST",
            SecretKey  = "WAYASECK_TEST_key",
        });
        Assert.Contains("services.wayapay.ng", client.BaseUrl);
        Assert.DoesNotContain("staging", client.BaseUrl);
    }

    [Fact]
    public void UsesStagingUrl_WhenEnvironmentIsStaging()
    {
        using var client = new WayaPayClient(new WayaPayOptions
        {
            MerchantId  = "MER_TEST",
            SecretKey   = "WAYASECK_TEST_key",
        });
        Assert.Contains("staging", client.BaseUrl);
    }

    [Fact]
    public void UsesCustomBaseUrl_WhenProvided()
    {
        using var client = new WayaPayClient(new WayaPayOptions
        {
            MerchantId = "MER_TEST",
            SecretKey  = "WAYASECK_TEST_key",
            BaseUrl    = "https://custom.example.com/api",
        });
        Assert.Equal("https://custom.example.com/api", client.BaseUrl);
    }
}
