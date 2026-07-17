namespace WayaQuick.Models.Identity;

/// <summary>Request body for POST /api/v2/identity-verification/bvn. Verifies a BVN and returns the matching holder record.</summary>
public sealed record BvnIdentityRequestModel
{
    /// <summary>Quoted string, e.g. "0000000000".</summary>
    public required string Bvn { get; init; }
}