namespace WayaPay.Models.Payout;

/// <summary>Response from POST /api/v2/payment-payout/initiate. PROCESSING means accepted, not settled — confirm via webhook or status check before treating as delivered.</summary>
public sealed record PayoutResponseModel
{
    /// <summary>WayaQuick's internal payout reference. Use this to track and confirm the payout.</summary>
    public string PayoutReference { get; init; } = "";

    /// <summary>Echoes back your unique reference supplied in the request.</summary>
    public string? MerchantReference { get; init; }

    /// <summary>Payout status, e.g. "PROCESSING". Does not mean settled.</summary>
    public string Status { get; init; } = "";

    /// <summary>Human-readable status message.</summary>
    public string? Message { get; init; }
}