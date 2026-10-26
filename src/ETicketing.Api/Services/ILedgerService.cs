namespace ETicketing.Api.Services
{
    public interface ILedgerService
    {
        Task PostPaymentAsync(int paymentTransactionId,CancellationToken cancellationToken);
    }
}
