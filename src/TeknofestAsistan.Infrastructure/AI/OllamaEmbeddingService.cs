using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.Infrastructure.AI;

public class OllamaEmbeddingService(HttpClient httpClient, IOptions<OllamaOptions> options) : IEmbeddingService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OllamaOptions _options = options.Value;

    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new OllamaEmbeddingRequest(_options.EmbeddingModel, text);
        using var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Ollama embedding yanıtı boş döndü.");

        return result.Embedding;
    }

    private record OllamaEmbeddingRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt);

    private record OllamaEmbeddingResponse(
        [property: JsonPropertyName("embedding")] float[] Embedding);
}
