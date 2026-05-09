using Tubes_POS_API.Models.DTOs;

namespace Tubes_POS_API.Services;

/// <summary>
/// Kontrak service untuk modul Transaction.
/// </summary>
public interface ITransactionService
{
    Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request);
    Task<TransactionResponse> GetTransactionByIdAsync(int id);
    Task<List<TransactionResponse>> GetAllTransactionsAsync();
}
