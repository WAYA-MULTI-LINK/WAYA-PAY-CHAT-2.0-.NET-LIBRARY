using System.Text.RegularExpressions;
using WayaPay.Models.Identity;

namespace WayaPay.Services;

public sealed class Identity
{
    private static readonly Regex ElevenDigits = new(@"^\d{11}$", RegexOptions.Compiled);

    private readonly WayaPayClient _client;

    internal Identity(WayaPayClient client) => _client = client;

    /// <summary>POST /identity-verification/bvn. BVN is validated locally as exactly 11 digits before the request is sent.</summary>
    public async Task<BvnIdentityResponseModel> VerifyBvnAsync(BvnIdentityRequestModel input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!ElevenDigits.IsMatch(input.Bvn))
            throw new ArgumentException("Bvn must be exactly 11 digits.", nameof(input));

        return await _client.RequestAsync<BvnIdentityResponseModel>(
                HttpMethod.Post, "/identity-verification/bvn", input, cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Empty response data from /identity-verification/bvn");
    }
}
