using WayaPay.Models.collection;

namespace WayaPay.Services;

public sealed class Collection
{
    private readonly WayaPayClient _client;

    internal Collection(WayaPayClient client) => _client = client;

    /// <summary>POST /payment-collect/initiate. Returns a checkout URL to redirect the customer to.</summary>
    public async Task<CollectionResponseModel> InitiateAsync(CollectionRequestModel input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        return await _client.RequestAsync<CollectionResponseModel>(
                HttpMethod.Post, "/payment-collect/initiate", input, cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Empty response data from /payment-collect/initiate");
    }
}
