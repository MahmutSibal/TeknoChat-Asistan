using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.API.Controllers;

/// <summary>Sistem Yöneticisi monitoring view — never exposed to other roles.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SistemYoneticisi")]
public class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    private readonly IAnalyticsService _analyticsService = analyticsService;

    [HttpGet("competitions/{competitionId:int}")]
    public async Task<ActionResult<CompetitionAnalyticsDto>> GetCompetitionAnalytics(int competitionId, CancellationToken cancellationToken) =>
        Ok(await _analyticsService.GetCompetitionAnalyticsAsync(competitionId, cancellationToken));
}
