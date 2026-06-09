namespace WayaPay.Models.collection;

/// <summary>
/// Status of a single collection (deposit) transaction.
/// Use <see cref="RefNo"/> as the idempotency key when fulfilling a SUCCESSFUL payment.
/// Inspect <see cref="Status"/> together with <see cref="CollectionStatusExtensions"/> to decide
/// whether to keep polling, fulfil, or reconcile.
/// </summary>
public sealed record CollectionStatusModel
{
    /// <summary>Provider reference number. Stable idempotency key for fulfilment, e.g. "1779662251460508970".</summary>
    public string RefNo { get; init; } = "";

    /// <summary>WayaQuick's internal transaction ID (GUID).</summary>
    public string TranId { get; init; } = "";

    /// <summary>Merchant's unique identifier.</summary>
    public string? MerchantId { get; init; }

    /// <summary>Amount requested, quoted string, e.g. "1500.00".</summary>
    public string? Amount { get; init; }

    /// <summary>Customer email address.</summary>
    public string? CustomerEmail { get; init; }

    /// <summary>Amount actually paid, quoted string, e.g. "1500.00". May be less than <see cref="Amount"/> when PARTIAL.</summary>
    public string? AmountPaid { get; init; }

    /// <summary>Processing fee, quoted string, e.g. "15.00".</summary>
    public string? Fee { get; init; }

    /// <summary>ISO currency code, e.g. "NGN".</summary>
    public string? CurrencyCode { get; init; }

    /// <summary>Raw transaction status, e.g. "SUCCESSFUL". Parse with <see cref="CollectionStatusExtensions.ToCollectionStatus"/>.</summary>
    public string Status { get; init; } = "";

    /// <summary>Settlement status, e.g. "PENDING".</summary>
    public string? SettlementStatus { get; init; }

    /// <summary>Payment channel, e.g. "CARD".</summary>
    public string? Channel { get; init; }

    /// <summary>Processor that handled the transaction, e.g. "ISW".</summary>
    public string? ProcessedBy { get; init; }

    /// <summary>Merchant-supplied description, e.g. "Order #4523".</summary>
    public string? Description { get; init; }

    /// <summary>Environment the transaction ran in, e.g. "LIVE" or "TEST".</summary>
    public string? Environment { get; init; }

    /// <summary>Transaction timestamp, e.g. "2026-06-04T10:00:32".</summary>
    public string? TranDate { get; init; }
}

/// <summary>Known values of <see cref="CollectionStatusModel.Status"/>.</summary>
public enum CollectionStatus
{
    /// <summary>Status string not recognised by this SDK version. Treat as non-terminal and reconcile.</summary>
    Unknown = 0,

    // ----- In flight (non-terminal): keep polling; don't refund or retry -----
    Initiated,
    Pending,
    Processing,
    Approved,

    /// <summary>Customer underpaid into a virtual account. Non-terminal.</summary>
    Partial,

    // ----- Terminal: funds confirmed -----
    /// <summary>Funds confirmed — fulfil. Use RefNo for idempotency.</summary>
    Successful,

    /// <summary>Previously-successful transaction refunded.</summary>
    Refunded,

    // ----- Terminal: customer not debited — no fulfilment -----
    Failed,
    Declined,
    Rejected,
    Abandoned,
    Expired,
    Cancelled,
    CustomerError,
    FraudError,

    // ----- Terminal: outcome unknown — reconcile, don't refund unilaterally -----
    Timeout,
    Error,
    SystemError,
    BankError,
}

/// <summary>How a <see cref="CollectionStatus"/> should be acted on.</summary>
public enum CollectionOutcome
{
    /// <summary>In flight — keep polling; don't refund or retry.</summary>
    InFlight,

    /// <summary>Funds confirmed — fulfil the order.</summary>
    Succeeded,

    /// <summary>Previously-successful transaction was refunded.</summary>
    Refunded,

    /// <summary>Customer not debited — do not fulfil.</summary>
    NotDebited,

    /// <summary>Outcome unknown — reconcile; don't refund unilaterally.</summary>
    Indeterminate,
}

/// <summary>Helpers for interpreting a collection status.</summary>
public static class CollectionStatusExtensions
{
    /// <summary>Parses the raw <see cref="CollectionStatusModel.Status"/> string. Returns <see cref="CollectionStatus.Unknown"/> for unrecognised values.</summary>
    public static CollectionStatus ToCollectionStatus(this string? status) => status?.Trim().ToUpperInvariant() switch
    {
        "INITIATED" => CollectionStatus.Initiated,
        "PENDING" => CollectionStatus.Pending,
        "PROCESSING" => CollectionStatus.Processing,
        "APPROVED" => CollectionStatus.Approved,
        "PARTIAL" => CollectionStatus.Partial,
        "SUCCESSFUL" => CollectionStatus.Successful,
        "REFUNDED" => CollectionStatus.Refunded,
        "FAILED" => CollectionStatus.Failed,
        "DECLINED" => CollectionStatus.Declined,
        "REJECTED" => CollectionStatus.Rejected,
        "ABANDONED" => CollectionStatus.Abandoned,
        "EXPIRED" => CollectionStatus.Expired,
        "CANCELLED" => CollectionStatus.Cancelled,
        "CUSTOMER_ERROR" => CollectionStatus.CustomerError,
        "FRAUD_ERROR" => CollectionStatus.FraudError,
        "TIMEOUT" => CollectionStatus.Timeout,
        "ERROR" => CollectionStatus.Error,
        "SYSTEM_ERROR" => CollectionStatus.SystemError,
        "BANK_ERROR" => CollectionStatus.BankError,
        _ => CollectionStatus.Unknown,
    };

    /// <summary>Convenience overload that parses <see cref="CollectionStatusModel.Status"/>.</summary>
    public static CollectionStatus ParsedStatus(this CollectionStatusModel model) =>
        model.Status.ToCollectionStatus();

    /// <summary>
    /// Maps a status to the action a merchant should take.
    /// <see cref="CollectionStatus.Unknown"/> maps to <see cref="CollectionOutcome.Indeterminate"/> — reconcile rather than guess.
    /// </summary>
    public static CollectionOutcome Outcome(this CollectionStatus status) => status switch
    {
        CollectionStatus.Initiated
            or CollectionStatus.Pending
            or CollectionStatus.Processing
            or CollectionStatus.Approved
            or CollectionStatus.Partial => CollectionOutcome.InFlight,

        CollectionStatus.Successful => CollectionOutcome.Succeeded,
        CollectionStatus.Refunded => CollectionOutcome.Refunded,

        CollectionStatus.Failed
            or CollectionStatus.Declined
            or CollectionStatus.Rejected
            or CollectionStatus.Abandoned
            or CollectionStatus.Expired
            or CollectionStatus.Cancelled
            or CollectionStatus.CustomerError
            or CollectionStatus.FraudError => CollectionOutcome.NotDebited,

        // Timeout / Error / SystemError / BankError / Unknown
        _ => CollectionOutcome.Indeterminate,
    };

    /// <summary>True once the status will no longer change. Non-terminal statuses should be polled.</summary>
    public static bool IsTerminal(this CollectionStatus status) => status switch
    {
        CollectionStatus.Initiated
            or CollectionStatus.Pending
            or CollectionStatus.Processing
            or CollectionStatus.Approved
            or CollectionStatus.Partial
            or CollectionStatus.Unknown => false,
        _ => true,
    };
}
