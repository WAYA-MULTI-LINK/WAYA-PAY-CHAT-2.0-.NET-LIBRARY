namespace WayaPay.DotNetSdk.Models;

public class InitializePaymentRequest
{
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CallBackUrl { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string PaymentRef { get; set; } = string.Empty;
    public PaymentMetadata Metadata { get; set; } = new();
}

public class PaymentMetadata
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string? CancelUrl { get; set; }
}