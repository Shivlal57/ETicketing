using ETicketing.Api.DTOs;

namespace ETicketing.Api.Services
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketResponseDtos>> GetTicketsAsync(CancellationToken cancellationToken);

        Task<TicketResponseDtos?> GetTicketByIdAsync(int id,CancellationToken cancellationToken);
    }
}
