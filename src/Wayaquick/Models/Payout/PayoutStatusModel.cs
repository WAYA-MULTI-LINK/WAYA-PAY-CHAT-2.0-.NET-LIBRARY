namespace WayaQuick.Models.Payout;

/// <summary>
/// Status of a single payout (disbursement) transaction.
/// Use <see cref="TransactionReference"/> as the idempotency key.
/// Inspect <see cref="Status"/> together with <see cref="PayoutStatusExtensions"/> to decide
/// whether to keep reconciling, treat as delivered, or treat as failed.
/// </summary>
public sealed record PayoutStatusModel
{
    /// <summary>Your unique reference, e.g. "PAYOUT-20260604-001". Stable idempotency key.</summary>
    public string TransactionReference { get; init; } = "";

    /// <summary>Raw payout status, e.g. "SUCCESS". Parse with <see cref="PayoutStatusExtensions.ToPayoutStatus"/>.</summary>
    public string Status { get; init; } = "";

    /// <summary>Amount disbursed, quoted string, e.g. "500.00".</summary>
    public string? Amount { get; init; }

    /// <summary>Destination NUBAN account number, e.g. "0123456789".</summary>
    public string? DestinationAccountNumber { get; init; }

    /// <summary>Destination account name, e.g. "JOHN DOE".</summary>
    public string? DestinationAccountName { get; init; }

    /// <summary>Destination bank name, e.g. "GTBank".</summary>
    public string? DestinationBankName { get; init; }

    /// <summary>Bank narration / transfer description.</summary>
    public string? Narration { get; init; }

    /// <summary>Creation timestamp, e.g. "2026-06-04T10:00:32".</summary>
    public string? CreatedAt { get; init; }
}

/// <summary>Known values of <see cref="PayoutStatusModel.Status"/>.</summary>
public enum PayoutStatus
{
    /// <summary>Status string not recognised by this SDK version. Treat as non-terminal and reconcile.</summary>
    Unknown = 0,

    /// <summary>Submitted; terminal outcome not yet recorded (reconciling). Non-terminal.</summary>
    Pending,

    /// <summary>Completed successfully.</summary>
    Success,

    /// <summary>Failed/reversed — the merchant wallet was re-credited.</summary>
    Reversed,
}

/// <summary>How a <see cref="PayoutStatus"/> should be acted on.</summary>
public enum PayoutOutcome
{
    /// <summary>Submitted; terminal outcome not yet recorded — keep reconciling.</summary>
    Reconciling,

    /// <summary>Completed successfully — funds delivered.</summary>
    Succeeded,

    /// <summary>Failed/reversed — the merchant wallet was re-credited.</summary>
    Reversed,
}

/// <summary>Helpers for interpreting a payout status.</summary>
public static class PayoutStatusExtensions
{
    /// <summary>Parses the raw <see cref="PayoutStatusModel.Status"/> string. Returns <see cref="PayoutStatus.Unknown"/> for unrecognised values.</summary>
    public static PayoutStatus ToPayoutStatus(this string? status) => status?.Trim().ToUpperInvariant() switch
    {
        "PENDING" => PayoutStatus.Pending,
        "SUCCESS" => PayoutStatus.Success,
        "REVERSED" => PayoutStatus.Reversed,
        _ => PayoutStatus.Unknown,
    };

    /// <summary>Convenience overload that parses <see cref="PayoutStatusModel.Status"/>.</summary>
    public static PayoutStatus ParsedStatus(this PayoutStatusModel model) =>
        model.Status.ToPayoutStatus();

    /// <summary>
    /// Maps a status to the action a merchant should take.
    /// <see cref="PayoutStatus.Unknown"/> maps to <see cref="PayoutOutcome.Reconciling"/> — reconcile rather than guess.
    /// </summary>
    public static PayoutOutcome Outcome(this PayoutStatus status) => status switch
    {
        PayoutStatus.Success => PayoutOutcome.Succeeded,
        PayoutStatus.Reversed => PayoutOutcome.Reversed,
        // Pending / Unknown
        _ => PayoutOutcome.Reconciling,
    };

    /// <summary>True once the status will no longer change. Non-terminal statuses should be reconciled.</summary>
    public static bool IsTerminal(this PayoutStatus status) => status switch
    {
        PayoutStatus.Pending or PayoutStatus.Unknown => false,
        _ => true,
    };
}
