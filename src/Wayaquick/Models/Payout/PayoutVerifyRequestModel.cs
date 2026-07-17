namespace WayaQuick.Models.Payout;

/// <summary>Request body for POST /api/v2/verify-account. Resolves an account number to its registered name. Always call this before initiating a payout.</summary>
public sealed record PayoutVerifyRequestModel
{
    /// <summary>10-digit NUBAN account number.</summary>
    public required string AccountNumber { get; init; }

    /// <summary>"WAYA-BANK" for intra-bank, "OTHERS" for inter-bank.</summary>
    public required string EnquiryType { get; init; }

    /// <summary>CBN bank code. Required when EnquiryType is "OTHERS".</summary>
    public string? BankCode { get; init; }
}