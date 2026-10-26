using ETicketing.Api.Data;
using ETicketing.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETicketing.Api.Services
{
    public sealed class LedgerService : ILedgerService
    {
        private readonly AppDbContext _dbContext;
        public LedgerService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task PostPaymentAsync(int paymentTransactionId,CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var payment = await _dbContext.PaymentTransactions.SingleOrDefaultAsync(p => p.Id == paymentTransactionId,cancellationToken);

                if (payment is null)
                {
                    throw new KeyNotFoundException("Payment transaction not found.");
                }
                var existingEntries = await _dbContext.LedgerEntries.CountAsync( e => e.PaymentTransactionId == payment.Id,cancellationToken);

                if (existingEntries != 0)
                {
                    throw new InvalidOperationException( "Ledger entries already exist for this payment.");
                }

                if (payment.Status != PaymentStatus.Succeeded)
                {
                    throw new InvalidOperationException("Only successful payments can be posted.");
                }

                var order = await _dbContext.Orders.SingleAsync(o => o.Id == payment.OrderId,cancellationToken);

                if (order.Status == "Paid")
                {
                    throw new InvalidOperationException("Order has already been paid.");
                }

                var now = DateTime.UtcNow;

                _dbContext.LedgerEntries.AddRange(
                    new LedgerEntry
                    {
                        PaymentTransactionId = payment.Id,
                        AccountCode = "CASH",
                        EntryType = "Debit",
                        Amount = payment.Amount,
                        CreatedAt = now
                    },
                    new LedgerEntry
                    {
                        PaymentTransactionId = payment.Id,
                        AccountCode = "TICKET_REVENUE",
                        EntryType = "Credit",
                        Amount = payment.Amount,
                        CreatedAt = now
                    });

                order.Status = "Paid";
                order.UpdatedAt = now;

                await _dbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}