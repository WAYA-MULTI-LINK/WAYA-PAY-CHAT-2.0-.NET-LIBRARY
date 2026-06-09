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

## Check payout status

Reconcile a payout by the reference you sent at initiation.

```csharp
var payout = await client.Payouts.GetStatusAsync("PAYOUT-20260604-001");

switch (payout.ParsedStatus().Outcome())
{
    case PayoutOutcome.Succeeded:   /* funds delivered */            break;
    case PayoutOutcome.Reversed:    /* failed — wallet re-credited */ break;
    case PayoutOutcome.Reconciling: /* PENDING — check again later */ break;
}
```

| `Status`   | Terminal | Meaning |
|------------|----------|---------|
| `PENDING`  | no  | Submitted; terminal outcome not yet recorded (reconciling). |
| `SUCCESS`  | yes | Completed successfully. |
| `REVERSED` | yes | Failed/reversed — the merchant wallet was re-credited. |

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

## Check collection (deposit) status

The deposit webhook is the primary signal; this endpoint is the pull/safety-net path for reconciliation. Look it up by `refNo` (the gateway `transactionId` / webhook `OrderId`).

```csharp
var deposit = await client.Collection.GetStatusAsync("1779662251460508970");

if (deposit.ParsedStatus() == CollectionStatus.Successful)
{
    // Funds confirmed — fulfil. Use deposit.RefNo as the idempotency key.
}
else if (!deposit.ParsedStatus().IsTerminal())
{
    // Still in flight — keep polling; don't refund or retry.
}
```

`Amount` is the expected amount; `AmountPaid` is what was actually received — it can be smaller (`PARTIAL` underpayment) or larger (overpayment). Use `Status` + `AmountPaid` as authoritative.

| `Status` | Terminal | Outcome | Meaning |
|----------|----------|---------|---------|
| `INITIATED` / `PENDING` / `PROCESSING` / `APPROVED` | no | `InFlight` | In flight — keep polling; don't refund or retry. |
| `PARTIAL` | no | `InFlight` | Customer underpaid into a virtual account. |
| `SUCCESSFUL` | yes | `Succeeded` | Funds confirmed — fulfil (use `RefNo` for idempotency). |
| `REFUNDED` | yes | `Refunded` | Previously-successful transaction refunded. |
| `FAILED` / `DECLINED` / `REJECTED` / `ABANDONED` / `EXPIRED` / `CANCELLED` / `CUSTOMER_ERROR` / `FRAUD_ERROR` | yes | `NotDebited` | Customer not debited — no fulfilment. |
| `TIMEOUT` / `ERROR` / `SYSTEM_ERROR` / `BANK_ERROR` | yes | `Indeterminate` | Outcome unknown — reconcile, don't refund unilaterally. |

A reference that doesn't belong to the authenticated merchant returns `404` (surfaced as `HttpRequestException`).

## Process webhooks

WayaPay POSTs your server whenever a transaction becomes `SUCCESSFUL`, `PARTIAL`, or `FAILED`, so you can fulfil orders in real time instead of polling. **Verify every webhook before acting on it** — `ConstructEvent` checks the HMAC-SHA256 signature and the replay window, and throws `WayaPayWebhookException` on anything it can't trust.

The signature is computed over the **exact raw request bytes**. Capture the body before any JSON middleware re-serialises it, or the recomputed HMAC won't match.

```csharp
using WayaPay;
using WayaPay.Models.Webhook;

app.MapPost("/waya/webhook", async (HttpRequest request) =>
{
    // Read the RAW body — do not let model binding touch it first.
    using var reader = new StreamReader(request.Body);
    var rawBody = await reader.ReadToEndAsync();

    WebhookEvent evt;
    try
    {
        evt = WayaPayWebhook.ConstructEvent(
            payload:   rawBody,
            timestamp: request.Headers[WayaPayWebhook.TimestampHeader],
            signature: request.Headers[WayaPayWebhook.SignatureHeader],
            secret:    webhookSecret); // merchantSecretTestKey or merchantProductionSecretKey
    }
    catch (WayaPayWebhookException)
    {
        return Results.Unauthorized(); // unsigned / forged / stale — reject
    }

    // Acknowledge fast (within ~10s), then queue the real work. OrderId is your idempotency key.
    switch (evt.ParsedStatus())
    {
        case WebhookStatus.Successful: /* upsert by evt.OrderId, then fulfil */ break;
        case WebhookStatus.Partial:    /* hold; query status by OrderId for amount paid */ break;
        case WebhookStatus.Failed:     /* no fulfilment */ break;
    }

    return Results.Ok();
});
```

| `Status` | `WebhookStatus` | What to do |
|----------|-----------------|------------|
| `SUCCESSFUL` | `Successful` | Fulfil the order. Check `OrderId` for idempotency. |
| `PARTIAL`    | `Partial`    | Hold fulfilment — query the status endpoint by `OrderId` for the latest amount paid. |
| `FAILED`     | `Failed`     | No fulfilment. |

Notes:
- The merchant secret is your `merchantSecretTestKey` (TEST) or `merchantProductionSecretKey` (PRODUCTION). Keep one verifier per environment and route by which key validates.
- The same `OrderId` may fire more than once (a `PARTIAL` then a `SUCCESSFUL`, or a re-emitted `SUCCESSFUL`). Always **upsert** keyed by `OrderId`; never blindly insert.
- Replay protection rejects timestamps outside a 5-minute window by default. Override via the `tolerance` parameter (pass `Timeout.InfiniteTimeSpan` to disable — not recommended).
- Delivery is fire-and-forget: respond `200` quickly, do heavy work off-thread, and reconcile periodically with the status endpoint.

For a signature-only check (no replay window), use `WayaPayWebhook.VerifySignature(...)`, which returns a `bool`.

### Via the client

If you set `WebhookSecret` on `WayaPayOptions`, the same calls are available on the client without passing the secret each time:

```csharp
var client = new WayaPayClient(new WayaPayOptions
{
    MerchantId    = "MER_...",
    SecretKey     = "WAYASECK_TEST_...",
    WebhookSecret = "your-merchant-webhook-secret",
});

var evt = client.Webhooks.ConstructEvent(rawBody, timestamp, signature);
```

`client.Webhooks.ConstructEvent` / `VerifySignature` also have overloads that take an explicit `secret`, so a single endpoint can route TEST vs PRODUCTION by trying each key.

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
    MerchantId    = "MER_...",
    SecretKey     = "WAYASECK_...",
    WebhookSecret = "...",     // optional: enables client.Webhooks without passing a secret
    TimeoutMs     = 30_000,    // default: 30 s
    MaxRetries    = 2,         // default: 2 — GET only, exponential backoff
    HttpClient    = ...,       // optional: inject your own (DI, handler chains, test fakes)
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

See [samples/ConsoleDemo/Program.cs](samples/ConsoleDemo/Program.cs) for a runnable end-to-end demo covering banks, account verification, BVN, payouts, collections, status checks, and webhook verification.

```bash
WAYA_MERCHANT_ID=MER_... WAYA_SECRET_KEY=WAYASECK_TEST_... dotnet run --project samples/ConsoleDemo
```

## Going live

On the merchant dashboard: finish KYC, grab your Merchant ID, generate your secret key under **Settings → API Keys and Webhooks**, and whitelist your server IPs. Swap `WAYASECK_TEST_...` for `WAYASECK_...` — the rest of your code stays the same.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

MIT
