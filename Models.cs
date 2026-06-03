namespace WayaPay;

/// <summary>The envelope every endpoint returns.</summary>
public sealed record WayaPayResponse<T>
{
    public bool Success { get; init; }
    public string? Code { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public string? Timestamp { get; init; }
}

public sealed record Bank
{
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
    public string Id { get; init; } = "";
    public bool Status { get; init; }
}

public sealed record VerifyAccountResult
{
    public bool Successful { get; init; }
    public string? ResponseCode { get; init; }
    public string? ResponseMessage { get; init; }
    public string AccountNumber { get; init; } = "";
    public string AccountName { get; init; } = "";
    public string? BankCode { get; init; }
    public string? BankName { get; init; }
    public string? EnquiryType { get; init; }
}

public sealed record DynamicAccount
{
    public long Id { get; init; }
    public string VirtualAccountNumber { get; init; } = "";
    public string NubanNumber { get; init; } = "";
    public string AccountName { get; init; } = "";
    public string? CustomerId { get; init; }
    public string? AccountType { get; init; }
    public string? Status { get; init; }
    public bool IsActive { get; init; }
    public bool CanReceivePayments { get; init; }
    public string? ReferenceId { get; init; }
    public string? Metadata { get; init; }
    public decimal TotalLimit { get; init; }
    public decimal CurrentBalance { get; init; }
    public string? AssignedAt { get; init; }
    public string? ExpiresAt { get; init; }
    public string? CreatedAt { get; init; }
}

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

public sealed record PayoutResult
{
    public string PayoutReference { get; init; } = "";
    public string? MerchantReference { get; init; }
    public string Status { get; init; } = "";
    public string? Message { get; init; }
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
