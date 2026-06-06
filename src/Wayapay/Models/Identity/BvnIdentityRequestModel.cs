namespace WayaPay.Models.Identity;

public sealed record  BvnIdentityRequestModel
{
    /// <summary>Quoted string, e.g. "0000000000".</summary>
    public required string Bvn { get; init; }
}