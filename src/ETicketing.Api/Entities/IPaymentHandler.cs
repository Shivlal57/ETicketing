namespace ETicketing.Api.Entities
{
    public interface IPaymentHandler
    {
        PaymentMethod Method { get; }

        Task<PaymentStatus> ProcessAsync(CancellationToken cancellationToken);
    }
}
