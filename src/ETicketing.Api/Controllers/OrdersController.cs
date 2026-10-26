using ETicketing.Api.DTOs;
using ETicketing.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ETicketing.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(
                    request,
                    cancellationToken);

                return CreatedAtAction(
                    nameof(CreateOrder),
                    new { id = order.Id },
                    order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "Ticket not found.")
                    return NotFound(new { message = ex.Message });
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
