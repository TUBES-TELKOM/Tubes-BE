namespace Tubes_POS_API.Models.DTOs;

public sealed class TransactionHistoryResponse
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public DateTime TransactionDate { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
}
