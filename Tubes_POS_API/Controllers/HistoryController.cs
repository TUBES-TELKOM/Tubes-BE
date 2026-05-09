using Microsoft.AspNetCore.Mvc;
using Tubes_POS_API.Models;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly HistoryService _historyService;
        private readonly ReportService _reportService;

        public HistoryController(HistoryService historyService, ReportService reportService)
        {
            _historyService = historyService;
            _reportService = reportService;
        }

        // GET api/history
    [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TransactionHistory>>>> GetAll()
        {
            var result = await _historyService.GetAllAsync();

            return Ok(new ApiResponse<List<TransactionHistory>>
            {
                Message = $"Ditemukan {result.Count} riwayat transaksi.",
                Data = result
            });
        }

        // GET api/history/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TransactionHistory>>> GetById(int id)
        {
            var result = await _historyService.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Transaksi tidak ditemukan.",
                    Data = null
                });
            }

            return Ok(new ApiResponse<TransactionHistory>
            {
                Message = "Detail riwayat transaksi.",
                Data = result
            });
        }

        // GET api/history/filter?start=2025-01-01&end=2025-12-31
        [HttpGet("filter")]
        public async Task<ActionResult<ApiResponse<List<TransactionHistory>>>> Filter(DateTime start, DateTime end)
        {
            var result = await _historyService.GetByDateRangeAsync(start, end);

            return Ok(new ApiResponse<List<TransactionHistory>>
            {
                Message = $"Ditemukan {result.Count} riwayat transaksi.",
                Data = result
            });
        }

        // GET api/history/report?start=2025-01-01&end=2025-12-31
        [HttpGet("report")]
        public async Task<ActionResult<ApiResponse<object>>> GetReport(DateTime start, DateTime end)
        {
            var result = await _reportService.GetReportAsync(start, end);

            return Ok(new ApiResponse<object>
            {
                Message = "Laporan transaksi.",
                Data = result
            });
        }
    }
}
