// Run with:
//   WAYA_MERCHANT_ID=MER_... WAYA_SECRET_KEY=WAYASECK_... dotnet run

using WayaPay;
using WayaPay.Models.Payout;
using WayaPay.Models.Identity;
using WayaPay.Models.collection;

var client = new WayaPayClient(new WayaPayOptions
{
    MerchantId = Environment.GetEnvironmentVariable("WAYA_MERCHANT_ID")!,
    SecretKey  = Environment.GetEnvironmentVariable("WAYA_SECRET_KEY")!,
});

try
{
    // 1. List supported banks — grab codes for payouts and account verification
    var banks = await client.Payouts.ListBanksAsync();
    Console.WriteLine($"Banks loaded: {banks.Count}");
    var gtb = banks.FirstOrDefault(b => b.Name.Contains("Guaranty", StringComparison.OrdinalIgnoreCase));
    if (gtb is not null)
        Console.WriteLine($"  GTB code: {gtb.Code}");

    // 2. Verify a destination account before moving money
    var verified = await client.Payouts.VerifyAccountAsync(new PayoutVerifyRequestModel
    {
        AccountNumber = "0123456789",
        EnquiryType   = "OTHERS",
        BankCode      = "044",
    });
    Console.WriteLine($"Resolved: {verified.AccountName} @ {verified.BankName} ({verified.ResponseCode})");

    // 3. BVN identity check
    var identity = await client.Identity.VerifyBvnAsync(new BvnIdentityRequestModel
    {
        Bvn = "22500809037",
    });
    Console.WriteLine($"BVN holder: {identity.FirstName} {identity.LastName} | watch-listed: {identity.WatchListed}");

    // 4. Initiate a payout — always verify the account first (step 2)
    var payout = await client.Payouts.InitiateAsync(new PayoutRequestModel
    {
        Amount        = 250.00m,
        Currency      = "NGN",
        AccountNumber = verified.AccountNumber,
        BankCode      = "044",
        AccountName   = verified.AccountName,
        Reference     = WayaPayClient.GenerateReference("PAYOUT"),
        Narration     = "Demo payout",
    });
    Console.WriteLine($"Payout: {payout.PayoutReference} — {payout.Status}");

    // 5. Initiate a collection — returns a checkout URL to redirect the customer to
    var collection = await client.Collection.InitiateAsync(new CollectionRequestModel
    {
        Amount        = "1500.00",
        Currency      = "NGN",
        Email         = "customer@example.com",
        TransactionId = WayaPayClient.GenerateReference("TXN"),
        FirstName     = "John",
        LastName      = "Doe",
        Phone         = "08012345678",
        Description   = "Demo collection",
    });
    Console.WriteLine($"Checkout URL: {collection.CheckOutUrl}");
    Console.WriteLine($"Transaction ID: {collection.TransactionId}");
}
catch (HttpRequestException e)
{
    Console.Error.WriteLine($"API error: {e.Message}");
    Environment.Exit(1);
}
