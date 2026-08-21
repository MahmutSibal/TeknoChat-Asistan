using System.ComponentModel.DataAnnotations;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Application.Dtos;

public record ChatQueryRequestDto(
    [Range(1, int.MaxValue)] int CompetitionId,
    int? CategoryId,
    [Required, MaxLength(2000)] string QuestionText,
    int? UserId,
    /// <summary>Client-generated id echoed back on each SignalR AnswerChunk so the caller can match
    /// streamed tokens to this specific question. Optional — streaming is skipped without it.</summary>
    string? CorrelationId = null);

public record CitationDto(int SourceDocumentId, string SourceTitle, double RelevanceScore);

// SupportTicketStatus/SupportResolution are only set when IsEscalated — lets a competitor see
// whether their support ticket is still open and, once resolved, read Destek Ekibi's answer via my-history.
public record ChatQueryResponseDto(
    int Id,
    string QuestionText,
    string? AnswerText,
    ConfidenceLevel ConfidenceLevel,
    bool IsEscalated,
    IReadOnlyList<CitationDto> Citations,
    SupportTicketStatus? SupportTicketStatus,
    string? SupportResolution,
    string? EscalationReason,
    AnswerMode? AnswerMode);
