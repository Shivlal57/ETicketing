namespace ETicketing.Api.Services
{
    public interface ILedgerService
    {
        Task CompletePaymentAsync(int paymentTransactionId,CancellationToken cancellationToken);
    }
}
