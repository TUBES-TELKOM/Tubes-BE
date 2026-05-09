using Tubes_POS_API.Models.DTOs;

namespace Tubes_POS_API.Services;

public interface ITransactionService
{
    Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request);
    Task<TransactionResponse> GetTransactionByIdAsync(int id);
    Task<List<TransactionResponse>> GetAllTransactionsAsync();
    Task<TransactionResponse> AddItemAsync(int transactionId, AddItemRequest request);
    Task<TransactionResponse> RemoveItemAsync(int transactionId, int itemId);
    Task<TransactionResponse> UpdateItemQuantityAsync(int transactionId, int itemId, UpdateItemRequest request);
}
