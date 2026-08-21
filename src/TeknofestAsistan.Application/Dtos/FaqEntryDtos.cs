using System.ComponentModel.DataAnnotations;

namespace TeknofestAsistan.Application.Dtos;

public record FaqEntryDto(
    int Id,
    string Question,
    string Answer,
    int CompetitionId,
    int? CategoryId,
    bool IsActive,
    DateTime CreatedAt);

public record CreateFaqEntryDto(
    [Required, MaxLength(1000)] string Question,
    [Required] string Answer,
    [Range(1, int.MaxValue)] int CompetitionId,
    int? CategoryId,
    [Range(1, int.MaxValue)] int CreatedByUserId,
    int? SourceChatQueryId);
