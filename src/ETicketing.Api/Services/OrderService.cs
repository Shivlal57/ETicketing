using ETicketing.Api.Data;
using ETicketing.Api.DTOs;
using ETicketing.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETicketing.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _dbContext;
        public OrderService(AppDbContext dbContext = null)
        {
            _dbContext = dbContext;
        }
        public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request,CancellationToken cancellationToken)
        {
            if (request.Quantity < 1 || request.Quantity > 10)
            {
                throw new ArgumentException(
                    "Quantity must be between 1 and 10.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var affectedRows = await _dbContext.Tickets
                    .Where(t =>
                        t.Id == request.TicketId &&
                        t.IsActive &&
                        t.AvailableQuota >= request.Quantity)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            t => t.AvailableQuota,
                            t => t.AvailableQuota - request.Quantity),
                        cancellationToken);

                if (affectedRows == 0)
                {
                    var ticketExists = await _dbContext.Tickets
                        .AnyAsync(
                            t => t.Id == request.TicketId && t.IsActive,
                            cancellationToken);

                    throw new InvalidOperationException(
                        ticketExists
                            ? "Not enough tickets available."
                            : "Ticket not found.");
                }
                var ticket = await _dbContext.Tickets
                    .AsNoTracking()
                    .Where(t => t.Id == request.TicketId)
                    .Select(t => new { t.Id, t.Name, t.Price })
                    .SingleAsync(cancellationToken);

                var order = new Order
                {
                    TicketId = ticket.Id,
                    Quantity = request.Quantity,
                    UnitPrice = ticket.Price,
                    TotalAmount = ticket.Price * request.Quantity,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Orders.Add(order);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new OrderResponseDto
                {
                    Id = order.Id,
                    TicketId = order.TicketId,
                    TicketName = ticket.Name,
                    Quantity = order.Quantity,
                    UnitPrice = order.UnitPrice,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    CreatedAt = order.CreatedAt
                };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
