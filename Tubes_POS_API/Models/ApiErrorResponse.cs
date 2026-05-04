namespace Tubes_POS_API.Models;

public sealed class ApiErrorResponse
{
    public bool Success { get; init; } = false;

    public string Message { get; init; } = string.Empty;

    public int StatusCode { get; init; }

    public IEnumerable<string> Errors { get; init; } = [];
}
