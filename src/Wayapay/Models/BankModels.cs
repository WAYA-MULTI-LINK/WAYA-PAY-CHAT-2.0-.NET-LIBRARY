namespace WayaPay;

public sealed record Bank
{
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
    public string Id { get; init; } = "";
    public bool Status { get; init; }
}
