using ETicketing.Api.DTOs;

namespace ETicketing.Api.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request,CancellationToken cancellationToken);
    }
}
