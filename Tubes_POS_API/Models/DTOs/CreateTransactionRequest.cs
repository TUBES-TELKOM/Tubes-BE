using System.ComponentModel.DataAnnotations;

namespace Tubes_POS_API.Models.DTOs;

public sealed class CreateTransactionRequest
{
    [MaxLength(100)]
    public string? CustomerName { get; set; }

    [MaxLength(20)]
    public string? TableNumber { get; set; }

    public List<TransactionItemRequest> Items { get; set; } = [];

    public decimal PaidAmount { get; set; }

    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "cash";
}

public sealed class TransactionItemRequest
{
    [Required]
    public int MenuId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
