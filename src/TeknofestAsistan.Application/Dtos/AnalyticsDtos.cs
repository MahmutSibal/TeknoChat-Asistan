using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Application.Dtos;

public record ConfidenceBucketDto(ConfidenceLevel Level, int Count);

public record TopQuestionDto(string QuestionText, int Count);

public record CompetitionAnalyticsDto(
    int CompetitionId,
    int TotalQuestions,
    int EscalatedCount,
    double EscalationRatePercent,
    IReadOnlyList<ConfidenceBucketDto> ConfidenceDistribution,
    IReadOnlyList<TopQuestionDto> TopQuestions,
    int OpenSupportTickets,
    int ResolvedSupportTickets);
