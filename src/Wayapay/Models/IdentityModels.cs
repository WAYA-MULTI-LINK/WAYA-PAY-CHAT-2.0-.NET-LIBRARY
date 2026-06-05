namespace WayaPay;

public sealed record BvnResult
{
    public string Bvn { get; init; } = "";
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? DateOfBirth { get; init; }
    public string? PhoneNumber1 { get; init; }
    public string? RegistrationDate { get; init; }
    public string? Gender { get; init; }
    public string? LgaOfOrigin { get; init; }
    public string? LgaOfResidence { get; init; }
    public string? MaritalStatus { get; init; }
    public string? Nationality { get; init; }
    public string? ResidentialAddress { get; init; }
    public string? StateOfOrigin { get; init; }

    /// <summary>"False" when clear. Treat anything else with care.</summary>
    public string? WatchListed { get; init; }
}
