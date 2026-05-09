using System.ComponentModel.DataAnnotations;

namespace Tubes_POS_API.Models.DTOs;

public sealed class CreateTransactionRequest
{
    [MaxLength(100)]
    public string? CustomerName { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Minimal 1 item untuk transaksi.")]
    public List<TransactionItemRequest> Items { get; set; } = [];

    public decimal PaidAmount { get; set; }
    
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "cash";
}

public class TransactionItemRequest
{
    [Required]
    public int MenuId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
