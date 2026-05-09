using System.ComponentModel.DataAnnotations;

namespace Tubes_POS_API.Models.DTOs;

public sealed class MenuRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;
}
