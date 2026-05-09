using Microsoft.AspNetCore.Mvc;
using Tubes_POS_API.Models;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> Process([FromBody] PaymentRequest request)
    {
        var result = await _paymentService.ProcessPaymentAsync(request);

        return Ok(new ApiResponse<PaymentResponse>
        {
            Message = "Pembayaran berhasil diproses.",
            Data = result
        });
    }
}
