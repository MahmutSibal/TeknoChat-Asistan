using TeknofestAsistan.Application.Dtos;

namespace TeknofestAsistan.Application.Interfaces;

public interface IChatQueryService
{
    /// <summary>
    /// Answers a competitor's natural-language question using only verified, active source documents
    /// scoped to the selected competition/category. When confidence is insufficient, no answer is
    /// fabricated: the query is escalated to a support ticket instead.
    /// </summary>
    Task<ChatQueryResponseDto> AskAsync(ChatQueryRequestDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatQueryResponseDto>> GetHistoryAsync(int competitionId, CancellationToken cancellationToken = default);

    /// <summary>Scoped to a single competitor's own questions only — never exposes other users' data.</summary>
    Task<IReadOnlyList<ChatQueryResponseDto>> GetMyHistoryAsync(int userId, int competitionId, CancellationToken cancellationToken = default);
}
