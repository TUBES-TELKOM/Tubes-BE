using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> GetAll()
        {
            var result = await _historyService.GetAllAsync();
            return Ok(result);
        }

        // GET api/history/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _historyService.GetByIdAsync(id);

            if (result == null)
                return NotFound("Transaksi tidak ditemukan.");

            return Ok(result);
        }

        // GET api/history/filter?start=2025-01-01&end=2025-12-31
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(DateTime start, DateTime end)
        {
            var result = await _historyService.GetByDateRangeAsync(start, end);
            return Ok(result);
        }

        // GET api/history/report?start=2025-01-01&end=2025-12-31
        [HttpGet("report")]
        public async Task<IActionResult> GetReport(DateTime start, DateTime end)
        {
            var result = await _reportService.GetReportAsync(start, end);
            return Ok(result);
        }
    }
}