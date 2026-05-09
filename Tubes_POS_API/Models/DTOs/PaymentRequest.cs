using System.ComponentModel.DataAnnotations;

namespace Tubes_POS_API.Models.DTOs;

public sealed class PaymentRequest
{
    [Required]
    public int TransactionId { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal PaidAmount { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "cash";
}
