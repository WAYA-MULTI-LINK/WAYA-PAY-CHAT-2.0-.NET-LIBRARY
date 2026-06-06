namespace WayaPay.Models.Payout;

public sealed record PayoutVerifyRequestModel
{
    /// <summary>10-digit NUBAN account number.</summary>
    public required string AccountNumber { get; init; }

    /// <summary>"WAYA-BANK" for intra-bank, "OTHERS" for inter-bank.</summary>
    public required string EnquiryType { get; init; }

    /// <summary>CBN bank code. Required when EnquiryType is "OTHERS".</summary>
    public string? BankCode { get; init; }
}