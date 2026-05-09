namespace Tubes_POS_API
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        [HttpPost]
        public IActionResult Pay(decimal total, decimal cash)
        {
            PaymentService payment = new PaymentService();

            var result = payment.Pay(total, cash);

            return Ok(result);
        }
    }
}
