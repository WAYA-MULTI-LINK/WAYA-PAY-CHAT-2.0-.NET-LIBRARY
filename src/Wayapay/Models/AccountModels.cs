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
