namespace Tubes_POS_API.Models;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; } = true;

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }
}
