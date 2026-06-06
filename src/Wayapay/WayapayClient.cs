using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WayaPay.Services;

namespace WayaPay;

/// <summary>
/// WayaQuick Merchant API v2 client.
/// Server-side only — your secret key lives here and only here.
/// Never ship it to a browser, a mobile app, or a public repo.
/// </summary>
public sealed class WayaPayClient : IDisposable
{
    private const string BaseUrl = "https://services.wayapay.ng/merchant-middleware/api/v2";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string _merchantId;
    private readonly string _secretKey;
    private readonly TimeSpan _timeout;
    private readonly HttpClient _http;
    private readonly bool _ownsHttp;

    public int MaxRetries { get; }

    public Identity Identity { get; }
    public Payouts Payouts { get; }
    public Collection Collection { get; }

    public WayaPayClient(WayaPayOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrEmpty(options.MerchantId))
            throw new ArgumentException("MerchantId is required.", nameof(options));
        if (string.IsNullOrEmpty(options.SecretKey))
            throw new ArgumentException("SecretKey is required.", nameof(options));

        _merchantId = options.MerchantId;
        _secretKey = options.SecretKey;
        _timeout = TimeSpan.FromMilliseconds(options.TimeoutMs);
        MaxRetries = options.MaxRetries;

        _http = options.HttpClient ?? new HttpClient();
        _ownsHttp = options.HttpClient is null;

        Identity = new Identity(this);
        Payouts = new Payouts(this);
        Collection = new Collection(this);
    }

    /// <summary>Low-level request helper used by all services. Returns the envelope's Data field.</summary>
    public async Task<T?> RequestAsync<T>(
        HttpMethod method,
        string path,
        object? body = null,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, query);
        var retryable = method == HttpMethod.Get;
        var ceiling = retryable ? MaxRetries : 0;
        var attempt = 0;

        while (true)
        {
            using var request = new HttpRequestMessage(method, url);
            request.Headers.TryAddWithoutValidation("X-Merchant-Id", _merchantId);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (body is not null)
            {
                var jsonBody = JsonSerializer.Serialize(body, Json);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_timeout);

            string raw;
            int status;
            bool ok;

            try
            {
                using var response = await _http
                    .SendAsync(request, HttpCompletionOption.ResponseContentRead, timeoutCts.Token)
                    .ConfigureAwait(false);
                status = (int)response.StatusCode;
                ok = response.IsSuccessStatusCode;
                raw = await response.Content.ReadAsStringAsync(timeoutCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                var err = new TimeoutException($"Request to {path} timed out after {_timeout.TotalMilliseconds:N0}ms.");
                if (retryable && attempt < ceiling) { attempt++; await BackoffAsync(attempt, cancellationToken).ConfigureAwait(false); continue; }
                throw err;
            }
            catch (HttpRequestException ex)
            {
                if (retryable && attempt < ceiling) { attempt++; await BackoffAsync(attempt, cancellationToken).ConfigureAwait(false); continue; }
                throw new HttpRequestException($"Network error on {path}: {ex.Message}", ex);
            }

            WayaPayResponse<T>? envelope = null;
            if (!string.IsNullOrEmpty(raw))
            {
                try
                {
                    envelope = JsonSerializer.Deserialize<WayaPayResponse<T>>(raw, Json);
                }
                catch (JsonException)
                {
                    throw new InvalidOperationException($"Non-JSON response from {path} (HTTP {status}): {raw}");
                }
            }

            var failed = !ok || envelope is { Success: false };
            if (failed)
            {
                var transient = status is >= 500 or 429;
                if (retryable && transient && attempt < ceiling)
                {
                    attempt++;
                    await BackoffAsync(attempt, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                var errorMessage = envelope?.Message ?? $"Request to {path} failed with HTTP {status}.";
                throw new HttpRequestException(errorMessage);
            }

            return envelope is null ? default : envelope.Data;
        }
    }

    /// <summary>
    /// Generate a unique reference — your idempotency and reconciliation key.
    /// Use one fresh reference per logical operation; reuse the same one on retries.
    /// </summary>
    public static string GenerateReference(string prefix = "WP")
    {
        var ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var hex = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
        return $"{prefix}-{ms}-{hex}";
    }

    private static string BuildUrl(string path, IReadOnlyDictionary<string, string?>? query)
    {
        var url = BaseUrl + path;
        if (query is null) return url;

        var pairs = query
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}")
            .ToList();

        return pairs.Count == 0 ? url : $"{url}?{string.Join("&", pairs)}";
    }

    private static async Task BackoffAsync(int attempt, CancellationToken ct)
    {
        var baseMs = Math.Min(1000 * (int)Math.Pow(2, attempt - 1), 4000);
        var jitter = Random.Shared.Next(0, 200);
        await Task.Delay(baseMs + jitter, ct).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_ownsHttp) _http.Dispose();
    }
}
