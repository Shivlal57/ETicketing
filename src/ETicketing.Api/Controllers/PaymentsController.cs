using ETicketing.Api.DTOs;
using ETicketing.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ETicketing.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<PaymentResponseDto>> Checkout(PaymentRequestDto request,CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _paymentService.CheckoutAsync(request,cancellationToken);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("{transactionId:int}")]
        public async Task<ActionResult<PaymentResponseDto>> GetPayment(int transactionId,CancellationToken cancellationToken)
        {
            var payment = await _paymentService.GetPaymentAsync(transactionId, cancellationToken);

            if (payment is null)
            {
                return NotFound(new { message = "Payment not found." });
            }

            return Ok(payment);
        }
    }
}
