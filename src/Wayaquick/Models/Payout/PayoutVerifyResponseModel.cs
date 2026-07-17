namespace WayaQuick.Models.Payout;

/// <summary>Response from POST /api/v2/verify-account. Contains the resolved account name and bank details.</summary>
public sealed record PayoutVerifyResponseModel
{
    /// <summary>True when the account was resolved successfully.</summary>
    public bool Successful { get; init; }

    /// <summary>Provider response code. "00" means approved.</summary>
    public string? ResponseCode { get; init; }

    /// <summary>Human-readable provider message, e.g. "Approved".</summary>
    public string? ResponseMessage { get; init; }

    /// <summary>10-digit NUBAN account number.</summary>
    public string AccountNumber { get; init; } = "";

    /// <summary>Registered account name as held by the bank.</summary>
    public string AccountName { get; init; } = "";

    /// <summary>CBN bank code of the destination bank.</summary>
    public string? BankCode { get; init; }

    /// <summary>Full name of the destination bank.</summary>
    public string? BankName { get; init; }

    /// <summary>"WAYA-BANK" for intra-bank, "OTHERS" for inter-bank.</summary>
    public string? EnquiryType { get; init; }
}