using System.ComponentModel.DataAnnotations;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Application.Dtos;

public record SupportTicketDto(
    int Id,
    int ChatQueryId,
    string QuestionText,
    int CompetitionId,
    int? AssignedToUserId,
    SupportTicketStatus Status,
    string? Resolution,
    DateTime CreatedAt,
    DateTime? ResolvedAt);

public record AssignSupportTicketDto([Range(1, int.MaxValue)] int AssignedToUserId);

public record ResolveSupportTicketDto([Required] string Resolution);
