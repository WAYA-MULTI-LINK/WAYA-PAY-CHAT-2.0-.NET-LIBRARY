namespace WayaPay;

public sealed record VerifyAccountInput
{
    public required string AccountNumber { get; init; }

    /// <summary>Required for OTHERS, optional for WAYABANK.</summary>
    public string? BankCode { get; init; }

    /// <summary>OTHERS for any external bank, WAYABANK for an internal WayaBank account.</summary>
    public string EnquiryType { get; init; } = "OTHERS";
}

public sealed record CreateDynamicAccountInput
{
    public required string AccountName { get; init; }
    public required string CustomerId { get; init; }

    /// <summary>Your unique reference. Auto generated when left null.</summary>
    public string? ReferenceId { get; init; }
    public required string Purpose { get; init; }
    public string Mode { get; init; } = "ONE_TIME";
}

public sealed record PayoutInput
{
    public required decimal Amount { get; init; }
    public string Currency { get; init; } = "NGN";
    public required string AccountNumber { get; init; }
    public required string BankCode { get; init; }
    public required string AccountName { get; init; }

    /// <summary>Your dedup and tracking key. Auto generated when left null.</summary>
    public string? Reference { get; init; }
    public required string Narration { get; init; }
}

public sealed record CollectInput
{
    public string PaymentLinkType { get; init; } = "ONE_TIME_PAYMENT_LINK";
    public required string PaymentLinkName { get; init; }
    public required string Description { get; init; }
    public required decimal PayableAmount { get; init; }
    public string Currency { get; init; } = "NGN";
    public string? SuccessMessage { get; init; }
    public string? PhoneNumber { get; init; }
    public required string RedirectLink { get; init; }
    public string? CustomURL { get; init; }
    public int? TotalCount { get; init; }
    public string? ChargeInterval { get; init; }
    public string? PlanId { get; init; }

    /// <summary>Required when <see cref="LinkCanExpire"/> is true.</summary>
    public string? ExpiryDate { get; init; }
    public bool? LinkCanExpire { get; init; }
    public Dictionary<string, object>? OtherDetailsJSON { get; init; }
}

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
