using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Entities.Enums;
using Tubes_POS_API.Models.DTOs;

namespace Tubes_POS_API.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly PaymentStateMachine _stateMachine;

    public PaymentService(AppDbContext db, PaymentStateMachine stateMachine)
    {
        _db = db;
        _stateMachine = stateMachine;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        var transaction = await _db.Transactions
            .Include(t => t.Items)
            .Include(t => t.Payment)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId)
            ?? throw new KeyNotFoundException($"Transaksi dengan ID {request.TransactionId} tidak ditemukan.");

        if (transaction.Payment is not null)
        {
            throw new InvalidOperationException("Transaksi ini sudah memiliki pembayaran.");
        }

        if (transaction.Status != TransactionStatus.Created)
        {
            throw new InvalidOperationException("Transaksi sudah diproses dan tidak bisa dibayar ulang.");
        }

        if (transaction.Items.Count == 0)
        {
            throw new InvalidOperationException("Transaksi belum memiliki item.");
        }

        var totalAmount = transaction.Items.Sum(item => item.Quantity * item.UnitPrice);
        transaction.TotalAmount = totalAmount;

        if (request.PaidAmount < totalAmount)
        {
            _stateMachine.Fail();
            throw new ArgumentException("Uang tidak cukup.");
        }

        _stateMachine.MarkPaid();
        _stateMachine.Complete();

        var change = request.PaidAmount - totalAmount;
        var paymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "cash" : request.PaymentMethod;

        var payment = new Payment
        {
            TransactionId = transaction.Id,
            AmountPaid = request.PaidAmount,
            ChangeAmount = change,
            PaymentMethod = paymentMethod,
            Status = _stateMachine.CurrentState,
            CreatedAt = DateTime.UtcNow
        };

        transaction.PaidAmount = request.PaidAmount;
        transaction.Change = change;
        transaction.PaymentMethod = paymentMethod;
        transaction.Status = TransactionStatus.Completed;
        transaction.UpdatedAt = DateTime.UtcNow;

        _db.TransactionHistories.Add(new TransactionHistory
        {
            TransactionId = transaction.Id,
            TransactionDate = DateTime.UtcNow,
            PaymentMethod = paymentMethod,
            TotalAmount = totalAmount
        });

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return new PaymentResponse
        {
            PaymentId = payment.Id,
            TransactionId = transaction.Id,
            TransactionCode = transaction.TransactionCode,
            TotalAmount = totalAmount,
            PaidAmount = payment.AmountPaid,
            ChangeAmount = payment.ChangeAmount,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status.ToString(),
            CreatedAt = payment.CreatedAt
        };
    }
}
