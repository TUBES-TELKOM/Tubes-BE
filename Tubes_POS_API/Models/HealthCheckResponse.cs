namespace Tubes_POS_API.Models;

public sealed class HealthCheckResponse
{
    public string Status { get; init; } = "ok";

    public string Probe { get; init; } = "health";

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public Dictionary<string, string> Checks { get; init; } = [];
}
