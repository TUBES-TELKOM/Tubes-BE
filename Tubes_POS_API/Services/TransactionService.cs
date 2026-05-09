using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Entities.Enums;
using Tubes_POS_API.Models.DTOs;

namespace Tubes_POS_API.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly AppDbContext _db;

    private static readonly Dictionary<TransactionStatus, HashSet<string>> AllowedOperations = new()
    {
        [TransactionStatus.Created] = ["AddItem", "RemoveItem", "UpdateItem"],
        [TransactionStatus.Paid] = [],
        [TransactionStatus.Completed] = [],
        [TransactionStatus.Cancelled] = []
    };

    private static readonly Dictionary<TransactionStatus, string> StatusErrorMessages = new()
    {
        [TransactionStatus.Created] = string.Empty,
        [TransactionStatus.Paid] = "Transaksi sudah dibayar, tidak bisa diubah.",
        [TransactionStatus.Completed] = "Transaksi sudah selesai, tidak bisa diubah.",
        [TransactionStatus.Cancelled] = "Transaksi sudah dibatalkan, tidak bisa diubah."
    };

    private static readonly Dictionary<string, decimal> CategoryTaxRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Makanan"] = 0.11m,
        ["Minuman"] = 0.11m,
        ["Promo"] = 0.0m,
        ["Lainnya"] = 0.0m
    };

    private static readonly Dictionary<string, string> CodePrefixTable = new(StringComparer.OrdinalIgnoreCase)
    {
        ["default"] = "TRX",
        ["online"] = "ONL"
    };

    public TransactionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request)
    {
        var transaction = new Transaction
        {
            TransactionCode = GenerateTransactionCode(),
            CustomerName = request.CustomerName,
            TableNumber = request.TableNumber,
            TotalAmount = 0m,
            PaidAmount = 0m,
            Change = 0m,
            PaymentMethod = "cash",
            Status = TransactionStatus.Created,
            CreatedAt = DateTime.UtcNow,
            Items = []
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        return await GetTransactionByIdAsync(transaction.Id);
    }

    public async Task<TransactionResponse> GetTransactionByIdAsync(int id)
    {
        var transaction = await FindTransactionWithItemsAsync(id);
        return MapToResponse(transaction);
    }

    public async Task<List<TransactionResponse>> GetAllTransactionsAsync()
    {
        var transactions = await _db.Transactions
            .Include(t => t.Items)
                .ThenInclude(ti => ti.Menu)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return transactions.Select(MapToResponse).ToList();
    }

    public async Task<TransactionResponse> AddItemAsync(int transactionId, AddItemRequest request)
    {
        var transaction = await FindTransactionWithItemsAsync(transactionId);

        ValidateOperation(transaction.Status, "AddItem");

        var menu = await _db.Menus.FindAsync(request.MenuId)
            ?? throw new KeyNotFoundException($"Menu dengan ID {request.MenuId} tidak ditemukan.");

        if (!menu.IsAvailable)
            throw new ArgumentException($"Menu '{menu.Name}' sedang tidak tersedia.");

        var unitPrice = CalculateUnitPrice(menu);
        var existingItem = transaction.Items.FirstOrDefault(i => i.MenuId == request.MenuId);

        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            transaction.Items.Add(new TransactionItem
            {
                TransactionId = transaction.Id,
                MenuId = request.MenuId,
                Quantity = request.Quantity,
                UnitPrice = unitPrice
            });
        }

        RecalculateTotal(transaction);
        transaction.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetTransactionByIdAsync(transactionId);
    }

    public async Task<TransactionResponse> RemoveItemAsync(int transactionId, int itemId)
    {
        var transaction = await FindTransactionWithItemsAsync(transactionId);

        ValidateOperation(transaction.Status, "RemoveItem");

        var item = transaction.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Item dengan ID {itemId} tidak ditemukan dalam transaksi.");

        transaction.Items.Remove(item);
        _db.TransactionItems.Remove(item);

        RecalculateTotal(transaction);
        transaction.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return MapToResponse(transaction);
    }

    public async Task<TransactionResponse> UpdateItemQuantityAsync(int transactionId, int itemId, UpdateItemRequest request)
    {
        var transaction = await FindTransactionWithItemsAsync(transactionId);

        ValidateOperation(transaction.Status, "UpdateItem");

        var item = transaction.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Item dengan ID {itemId} tidak ditemukan dalam transaksi.");

        item.Quantity = request.Quantity;

        RecalculateTotal(transaction);
        transaction.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetTransactionByIdAsync(transactionId);
    }

    private static void ValidateOperation(TransactionStatus status, string operation)
    {
        if (!AllowedOperations.TryGetValue(status, out var allowed) || !allowed.Contains(operation))
        {
            var message = StatusErrorMessages.GetValueOrDefault(status, "Operasi tidak diizinkan.");
            throw new InvalidOperationException(message);
        }
    }

    private static void RecalculateTotal(Transaction transaction)
    {
        transaction.TotalAmount = transaction.Items.Sum(i => i.Quantity * i.UnitPrice);
    }

    private static decimal CalculateUnitPrice(Menu menu)
    {
        var taxRate = CategoryTaxRates.GetValueOrDefault(menu.Category, 0m);
        return menu.Price + (menu.Price * taxRate);
    }

    private static string GenerateTransactionCode()
    {
        var prefix = CodePrefixTable.GetValueOrDefault("default", "TRX");
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"{prefix}-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}-{suffix}";
    }

    private async Task<Transaction> FindTransactionWithItemsAsync(int id)
    {
        return await _db.Transactions
            .Include(t => t.Items)
                .ThenInclude(ti => ti.Menu)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Transaksi dengan ID {id} tidak ditemukan.");
    }

    private static TransactionResponse MapToResponse(Transaction transaction)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            TransactionCode = transaction.TransactionCode,
            CustomerName = transaction.CustomerName,
            TableNumber = transaction.TableNumber,
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
