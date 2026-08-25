namespace TeknofestAsistan.Application.Interfaces;

public record SystemStatusDto(bool Ollama, bool ClaudeBulut, bool TemelArama);

/// <summary>Tracks whether each RAG tier is currently reachable, for display only (e.g. a sidebar
/// indicator) — never used to gate the actual answer flow, which always tries each tier live and
/// falls back on failure regardless of this cached read.</summary>
public interface ISystemStatusService
{
    Task<SystemStatusDto> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Called after a real Ollama attempt (embedding or generation) so the cached status
    /// reflects actual usage immediately, instead of waiting for the next background check.</summary>
    void RecordOllamaResult(bool success);

    /// <summary>Called after a real Claude attempt, same rationale as <see cref="RecordOllamaResult"/>.</summary>
    void RecordClaudeResult(bool success);
}
