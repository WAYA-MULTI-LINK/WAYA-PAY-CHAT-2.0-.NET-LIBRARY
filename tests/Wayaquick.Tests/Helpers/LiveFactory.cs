using WayaQuick;

namespace Wayaquick.Tests.Helpers;

internal static class LiveFactory
{
    internal static WayaQuickClient Client()
    {
        var merchantId = Environment.GetEnvironmentVariable("WAYAQUICK_MERCHANT_ID")
            ?? throw new InvalidOperationException(
                "Missing WAYAQUICK_MERCHANT_ID. Run: export WAYAQUICK_MERCHANT_ID=<your-id>");

        var secretKey = Environment.GetEnvironmentVariable("WAYAQUICK_SECRET_KEY")
            ?? throw new InvalidOperationException(
                "Missing WAYAQUICK_SECRET_KEY. Run: export WAYAQUICK_SECRET_KEY=<your-key>");

        // Default to staging. Set WAYAQUICK_ENV=production for a live production account.
        var env = Environment.GetEnvironmentVariable("WAYAQUICK_ENV") ?? "staging";

        return new WayaQuickClient(new WayaQuickOptions
        {
            MerchantId = merchantId,
            SecretKey = secretKey,
        });
    }
}
