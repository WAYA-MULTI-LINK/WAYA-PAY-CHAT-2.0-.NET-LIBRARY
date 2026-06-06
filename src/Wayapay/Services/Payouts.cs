using WayaPay.Models.Payout;

namespace WayaPay.Services;

public sealed class Payouts
{
    private readonly WayaPayClient _client;

    internal Payouts(WayaPayClient client) => _client = client;

    /// <summary>GET /get-bank-list. Returns all supported banks and their CBN codes.</summary>
    public async Task<List<PayoutBankResponseModel>> ListBanksAsync(CancellationToken cancellationToken = default) =>
        await _client.RequestAsync<List<PayoutBankResponseModel>>(
                HttpMethod.Get, "/get-bank-list", cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? [];

    /// <summary>
    /// POST /verify-account. Resolves an account number to its registered name.
    /// BankCode is required when EnquiryType is "OTHERS". Always call this before initiating a payout.
    /// </summary>
    public async Task<PayoutVerifyResponseModel> VerifyAccountAsync(PayoutVerifyRequestModel input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.EnquiryType == "OTHERS" && string.IsNullOrEmpty(input.BankCode))
            throw new ArgumentException("BankCode is required when EnquiryType is \"OTHERS\".", nameof(input));

        return await _client.RequestAsync<PayoutVerifyResponseModel>(
                HttpMethod.Post, "/verify-account", input, cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Empty response data from /verify-account");
    }

    /// <summary>
    /// POST /payment-payout/initiate. PROCESSING means accepted, not settled.
    /// Confirm via webhook or status check with PayoutReference before treating as delivered.
    /// </summary>
    public async Task<PayoutResponseModel> InitiateAsync(PayoutRequestModel input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        return await _client.RequestAsync<PayoutResponseModel>(
                HttpMethod.Post, "/payment-payout/initiate", input, cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Empty response data from /payment-payout/initiate");
    }
}
