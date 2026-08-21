using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.Infrastructure.AI;

public class OllamaAnswerGenerationService(HttpClient httpClient, IOptions<OllamaOptions> options) : IAnswerGenerationService
{
    private const string SystemPrompt =
        "Sen TEKNOFEST yarışmacılarına yardımcı olan bir destek asistanısın. " +
        "Yalnızca sana verilen KAYNAK METİNLER içindeki bilgiye dayanarak kısa bir yanıt ver. " +
        "Kaynaklarda yer almayan hiçbir bilgiyi uydurma veya tahmin etme. " +
        "Kaynaklar soruyu yanıtlamaya yetmiyorsa bunu açıkça belirt. " +
        "ÖNEMLİ: Yanıtının tamamını yalnızca Türkçe yaz. Başka hiçbir dilden (İngilizce, Çince vb.) " +
        "tek bir kelime veya karakter bile kullanma.";

    private readonly HttpClient _httpClient = httpClient;
    private readonly OllamaOptions _options = options.Value;

    public async Task<string> GenerateAnswerAsync(
        string question,
        IReadOnlyList<string> contextChunks,
        Func<string, Task>? onChunk = null,
        CancellationToken cancellationToken = default)
    {
        var context = new StringBuilder();
        for (var i = 0; i < contextChunks.Count; i++)
        {
            context.AppendLine($"[Kaynak {i + 1}]");
            context.AppendLine(contextChunks[i]);
            context.AppendLine();
        }

        var userPrompt = $"KAYNAK METİNLER:\n{context}\nSORU: {question}";

        var request = new OllamaChatRequest(
            _options.ChatModel,
            [
                new OllamaChatMessage("system", SystemPrompt),
                new OllamaChatMessage("user", userPrompt)
            ],
            true);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/chat") { Content = JsonContent.Create(request) };
        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Ollama streams one JSON object per line (NDJSON), each carrying the next token in
        // message.content, until a final line with done:true.
        var fullText = new StringBuilder();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            OllamaChatResponse? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line);
            }
            catch (JsonException)
            {
                continue;
            }

            if (chunk is null) continue;

            var text = chunk.Message?.Content;
            if (!string.IsNullOrEmpty(text))
            {
                fullText.Append(text);
                if (onChunk is not null)
                {
                    await onChunk(text);
                }
            }

            if (chunk.Done)
            {
                break;
            }
        }

        return fullText.ToString().Trim();
    }

    private record OllamaChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private record OllamaChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<OllamaChatMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream);

    private record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaChatMessage? Message,
        [property: JsonPropertyName("done")] bool Done);
}
