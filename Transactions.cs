using System.Runtime.CompilerServices;

namespace WayaPay.Resources;

public sealed class Transactions
{
    private readonly WayaPayClient _client;

    internal Transactions(WayaPayClient client) => _client = client;

    /// <summary>GET /transaction/verify?reference=. Trust status over assumptions.</summary>
    public async Task<TransactionResult> VerifyAsync(string reference, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(reference))
            throw WayaPayClient.Missing("reference", "transaction verify");

        var query = new Dictionary<string, string?> { ["reference"] = reference };
        return await _client.RequestAsync<TransactionResult>(
            HttpMethod.Get, "/transaction/verify", query: query, cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? throw WayaPayClient.EmptyData("/transaction/verify");
    }

    /// <summary>GET /transaction/history. One page.</summary>
    public async Task<HistoryResult> HistoryAsync(HistoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        filter ??= new HistoryFilter();
        var query = new Dictionary<string, string?>
        {
            ["page"] = filter.Page.ToString(),
            ["size"] = filter.Size.ToString(),
            ["status"] = filter.Status,
            ["from"] = filter.From,
            ["to"] = filter.To,
        };
        return await _client.RequestAsync<HistoryResult>(
            HttpMethod.Get, "/transaction/history", query: query, cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? new HistoryResult();
    }

    /// <summary>
    /// Walk every page of history as one lazy async stream. Built for reconciliation.
    ///
    /// <code>await foreach (var txn in client.Transactions.HistoryAllAsync(new() { Status = "SUCCESS" })) { ... }</code>
    /// </summary>
    public async IAsyncEnumerable<HistoryItem> HistoryAllAsync(
        HistoryFilter? filter = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        filter ??= new HistoryFilter();
        var page = filter.Page;
        var size = filter.Size;

        while (true)
        {
            var data = await HistoryAsync(filter with { Page = page, Size = size }, cancellationToken).ConfigureAwait(false);
            foreach (var item in data.Items)
                yield return item;

            page++;
            if (data.TotalPages <= 0 || page >= data.TotalPages)
                break;
        }
    }
}
