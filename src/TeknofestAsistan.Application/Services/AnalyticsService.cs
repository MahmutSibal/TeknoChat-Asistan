using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Domain.Entities;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Application.Services;

public class AnalyticsService(IUnitOfWork unitOfWork) : IAnalyticsService
{
    private const int TopQuestionCount = 10;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<CompetitionAnalyticsDto> GetCompetitionAnalyticsAsync(int competitionId, CancellationToken cancellationToken = default)
    {
        var queries = await _unitOfWork.Repository<ChatQuery>().FindAsync(q => q.CompetitionId == competitionId, cancellationToken);

        var total = queries.Count;
        var escalated = queries.Count(q => q.IsEscalated);
        var escalationRate = total == 0 ? 0 : Math.Round(100.0 * escalated / total, 1);

        var confidenceDistribution = Enum.GetValues<ConfidenceLevel>()
            .Select(level => new ConfidenceBucketDto(level, queries.Count(q => q.ConfidenceLevel == level)))
            .ToList();

        var topQuestions = queries
            .GroupBy(q => q.QuestionText.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new TopQuestionDto(g.First().QuestionText.Trim(), g.Count()))
            .OrderByDescending(t => t.Count)
            .Take(TopQuestionCount)
            .ToList();

        var queryIds = queries.Select(q => q.Id).ToList();
        var tickets = await _unitOfWork.Repository<SupportTicket>().FindAsync(t => queryIds.Contains(t.ChatQueryId), cancellationToken);
        var openTickets = tickets.Count(t => t.Status is SupportTicketStatus.Acik or SupportTicketStatus.Islemde);
        var resolvedTickets = tickets.Count(t => t.Status is SupportTicketStatus.Cozuldu or SupportTicketStatus.Kapatildi);

        return new CompetitionAnalyticsDto(
            competitionId, total, escalated, escalationRate,
            confidenceDistribution, topQuestions, openTickets, resolvedTickets);
    }
}
