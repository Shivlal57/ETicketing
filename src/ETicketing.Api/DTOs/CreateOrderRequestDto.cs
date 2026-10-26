using System.ComponentModel.DataAnnotations;

namespace ETicketing.Api.DTOs
{
    public class CreateOrderRequestDto
    {
        [Range(1, int.MaxValue)]
        public int TicketId { get; set; }

        [Range(1, 10)]
        public int Quantity { get; set; }
    }
}
