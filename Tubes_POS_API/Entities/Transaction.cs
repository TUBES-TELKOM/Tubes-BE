using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tubes_POS_API.Entities;

public sealed class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string TransactionCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CustomerName { get; set; }

    [MaxLength(20)]
    public string? TableNumber { get; set; }

    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    [Precision(18, 2)]
    public decimal PaidAmount { get; set; }

    [Precision(18, 2)]
    public decimal Change { get; set; }

    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "cash";

    public TransactionStatus Status { get; set; } = TransactionStatus.Created;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty(nameof(TransactionItem.Transaction))]
    public List<TransactionItem> Items { get; set; } = [];
}
