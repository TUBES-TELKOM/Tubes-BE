namespace Tubes_POS_API.Models.DTOs;

public sealed class TransactionItemResponse
{
    public int Id { get; set; }
    public int MenuId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
