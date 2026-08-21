using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompetitionsController(ICompetitionService competitionService) : ControllerBase
{
    private readonly ICompetitionService _competitionService = competitionService;

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<CompetitionDto>>> GetAll(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        Ok(await _competitionService.GetAllAsync(pageNumber, pageSize, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompetitionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var competition = await _competitionService.GetByIdAsync(id, cancellationToken);
        return competition is null ? NotFound() : Ok(competition);
    }

    [HttpPost]
    [Authorize(Roles = "SistemYoneticisi")]
    public async Task<ActionResult<CompetitionDto>> Create(CreateCompetitionDto dto, CancellationToken cancellationToken)
    {
        var created = await _competitionService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "SistemYoneticisi")]
    public async Task<ActionResult<CompetitionDto>> Update(int id, UpdateCompetitionDto dto, CancellationToken cancellationToken)
    {
        var updated = await _competitionService.UpdateAsync(id, dto, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}
