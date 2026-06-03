namespace WayaPay.Resources;

public sealed class Banks
{
    private readonly WayaPayClient _client;

    internal Banks(WayaPayClient client) => _client = client;

    /// <summary>GET /account-enquiry/get-bank-list</summary>
    public async Task<List<Bank>> ListAsync(CancellationToken cancellationToken = default) =>
        await _client.RequestAsync<List<Bank>>(
            HttpMethod.Get, "/account-enquiry/get-bank-list", cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? new List<Bank>();
}
