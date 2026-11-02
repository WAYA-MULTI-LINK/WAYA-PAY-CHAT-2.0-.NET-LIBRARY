namespace WayaQuick.Models.Identity;

/// <summary>Request body for POST /api/v2/identity-verification/bvn/initiate.</summary>
public sealed record BvnInitiateRequestModel
{
    /// <summary>11-digit BVN string.</summary>
    public required string Bvn { get; init; }
}

