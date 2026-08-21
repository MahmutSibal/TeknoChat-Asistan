namespace TeknofestAsistan.Application.Interfaces;

/// <summary>Produces semantic embedding vectors for text via the configured AI model (Ollama).</summary>
public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}
