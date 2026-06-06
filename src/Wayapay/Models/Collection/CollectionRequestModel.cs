using System.Text.Json.Nodes;

namespace WayaPay.Models.collection;

public sealed record CollectionRequestModel
{
    /// <summary>Quoted string, e.g. "1500.00".</summary>
    public required string Amount { get; init; }
    
    /// <summary>Quoted string, e.g. "NGN".</summary>
    public required string Currency { get; init; }
    
    /// <summary>Quoted string, e.g. "example@email.com".</summary>
    public required string Email { get; init; }

    /// <summary>Your unique reference / idempotency key.</summary>
    public required string TransactionId { get; init; }

    /// <summary>Quoted string, e.g. "firstname".</summary>
    public required string FirstName { get; init; }
    
    /// <summary>Quoted string, e.g. "lastname".</summary>
    public required string LastName { get; init; }
    
    /// <summary>Quoted string, e.g. "phone".</summary>
    public required string Phone { get; init; }
    
    /// <summary>Quoted string, e.g. "description".</summary>
    public required string Description { get; init; }

    /// <summary>Arbitrary metadata sent as a JSON object.</summary>
    public JsonObject? Meta { get; init; }
}