using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WayaPay.DotNetSdk.Models;

namespace WayaPay.DotNetSdk;

public class WayaPayClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public WayaPayClient(WayaPayOptions options, HttpClient? httpClient = null)
    {
        if (string.IsNullOrWhiteSpace(options.MerchantId))
            throw new ArgumentException("MerchantId is required");

        if (string.IsNullOrWhiteSpace(options.PublicKey))
            throw new ArgumentException("PublicKey is required");

        if (string.IsNullOrWhiteSpace(options.Environment))
            throw new ArgumentException("Environment is required");

        var isProd = options.Environment.Trim().ToLower() is "production" or "prod";

        _baseUrl = isProd
            ? "https://services.wayapay.ng"
            : "https://services.staging.wayapay.ng";

        _httpClient = httpClient ?? new HttpClient();

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Merchant-ID", options.MerchantId);
        _httpClient.DefaultRequestHeaders.Add("API-Secret-Key", options.PublicKey);
    }

    public Task<Dictionary<string, object>?> InitializePaymentAsync(
        InitializePaymentRequest request
    )
    {
        if (string.IsNullOrWhiteSpace(request.Currency))
            return ValidationError("currency is required");

        if (request.Amount <= 0)
            return ValidationError("amount is required");

        if (string.IsNullOrWhiteSpace(request.CallBackUrl))
            return ValidationError("callBackUrl is required");

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            return ValidationError("idempotencyKey is required");

        if (string.IsNullOrWhiteSpace(request.PaymentRef))
            return ValidationError("paymentRef is required");

        if (request.Metadata == null)
            return ValidationError("metadata is required");

        if (string.IsNullOrWhiteSpace(request.Metadata.FirstName))
            return ValidationError("metadata.firstName is required");

        if (string.IsNullOrWhiteSpace(request.Metadata.LastName))
            return ValidationError("metadata.lastName is required");

        if (string.IsNullOrWhiteSpace(request.Metadata.PhoneNumber))
            return ValidationError("metadata.phoneNumber is required");

        if (string.IsNullOrWhiteSpace(request.Metadata.EmailAddress))
            return ValidationError("metadata.emailAddress is required");

        return SendAsync(HttpMethod.Post, "/payment-collect/initiate", request);
    }

    public Task<Dictionary<string, object>?> InitiatePayoutAsync(
        InitiatePayoutRequest request
    )
    {
        if (string.IsNullOrWhiteSpace(request.Currency))
            return ValidationError("currency is required");

        if (request.Amount <= 0)
            return ValidationError("amount is required");

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            return ValidationError("idempotencyKey is required");

        if (string.IsNullOrWhiteSpace(request.BankCode))
            return ValidationError("bankCode is required");

        if (string.IsNullOrWhiteSpace(request.AccountNumber))
            return ValidationError("accountNumber is required");

        return SendAsync(HttpMethod.Post, "/payment-payout/initiate", request);
    }

    public Task<Dictionary<string, object>?> VerifyTransactionAsync(
        string transactionRef
    )
    {
        if (string.IsNullOrWhiteSpace(transactionRef))
            return ValidationError("transactionRef is required");

        var endpoint = $"/payment/transaction?ref={Uri.EscapeDataString(transactionRef)}";

        return SendAsync(HttpMethod.Get, endpoint);
    }

    public Task<Dictionary<string, object>?> FetchBankListAsync()
    {
        return SendAsync(HttpMethod.Get, "/banks-list");
    }

    public Task<Dictionary<string, object>?> VerifyAccountAsync(
        VerifyAccountRequest request
    )
    {
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
            return ValidationError("accountNumber is required");

        if (string.IsNullOrWhiteSpace(request.BankCode))
            return ValidationError("bankCode is required");

        return SendAsync(HttpMethod.Get, "/account-verification", request);
    }

    private async Task<Dictionary<string, object>?> SendAsync(
        HttpMethod method,
        string endpoint,
        object? body = null
    )
    {
        var request = new HttpRequestMessage(method, _baseUrl + endpoint);

        request.Headers.Add("Accept", "application/json");

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );
        }

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
        {
            return new Dictionary<string, object>
            {
                ["status"] = response.IsSuccessStatusCode,
                ["message"] = response.IsSuccessStatusCode
                    ? "Request completed successfully"
                    : "Request failed",
                ["code"] = (int)response.StatusCode
            };
        }

        return JsonSerializer.Deserialize<Dictionary<string, object>>(
            content,
            JsonOptions
        );
    }

    private static Task<Dictionary<string, object>?> ValidationError(string message)
    {
        return Task.FromResult<Dictionary<string, object>?>(
            new Dictionary<string, object>
            {
                ["status"] = false,
                ["message"] = message
            }
        );
    }
}