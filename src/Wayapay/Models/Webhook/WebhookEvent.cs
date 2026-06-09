namespace WayaPay.Models.Webhook;

/// <summary>
/// A transaction webhook delivered by WayaPay when a payment becomes SUCCESSFUL, PARTIAL, or FAILED.
/// Construct one only via <see cref="WayaPay.WayaPayWebhook.ConstructEvent"/>, which verifies the
/// signature first. Use <see cref="OrderId"/> as your idempotency key — the same OrderId may fire
/// more than once (e.g. a PARTIAL followed by a SUCCESSFUL).
/// </summary>
/// <remarks>
/// The wire contract mixes casing: the first fields are PascalCase (OrderId, Amount, …) while newer
/// fields are camelCase (customer, merchantId, …). Deserialization is case-insensitive, so both bind.
/// Absent fields are sent as omitted, not null — treat missing optional fields as null.
/// </remarks>
public sealed record WebhookEvent
{
    /// <summary>The transaction reference (refNo). Use this as your idempotency key.</summary>
    public string OrderId { get; init; } = "";

    /// <summary>Amount the customer was charged.</summary>
    public decimal Amount { get; init; }

    /// <summary>The description supplied at checkout.</summary>
    public string? Description { get; init; }

    /// <summary>Processing fee deducted. Net to merchant = <see cref="Amount"/> minus <see cref="Fee"/>.</summary>
    public decimal Fee { get; init; }

    /// <summary>ISO currency code. Always "NGN" today.</summary>
    public string? Currency { get; init; }

    /// <summary>Raw status: "SUCCESSFUL", "PARTIAL", or "FAILED". Parse with <see cref="WebhookEventExtensions.ToWebhookStatus"/>.</summary>
    public string Status { get; init; } = "";

    /// <summary>Transaction time on the gateway, ISO-8601 local, e.g. "2026-06-07T14:30:12".</summary>
    public string? TranTime { get; init; }

    /// <summary>Same instant, formatted "yyyy-MM-dd HH:mm:ss".</summary>
    public string? TransactionDate { get; init; }

    /// <summary>Channel: "CARD", "WALLET", "USSD", "BANK", "PAYATTITUDE".</summary>
    public string? ProductName { get; init; }

    /// <summary>Your business name as registered on WayaPay.</summary>
    public string? BusinessName { get; init; }

    /// <summary>The paying customer's details.</summary>
    public WebhookCustomer? Customer { get; init; }

    /// <summary>Your merchant ID. Same value for every webhook to your account.</summary>
    public string? MerchantId { get; init; }

    /// <summary>The branch tag if you've configured one; otherwise null.</summary>
    public string? BranchCategory { get; init; }

    /// <summary>True for charges driven by a subscription / saved card.</summary>
    public bool RecurrentPayment { get; init; }
}

/// <summary>The paying customer embedded in a <see cref="WebhookEvent"/>.</summary>
public sealed record WebhookCustomer
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? CustomerId { get; init; }
}

/// <summary>Known values of <see cref="WebhookEvent.Status"/>.</summary>
public enum WebhookStatus
{
    /// <summary>Status string not recognised by this SDK version. Don't fulfil; reconcile.</summary>
    Unknown = 0,

    /// <summary>Customer paid the full amount (or more). Funds queued for settlement — fulfil the order.</summary>
    Successful,

    /// <summary>Paid into a virtual account but less than expected. Hold fulfilment; a top-up sends a later SUCCESSFUL.</summary>
    Partial,

    /// <summary>Declined, abandoned, or upstream-rejected. Funds never moved — no fulfilment.</summary>
    Failed,
}

/// <summary>Helpers for interpreting a webhook status.</summary>
public static class WebhookEventExtensions
{
    /// <summary>Parses the raw <see cref="WebhookEvent.Status"/> string. Returns <see cref="WebhookStatus.Unknown"/> for unrecognised values.</summary>
    public static WebhookStatus ToWebhookStatus(this string? status) => status?.Trim().ToUpperInvariant() switch
    {
        "SUCCESSFUL" => WebhookStatus.Successful,
        "PARTIAL" => WebhookStatus.Partial,
        "FAILED" => WebhookStatus.Failed,
        _ => WebhookStatus.Unknown,
    };

    /// <summary>Convenience overload that parses <see cref="WebhookEvent.Status"/>.</summary>
    public static WebhookStatus ParsedStatus(this WebhookEvent evt) => evt.Status.ToWebhookStatus();

    /// <summary>True only when the customer paid in full — safe to fulfil the order (after an idempotency check on <see cref="WebhookEvent.OrderId"/>).</summary>
    public static bool ShouldFulfil(this WebhookEvent evt) => evt.ParsedStatus() == WebhookStatus.Successful;
}
