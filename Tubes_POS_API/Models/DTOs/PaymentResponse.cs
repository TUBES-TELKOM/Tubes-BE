namespace Tubes_POS_API.Models.DTOs;

public sealed class PaymentResponse
{
    public int PaymentId { get; set; }
    public int TransactionId { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
