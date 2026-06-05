namespace WayaPay.Services;

public sealed class Collect
{
    private readonly WayaPayClient _client;

    internal Collect(WayaPayClient client) => _client = client;

    /// <summary>
    /// POST /payment-collect/initiate. If LinkCanExpire is true, ExpiryDate is required.
    /// </summary>
    public async Task<CollectResult> CreateAsync(CollectInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.LinkCanExpire == true && string.IsNullOrEmpty(input.ExpiryDate))
            throw WayaPayClient.Missing("expiryDate", "payment collect (expiry)");

        return await _client.RequestAsync<CollectResult>(
            HttpMethod.Post, "/payment-collect/initiate", input, cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? throw WayaPayClient.EmptyData("/payment-collect/initiate");
    }
}
