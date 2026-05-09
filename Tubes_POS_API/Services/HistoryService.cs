using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tubes_POS_API.Services
{
    public class HistoryService
    {
        private readonly AppDbContext _context;

        // Constructor: minta DbContext lewat dependency injection
        public HistoryService(AppDbContext context)
        {
            _context = context;
        }

        // Ambil semua riwayat transaksi
        public async Task<List<TransactionHistory>> GetAllAsync()
        {
            return await _context.TransactionHistories
                .OrderByDescending(h => h.TransactionDate)
                .ToListAsync();
        }

        // Ambil berdasarkan ID
        public async Task<TransactionHistory?> GetByIdAsync(int id)
        {
            return await _context.TransactionHistories
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        // Filter berdasarkan tanggal
        public async Task<List<TransactionHistory>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.TransactionHistories
                .Where(h => h.TransactionDate >= start && h.TransactionDate <= end)
                .OrderByDescending(h => h.TransactionDate)
                .ToListAsync();
        }

        // Filter berdasarkan metode pembayaran
        public async Task<List<TransactionHistory>> GetByPaymentMethodAsync(string method)
        {
            return await _context.TransactionHistories
                .Where(h => h.PaymentMethod.ToLower() == method.ToLower())
                .ToListAsync();
        }
    }
}
