namespace WayaQuick;

/// <summary>
/// Generic envelope returned by every WayaQuick endpoint.
/// code "00" + success true = OK; anything else = error.
/// </summary>
public sealed record WayaQuickResponse<T>
{
    public bool Success { get; init; }

    /// <summary>Bank-style response code. "00" means success.</summary>
    public string? Code { get; init; }

    /// <summary>Human-readable message describing the response.</summary>
    public string? Message { get; init; }

    /// <summary>Typed payload. Null on errors.</summary>
    public T? Data { get; init; }

    /// <summary>Timestamp of the response.</summary>
    public string? Timestamp { get; init; }
}
