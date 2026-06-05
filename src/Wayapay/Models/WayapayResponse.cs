namespace WayaPay;

/// <summary>The envelope every endpoint returns.</summary>
public sealed record WayaPayResponse<T>
{
    public bool Success { get; init; }
    public string? Code { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public string? Timestamp { get; init; }
}
