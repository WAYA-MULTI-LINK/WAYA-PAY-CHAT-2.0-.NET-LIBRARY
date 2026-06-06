using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WayaPay.Services;

namespace WayaPay;

/// <summary>
/// WayaPay Merchant API v2 client.
///
/// Server side only. Your secret key lives here and only here.
/// Never ship it to a browser, a mobile app, or a public repo.
/// </summary>
public sealed class WayaPayClient : IDisposable
{
    private static readonly Dictionary<string, string> Environments = new()
    {
        ["production"] = "https://services.wayapay.ng/merchant-middleware/api/v2",
    };

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string _merchantId;
    private readonly string _secretKey;
    private readonly TimeSpan _timeout;
    private readonly HttpClient _http;
    private readonly bool _ownsHttp;

    public string BaseUrl { get; }
    public int MaxRetries { get; }

    public Banks Banks { get; }
    public Accounts Accounts { get; }
    public Identity Identity { get; }
    public Payouts Payouts { get; }
    public Collection Collection { get; }
    public Transactions Transactions { get; }

    public WayaPayClient(WayaPayOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrEmpty(options.MerchantId))
            throw new WayaPayException("MerchantId is required", type: WayaPayErrorType.Config);
        if (string.IsNullOrEmpty(options.SecretKey))
            throw new WayaPayException("SecretKey is required", type: WayaPayErrorType.Config);

        _merchantId = options.MerchantId;
        _secretKey = options.SecretKey;
        _timeout = TimeSpan.FromMilliseconds(options.TimeoutMs);
        MaxRetries = options.MaxRetries;

        var baseUrl = options.BaseUrl
            ?? (Environments.TryGetValue(options.Environment, out var env) ? env : Environments["production"]);
        BaseUrl = baseUrl.TrimEnd('/');

        _http = options.HttpClient ?? new HttpClient();
        _ownsHttp = options.HttpClient is null;

        Banks = new Banks(this);
        Accounts = new Accounts(this);
        Identity = new Identity(this);
        Payouts = new Payouts(this);
        Collection = new Collection(this);
        Transactions = new Transactions(this);
    }

    /// <summary>Low level request. Resources call this. Returns the envelope's Data.</summary>
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
                var err = new WayaPayException(
                    $"Request timed out after {_timeout.TotalMilliseconds:N0}ms",
                    type: WayaPayErrorType.Timeout);
                if (retryable && attempt < ceiling) { attempt++; await BackoffAsync(attempt, cancellationToken).ConfigureAwait(false); continue; }
                throw err;
            }
            catch (HttpRequestException ex)
            {
                var err = new WayaPayException(ex.Message, type: WayaPayErrorType.Network, raw: ex);
                if (retryable && attempt < ceiling) { attempt++; await BackoffAsync(attempt, cancellationToken).ConfigureAwait(false); continue; }
                throw err;
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
                    throw new WayaPayException(
                        $"Non JSON response (HTTP {status})",
                        status: status, raw: raw, type: WayaPayErrorType.Api);
                }
            }

            var failed = !ok || (envelope is { Success: false });
            if (failed)
            {
                var transient = status >= 500 || status == 429;
                if (retryable && transient && attempt < ceiling)
                {
                    attempt++;
                    await BackoffAsync(attempt, cancellationToken).ConfigureAwait(false);
                    continue;
                }
                throw new WayaPayException(
                    envelope?.Message ?? $"Request failed with HTTP {status}",
                    errorCode: envelope?.Code,
                    status: status,
                    raw: (object?)envelope ?? raw,
                    type: WayaPayErrorType.Api);
            }

            return envelope is null ? default : envelope.Data;
        }
    }

    private string BuildUrl(string path, IReadOnlyDictionary<string, string?>? query)
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

    internal static WayaPayException Missing(string fields, string context) =>
        new($"Missing required field(s) for {context}: {fields}", type: WayaPayErrorType.Validation);

    internal static WayaPayException EmptyData(string path) =>
        new($"Empty response data from {path}", type: WayaPayErrorType.Api);

    /// <summary>
    /// Generate a unique reference. Your dedup and reconciliation key.
    /// One per logical operation. Retries reuse it, new operations get a fresh one.
    /// </summary>
    public static string GenerateReference(string prefix = "WP")
    {
        var ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var hex = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
        return $"{prefix}-{ms}-{hex}";
    }

    public void Dispose()
    {
        if (_ownsHttp) _http.Dispose();
    }
}
