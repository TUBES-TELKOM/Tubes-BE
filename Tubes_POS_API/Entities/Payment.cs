using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tubes_POS_API.Entities;

public sealed class Payment
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Transaction))]
    public int TransactionId { get; set; }

    [Precision(18, 2)]
    public decimal PaidAmount { get; set; }

    [Precision(18, 2)]
    public decimal ChangeAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Transaction? Transaction { get; set; }
}
