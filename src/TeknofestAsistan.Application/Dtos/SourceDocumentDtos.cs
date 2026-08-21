using System.ComponentModel.DataAnnotations;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Application.Dtos;

public record SourceDocumentDto(
    int Id,
    string Title,
    SourceDocumentType DocumentType,
    int CompetitionId,
    int? CategoryId,
    string? FileName,
    int UploadedByUserId,
    DateTime ValidFrom,
    DateTime? ValidUntil,
    bool IsActive,
    int Version,
    DateTime CreatedAt);

public record CreateSourceDocumentDto(
    [Required, MaxLength(300)] string Title,
    SourceDocumentType DocumentType,
    [Range(1, int.MaxValue)] int CompetitionId,
    int? CategoryId,
    string? FileName,
    [Required] string Content,
    [Range(1, int.MaxValue)] int UploadedByUserId,
    DateTime? ValidFrom,
    DateTime? ValidUntil);

/// <summary>Metadata that accompanies an uploaded file — the actual content comes from the file
/// itself (extracted server-side), not from this DTO.</summary>
public record UploadSourceDocumentDto(
    [Required, MaxLength(300)] string Title,
    SourceDocumentType DocumentType,
    [Range(1, int.MaxValue)] int CompetitionId,
    int? CategoryId,
    [Range(1, int.MaxValue)] int UploadedByUserId,
    DateTime? ValidFrom,
    DateTime? ValidUntil);

public record ReembedResultDto(int ChunksFixed);
