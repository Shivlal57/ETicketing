namespace ETicketing.Api.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public Ticket Ticket { get; set; } = null!;
    }
}
