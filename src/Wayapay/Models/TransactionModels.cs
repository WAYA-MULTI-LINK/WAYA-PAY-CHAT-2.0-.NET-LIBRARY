namespace WayaPay;

public sealed record HistoryFilter
{
    public int Page { get; init; }
    public int Size { get; init; } = 20;
    public string? Status { get; init; }

    /// <summary>Start of the date range, ISO 8601.</summary>
    public string? From { get; init; }

    /// <summary>End of the date range, ISO 8601.</summary>
    public string? To { get; init; }
}

public sealed record TransactionResult
{
    public string TransactionReference { get; init; } = "";
    public string? MerchantReference { get; init; }

    /// <summary>SUCCESS means settled. Anything else means keep waiting or investigate.</summary>
    public string Status { get; init; } = "";
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public string? Channel { get; init; }
    public string? CustomerEmail { get; init; }
    public string? PaidAt { get; init; }
}

public sealed record HistoryItem
{
    public string TransactionReference { get; init; } = "";
    public string? MerchantReference { get; init; }
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public string Status { get; init; } = "";
    public string? Channel { get; init; }
    public string? CustomerEmail { get; init; }
    public string? CreatedAt { get; init; }
}

public sealed record HistoryResult
{
    public List<HistoryItem> Items { get; init; } = new();
    public int Page { get; init; }
    public int Size { get; init; }
    public int TotalElements { get; init; }
    public int TotalPages { get; init; }
}
