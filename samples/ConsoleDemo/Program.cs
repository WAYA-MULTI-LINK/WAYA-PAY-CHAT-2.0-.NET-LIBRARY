// Run with:
//   WAYA_MERCHANT_ID=MER_... WAYA_SECRET_KEY=WAYASECK_... dotnet run

using System.Security.Cryptography;
using System.Text;
using WayaPay;
using WayaPay.Models.Payout;
using WayaPay.Models.Identity;
using WayaPay.Models.collection;
using WayaPay.Models.Webhook;

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

    // 6. Check payout status — reconcile by the reference you sent at initiation
    var payoutStatus = await client.Payouts.GetStatusAsync(payout.MerchantReference ?? payout.PayoutReference);
    switch (payoutStatus.ParsedStatus().Outcome())
    {
        case PayoutOutcome.Succeeded:   Console.WriteLine("Payout delivered."); break;
        case PayoutOutcome.Reversed:    Console.WriteLine("Payout reversed — wallet re-credited."); break;
        case PayoutOutcome.Reconciling: Console.WriteLine("Payout still reconciling — check again later."); break;
    }

    // 7. Initiate a collection — returns a checkout URL to redirect the customer to
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

    // 8. Check collection status — the pull/safety-net path alongside the webhook
    var collectionStatus = await client.Collection.GetStatusAsync(collection.TransactionId);
    Console.WriteLine($"Collection status: {collectionStatus.Status} (paid {collectionStatus.AmountPaid})");
    if (collectionStatus.ParsedStatus() == CollectionStatus.Successful)
        Console.WriteLine($"Funds confirmed — fulfil order using refNo {collectionStatus.RefNo}");

    // 9. Verify a webhook (offline demo). In production WayaPay POSTs this to your HTTPS endpoint;
    //    here we sign a sample body locally to show the verification flow end to end.
    const string webhookSecret = "WAYASECK_TEST_demo_webhook_secret";
    const string rawBody =
        """{"OrderId":"1779662251460508970","Amount":1500.00,"Fee":15.00,"Currency":"NGN","Status":"SUCCESSFUL","productName":"CARD","customer":{"email":"john@example.com"},"merchantId":"MER_xyz","recurrentPayment":false}""";
    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
    var signature = Convert.ToBase64String(HMACSHA256.HashData(
        Encoding.UTF8.GetBytes(webhookSecret), Encoding.UTF8.GetBytes($"{timestamp}.{rawBody}")));

    try
    {
        // Via the client wrapper. With WebhookSecret set in WayaPayOptions you can drop the secret arg:
        //   client.Webhooks.ConstructEvent(rawBody, timestamp, signature);
        var evt = client.Webhooks.ConstructEvent(rawBody, timestamp, signature, webhookSecret);
        Console.WriteLine($"Webhook verified: {evt.OrderId} — {evt.Status} ({evt.Amount} {evt.Currency})");
        if (evt.ShouldFulfil())
            Console.WriteLine($"  Fulfil order — idempotency key {evt.OrderId}");
    }
    catch (WayaPayWebhookException e)
    {
        Console.Error.WriteLine($"Rejected webhook: {e.Message}");
    }
}
catch (HttpRequestException e)
{
    Console.Error.WriteLine($"API error: {e.Message}");
    Environment.Exit(1);
}
