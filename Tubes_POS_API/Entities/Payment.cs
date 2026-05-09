using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tubes_POS_API.Entities.Enums;

namespace Tubes_POS_API.Entities;

public sealed class Payment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TransactionId { get; set; }

    [Precision(18, 2)]
    public decimal AmountPaid { get; set; }

    [Precision(18, 2)]
    public decimal ChangeAmount { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "cash";

    public PaymentStatus Status { get; set; } = PaymentStatus.Created;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TransactionId))]
    public Transaction? Transaction { get; set; }
}
