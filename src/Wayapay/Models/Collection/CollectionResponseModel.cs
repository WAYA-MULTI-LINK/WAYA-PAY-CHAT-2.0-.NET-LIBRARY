namespace WayaPay.Models.collection;

public sealed record CollectionResponseModel
{
    /// <summary>Echoes back your transactionId.</summary>
    public string UniqueId { get; init; } = "";

    /// <summary>WayaQuick's internal transaction ID.</summary>
    public string TransactionId { get; init; } = "";

    public string? Email { get; init; }

    /// <summary>Quoted string, e.g. "1500.00".</summary>
    public string? Amount { get; init; }

    /// <summary>Redirect the customer here to complete payment.</summary>
    public string CheckOutUrl { get; init; } = "";

    public string? MerchantId { get; init; }
}
