namespace WayaPay;

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

public sealed record CollectResult
{
    public string? MerchantId { get; init; }
    public string? PaymentLinkId { get; init; }
    public string? PaymentLinkType { get; init; }
    public string? PaymentLinkName { get; init; }
    public string? Description { get; init; }
    public decimal PayableAmount { get; init; }
    public string? Currency { get; init; }
    public string? AmountText { get; init; }
    public string? SuccessMessage { get; init; }
    public string? RedirectLink { get; init; }
    public string CustomerPaymentLink { get; init; } = "";
    public string ShortUrl { get; init; } = "";
    public string? Status { get; init; }
    public bool Deleted { get; init; }

    /// <summary>"TEST" or live. Never confuse a sandbox link for a real one.</summary>
    public string? MerchantKeyMode { get; init; }
    public string PaymentLinkReference { get; init; } = "";
    public string? ExpiryDate { get; init; }
    public int TotalCount { get; init; }
    public bool LinkCanExpire { get; init; }
    public bool IsSubscriptionPaymentLink { get; init; }
    public long CreatedBy { get; init; }
    public string? CreatedAt { get; init; }
}
