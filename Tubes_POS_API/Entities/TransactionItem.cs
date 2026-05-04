using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tubes_POS_API.Entities;

public sealed class TransactionItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Transaction))]
    public int TransactionId { get; set; }

    [Required]
    [ForeignKey(nameof(Menu))]
    public int MenuId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Precision(18, 2)]
    public decimal UnitPrice { get; set; }

    [NotMapped]
    public decimal Subtotal => Quantity * UnitPrice;

    public Transaction? Transaction { get; set; }

    public Menu? Menu { get; set; }
}
