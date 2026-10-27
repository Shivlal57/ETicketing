using ETicketing.Api.Data;
using ETicketing.Api.Entities;
using ETicketing.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ETicketing.Api.BackgroundServices
{
    public sealed class QrPaymentConfirmationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<QrPaymentConfirmationWorker> _logger;

        public QrPaymentConfirmationWorker(IServiceScopeFactory scopeFactory,ILogger<QrPaymentConfirmationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QR payment confirmation worker started.");

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingPaymentsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,"Error while processing pending QR payments.");
                }

                try
                {
                    if (!await timer.WaitForNextTickAsync(stoppingToken))
                        break;
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("QR payment confirmation worker stopped.");
        }

        private async Task ProcessPendingPaymentsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var ledgerService = scope.ServiceProvider.GetRequiredService<ILedgerService>();

            var now = DateTime.UtcNow;

            var paymentIds = await dbContext.PaymentTransactions
                .AsNoTracking()
                .Where(p =>
                    p.PaymentMethod == PaymentMethod.QR &&
                    p.Status == PaymentStatus.Pending &&
                    p.ConfirmationDueAt != null &&
                    p.ConfirmationDueAt <= now)
                .OrderBy(p => p.ConfirmationDueAt)
                .Select(p => p.Id)
                .Take(20)
                .ToListAsync(cancellationToken);

            foreach (var paymentId in paymentIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await ledgerService.CompletePaymentAsync(paymentId,cancellationToken);

                    _logger.LogInformation("QR payment {PaymentId} completed successfully.",paymentId);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to complete QR payment {PaymentId}. It will be retried on a later poll if it remains pending.",paymentId);
                }
            }
        }
    }
}
