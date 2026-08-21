using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "DestekEkibi,SistemYoneticisi")]
public class SupportTicketsController(ISupportTicketService supportTicketService) : ControllerBase
{
    private readonly ISupportTicketService _supportTicketService = supportTicketService;

    [HttpGet("open")]
    public async Task<ActionResult<PagedResultDto<SupportTicketDto>>> GetOpen(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        Ok(await _supportTicketService.GetOpenAsync(pageNumber, pageSize, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupportTicketDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var ticket = await _supportTicketService.GetByIdAsync(id, cancellationToken);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost("{id:int}/assign")]
    public async Task<ActionResult<SupportTicketDto>> Assign(int id, AssignSupportTicketDto dto, CancellationToken cancellationToken)
    {
        var updated = await _supportTicketService.AssignAsync(id, dto, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("{id:int}/resolve")]
    public async Task<ActionResult<SupportTicketDto>> Resolve(int id, ResolveSupportTicketDto dto, CancellationToken cancellationToken)
    {
        var updated = await _supportTicketService.ResolveAsync(id, dto, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}
