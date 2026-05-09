using System.ComponentModel.DataAnnotations;

namespace Tubes_POS_API.Models.DTOs;

public sealed class CreateTransactionRequest
{
    [MaxLength(100)]
    public string? CustomerName { get; set; }

    [MaxLength(20)]
    public string? TableNumber { get; set; }
}
