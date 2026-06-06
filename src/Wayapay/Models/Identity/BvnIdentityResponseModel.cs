namespace WayaPay.Models.Identity;

/// <summary>Response from POST /api/v2/identity-verification/bvn. Contains the BVN holder's KYC record.</summary>
public sealed record BvnIdentityResponseModel
{
    /// <summary>11-digit Bank Verification Number.</summary>
    public string Bvn { get; init; } = "";

    /// <summary>First name as registered on the BVN.</summary>
    public string? FirstName { get; init; }

    /// <summary>Middle name as registered on the BVN.</summary>
    public string? MiddleName { get; init; }

    /// <summary>Last name as registered on the BVN.</summary>
    public string? LastName { get; init; }

    /// <summary>Date of birth, e.g. "15-Aug-2001".</summary>
    public string? DateOfBirth { get; init; }

    /// <summary>Primary phone number linked to the BVN.</summary>
    public string? PhoneNumber1 { get; init; }

    /// <summary>Date the BVN was registered, e.g. "19-Nov-2018".</summary>
    public string? RegistrationDate { get; init; }

    /// <summary>Email address linked to the BVN.</summary>
    public string? Email { get; init; }

    /// <summary>"Male" or "Female".</summary>
    public string? Gender { get; init; }

    /// <summary>Local government area of origin.</summary>
    public string? LgaOfOrigin { get; init; }

    /// <summary>Local government area of residence.</summary>
    public string? LgaOfResidence { get; init; }

    /// <summary>Marital status, e.g. "Single".</summary>
    public string? MaritalStatus { get; init; }

    /// <summary>Nationality, e.g. "Nigeria".</summary>
    public string? Nationality { get; init; }

    /// <summary>Full residential address on record.</summary>
    public string? ResidentialAddress { get; init; }

    /// <summary>State of origin, e.g. "Enugu State".</summary>
    public string? StateOfOrigin { get; init; }

    /// <summary>"False" when clear. Treat anything other than "False" with care.</summary>
    public string? WatchListed { get; init; }

    /// <summary>Base64-encoded portrait image.</summary>
    public string? Base64Image { get; init; }
}
