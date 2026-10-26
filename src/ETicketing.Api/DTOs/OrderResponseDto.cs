namespace ETicketing.Api.DTOs
{
    public class OrderResponseDto
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string TicketName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
