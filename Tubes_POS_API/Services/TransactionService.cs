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
    // CREATE TRANSACTION
    // =========================================================================
    public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request)
    {
        var transaction = new Transaction
        {
            TransactionCode = GenerateTransactionCode(),
            CustomerName = request.CustomerName,
            TableNumber = request.TableNumber,
            Status = TransactionStatus.Created,
            TotalAmount = 0,
            CreatedAt = DateTime.UtcNow,
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
    // ADD ITEM TO CART
    // =========================================================================
    public async Task<TransactionResponse> AddItemAsync(int transactionId, AddItemRequest request)
    {
        var transaction = await FindTransactionWithItemsAsync(transactionId);

        // Table-driven: cek apakah operasi "AddItem" diizinkan untuk status saat ini
        ValidateOperation(transaction.Status, "AddItem");

        // Validasi menu: harus ada dan tersedia
        var menu = await _db.Menus.FindAsync(request.MenuId)
            ?? throw new KeyNotFoundException($"Menu dengan ID {request.MenuId} tidak ditemukan.");

        if (!menu.IsAvailable)
            throw new ArgumentException($"Menu '{menu.Name}' sedang tidak tersedia.");

        // Cek apakah item menu sudah ada di cart — jika ya, tambah quantity
        var existingItem = transaction.Items.FirstOrDefault(i => i.MenuId == request.MenuId);

        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            var newItem = new TransactionItem
            {
                TransactionId = transactionId,
                MenuId = request.MenuId,
                Quantity = request.Quantity,
                UnitPrice = menu.Price,
            };
            transaction.Items.Add(newItem);
        }

        // Recalculate total
        RecalculateTotal(transaction);
        transaction.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Reload untuk mendapatkan data Menu navigation property yang lengkap
        return await GetTransactionByIdAsync(transactionId);
    }

    // =========================================================================
    // REMOVE ITEM FROM CART
    // =========================================================================
    public async Task<TransactionResponse> RemoveItemAsync(int transactionId, int itemId)
    {
        var transaction = await FindTransactionWithItemsAsync(transactionId);

        // Table-driven: cek apakah operasi "RemoveItem" diizinkan
        ValidateOperation(transaction.Status, "RemoveItem");

        var item = transaction.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Item dengan ID {itemId} tidak ditemukan dalam transaksi.");

        transaction.Items.Remove(item);
        _db.TransactionItems.Remove(item);

        // Recalculate total
        RecalculateTotal(transaction);
        transaction.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return MapToResponse(transaction);
    }

    // =========================================================================
    // UPDATE ITEM QUANTITY
    // =========================================================================
    public async Task<TransactionResponse> UpdateItemQuantityAsync(int transactionId, int itemId, UpdateItemRequest request)
    {
        var transaction = await FindTransactionWithItemsAsync(transactionId);

        // Table-driven: cek apakah operasi "UpdateItem" diizinkan
        ValidateOperation(transaction.Status, "UpdateItem");

        var item = transaction.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Item dengan ID {itemId} tidak ditemukan dalam transaksi.");

        item.Quantity = request.Quantity;

        // Recalculate total
        RecalculateTotal(transaction);
        transaction.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetTransactionByIdAsync(transactionId);
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
            TableNumber = transaction.TableNumber,
            TotalAmount = transaction.TotalAmount,
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
