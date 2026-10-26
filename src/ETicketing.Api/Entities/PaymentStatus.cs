namespace ETicketing.Api.Entities
{
    public enum PaymentStatus
    {
        Pending = 1,
        Succeeded = 2,
        Failed = 3,
        Expired = 4
    }
    public enum PaymentMethod
    {
        CreditCard = 1,
        QR = 2
    }
}
