namespace WayaQuick.Models;

/// <summary>
/// Error envelope returned by WayaQuick when success is false.
/// Check Code for the bank-style error code and Message for a human-readable description.
/// </summary>
public sealed record WayaquickErrorResponse
{
    /// <summary>Always false on error responses.</summary>
    public bool Success { get; init; }

    /// <summary>
    /// Bank-style error code, e.g. "01" (unauthorized), "57" (IP not whitelisted).
    /// See the full code table in the API docs.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>Human-readable description of the error.</summary>
    public string? Message { get; init; }

    /// <summary>ISO-8601 timestamp of when the error occurred.</summary>
    public string? Timestamp { get; init; }
}
