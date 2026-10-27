using ETicketing.Api.Data;
using ETicketing.Api.DTOs;
using ETicketing.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETicketing.Api.Services
{
    public sealed class PaymentService : IPaymentService
    {
        private readonly AppDbContext _dbContext;
        private readonly IReadOnlyDictionary<PaymentMethod, IPaymentHandler> _handlers;
        private readonly ILedgerService _ledgerService;

        public PaymentService(AppDbContext dbContext, IEnumerable<IPaymentHandler> handlers, ILedgerService ledgerService)
        {
            _dbContext = dbContext;
            _handlers = handlers.ToDictionary(handler => handler.Method);
            _ledgerService = ledgerService;
        }

        public async Task<PaymentResponseDto> CheckoutAsync(PaymentRequestDto request, CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(PaymentMethod), request.PaymentMethod))
            {
                throw new ArgumentException("Invalid payment method.");
            }
            if (!_handlers.TryGetValue(request.PaymentMethod, out var handler))
            {
                throw new ArgumentException("Unsupported payment method.");
            }

            await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var order = await _dbContext.Orders.SingleOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order is null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            if (order.Status != "Pending")
            {
                throw new InvalidOperationException("This order is not available for checkout.");
            }

            var existingPayment = await _dbContext.PaymentTransactions
                .AnyAsync(
                    p => p.OrderId == order.Id &&
                         (p.Status == PaymentStatus.Pending ||
                          p.Status == PaymentStatus.Succeeded),
                    cancellationToken);

            if (existingPayment)
            {
                throw new InvalidOperationException("A pending or successful payment already exists for this order.");
            }

            var simulatedStatus = await handler.ProcessAsync(cancellationToken);
            if (simulatedStatus != PaymentStatus.Succeeded && simulatedStatus != PaymentStatus.Pending)
            {
                throw new InvalidOperationException("The payment handler returned an unsupported status.");
            }

            var now = DateTime.UtcNow;

            var payment = new PaymentTransaction
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                PaymentMethod = request.PaymentMethod,
                Status = PaymentStatus.Pending,
                CreatedAt = now,
                CompletedAt = null,
                ConfirmationDueAt =request.PaymentMethod == PaymentMethod.QR ? now.AddSeconds(8) : null
            };

            _dbContext.PaymentTransactions.Add(payment);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            if (simulatedStatus == PaymentStatus.Succeeded)
            {
                await _ledgerService.CompletePaymentAsync(
                    payment.Id,
                    cancellationToken);
            }

            var result = await _dbContext.PaymentTransactions.AsNoTracking().SingleAsync(p => p.Id == payment.Id,cancellationToken);

            return MapResponse(result);
        }

        public async Task<PaymentResponseDto?> GetPaymentAsync(int transactionId, CancellationToken cancellationToken)
        {
            var payment = await _dbContext.PaymentTransactions.AsNoTracking().SingleOrDefaultAsync(p => p.Id == transactionId, cancellationToken);
            return payment is null ? null : MapResponse(payment);
        }

        private static PaymentResponseDto MapResponse(PaymentTransaction payment)
        {
            return new PaymentResponseDto
            {
                TransactionId = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.CompletedAt
            };
        }
    }
}
