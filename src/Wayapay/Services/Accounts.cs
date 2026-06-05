namespace WayaPay.Services;

public sealed class Accounts
{
    private readonly WayaPayClient _client;

    internal Accounts(WayaPayClient client) => _client = client;

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
    /// POST /account-enquiry/create-dynamic-account.
    /// Auto generates ReferenceId when omitted.
    /// </summary>
    public async Task<DynamicAccount> CreateDynamicAsync(CreateDynamicAccountInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var body = string.IsNullOrEmpty(input.ReferenceId)
            ? input with { ReferenceId = WayaPayClient.GenerateReference("DYN") }
            : input;

        return await _client.RequestAsync<DynamicAccount>(
            HttpMethod.Post, "/account-enquiry/create-dynamic-account", body, cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? throw WayaPayClient.EmptyData("/account-enquiry/create-dynamic-account");
    }
}
