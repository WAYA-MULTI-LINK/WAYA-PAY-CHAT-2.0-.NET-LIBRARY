using WayaPay;

namespace Wayapay.Tests.Helpers;

/// <summary>
/// Builds a real WayaPayClient using credentials from environment variables.
/// Only used by [Trait("Category","Live")] integration tests.
/// Set these before running live tests:
///   export WAYAPAY_MERCHANT_ID=MER_BdVFq17797046929104WEpS
///   export WAYAPAY_SECRET_KEY=WAYASECK_PROD_0xdff7910a5b97472a950fd4a2a427470a
/// </summary>
internal static class LiveFactory
{
    internal static WayaPayClient Client()
    {
        var merchantId = Environment.GetEnvironmentVariable("WAYAPAY_MERCHANT_ID")
            ?? throw new InvalidOperationException(
                "Set WAYAPAY_MERCHANT_ID env var before running live tests.");

        var secretKey = Environment.GetEnvironmentVariable("WAYAPAY_SECRET_KEY")
            ?? throw new InvalidOperationException(
                "Set WAYAPAY_SECRET_KEY env var before running live tests.");

        // Key prefix determines environment: WAYASECK_PROD_ → production, WAYASECK_TEST_ → staging
        var env = secretKey.StartsWith("WAYASECK_PROD_", StringComparison.Ordinal)
            ? "production"
            : "staging";

        return new WayaPayClient(new WayaPayOptions
        {
            MerchantId  = merchantId,
            SecretKey   = secretKey,
            Environment = env,
        });
    }
}
