namespace ETicketing.Api.Entities
{
    public sealed class QRHandler : IPaymentHandler
    {
        public PaymentMethod Method => PaymentMethod.QR;

        public Task<PaymentStatus> ProcessAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(PaymentStatus.Pending);
        }
    }
}
