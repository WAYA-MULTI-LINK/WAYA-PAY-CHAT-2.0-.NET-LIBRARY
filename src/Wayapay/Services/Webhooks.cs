using WayaPay.Models.Webhook;

namespace WayaPay.Services;

/// <summary>
/// Verifies and parses incoming transaction webhooks. A thin, discoverable wrapper over the static
/// <see cref="WayaPayWebhook"/> — the overloads without a <c>secret</c> parameter use
/// <see cref="WayaPayOptions.WebhookSecret"/>; the ones with it let you route per environment.
/// </summary>
public sealed class Webhooks
{
    private readonly string? _secret;

    internal Webhooks(string? webhookSecret) => _secret = webhookSecret;

    /// <summary>
    /// Verifies the signature and replay window using the configured <see cref="WayaPayOptions.WebhookSecret"/>,
    /// then parses the body. Throws <see cref="WayaPayWebhookException"/> if verification fails.
    /// </summary>
    public WebhookEvent ConstructEvent(string payload, string? timestamp, string? signature, TimeSpan? tolerance = null) =>
        WayaPayWebhook.ConstructEvent(payload, timestamp, signature, RequireSecret(), tolerance);

    /// <summary>Same as <see cref="ConstructEvent(string,string,string,TimeSpan?)"/> but with an explicit secret (e.g. to route TEST vs PRODUCTION).</summary>
    public WebhookEvent ConstructEvent(string payload, string? timestamp, string? signature, string secret, TimeSpan? tolerance = null) =>
        WayaPayWebhook.ConstructEvent(payload, timestamp, signature, secret, tolerance);

    /// <summary>Signature-only check (no replay window) using the configured <see cref="WayaPayOptions.WebhookSecret"/>.</summary>
    public bool VerifySignature(string payload, string? timestamp, string? signature) =>
        WayaPayWebhook.VerifySignature(payload, timestamp, signature, RequireSecret());

    /// <summary>Signature-only check (no replay window) with an explicit secret.</summary>
    public bool VerifySignature(string payload, string? timestamp, string? signature, string secret) =>
        WayaPayWebhook.VerifySignature(payload, timestamp, signature, secret);

    private string RequireSecret() => _secret ?? throw new InvalidOperationException(
        "No webhook secret configured. Set WayaPayOptions.WebhookSecret, or call the overload that takes an explicit secret.");
}
