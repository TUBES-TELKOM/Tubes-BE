using Tubes_POS_API.Services;

namespace Tubes_POS_API.Services
{
    public class ReportService
    {
        // Reuse HistoryService, tidak perlu akses DB langsung
        private readonly HistoryService _historyService;

        public ReportService(HistoryService historyService)
        {
            _historyService = historyService;
        }

        // Buat laporan sederhana dari rentang tanggal
        public async Task<object> GetReportAsync(DateTime start, DateTime end)
        {
            var data = await _historyService.GetByDateRangeAsync(start, end);

            // Hitung total
            int totalTransaksi = data.Count;
            decimal totalPendapatan = data.Sum(h => h.TotalAmount);
            decimal rataRata = totalTransaksi > 0 ? totalPendapatan / totalTransaksi : 0;

            // Table-driven: breakdown per metode pembayaran
            string[] metodePembayaran = { "cash", "debit", "qris", "transfer" };

            var breakdown = new Dictionary<string, decimal>();
            foreach (var metode in metodePembayaran)
            {
                decimal total = data
                    .Where(h => h.PaymentMethod.ToLower() == metode)
                    .Sum(h => h.TotalAmount);

                breakdown[metode] = total;
            }

            // Return sebagai object anonim (mudah dibaca)
            return new
            {
                StartDate = start,
                EndDate = end,
                TotalTransaksi = totalTransaksi,
                TotalPendapatan = totalPendapatan,
                RataRata = rataRata,
                Breakdown = breakdown
            };
        }
    }
}
