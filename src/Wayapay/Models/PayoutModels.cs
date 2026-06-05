namespace WayaPay;

public sealed record PayoutInput
{
    public required decimal Amount { get; init; }
    public string Currency { get; init; } = "NGN";
    public required string AccountNumber { get; init; }
    public required string BankCode { get; init; }
    public required string AccountName { get; init; }

    /// <summary>Your dedup and tracking key. Auto generated when left null.</summary>
    public string? Reference { get; init; }
    public required string Narration { get; init; }
}

public sealed record PayoutResult
{
    public string PayoutReference { get; init; } = "";
    public string? MerchantReference { get; init; }
    public string Status { get; init; } = "";
    public string? Message { get; init; }
}
