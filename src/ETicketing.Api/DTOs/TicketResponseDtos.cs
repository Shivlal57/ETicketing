namespace ETicketing.Api.DTOs
{
    public class TicketResponseDtos
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int AvailableQuota { get; set; }
    }
}
