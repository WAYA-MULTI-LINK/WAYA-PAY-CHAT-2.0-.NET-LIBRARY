// Run with:
//   WAYA_MERCHANT_ID=MER_... WAYA_SECRET_KEY=WAYASECK_TEST_... dotnet run

using WayaPay;

var client = new WayaPayClient(new WayaPayOptions
{
    MerchantId = Environment.GetEnvironmentVariable("WAYA_MERCHANT_ID")!,
    SecretKey = Environment.GetEnvironmentVariable("WAYA_SECRET_KEY")!,
    Environment = "staging",
});

try
{
    // 1. Banks
    var banks = await client.Banks.ListAsync();
    Console.WriteLine($"Banks: {banks.Count}");

    // 2. Verify a destination before you ever move money
    var verified = await client.Accounts.VerifyAsync(new VerifyAccountInput
    {
        AccountNumber = "0123456789",
        BankCode = "044",
    });
    Console.WriteLine($"Resolved name: {verified.AccountName}");

    // 3. Mint a virtual account for an order
    var vacct = await client.Accounts.CreateDynamicAsync(new CreateDynamicAccountInput
    {
        AccountName = "ORDER-7821 PAYMENT",
        CustomerId = "CUST-98765",
        ReferenceId = "ORDER-7821",
        Purpose = "Order payment",
    });
    Console.WriteLine($"Pay into: {vacct.VirtualAccountNumber}");

    // 4. BVN check
    var bvn = await client.Identity.VerifyBvnAsync("22212345678");
    Console.WriteLine($"BVN holder: {bvn.FirstName} {bvn.LastName} | watchListed: {bvn.WatchListed}");

    // 5. Pay someone out. Verify the name above first.
    var payout = await client.Payouts.InitiateAsync(new PayoutInput
    {
        Amount = 25000m,
        AccountNumber = verified.AccountNumber,
        BankCode = "058",
        AccountName = verified.AccountName,
        Reference = WayaPayClient.GenerateReference("PAYOUT"),
        Narration = "Salary payment May 2026",
    });
    Console.WriteLine($"Payout: {payout.PayoutReference} {payout.Status}");

    // 6. Create a payment link
    var link = await client.Collection.CreateAsync(new CollectInput
    {
        PaymentLinkName = "Order #1234",
        Description = "Order #1234 - 2 items",
        PayableAmount = 1500m,
        RedirectLink = "https://merchant.example.com/callback",
    });
    Console.WriteLine($"Send customer to: {link.ShortUrl}");

    // 7. Verify a transaction
    var txn = await client.Transactions.VerifyAsync(payout.PayoutReference);
    Console.WriteLine($"Txn status: {txn.Status}");

    // 8. Reconcile every successful transaction in a window
    var count = 0;
    await foreach (var t in client.Transactions.HistoryAllAsync(new HistoryFilter
    {
        Status = "SUCCESS",
        From = "2026-05-01T00:00:00Z",
        To = "2026-05-31T23:59:59Z",
    }))
    {
        count++;
    }
    Console.WriteLine($"Reconciled: {count} transactions");
}
catch (WayaPayException e)
{
    Console.Error.WriteLine($"[{e.Type}] code={e.ErrorCode} status={e.Status} :: {e.Message}");
    Environment.Exit(1);
}
