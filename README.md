# WayaPay .NET SDK

Official .NET SDK for the [WayaPay](https://wayapay.ng) payment gateway. Accept payments, initiate payouts, verify accounts, and more — all in a few lines of C#.

---

## Installation

```bash
dotnet add package Wayapay
```

Or via the NuGet Package Manager in Visual Studio — search for `Wayapay`.

---

## Requirements

- .NET Standard 2.1 or higher
- .NET 6 / 7 / 8 (fully supported)

---

## Quick Start

```csharp
using WayaPay.DotNetSdk;
using WayaPay.DotNetSdk.Models;

var client = new WayaPayClient(new WayaPayOptions
{
    MerchantId  = "your-merchant-id",
    PublicKey   = "your-api-secret-key",
    Environment = "test" // or "production"
});
```

---

## Usage

### Initialize Payment

Starts a payment collection and returns a transaction ID you redirect your customer to.

```csharp
var response = await client.InitializePaymentAsync(new InitializePaymentRequest
{
    Currency       = "NGN",
    Amount         = 5000,
    CallBackUrl    = "https://yoursite.com/callback",
    IdempotencyKey = Guid.NewGuid().ToString(),
    PaymentRef     = "ORDER-001",
    Metadata = new PaymentMetadata
    {
        FirstName    = "John",
        LastName     = "Doe",
        PhoneNumber  = "08012345678",
        EmailAddress = "john@example.com",
        CancelUrl    = "https://yoursite.com/cancel"
    }
});
```

---

### Initiate Payout

Sends funds from your merchant balance to a bank account. Always verify the account first.

```csharp
var response = await client.InitiatePayoutAsync(new InitiatePayoutRequest
{
    Currency       = "NGN",
    Amount         = 10000,
    IdempotencyKey = Guid.NewGuid().ToString(),
    BankCode       = "044",
    AccountNumber  = "0123456789"
});
```

---

### Verify Transaction

Retrieves the current status of a transaction by its reference.

```csharp
var response = await client.VerifyTransactionAsync("your-transaction-ref");
```

---

### Fetch Bank List

Returns all supported banks and their codes. Use these codes for payouts and account verification.

```csharp
var response = await client.FetchBankListAsync();
```

---

### Verify Account

Resolves an account number and returns the registered account name. Always call this before initiating a payout.

```csharp
var response = await client.VerifyAccountAsync(new VerifyAccountRequest
{
    AccountNumber = "0123456789",
    BankCode      = "044"
});
```

---

## Environments

| Value | Description |
|---|---|
| `test` | Staging environment for development |
| `production` or `prod` | Live environment for real transactions |

---

## Error Handling

Every method returns a `Dictionary<string, object>` response. Always check the `status` field before proceeding.

```csharp
var response = await client.VerifyTransactionAsync("ref-001");

if (response != null && response["status"] is true)
{
    // success — use response["data"]
}
else
{
    // failed — check response["message"]
    Console.WriteLine(response?["message"]);
}
```

---

## Using a Custom HttpClient

You can inject your own `HttpClient` instance — useful for testing or when you need custom timeout and retry policies.

```csharp
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};

var client = new WayaPayClient(new WayaPayOptions
{
    MerchantId  = "your-merchant-id",
    PublicKey   = "your-api-secret-key",
    Environment = "test"
}, httpClient);
```

---

## License

MIT