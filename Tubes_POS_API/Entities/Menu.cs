namespace Tubes_POS_API.Entities;

public sealed class Menu
{
    [System.ComponentModel.DataAnnotations.Key]
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(255)]
    public string? Description { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
