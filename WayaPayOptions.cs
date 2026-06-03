namespace WayaPay;

/// <summary>Configuration for <see cref="WayaPayClient"/>.</summary>
public sealed class WayaPayOptions
{
    /// <summary>Your Merchant ID, format MER_...</summary>
    public required string MerchantId { get; init; }

    /// <summary>Your secret key. WAYASECK_TEST_... on staging, WAYASECK_... on live.</summary>
    public required string SecretKey { get; init; }

    /// <summary>"staging" or "production". Ignored when <see cref="BaseUrl"/> is set.</summary>
    public string Environment { get; init; } = "production";

    /// <summary>Override the base URL entirely.</summary>
    public string? BaseUrl { get; init; }

    /// <summary>Per request timeout in milliseconds.</summary>
    public int TimeoutMs { get; init; } = 30_000;

    /// <summary>Max retries. Applies to GET only.</summary>
    public int MaxRetries { get; init; } = 2;

    /// <summary>
    /// Inject your own HttpClient (DI, handler chains, tests). When supplied it is used as is
    /// and not disposed. When null, the client creates and owns one internally.
    /// </summary>
    public HttpClient? HttpClient { get; init; }
}
