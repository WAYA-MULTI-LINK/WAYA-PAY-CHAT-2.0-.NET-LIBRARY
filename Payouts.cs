namespace WayaPay.Resources;

public sealed class Payouts
{
    private readonly WayaPayClient _client;

    internal Payouts(WayaPayClient client) => _client = client;

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
