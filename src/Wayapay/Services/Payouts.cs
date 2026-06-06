namespace WayaPay.Services;

public sealed class Payouts
{
    private readonly WayaPayClient _client;

    internal Payouts(WayaPayClient client) => _client = client;

    /// <summary>GET /account-enquiry/get-bank-list</summary>
    public async Task<List<Bank>> ListAsync(CancellationToken cancellationToken = default) =>
        await _client.RequestAsync<List<Bank>>(
                HttpMethod.Get, "/account-enquiry/get-bank-list", cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? new List<Bank>();
    
    /// <summary>
    /// POST /account-enquiry/verify-account.
    /// BankCode is required for OTHERS, optional for WAYABANK.
    /// </summary>
    public async Task<VerifyAccountResult> VerifyAsync(VerifyAccountInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.EnquiryType != "WAYABANK" && string.IsNullOrEmpty(input.BankCode))
            throw WayaPayClient.Missing("bankCode", "account verification (external bank)");

        return await _client.RequestAsync<VerifyAccountResult>(
                HttpMethod.Post, "/account-enquiry/verify-account", input, cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? throw WayaPayClient.EmptyData("/account-enquiry/verify-account");
    }

    /// <summary>
    /// POST /payment-payout/initiate. Auto generates Reference when omitted.
    /// PROCESSING means accepted, not settled. Verify with the reference afterwards.
    /// </summary>
    public async Task<PayoutResult> InitiateAsync(PayoutInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var body = string.IsNullOrEmpty(input.Reference)
            ? input with { Reference = WayaPayClient.GenerateReference("PAYOUT") }
            : input;

        return await _client.RequestAsync<PayoutResult>(
                HttpMethod.Post, "/payment-payout/initiate", body, cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? throw WayaPayClient.EmptyData("/payment-payout/initiate");
    }
}