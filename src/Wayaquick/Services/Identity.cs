using System.Text.RegularExpressions;
using WayaQuick.Models.Identity;

namespace WayaQuick.Services;

public sealed class Identity
{
    private static readonly Regex ElevenDigits = new(@"^\d{11}$", RegexOptions.Compiled);

    private readonly WayaQuickClient _client;

    internal Identity(WayaQuickClient client) => _client = client;

    /// <summary>
    /// POST /identity-verification/bvn/initiate.
    /// Initiates a two-step BVN verification by submitting an 11-digit BVN and dispatching an OTP to the registered phone number.
    /// </summary>
    public async Task<BvnInitiateResponseModel> InitiateBvnAsync(
        BvnInitiateRequestModel input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (string.IsNullOrWhiteSpace(input.Bvn) || !ElevenDigits.IsMatch(input.Bvn))
            throw new ArgumentException("Bvn must be exactly 11 digits.", nameof(input));

        return await _client.RequestAsync<BvnInitiateResponseModel>(
                HttpMethod.Post, "/identity-verification/bvn/initiate", input, cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Empty response data from /identity-verification/bvn/initiate");
    }

    /// <summary>Convenience overload taking the BVN string directly.</summary>
    public Task<BvnInitiateResponseModel> InitiateBvnAsync(
        string bvn,
        CancellationToken cancellationToken = default) =>
        InitiateBvnAsync(new BvnInitiateRequestModel { Bvn = bvn }, cancellationToken);

    /// <summary>
    /// POST /identity-verification/bvn/complete.
    /// Completes the BVN verification using the session reference from InitiateBvnAsync and the OTP received by the user.
    /// </summary>
    public async Task<BvnIdentityResponseModel> CompleteBvnAsync(
        BvnCompleteRequestModel input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (string.IsNullOrWhiteSpace(input.Reference))
            throw new ArgumentException("Reference is required.", nameof(input));
        if (string.IsNullOrWhiteSpace(input.Otp))
            throw new ArgumentException("Otp is required.", nameof(input));

        return await _client.RequestAsync<BvnIdentityResponseModel>(
                HttpMethod.Post, "/identity-verification/bvn/complete", input, cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Empty response data from /identity-verification/bvn/complete");
    }

    /// <summary>Convenience overload taking the reference and OTP strings directly.</summary>
    public Task<BvnIdentityResponseModel> CompleteBvnAsync(
        string reference,
        string otp,
        CancellationToken cancellationToken = default) =>
        CompleteBvnAsync(new BvnCompleteRequestModel { Reference = reference, Otp = otp }, cancellationToken);

    /// <summary>
    /// POST /identity-verification/bvn. BVN is validated locally as exactly 11 digits before the request is sent.
    /// </summary>
    [Obsolete("WayaQuick BVN verification requires a two-step OTP flow. Use InitiateBvnAsync followed by CompleteBvnAsync instead.")]
    public async Task<BvnIdentityResponseModel> VerifyBvnAsync(
        BvnIdentityRequestModel input,
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

    /// <summary>Convenience overload for legacy VerifyBvnAsync.</summary>
    [Obsolete("WayaQuick BVN verification requires a two-step OTP flow. Use InitiateBvnAsync followed by CompleteBvnAsync instead.")]
    public Task<BvnIdentityResponseModel> VerifyBvnAsync(
        string bvn,
        CancellationToken cancellationToken = default) =>
        VerifyBvnAsync(new BvnIdentityRequestModel { Bvn = bvn }, cancellationToken);
}
