namespace ETicketing.Api.Entities
{
    public sealed class CreditCardHandler : IPaymentHandler
    {
        public PaymentMethod Method => PaymentMethod.CreditCard;

        public Task<PaymentStatus> ProcessAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(PaymentStatus.Succeeded);
        }
    }
}
