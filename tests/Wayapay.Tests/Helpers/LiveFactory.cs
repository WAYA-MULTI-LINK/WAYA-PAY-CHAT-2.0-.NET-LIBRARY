using WayaPay;

namespace Wayapay.Tests.Helpers;

internal static class LiveFactory
{
    internal static WayaPayClient Client()
    {
        var merchantId = Environment.GetEnvironmentVariable("WAYAPAY_MERCHANT_ID")
            ?? throw new InvalidOperationException(
                "Missing WAYAPAY_MERCHANT_ID. Run: export WAYAPAY_MERCHANT_ID=<your-id>");

        var secretKey = Environment.GetEnvironmentVariable("WAYAPAY_SECRET_KEY")
            ?? throw new InvalidOperationException(
                "Missing WAYAPAY_SECRET_KEY. Run: export WAYAPAY_SECRET_KEY=<your-key>");

        // Default to staging. Set WAYAPAY_ENV=production for a live production account.
        var env = Environment.GetEnvironmentVariable("WAYAPAY_ENV") ?? "staging";

        return new WayaPayClient(new WayaPayOptions
        {
            MerchantId  = merchantId,
            SecretKey   = secretKey,
        
        });
    }
}
