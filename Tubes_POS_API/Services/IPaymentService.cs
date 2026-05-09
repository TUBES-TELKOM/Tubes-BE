using Tubes_POS_API.Models.DTOs;

namespace Tubes_POS_API.Services;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
}
