namespace WayaPay;

/// <summary>Category of a <see cref="WayaPayException"/>.</summary>
public enum WayaPayErrorType
{
    Api,
    Validation,
    Network,
    Timeout,
    Config,
}

/// <summary>
/// Single exception type for everything that goes wrong.
/// Branch on <see cref="Type"/> for category, <see cref="ErrorCode"/> for the WayaPay envelope code.
/// </summary>
public sealed class WayaPayException : Exception
{
    /// <summary>WayaPay code, e.g. "07". Null when not an API error.</summary>
    public string? ErrorCode { get; }

    /// <summary>HTTP status, when known.</summary>
    public int? Status { get; }

    /// <summary>Raw decoded body or underlying error, for logging.</summary>
    public object? Raw { get; }

    /// <summary>Error category.</summary>
    public WayaPayErrorType Type { get; }

    public WayaPayException(
        string message,
        string? errorCode = null,
        int? status = null,
        object? raw = null,
        WayaPayErrorType type = WayaPayErrorType.Api)
        : base(message)
    {
        ErrorCode = errorCode;
        Status = status;
        Raw = raw;
        Type = type;
    }
}
