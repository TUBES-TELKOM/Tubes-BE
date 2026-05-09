using System.ComponentModel.DataAnnotations;

namespace Tubes_POS_API.Models.DTOs;

public sealed class UpdateItemRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity harus minimal 1.")]
    public int Quantity { get; set; }
}
