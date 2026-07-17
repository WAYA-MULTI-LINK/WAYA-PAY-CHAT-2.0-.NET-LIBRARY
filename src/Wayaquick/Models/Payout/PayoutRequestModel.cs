namespace WayaQuick.Models.Payout;

/// <summary>Request body for POST /api/v2/payment-payout/initiate. Transfers funds to a bank account.</summary>
public sealed record PayoutRequestModel
{
    /// <summary>Amount to transfer. Must be greater than 0.</summary>
    public required decimal Amount { get; init; }

    /// <summary>ISO-4217 currency code, e.g. "NGN".</summary>
    public required string Currency { get; init; }

    /// <summary>10-digit NUBAN destination account number.</summary>
    public required string AccountNumber { get; init; }

    /// <summary>CBN bank code from the bank list endpoint.</summary>
    public required string BankCode { get; init; }

    /// <summary>Your unique reference / idempotency key. Generate a fresh one per operation.</summary>
    public required string Reference { get; init; }

    /// <summary>Destination account name. Should match the name returned by account verification.</summary>
    public string? AccountName { get; init; }

    /// <summary>Statement narration shown on the recipient's bank statement.</summary>
    public string? Narration { get; init; }
}