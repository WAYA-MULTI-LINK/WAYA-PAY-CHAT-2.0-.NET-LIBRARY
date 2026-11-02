namespace WayaQuick.Models.Identity;

/// <summary>Response data from POST /api/v2/identity-verification/bvn/initiate.</summary>
public sealed record BvnInitiateResponseModel
{
    /// <summary>Session reference to pass to CompleteBvnAsync.</summary>
    public string Reference { get; init; } = "";

    /// <summary>Masked phone number the OTP was dispatched to, e.g. "234****0000".</summary>
    public string PhoneNumber { get; init; } = "";

    /// <summary>Session expiration timestamp.</summary>
    public string ExpiresAt { get; init; } = "";
}

