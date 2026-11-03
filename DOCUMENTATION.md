# WayaQuick .NET SDK Documentation (v2.0.2)

Official integration guide for the **WayaQuick Merchant API v2** using the official .NET SDK.

---

## 1. Package Overview

* **Package Name**: [`WayaQuick.Integration`](https://www.nuget.org/packages/WayaQuick.Integration)
* **Current Version**: `2.0.2`
* **Target Framework**: `.NET 8.0` (`net8.0`)
* **Dependencies**: Zero external dependencies (uses standard `System.Net.Http`, `System.Text.Json`, `System.Security.Cryptography`)
* **Thread Safety**: `WayaQuickClient` is thread-safe and designed to be registered as a singleton.
* **Runnable Example**: [`samples/ConsoleDemo/Program.cs`](samples/ConsoleDemo/Program.cs)

### Installation

```bash
dotnet add package WayaQuick.Integration --version 2.0.2
```

---

## 2. Initialization & Authentication

All API calls require your **Merchant ID** (`MER_...`) and **Secret Key** (`WAYASECK_TEST_...` for test sandbox, `WAYASECK_...` for live production):

```csharp
using WayaQuick;

var client = new WayaQuickClient(new WayaQuickOptions
{
    MerchantId = Environment.GetEnvironmentVariable("WAYA_MERCHANT_ID")!,
    SecretKey  = Environment.GetEnvironmentVariable("WAYA_SECRET_KEY")!,
    // Optional:
    // WebhookSecret = Environment.GetEnvironmentVariable("WAYA_WEBHOOK_SECRET"),
    // TimeoutMs     = 30_000, // default 30s
    // MaxRetries    = 2,      // default 2 (GET only)
});
```

### Dependency Injection (ASP.NET Core)

```csharp
// Program.cs
builder.Services.AddHttpClient("wayaquick");
builder.Services.AddSingleton(sp => new WayaQuickClient(new WayaQuickOptions
{
    MerchantId = builder.Configuration["WayaQuick:MerchantId"]!,
    SecretKey  = builder.Configuration["WayaQuick:SecretKey"]!,
    HttpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("wayaquick"),
}));
```

---

## 3. Core Features

### 3.1. Reference Generation (Idempotency)

Generate a collision-resistant, timestamped reference for payouts and transactions:

```csharp
string reference = WayaQuickClient.GenerateReference("PAYOUT");
// Produces: PAYOUT-1748160000000-A1B2C3D4
```

---

### 3.2. List Banks (`GET /get-bank-list`)

Fetch supported banks with CBN codes:

```csharp
using WayaQuick.Models.Payout;

List<PayoutBankResponseModel> banks = await client.Payouts.ListBanksAsync();
foreach (var bank in banks)
{
    Console.WriteLine($"{bank.Name} ({bank.Code})");
}
```

---

### 3.3. Account Verification / Name Enquiry (`POST /verify-account`)

Validate the bank account number and retrieve the registered account name:

```csharp
// Inter-bank enquiry (requires BankCode):
var verified = await client.Payouts.VerifyAccountAsync(new PayoutVerifyRequestModel
{
    AccountNumber = "0123456789",
    BankCode      = "044",      // Access Bank
    EnquiryType   = "OTHERS",   // Default: OTHERS
});
Console.WriteLine($"Resolved Account Name: {verified.AccountName}");

// Intra-bank enquiry (Waya Bank to Waya Bank):
var wayaAccount = await client.Payouts.VerifyAccountAsync(new PayoutVerifyRequestModel
{
    AccountNumber = "0123456789",
    EnquiryType   = "WAYA-BANK", // BankCode is not required
});
```

---

### 3.4. BVN Identity Verification (Two-Step OTP Flow)

In compliance with data privacy regulations, BVN verification uses a 2-step OTP flow:

```csharp
// Step 1: Initiate verification (sends SMS OTP to customer's BVN phone number)
var session = await client.Identity.InitiateBvnAsync("22222222222"); // exactly 11 digits
Console.WriteLine($"OTP sent to {session.PhoneNumber}. Reference: {session.Reference}");

// Step 2: Complete verification with the received OTP
var profile = await client.Identity.CompleteBvnAsync(session.Reference, "123456");
Console.WriteLine($"Verified: {profile.FirstName} {profile.LastName}");
Console.WriteLine($"Watchlisted: {profile.WatchListed}");
```

---

### 3.5. Disburse Payouts (`POST /payment-payout/initiate`)

Send payouts from your merchant balance to any bank account:

```csharp
using WayaQuick.Models.Payout;

// 1. Submit payout request
var payout = await client.Payouts.InitiateAsync(new PayoutRequestModel
{
    Amount        = 25000.00m,
    Currency      = "NGN",
    AccountNumber = "0123456789",
    BankCode      = "044",
    AccountName   = "JOHN DOE",
    Reference     = WayaQuickClient.GenerateReference("PAYOUT"),
    Narration     = "Salary Disbursement",
});

Console.WriteLine($"Payout submitted. Status: {payout.Status}"); // "PROCESSING"

// 2. Reconcile payout status using reference:
var status = await client.Payouts.GetStatusAsync(payout.MerchantReference);
switch (status.ParsedStatus().Outcome())
{
    case PayoutOutcome.Succeeded:
        Console.WriteLine("Funds delivered.");
        break;
    case PayoutOutcome.Reversed:
        Console.WriteLine("Transfer failed; wallet re-credited.");
        break;
    case PayoutOutcome.Reconciling:
        Console.WriteLine("Transfer in flight; check again later.");
        break;
}
```

---

### 3.6. Payment Collections / Hosted Checkout (`POST /payment-collect/initiate`)

Collect customer payments via WayaQuick's hosted checkout page:

```csharp
using WayaQuick.Models.collection;

// 1. Create Checkout Session
var checkout = await client.Collection.InitiateAsync(new CollectionRequestModel
{
    TransactionId = WayaQuickClient.GenerateReference("TXN"),
    Amount        = "1500.00",
    Currency      = "NGN",
    FirstName     = "John",
    LastName      = "Doe",
    Email         = "customer@example.com",
    Phone         = "08012345678",
    Description   = "Order #1001",
});

Console.WriteLine($"Redirect customer to: {checkout.CheckOutUrl}");

// 2. Reconcile Status by TransactionId
var collectStatus = await client.Collection.GetStatusAsync(checkout.TransactionId);
if (collectStatus.ParsedStatus() == CollectionStatus.Successful)
{
    Console.WriteLine($"Funds confirmed! Amount received: {collectStatus.AmountPaid}. Fulfil order.");
}
```

---

### 3.7. Inbound Webhook Verification

```csharp
using Microsoft.AspNetCore.Mvc;
using WayaQuick.Models.Webhook;

[ApiController]
[Route("api/[controller]")]
public class WayaWebhookController : ControllerBase
{
    private readonly WayaQuickClient _client;

    public WayaWebhookController(WayaQuickClient client)
    {
        _client = client;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        // Read the exact raw body string
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();

        var timestamp = Request.Headers["x-waya-timestamp"].ToString();
        var signature = Request.Headers["x-waya-signature"].ToString();

        try
        {
            var evt = _client.Webhooks.ConstructEvent(
                rawBody,
                timestamp,
                signature,
                Environment.GetEnvironmentVariable("WAYA_SECRET_KEY")!
            );

            if (evt.ShouldFulfil())
            {
                // Fulfil order keyed by evt.OrderId
                Console.WriteLine($"Payment successful for order: {evt.OrderId}");
            }

            return Ok();
        }
        catch (WayaQuickWebhookException)
        {
            return Unauthorized();
        }
    }
}
```

---

## 4. Error Handling

API failures throw `HttpRequestException` (or SDK validation exceptions like `ArgumentException`):

```csharp
try
{
    var payout = await client.Payouts.InitiateAsync(request);
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Network / API Error: {ex.Message} (Status: {ex.StatusCode})");
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Client validation error: {ex.Message}");
}
```

---

## 5. Runnable Example Document

The SDK includes a complete console demo:

* **File Location**: [`samples/ConsoleDemo/Program.cs`](samples/ConsoleDemo/Program.cs)
* **Run Command**:
  ```bash
  WAYA_MERCHANT_ID=MER_... WAYA_SECRET_KEY=WAYASECK_TEST_... \
    dotnet run --project samples/ConsoleDemo
  ```

---

## 6. Parseable AI Text (Copy & Paste for LLMs)

Copy the prompt block below to give any AI coding tool complete knowledge of how to use this library:

```xml
<wayaquick_dotnet_sdk_context version="2.0.2">
  <system_role>
    You are an expert .NET C# backend engineer implementing the official WayaQuick .NET SDK (`WayaQuick.Integration`).
    Target .NET 8.0. Adhere strictly to the v2 API contract and method signatures below.
  </system_role>

  <rules>
    1. IMMUTABLE CLIENT: Register `WayaQuickClient` as a singleton in `IServiceCollection`.
    2. BVN 2-STEP OTP: BVN verification requires `await client.Identity.InitiateBvnAsync(bvn)` (sends OTP) followed by `await client.Identity.CompleteBvnAsync(reference, otp)`. Single-step verification is deprecated.
    3. ACCOUNT ENQUIRY: Use `await client.Payouts.VerifyAccountAsync(new PayoutVerifyRequestModel { AccountNumber = ..., BankCode = ..., EnquiryType = ... })`. If `EnquiryType == "WAYA-BANK"`, `BankCode` is not required.
    4. HOSTED CHECKOUT: Use `await client.Collection.InitiateAsync(new CollectionRequestModel { Amount = ..., Currency = ..., TransactionId = ..., ... })`. Redirect customer to `checkout.CheckOutUrl`.
    5. PAYOUTS ARE ASYNC: `await client.Payouts.InitiateAsync(...)` returns status `PROCESSING`. Reconcile by polling `await client.Payouts.GetStatusAsync(ref)` or via webhooks.
    6. WEBHOOK RAW BODY: `client.Webhooks.ConstructEvent(rawBody, timestamp, signature, secret)`. Compute over raw body string before model binding / JSON deserialization.
  </rules>

  <package_info>
    <package_name>WayaQuick.Integration</package_name>
    <version>2.0.2</version>
    <install>dotnet add package WayaQuick.Integration --version 2.0.2</install>
    <sample_file>samples/ConsoleDemo/Program.cs</sample_file>
  </package_info>

  <code_template>
    <![CDATA[
using WayaQuick;
using WayaQuick.Models.Payout;
using WayaQuick.Models.collection;
using WayaQuick.Models.Identity;

var client = new WayaQuickClient(new WayaQuickOptions
{
    MerchantId = Environment.GetEnvironmentVariable("WAYA_MERCHANT_ID")!,
    SecretKey  = Environment.GetEnvironmentVariable("WAYA_SECRET_KEY")!,
});

// 1. List Banks
var banks = await client.Payouts.ListBanksAsync();

// 2. Verify Account
var verified = await client.Payouts.VerifyAccountAsync(new PayoutVerifyRequestModel
{
    AccountNumber = "0123456789",
    BankCode      = "044",
    EnquiryType   = "OTHERS",
});

// 3. BVN 2-Step OTP
var bvnSession = await client.Identity.InitiateBvnAsync("22222222222");
var bvnProfile = await client.Identity.CompleteBvnAsync(bvnSession.Reference, "123456");

// 4. Initiate Payout
var payout = await client.Payouts.InitiateAsync(new PayoutRequestModel
{
    Amount        = 5000.00m,
    Currency      = "NGN",
    AccountNumber = verified.AccountNumber,
    BankCode      = "044",
    AccountName   = verified.AccountName,
    Reference     = WayaQuickClient.GenerateReference("PAYOUT"),
    Narration     = "Transfer payment",
});
var payoutStatus = await client.Payouts.GetStatusAsync(payout.MerchantReference);

// 5. Payment Collection
var checkout = await client.Collection.InitiateAsync(new CollectionRequestModel
{
    TransactionId = WayaQuickClient.GenerateReference("TXN"),
    Amount        = "1500.00",
    Currency      = "NGN",
    FirstName     = "John",
    LastName      = "Doe",
    Email         = "customer@example.com",
    Phone         = "08012345678",
    Description   = "Order #1001",
});
// Redirect customer to: checkout.CheckOutUrl
var collectStatus = await client.Collection.GetStatusAsync(checkout.TransactionId);
    ]]>
  </code_template>
</wayaquick_dotnet_sdk_context>
```

