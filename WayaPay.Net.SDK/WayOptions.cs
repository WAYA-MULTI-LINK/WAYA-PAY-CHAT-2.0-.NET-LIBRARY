namespace WayaPay.DotNetSdk;

public class WayaPayOptions
{
    public string MerchantId { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string Environment { get; set; } = "development";
}