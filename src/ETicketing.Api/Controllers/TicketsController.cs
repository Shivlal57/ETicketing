using ETicketing.Api.Data;
using ETicketing.Api.DTOs;
using ETicketing.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ETicketing.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketResponseDtos>>> GetTickets(CancellationToken cancellationToken)
        {
            var tickets = await _ticketService.GetTicketsAsync(cancellationToken);

            return Ok(tickets);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketResponseDtos>> GetTicketById(int id,CancellationToken cancellationToken)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id,cancellationToken);

            if (ticket is null)
            {
                return NotFound(new
                {
                    message = $"Ticket with ID {id} was not found."
                });
            }

            return Ok(ticket);
        }
    }
}
