using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaqController(IFaqEntryService faqEntryService) : ControllerBase
{
    private readonly IFaqEntryService _faqEntryService = faqEntryService;

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<FaqEntryDto>>> GetAll(
        [FromQuery] int competitionId, [FromQuery] int? categoryId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        Ok(await _faqEntryService.GetAllAsync(competitionId, categoryId, pageNumber, pageSize, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "DestekEkibi,IcerikYoneticisi,SistemYoneticisi")]
    public async Task<ActionResult<FaqEntryDto>> Create(CreateFaqEntryDto dto, CancellationToken cancellationToken)
    {
        var created = await _faqEntryService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { competitionId = created.CompetitionId }, created);
    }
}
