namespace Tubes_POS_API.Models.DTOs;

public sealed class TransactionResponse
{
    public int Id { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Change { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TransactionItemResponse> Items { get; set; } = [];
}
