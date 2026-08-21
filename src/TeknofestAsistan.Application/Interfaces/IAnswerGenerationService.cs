namespace TeknofestAsistan.Application.Interfaces;

/// <summary>
/// Composes a source-grounded answer from retrieved context chunks via the configured AI model (Ollama).
/// Never called when retrieval evidence is insufficient — that path escalates to a human instead.
/// </summary>
public interface IAnswerGenerationService
{
    /// <param name="onChunk">Optional — invoked once per incremental token as the model generates
    /// the answer, for live "typing" UX over SignalR. The full text is still returned at the end
    /// regardless of whether a callback is supplied.</param>
    Task<string> GenerateAnswerAsync(
        string question,
        IReadOnlyList<string> contextChunks,
        Func<string, Task>? onChunk = null,
        CancellationToken cancellationToken = default);
}
