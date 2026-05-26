namespace WayaPay.DotNetSdk.Models;

public class VerifyAccountRequest
{
    public string AccountNumber { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
}