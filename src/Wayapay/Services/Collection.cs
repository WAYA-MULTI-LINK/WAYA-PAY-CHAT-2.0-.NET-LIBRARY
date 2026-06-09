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

    /// <summary>
    /// GET /payment-collect/status/{refNo}. Returns the current state of a deposit by its refNo
    /// (the gateway transactionId / webhook OrderId). Use for reconciliation alongside the deposit
    /// webhook — the webhook is the primary signal; this is the pull/safety-net path.
    /// Inspect <see cref="CollectionStatusModel.Status"/> with <see cref="CollectionStatusExtensions"/>.
    /// </summary>
    public async Task<CollectionStatusModel> GetStatusAsync(string refNo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refNo))
            throw new ArgumentException("refNo is required.", nameof(refNo));

        var path = $"/payment-collect/status/{Uri.EscapeDataString(refNo)}";

        return await _client.RequestAsync<CollectionStatusModel>(
                HttpMethod.Get, path, cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Empty response data from {path}");
    }
}
