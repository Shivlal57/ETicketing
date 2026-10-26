using ETicketing.Api.Data;
using ETicketing.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ETicketing.Api.Services
{
    public class TicketService : ITicketService
    {
        private readonly AppDbContext _dbContext;
        public TicketService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<TicketResponseDtos?> GetTicketByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbContext.Tickets
            .AsNoTracking()
            .Where(t => t.Id == id && t.IsActive)
            .Select(t => new TicketResponseDtos
            {
                Id = t.Id,
                Name = t.Name,
                Price = t.Price,
                AvailableQuota = t.AvailableQuota
            }).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<TicketResponseDtos>> GetTicketsAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Tickets
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Id)
            .Select(t => new TicketResponseDtos
            {
                Id = t.Id,
                Name = t.Name,
                Price = t.Price,
                AvailableQuota = t.AvailableQuota
            }).ToListAsync(cancellationToken);
        }
    }
}
