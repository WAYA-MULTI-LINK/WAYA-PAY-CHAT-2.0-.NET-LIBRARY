using WayaPay.Models.collection;

namespace WayaPay.Services;

public sealed class Collection
{
    private readonly WayaPayClient _client;

    internal Collection(WayaPayClient client) => _client = client;

    /// <summary>
    /// POST /payment-collect/initiate. If LinkCanExpire is true, ExpiryDate is required.
    /// </summary>
    public async Task<CollectionResponseModel> CreateAsync(CollectionRequestModel input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.LinkCanExpire == true && string.IsNullOrEmpty(input.ExpiryDate))
            throw WayaPayClient.Missing("expiryDate", "payment collect (expiry)");

        return await _client.RequestAsync<CollectResult>(
            HttpMethod.Post, "/payment-collect/initiate", input, cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? throw WayaPayClient.EmptyData("/payment-collect/initiate");
    }
}
