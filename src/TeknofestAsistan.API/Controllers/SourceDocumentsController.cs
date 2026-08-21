using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.API.Controllers;

/// <summary>Internal content management — competitors never browse raw documents directly, they go
/// through ChatController instead, so this whole controller is staff-only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "IcerikYoneticisi,SistemYoneticisi,DestekEkibi")]
public class SourceDocumentsController(ISourceDocumentService sourceDocumentService) : ControllerBase
{
    private readonly ISourceDocumentService _sourceDocumentService = sourceDocumentService;

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<SourceDocumentDto>>> GetAll(
        [FromQuery] int competitionId, [FromQuery] int? categoryId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        Ok(await _sourceDocumentService.GetAllAsync(competitionId, categoryId, pageNumber, pageSize, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SourceDocumentDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var document = await _sourceDocumentService.GetByIdAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(document);
    }

    [HttpPost]
    [Authorize(Roles = "IcerikYoneticisi,SistemYoneticisi")]
    public async Task<ActionResult<SourceDocumentDto>> Create(CreateSourceDocumentDto dto, CancellationToken cancellationToken)
    {
        var created = await _sourceDocumentService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Uploads a real şartname/kılavuz/SSS file (PDF, DOCX or TXT) — text is extracted
    /// server-side, then chunked and embedded exactly like the plain-text Create endpoint.</summary>
    [HttpPost("upload")]
    [Authorize(Roles = "IcerikYoneticisi,SistemYoneticisi")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<SourceDocumentDto>> Upload(
        [FromForm] UploadSourceDocumentDto dto, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { message = "Dosya boş." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var created = await _sourceDocumentService.CreateFromFileAsync(dto, stream, file.FileName, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex) when (ex is NotSupportedException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Re-generates embeddings for any chunk that doesn't have one — e.g. after switching
    /// the embedding model, or documents uploaded while Ollama's embedding endpoint was unreachable.</summary>
    [HttpPost("reembed-missing")]
    [Authorize(Roles = "IcerikYoneticisi,SistemYoneticisi")]
    public async Task<ActionResult<ReembedResultDto>> ReembedMissing(
        [FromQuery] int? competitionId, CancellationToken cancellationToken)
    {
        var fixedCount = await _sourceDocumentService.ReembedMissingChunksAsync(competitionId, cancellationToken);
        return Ok(new ReembedResultDto(fixedCount));
    }

    [HttpPost("{id:int}/deactivate")]
    [Authorize(Roles = "IcerikYoneticisi,SistemYoneticisi")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var success = await _sourceDocumentService.DeactivateAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
}
