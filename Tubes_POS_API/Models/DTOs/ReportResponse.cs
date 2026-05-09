namespace Tubes_POS_API.Models.DTOs;

public sealed class ReportResponse
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int TotalTransaksi { get; set; }

    public decimal TotalPendapatan { get; set; }

    public decimal RataRata { get; set; }

    public Dictionary<string, decimal> Breakdown { get; set; } = [];
}
