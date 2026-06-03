using System.Text.RegularExpressions;

namespace WayaPay.Resources;

public sealed partial class Identity
{
    private readonly WayaPayClient _client;

    internal Identity(WayaPayClient client) => _client = client;

    /// <summary>POST /identity-verification/bvn. BVN is validated as 11 digits locally.</summary>
    public async Task<BvnResult> VerifyBvnAsync(string bvn, CancellationToken cancellationToken = default)
    {
        if (!ElevenDigits().IsMatch(bvn ?? string.Empty))
            throw new WayaPayException("bvn must be an 11 digit string", type: WayaPayErrorType.Validation);

        return await _client.RequestAsync<BvnResult>(
            HttpMethod.Post, "/identity-verification/bvn", new { bvn }, cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? throw WayaPayClient.EmptyData("/identity-verification/bvn");
    }

    [GeneratedRegex(@"^\d{11}$")]
    private static partial Regex ElevenDigits();
}
