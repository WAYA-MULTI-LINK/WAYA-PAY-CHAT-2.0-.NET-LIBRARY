# WayaPay (.NET)

.NET client for the **WayaPay Merchant API v2**. Collect, payout, accounts, identity, and reconciliation in one library. Targets `net8.0`, depends on nothing outside the framework.

This is a **server side** library. Your secret key lives here and only here. Never ship it to a browser, a mobile app, or a public repo. A leaked key is a wallet with the PIN on the back.

## Requirements

.NET 8.0 or newer.

## Install

```bash
dotnet add package WayaPay
```

Or reference the project directly:

```bash
dotnet add reference path/to/WayaPay.csproj
```

## Quickstart

```csharp
using WayaPay;

var client = new WayaPayClient(new WayaPayOptions
{
    MerchantId = "MER_...",                 // from the dashboard
    SecretKey  = "WAYASECK_TEST_...",        // WAYASECK_... on live
    Environment = "staging",                 // "staging" or "production"
});

var banks = await client.Banks.ListAsync();
```

Test against `staging` until your integration is steady, then change one word to `production`. The rest of your code stays the same.

## What you get back

Every method returns the envelope's `data`, already deserialized into a typed record. Success, code, and timestamp only matter when something fails, and failures throw. So the happy path stays clean and strongly typed:

```csharp
var acct = await client.Accounts.VerifyAsync(new VerifyAccountInput
{
    AccountNumber = "0123456789",
    BankCode = "044",
});
Console.WriteLine(acct.AccountName); // typed, with IDE autocomplete
```

## API

### Banks

```csharp
List<Bank> banks = await client.Banks.ListAsync();
```

### Accounts

```csharp
// Resolve an account number to its registered name
var result = await client.Accounts.VerifyAsync(new VerifyAccountInput
{
    AccountNumber = "0123456789",
    BankCode = "044",          // omit only when EnquiryType is "WAYABANK"
    EnquiryType = "OTHERS",    // default
});

// Mint a virtual NUBAN account for an order or customer
var vacct = await client.Accounts.CreateDynamicAsync(new CreateDynamicAccountInput
{
    AccountName = "ORDER-7821 PAYMENT",
    CustomerId = "CUST-98765",
    ReferenceId = "ORDER-7821",  // auto generated if left null
    Purpose = "Order payment",
    // Mode defaults to "ONE_TIME"
});
// Hand vacct.VirtualAccountNumber to the customer
```

### Identity

```csharp
var bvn = await client.Identity.VerifyBvnAsync("22212345678"); // 11 digits, validated locally
// treat anything other than "False" on bvn.WatchListed with care
```

### Payouts

```csharp
var payout = await client.Payouts.InitiateAsync(new PayoutInput
{
    Amount = 25000m,
    AccountNumber = "0123456789",
    BankCode = "058",
    AccountName = "JOHN DOE",     // match the verified name
    Narration = "Salary May 2026",
    // Currency defaults to "NGN", Reference auto generated if left null
});
// PROCESSING means accepted, not settled. Verify with the reference below.
```

### Collect

```csharp
var link = await client.Collect.CreateAsync(new CollectInput
{
    PaymentLinkName = "Order #1234",
    Description = "Order #1234 - 2 items",
    PayableAmount = 1500m,
    RedirectLink = "https://merchant.example.com/callback",
    // PaymentLinkType defaults to "ONE_TIME_PAYMENT_LINK", Currency to "NGN"
});
// Send the customer to link.ShortUrl. Keep link.PaymentLinkReference to reconcile.
```

If you set `LinkCanExpire = true`, you must also pass `ExpiryDate`. The library enforces it before the call leaves your server.

### Transactions

```csharp
// Verify one transaction
var txn = await client.Transactions.VerifyAsync("WQ-TXN-9F8E7D6C");
// txn.Status == "SUCCESS" means settled

// One page of history
var page = await client.Transactions.HistoryAsync(new HistoryFilter
{
    Page = 0, Size = 20, Status = "SUCCESS",
    From = "2026-05-01T00:00:00Z", To = "2026-05-24T00:00:00Z",
});

// Or stream every matching transaction across all pages (built for reconciliation)
await foreach (var t in client.Transactions.HistoryAllAsync(new HistoryFilter { Status = "SUCCESS" }))
{
    // process t, the SDK walks the pages for you lazily
}
```

## Required fields are compile time

Input records use C# `required` members, so leaving out a mandatory field is a build error, not a runtime surprise. Conditional and format rules that the type system cannot express (BankCode when not WAYABANK, an 11 digit BVN, ExpiryDate when LinkCanExpire is true) are validated locally and throw **before** any network call, so a bad input never burns a request.

## References

In v2, the unique reference you supply is your dedup and reconciliation key. Generate a fresh one per logical operation so retries map to the original record instead of spawning duplicates. The library auto fills it on payouts and dynamic accounts when you leave it null, or generate your own:

```csharp
var reference = WayaPayClient.GenerateReference("PAYOUT"); // PAYOUT-1748160000000-A1B2C3D4
```

## Errors

Everything that fails throws a `WayaPayException`. Branch on `Type` for category and `ErrorCode` for the WayaPay code.

```csharp
try
{
    await client.Payouts.InitiateAsync(input);
}
catch (WayaPayException e)
{
    e.Type;       // WayaPayErrorType.Api | Validation | Network | Timeout | Config
    e.ErrorCode;  // WayaPay code, e.g. "07". null when not an API error.
    e.Status;     // HTTP status when known
    e.Message;    // human readable
    e.Raw;        // raw body or underlying error, for your logs
}
```

## Timeouts and retries

Set on the options:

```csharp
new WayaPayOptions
{
    MerchantId = "...", SecretKey = "...",
    TimeoutMs = 30_000,
    MaxRetries = 2,
};
```

Retries apply to **GET only** (bank list, verify, history) and only on timeouts, network errors, 429, or 5xx, with exponential backoff. Writes (payout, collect, dynamic account, BVN) never auto retry, because retrying a write you are unsure about is how you pay someone twice. Retry those yourself, with the same reference, once you have checked the transaction status. Every method also takes a `CancellationToken`.

## Dependency injection and testing

Inject your own `HttpClient` to plug into `IHttpClientFactory`, add handler chains, or swap in a fake for tests. When you supply one it is used as is and never disposed.

```csharp
services.AddSingleton(sp => new WayaPayClient(new WayaPayOptions
{
    MerchantId = config["WayaPay:MerchantId"]!,
    SecretKey  = config["WayaPay:SecretKey"]!,
    Environment = "production",
    HttpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("wayapay"),
}));
```

In unit tests, back that `HttpClient` with a stub `HttpMessageHandler` and assert on the requests without touching the network.

## Before you go live

On the merchant dashboard: finish KYC, grab your Merchant ID, generate your secret key under Settings then API Keys and Webhooks, whitelist your server IPs, and configure payment preferences. Payment Collect will refuse to work until the last two are done.
