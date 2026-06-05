# WayaQuick (.NET)

.NET client for the **WayaQuick Merchant API v2**. Collect customer payments, send payouts, verify bank accounts, and run BVN identity checks in Nigeria. Targets `net8.0`, depends on nothing outside the framework.

This is a **server-side** library. Your secret key lives here and only here. Never ship it to a browser, a mobile app, or a public repo.

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

## Base URL

```
https://services.wayapay.ng/merchant-middleware
```

All paths in this reference are relative to the base URL above.

## Authentication

Every request carries two headers:

| Header | Value |
|---|---|
| `Authorization` | `Bearer {your_api_secret_key}` |
| `X-Merchant-Id` | Your Merchant ID (`MER_...`) |

The `_TEST_` / `_PROD_` prefix on your secret key selects the environment automatically. A missing or invalid key returns code `01`.

## Quickstart

```csharp
using WayaPay;

var client = new WayaPayClient(new WayaPayOptions
{
    MerchantId = "MER_...",           // from the dashboard
    SecretKey  = "WAYASECK_TEST_...", // WAYASECK_PROD_... on live
});

var banks = await client.Banks.ListAsync();
```

## Response envelope

Every method returns the envelope's `data`, already deserialized into a typed record. Failures throw, so the happy path stays clean:

```csharp
var acct = await client.Accounts.VerifyAsync(new VerifyAccountInput
{
    AccountNumber = "0123456789",
    BankCode      = "044",
    EnquiryType   = "OTHERS",
});
Console.WriteLine(acct.AccountName); // typed, with IDE autocomplete
```

The raw envelope looks like this:

```json
{ "success": true, "code": "00", "message": "...", "data": { }, "timestamp": "..." }
```

`code "00"` (and `success: true`) means success. On any error `success` is `false`, `data` is `null`, and `message` describes the problem.

## API

### Banks

```csharp
List<Bank> banks = await client.Banks.ListAsync();
```

Returns all supported banks and their CBN codes. Use these codes for payouts and account verification.

### Accounts

```csharp
// Resolve an account number to its registered name
// Always call this before initiating a payout
var result = await client.Accounts.VerifyAsync(new VerifyAccountInput
{
    AccountNumber = "0123456789",
    BankCode      = "044",       // required when EnquiryType is "OTHERS"
    EnquiryType   = "OTHERS",    // "WAYA-BANK" for intra-bank, "OTHERS" for inter-bank
});
Console.WriteLine(result.AccountName); // "JOHN ADEKUNLE DOE"
```

### Identity

```csharp
var bvn = await client.Identity.VerifyBvnAsync("22212345678"); // exactly 11 digits
// treat anything other than "False" on bvn.WatchListed with care
// full record includes demographics, enrollment bank, watch-list flag, and a base64 portrait
```

BVN data is sensitive personal information. Store, transmit, and log it only as your data-protection obligations allow.

### Payouts

```csharp
var payout = await client.Payouts.InitiateAsync(new PayoutInput
{
    Amount        = 25000m,
    Currency      = "NGN",
    AccountNumber = "0123456789",
    BankCode      = "044",
    AccountName   = "JOHN ADEKUNLE DOE",  // match the verified name
    Reference     = "PAYOUT-20260604-001", // your unique idempotency key — required
    Narration     = "Payout for order 1234",
});
// PROCESSING means accepted, not settled.
// Confirm via webhook or status check with payout.PayoutReference before treating as delivered.
```

`Reference` is required. Generate a fresh one per operation so retries map to the original record and never spawn duplicates. The library also provides a helper:

```csharp
var reference = WayaPayClient.GenerateReference("PAYOUT"); // PAYOUT-1748160000000-A1B2C3D4
```

### Collect

Starts a payment collection and returns a checkout URL. Redirect the customer to `CheckOutUrl` to complete payment.

```csharp
var collect = await client.Collect.InitiateAsync(new CollectInput
{
    Amount        = "1500.00",            // quoted string
    Currency      = "NGN",
    Email         = "john@example.com",
    TransactionId = "TXN-V2-20260604-01", // your unique reference
    FirstName     = "John",
    LastName      = "Doe",
    Phone         = "08012345678",
    Description   = "Order #1234",
    Meta          = new { orderId = "1234" },
});

// Redirect the customer
Response.Redirect(collect.CheckOutUrl);

// When the customer returns, confirm the result on your server
// before fulfilling the order — do not trust the redirect alone.
```

`merchantId` is taken from `X-Merchant-Id` automatically — do not send it in the body.

## Required fields are compile time

Input records use C# `required` members, so leaving out a mandatory field is a build error, not a runtime surprise. Conditional and format rules that the type system cannot express (e.g. `BankCode` required when `EnquiryType` is `OTHERS`, exactly 11-digit BVN) are validated locally and throw **before** any network call, so a bad input never burns a request.

## Error codes

| Code | Meaning | HTTP Status |
|---|---|---|
| `00` | Success | 200 / 201 |
| `01` | Unauthorized — missing or invalid API key | 401 |
| `13` | Validation error — bad or missing fields | 400, 422 |
| `25` | Not found — account or record not resolved | 404 |
| `57` | Forbidden | 403 |
| `91` | Upstream / provider unavailable | 502, 503, 504 |
| `92` | System busy — try again shortly | 429 |
| `94` | Duplicate — reference already used | 409 |
| `96` | Unexpected system error | 500 |

Everything that fails throws a `WayaPayException`. Branch on `Type` for category and `ErrorCode` for the WayaQuick code:

```csharp
try
{
    await client.Payouts.InitiateAsync(input);
}
catch (WayaPayException e)
{
    e.Type;       // WayaPayErrorType.Api | Validation | Network | Timeout | Config
    e.ErrorCode;  // WayaQuick code, e.g. "13". null when not an API error.
    e.Status;     // HTTP status when known
    e.Message;    // human readable
    e.Raw;        // raw body or underlying error, for your logs
}
```

## Timeouts and retries

```csharp
new WayaPayOptions
{
    MerchantId  = "...",
    SecretKey   = "...",
    TimeoutMs   = 30_000,
    MaxRetries  = 2,
};
```

Retries apply to **GET only** (bank list) and only on timeouts, network errors, `92` (429), or `91` (5xx), with exponential backoff. Writes (payout, collect, BVN) never auto-retry — retrying a write you are unsure about is how you pay someone twice. Retry those yourself, with the same reference, once you have checked the status. Every method also accepts a `CancellationToken`.

## Dependency injection and testing

Inject your own `HttpClient` to plug into `IHttpClientFactory`, add handler chains, or swap in a fake for tests. When you supply one it is used as-is and never disposed.

```csharp
services.AddSingleton(sp => new WayaPayClient(new WayaPayOptions
{
    MerchantId = config["WayaPay:MerchantId"]!,
    SecretKey  = config["WayaPay:SecretKey"]!,
    HttpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("wayaquick"),
}));
```

In unit tests, back that `HttpClient` with a stub `HttpMessageHandler` and assert on the requests without touching the network.

## Before you go live

On the merchant dashboard: finish KYC, grab your Merchant ID, generate your secret key under Settings → API Keys and Webhooks, and whitelist your server IPs. Swap your `WAYASECK_TEST_...` key for `WAYASECK_PROD_...` — the rest of your code stays the same.
