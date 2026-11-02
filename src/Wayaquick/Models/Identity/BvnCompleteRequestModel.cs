namespace WayaQuick.Models.Identity;

/// <summary>Request body for POST /api/v2/identity-verification/bvn/complete.</summary>
public sealed record BvnCompleteRequestModel
{
    /// <summary>Session reference returned from InitiateBvnAsync.</summary>
    public required string Reference { get; init; }

    /// <summary>One-time password received by the customer.</summary>
    public required string Otp { get; init; }
}

