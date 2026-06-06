namespace WayaPay.Models.Payout;

/// <summary>A single bank entry from GET /api/v2/get-bank-list. Use Code for payouts and account verification.</summary>
public sealed record PayoutBankResponseModel
{
    /// <summary>Quoted string, e.g. "044".</summary>
    public string Code { get; init; } = "";
    
    /// <summary>Quoted string, e.g. "Access Bank".</summary>
    public string Name { get; init; } = "";
    
    /// <summary>Quoted string, e.g. "044".</summary>
    public string Id { get; init; } = "";
    
    /// <summary>Quoted string, e.g. "true".</summary>
    public bool Status { get; init; }
}
