using ETicketing.Api.DTOs;

namespace ETicketing.Api.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CheckoutAsync(PaymentRequestDto request,CancellationToken cancellationToken);
        Task<PaymentResponseDto?> GetPaymentAsync(int transactionId,CancellationToken cancellationToken);
    }
}
