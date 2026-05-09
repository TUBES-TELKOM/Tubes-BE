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
    // operasi apa saja yang diizinkan pada setiap status transaksi.
    // Pendekatan ini menggantikan rantai if-else yang panjang dan rawan error.
    // =========================================================================
    private static readonly Dictionary<TransactionStatus, HashSet<string>> AllowedOperations = new()
    {
        [TransactionStatus.Created]   = [ "AddItem", "RemoveItem", "UpdateItem" ],
        [TransactionStatus.Paid]      = [],   // Tidak ada operasi cart yang diizinkan
        [TransactionStatus.Completed] = [],   // Read-only
        [TransactionStatus.Cancelled] = [],   // Read-only
    };

    // Table-driven: pesan error per status, menghindari if-else untuk error messages
    private static readonly Dictionary<TransactionStatus, string> StatusErrorMessages = new()
    {
        [TransactionStatus.Created]   = string.Empty,
        [TransactionStatus.Paid]      = "Transaksi sudah dibayar, tidak bisa diubah.",
        [TransactionStatus.Completed] = "Transaksi sudah selesai, tidak bisa diubah.",
        [TransactionStatus.Cancelled] = "Transaksi sudah dibatalkan, tidak bisa diubah.",
    };

    // Table-driven: format kode transaksi berdasarkan prefix
    private static readonly Dictionary<string, string> CodePrefixTable = new()
    {
        ["default"] = "TRX",
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

            var subtotal = menu.Price * reqItem.Quantity;
            totalAmount += subtotal;

            transactionItems.Add(new TransactionItem
            {
                MenuId = reqItem.MenuId,
                Quantity = reqItem.Quantity,
                UnitPrice = menu.Price
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
    /// Table-driven validation: cek apakah operasi diizinkan berdasarkan status.
    /// Lookup ke dictionary AllowedOperations, bukan if-else.
    /// </summary>
    private static void ValidateOperation(TransactionStatus status, string operation)
    {
        if (!AllowedOperations.TryGetValue(status, out var allowed) || !allowed.Contains(operation))
        {
            // Table-driven: ambil pesan error dari dictionary
            var message = StatusErrorMessages.GetValueOrDefault(status, "Operasi tidak diizinkan.");
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Hitung ulang total transaksi dari semua items.
    /// Total = SUM(quantity * unitPrice) per item.
    /// </summary>
    private static void RecalculateTotal(Transaction transaction)
    {
        transaction.TotalAmount = transaction.Items.Sum(i => i.Quantity * i.UnitPrice);
    }

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
