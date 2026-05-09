using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Entities.Enums;
using Tubes_POS_API.Models.DTOs;

namespace Tubes_POS_API.Services;

/// <summary>
/// Implementasi service untuk modul Transaction.
/// Menerapkan teknik Table-Driven Construction untuk validasi operasi
/// berdasarkan status transaksi.
/// </summary>
public sealed class TransactionService : ITransactionService
{
    private readonly AppDbContext _db;

    // =========================================================================
    // TABLE-DRIVEN CONSTRUCTION
    // =========================================================================
    // Menggunakan dictionary sebagai "tabel keputusan" untuk menentukan
    // tarif pajak (Tax Rate) berdasarkan kategori menu.
    // Pendekatan ini menggantikan if-else/switch case yang panjang,
    // sehingga jika ada kategori baru, kita cukup menambahkannya ke tabel ini.
    // =========================================================================
    private static readonly Dictionary<string, decimal> CategoryTaxRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Makanan"] = 0.11m, // PPN 11%
        ["Minuman"] = 0.11m, // PPN 11%
        ["Promo"]   = 0.0m,  // Bebas pajak
        ["Lainnya"] = 0.0m
    };

    // Table-driven: format kode transaksi berdasarkan prefix
    private static readonly Dictionary<string, string> CodePrefixTable = new(StringComparer.OrdinalIgnoreCase)
    {
        ["default"] = "TRX",
        ["online"]  = "ONL"
    };

    public TransactionService(AppDbContext db)
    {
        _db = db;
    }

    // =========================================================================
    // CREATE TRANSACTION (CHECKOUT)
    // =========================================================================
    public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request)
    {
        // 1. Ambil data Menu dari DB untuk memastikan harga benar (Backend is Source of Truth)
        var menuIds = request.Items.Select(i => i.MenuId).Distinct().ToList();
        var menus = await _db.Menus.Where(m => menuIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id);

        var transactionItems = new List<TransactionItem>();
        decimal totalAmount = 0;

        foreach (var reqItem in request.Items)
        {
            if (!menus.TryGetValue(reqItem.MenuId, out var menu))
                throw new KeyNotFoundException($"Menu dengan ID {reqItem.MenuId} tidak ditemukan.");

            if (!menu.IsAvailable)
                throw new ArgumentException($"Menu '{menu.Name}' sedang tidak tersedia.");

            // Ambil tax rate dari tabel keputusan (Table-Driven)
            // Jika kategori tidak ada di tabel, default ke 0% pajak
            var taxRate = CategoryTaxRates.GetValueOrDefault(menu.Category, 0m);
            var taxAmount = menu.Price * taxRate;
            
            var unitPriceWithTax = menu.Price + taxAmount;
            var subtotal = unitPriceWithTax * reqItem.Quantity;
            
            totalAmount += subtotal;

            transactionItems.Add(new TransactionItem
            {
                MenuId = reqItem.MenuId,
                Quantity = reqItem.Quantity,
                UnitPrice = unitPriceWithTax // Simpan harga yang sudah termasuk pajak
            });
        }

        // Kalkulasi kembalian (diserahkan sepenuhnya ke Person 3 nanti, 
        // tapi kita sediakan nilai sementaranya)
        decimal change = request.PaidAmount - totalAmount;

        var transaction = new Transaction
        {
            TransactionCode = GenerateTransactionCode(),
            CustomerName = request.CustomerName,
            TotalAmount = totalAmount,
            PaidAmount = request.PaidAmount,
            Change = change,
            PaymentMethod = request.PaymentMethod,
            Status = TransactionStatus.Completed, // Langsung completed di kasir warung
            CreatedAt = DateTime.UtcNow,
            Items = transactionItems
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        return MapToResponse(transaction);
    }

    // =========================================================================
    // GET TRANSACTION BY ID
    // =========================================================================
    public async Task<TransactionResponse> GetTransactionByIdAsync(int id)
    {
        var transaction = await FindTransactionWithItemsAsync(id);
        return MapToResponse(transaction);
    }

    // =========================================================================
    // GET ALL TRANSACTIONS
    // =========================================================================
    public async Task<List<TransactionResponse>> GetAllTransactionsAsync()
    {
        var transactions = await _db.Transactions
            .Include(t => t.Items)
                .ThenInclude(ti => ti.Menu)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return transactions.Select(MapToResponse).ToList();
    }



    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================



    /// <summary>
    /// Generate kode transaksi unik: TRX-yyyyMMdd-HHmmss-fff
    /// </summary>
    private static string GenerateTransactionCode()
    {
        var prefix = CodePrefixTable.GetValueOrDefault("default", "TRX");
        return $"{prefix}-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}";
    }

    /// <summary>
    /// Cari transaksi beserta items dan data menu. Throw 404 jika tidak ditemukan.
    /// </summary>
    private async Task<Transaction> FindTransactionWithItemsAsync(int id)
    {
        return await _db.Transactions
            .Include(t => t.Items)
                .ThenInclude(ti => ti.Menu)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Transaksi dengan ID {id} tidak ditemukan.");
    }

    /// <summary>
    /// Map entity Transaction ke DTO TransactionResponse.
    /// </summary>
    private static TransactionResponse MapToResponse(Transaction transaction)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            TransactionCode = transaction.TransactionCode,
            CustomerName = transaction.CustomerName,
            TotalAmount = transaction.TotalAmount,
            PaidAmount = transaction.PaidAmount,
            Change = transaction.Change,
            PaymentMethod = transaction.PaymentMethod,
            Status = transaction.Status.ToString(),
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
            Items = transaction.Items.Select(i => new TransactionItemResponse
            {
                Id = i.Id,
                MenuId = i.MenuId,
                MenuName = i.Menu?.Name ?? string.Empty,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal,
            }).ToList(),
        };
    }
}
