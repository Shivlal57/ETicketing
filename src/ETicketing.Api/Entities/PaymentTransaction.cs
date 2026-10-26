namespace ETicketing.Api.Entities
{
    public class PaymentTransaction
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public DateTime? ConfirmationDueAt { get; set; }

        public Order Order { get; set; } = null!;
    }
}
