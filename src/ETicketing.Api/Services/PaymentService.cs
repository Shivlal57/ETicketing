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

        public PaymentService(AppDbContext dbContext, IEnumerable<IPaymentHandler> handlers)
        {
            _dbContext = dbContext;
            _handlers = handlers.ToDictionary(handler => handler.Method);
        }

        public async Task<PaymentResponseDto> CheckoutAsync(PaymentRequestDto request, CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(PaymentMethod), request.PaymentMethod))
            {
                throw new ArgumentException("Invalid payment method.");
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

            var handler = _handlers[request.PaymentMethod];

            var now = DateTime.UtcNow;

            var status = await handler.ProcessAsync(cancellationToken);

            var payment = new PaymentTransaction
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                PaymentMethod = request.PaymentMethod,
                Status = status,
                CreatedAt = now,
                CompletedAt = status == PaymentStatus.Succeeded ? now : null,
                ConfirmationDueAt = request.PaymentMethod == PaymentMethod.QR ? now.AddSeconds(8) : null
            };

            _dbContext.PaymentTransactions.Add(payment);

            if (status == PaymentStatus.Succeeded)
            {
                order.Status = "Paid";
                order.UpdatedAt = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            return MapResponse(payment);
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
