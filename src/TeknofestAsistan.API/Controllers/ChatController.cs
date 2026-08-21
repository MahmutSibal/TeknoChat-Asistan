using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(IChatQueryService chatQueryService) : ControllerBase
{
    private readonly IChatQueryService _chatQueryService = chatQueryService;

    /// <summary>Competitors only — this is the competitor-facing Q&amp;A entry point.</summary>
    [HttpPost("ask")]
    [Authorize(Roles = "Yarismaci")]
    public async Task<ActionResult<ChatQueryResponseDto>> Ask(ChatQueryRequestDto dto, CancellationToken cancellationToken)
    {
        // UserId always comes from the authenticated token, never from client input, so a
        // competitor cannot attribute a question to another user.
        var effectiveDto = dto with { UserId = CurrentUserId };
        return Ok(await _chatQueryService.AskAsync(effectiveDto, cancellationToken));
    }

    /// <summary>Staff only — returns every competitor's questions for the competition, so a
    /// competitor account must never reach this (it would leak other competitors' questions).</summary>
    [HttpGet("history")]
    [Authorize(Roles = "DestekEkibi,IcerikYoneticisi,SistemYoneticisi")]
    public async Task<ActionResult<IReadOnlyList<ChatQueryResponseDto>>> GetHistory(
        [FromQuery] int competitionId, CancellationToken cancellationToken) =>
        Ok(await _chatQueryService.GetHistoryAsync(competitionId, cancellationToken));

    /// <summary>Competitors only — their own question history, scoped to their own user id from
    /// the token so they can never see anyone else's questions.</summary>
    [HttpGet("my-history")]
    [Authorize(Roles = "Yarismaci")]
    public async Task<ActionResult<IReadOnlyList<ChatQueryResponseDto>>> GetMyHistory(
        [FromQuery] int competitionId, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        return Ok(await _chatQueryService.GetMyHistoryAsync(userId, competitionId, cancellationToken));
    }

    private int? CurrentUserId
    {
        get
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim is null ? null : int.Parse(claim);
        }
    }
}
