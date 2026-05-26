namespace WayaPay.DotNetSdk.Models;

public class InitiatePayoutRequest
{
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
}