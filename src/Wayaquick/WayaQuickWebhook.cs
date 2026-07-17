using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WayaQuick.Models.Webhook;

namespace WayaQuick;

/// <summary>
/// Verifies and parses WayaQuick transaction webhooks. Signature verification needs no network call,
/// so this is a standalone static helper — pass the raw request body, the signature headers, and the
/// merchant secret for the event's environment.
/// </summary>
/// <remarks>
/// <para>
/// CRITICAL: verify every webhook before acting on it. An unsigned or wrongly-signed call is hostile —
/// <see cref="ConstructEvent"/> throws <see cref="WayaQuickWebhookException"/> rather than returning a value.
/// </para>
/// <para>
/// The secret is your <c>merchantSecretTestKey</c> for a TEST transaction or your
/// <c>merchantProductionSecretKey</c> for a PRODUCTION one. Most merchants keep one verifier per
/// environment and route by which key validates.
/// </para>
/// <para>
/// Capture the EXACT raw request bytes before any JSON parsing. If your framework deserialises and
/// re-serialises the body, the recomputed HMAC will not match.
/// </para>
/// </remarks>
public static class WayaQuickWebhook
{
    /// <summary>Header carrying the epoch-millisecond timestamp that is signed alongside the body.</summary>
    public const string TimestampHeader = "X-Waya-Timestamp";

    /// <summary>Header carrying the Base64 HMAC-SHA256 signature.</summary>
    public const string SignatureHeader = "X-Waya-Signature";

    /// <summary>Default replay-protection window. Webhooks older or newer than this are rejected.</summary>
    public static readonly TimeSpan DefaultTolerance = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Verifies the signature and replay window, then parses the body into a <see cref="WebhookEvent"/>.
    /// Throws <see cref="WayaQuickWebhookException"/> if verification fails — never returns an unverified event.
    /// </summary>
    /// <param name="payload">The exact raw request body bytes, as text.</param>
    /// <param name="timestamp">Value of the <see cref="TimestampHeader"/> header (epoch milliseconds).</param>
    /// <param name="signature">Value of the <see cref="SignatureHeader"/> header (Base64 HMAC-SHA256).</param>
    /// <param name="secret">The merchant secret for this event's environment (TEST or PRODUCTION).</param>
    /// <param name="tolerance">
    /// Replay window. Defaults to <see cref="DefaultTolerance"/> (5 minutes). Pass
    /// <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to skip the timestamp check
    /// (not recommended outside tests).
    /// </param>
    public static WebhookEvent ConstructEvent(
        string payload, string? timestamp, string? signature, string secret, TimeSpan? tolerance = null)
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (string.IsNullOrEmpty(secret))
            throw new ArgumentException("Merchant secret is required.", nameof(secret));

        if (!VerifySignature(payload, timestamp, signature, secret))
            throw new WayaQuickWebhookException("Webhook signature verification failed.");

        var window = tolerance ?? DefaultTolerance;
        if (window >= TimeSpan.Zero)
        {
            if (!long.TryParse(timestamp, out var tsMs))
                throw new WayaQuickWebhookException("Webhook timestamp is not a valid epoch-millisecond value.");

            var skew = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(tsMs);
            if (skew.Duration() > window)
                throw new WayaQuickWebhookException(
                    $"Webhook timestamp is outside the {window.TotalSeconds:N0}s tolerance window (possible replay).");
        }

        WebhookEvent? evt;
        try
        {
            evt = JsonSerializer.Deserialize<WebhookEvent>(payload, Json);
        }
        catch (JsonException ex)
        {
            throw new WayaQuickWebhookException("Webhook body is not valid JSON.", ex);
        }

        return evt ?? throw new WayaQuickWebhookException("Webhook body deserialised to null.");
    }

    /// <summary>
    /// Low-level signature check: returns true when <paramref name="signature"/> equals
    /// Base64(HMAC-SHA256("{timestamp}.{payload}", secret)). Does NOT check the replay window —
    /// prefer <see cref="ConstructEvent"/>. Comparison is constant-time.
    /// </summary>
    public static bool VerifySignature(string payload, string? timestamp, string? signature, string secret)
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
            return false;

        var signedPayload = $"{timestamp}.{payload}";
        var expected = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(signedPayload));

        Span<byte> provided = stackalloc byte[expected.Length];
        if (!Convert.TryFromBase64String(signature, provided, out var written) || written != expected.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(expected, provided);
    }
}

/// <summary>Thrown when a webhook fails signature verification, replay checks, or cannot be parsed.</summary>
public sealed class WayaQuickWebhookException : Exception
{
    public WayaQuickWebhookException(string message) : base(message) { }
    public WayaQuickWebhookException(string message, Exception inner) : base(message, inner) { }
}
