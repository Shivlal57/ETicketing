using ETicketing.Api.Data;
using ETicketing.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace ETicketing.Api.Services
{
    public sealed class LedgerService : ILedgerService
    {
        private readonly AppDbContext _dbContext;

        public LedgerService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CompletePaymentAsync(int paymentTransactionId,CancellationToken cancellationToken)
        {
            await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTime.UtcNow;
                var claimed = await _dbContext.PaymentTransactions
                    .Where(p =>
                        p.Id == paymentTransactionId &&
                        p.Status == PaymentStatus.Pending)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(
                                p => p.Status,
                                PaymentStatus.Succeeded)
                            .SetProperty(
                                p => p.CompletedAt,
                                now),
                        cancellationToken);

                if (claimed == 0)
                {
                    var existingPayment = await _dbContext.PaymentTransactions
                        .AsNoTracking()
                        .Include(p => p.Order)
                        .SingleOrDefaultAsync(
                            p => p.Id == paymentTransactionId,
                            cancellationToken);

                    if (existingPayment is null)
                    {
                        throw new KeyNotFoundException("Payment transaction not found.");
                    }

                    var existingEntries = await _dbContext.LedgerEntries
                        .CountAsync(e => e.PaymentTransactionId == paymentTransactionId,cancellationToken);

                    if (existingPayment.Status == PaymentStatus.Succeeded &&
                        existingPayment.Order.Status == "Paid" &&
                        existingEntries == 2)
                    {
                        await dbTransaction.CommitAsync(cancellationToken);
                        return;
                    }

                    throw new InvalidOperationException("Payment could not be claimed for completion.");
                }

                var payment = await _dbContext.PaymentTransactions
                    .Include(p => p.Order)
                    .SingleAsync(
                        p => p.Id == paymentTransactionId,
                        cancellationToken);

                var existingLedgerEntries = await _dbContext.LedgerEntries
                    .CountAsync(
                        e => e.PaymentTransactionId == paymentTransactionId,
                        cancellationToken);

                if (existingLedgerEntries != 0)
                {
                    throw new InvalidOperationException("Ledger entries already exist for this payment.");
                }

                if (payment.Order.Status != "Pending")
                {
                    throw new InvalidOperationException("The order is not awaiting payment.");
                }

                payment.Order.Status = "Paid";
                payment.Order.UpdatedAt = now;

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

                await _dbContext.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await dbTransaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }
    }
}