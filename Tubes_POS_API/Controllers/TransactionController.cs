using Microsoft.AspNetCore.Mvc;
using Tubes_POS_API.Models;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Buat transaksi baru.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Create(
        [FromBody] CreateTransactionRequest request)
    {
        var result = await _transactionService.CreateTransactionAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new ApiResponse<TransactionResponse>
        {
            Message = "Transaksi berhasil dibuat.",
            Data = result,
        });
    }

    /// <summary>
    /// Ambil detail transaksi berdasarkan ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> GetById(int id)
    {
        var result = await _transactionService.GetTransactionByIdAsync(id);

        return Ok(new ApiResponse<TransactionResponse>
        {
            Message = "Detail transaksi.",
            Data = result,
        });
    }

    /// <summary>
    /// Ambil semua transaksi.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TransactionResponse>>>> GetAll()
    {
        var result = await _transactionService.GetAllTransactionsAsync();

        return Ok(new ApiResponse<List<TransactionResponse>>
        {
            Message = $"Ditemukan {result.Count} transaksi.",
            Data = result,
        });
    }

}
