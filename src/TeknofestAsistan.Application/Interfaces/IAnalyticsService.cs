using TeknofestAsistan.Application.Dtos;

namespace TeknofestAsistan.Application.Interfaces;

/// <summary>Feeds the Sistem Yöneticisi's monitoring view: escalation rate, confidence spread,
/// and frequently asked topics, so quality issues and content gaps surface without reading raw logs.</summary>
public interface IAnalyticsService
{
    Task<CompetitionAnalyticsDto> GetCompetitionAnalyticsAsync(int competitionId, CancellationToken cancellationToken = default);
}
