using ETicketing.Api.Entities;

namespace ETicketing.Api.DTOs
{
    public class PaymentRequestDto
    {
        public int OrderId { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
    }
    public class PaymentResponseDto
    {
        public int TransactionId { get; set; }

        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
