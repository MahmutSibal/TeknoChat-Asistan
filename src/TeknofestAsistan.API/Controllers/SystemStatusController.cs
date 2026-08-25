using Microsoft.AspNetCore.Mvc;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.API.Controllers;

/// <summary>Powers the sidebar's "which RAG tier is live right now" indicator. Results are cached
/// server-side (see SystemStatusService) — this endpoint can be polled freely without generating
/// extra traffic against Ollama or Claude.</summary>
[ApiController]
[Route("api/[controller]")]
public class SystemStatusController(ISystemStatusService systemStatusService) : ControllerBase
{
    private readonly ISystemStatusService _systemStatusService = systemStatusService;

    [HttpGet]
    public async Task<ActionResult<SystemStatusDto>> Get(CancellationToken cancellationToken) =>
        Ok(await _systemStatusService.GetStatusAsync(cancellationToken));
}
