namespace Tubes_POS_API.Models.DTOs;

public sealed class TransactionResponse
{
    public int Id { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? TableNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TransactionItemResponse> Items { get; set; } = [];
}
