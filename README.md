# WayaPay .NET

.NET client for the **WayaQuick Merchant API v2**. Collect payments, send payouts, verify bank accounts, and run BVN identity checks in Nigeria.

Targets `net8.0`. No dependencies outside the framework. **Server-side only** — your secret key must never leave your server.

## Install

```bash
dotnet add package WayaPay
```

## Quickstart

```csharp
using WayaPay;

var client = new WayaPayClient(new WayaPayOptions
{
    MerchantId = "MER_...",            // from the dashboard
    SecretKey  = "WAYASECK_TEST_...",  // swap for WAYASECK_... on live
});
```

## List banks

```csharp
var banks = await client.Payouts.ListBanksAsync();
// List<PayoutBankResponseModel> — each has .Code and .Name
```

## Verify an account

Always verify before sending a payout — confirms the account exists and returns the registered name.

```csharp
var account = await client.Payouts.VerifyAccountAsync(new()
{
    AccountNumber = "0123456789",
    EnquiryType   = "OTHERS",   // "WAYA-BANK" for intra-bank
    BankCode      = "044",      // required when EnquiryType is "OTHERS"
});
Console.WriteLine(account.AccountName); // "JOHN DOE"
```

## Initiate a payout

```csharp
var payout = await client.Payouts.InitiateAsync(new()
{
    Amount        = 5000.00m,
    Currency      = "NGN",
    AccountNumber = "0123456789",
    BankCode      = "044",
    AccountName   = account.AccountName,
    Reference     = WayaPayClient.GenerateReference("PAYOUT"),
    Narration     = "April salary",
});
// payout.Status == "PROCESSING" means accepted, not yet settled
```

`GenerateReference` produces a timestamped, collision-resistant key (`PAYOUT-1748160000000-A1B2C3D4`). Generate a fresh one per operation and reuse the same one on retries.

## Collect a payment

```csharp
var collection = await client.Collection.InitiateAsync(new()
{
    Amount        = "1500.00",
    Currency      = "NGN",
    Email         = "customer@example.com",
    TransactionId = WayaPayClient.GenerateReference("TXN"),
    FirstName     = "John",
    LastName      = "Doe",
    Phone         = "08012345678",
    Description   = "Order #1234",
});
// Redirect the customer to collection.CheckOutUrl to complete payment.
// Confirm the result on your server before fulfilling the order.
```

## BVN identity check

```csharp
var identity = await client.Identity.VerifyBvnAsync(new()
{
    Bvn = "22500809037", // exactly 11 digits — validated locally before the request
});
Console.WriteLine($"{identity.FirstName} {identity.LastName}");
```

BVN data is sensitive personal information. Store, transmit, and log it only as your data-protection obligations allow.

## Error handling

Failed requests throw `HttpRequestException` with the API message as the exception message.

```csharp
try
{
    await client.Payouts.InitiateAsync(input);
}
catch (HttpRequestException e)
{
    Console.Error.WriteLine(e.Message); // e.g. "IP 1.2.3.4 is not whitelisted"
}
```

Input validation errors (missing required fields, malformed BVN, missing `BankCode`) throw `ArgumentException` or `ArgumentNullException` before any network call is made.

## Options

```csharp
new WayaPayOptions
{
    MerchantId = "MER_...",
    SecretKey  = "WAYASECK_...",
    TimeoutMs  = 30_000,   // default: 30 s
    MaxRetries = 2,        // default: 2 — GET only, exponential backoff
    HttpClient = ...,      // optional: inject your own (DI, handler chains, test fakes)
}
```

Retries apply to **GET requests only** (bank list) on timeouts, network errors, 429, and 5xx. Writes never auto-retry.

## Dependency injection

```csharp
services.AddSingleton(sp => new WayaPayClient(new WayaPayOptions
{
    MerchantId = config["WayaPay:MerchantId"]!,
    SecretKey  = config["WayaPay:SecretKey"]!,
    HttpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("wayapay"),
}));
```

## Full example

See [samples/ConsoleDemo/Program.cs](samples/ConsoleDemo/Program.cs) for a runnable end-to-end demo covering all five operations.

```bash
WAYA_MERCHANT_ID=MER_... WAYA_SECRET_KEY=WAYASECK_TEST_... dotnet run --project samples/ConsoleDemo
```

## Going live

On the merchant dashboard: finish KYC, grab your Merchant ID, generate your secret key under **Settings → API Keys and Webhooks**, and whitelist your server IPs. Swap `WAYASECK_TEST_...` for `WAYASECK_...` — the rest of your code stays the same.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

MIT
