using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.Infrastructure.AI;

/// <summary>Cloud fallback for answer generation — tried only when the primary local Ollama model
/// is unreachable, so this never runs on the common path and adds no traffic under normal
/// operation. Same grounding contract as the Ollama implementation.</summary>
public class ClaudeAnswerGenerationService(HttpClient httpClient, IOptions<ClaudeOptions> options) : IAnswerGenerationService
{
    private const string SystemPrompt =
        "Sen TEKNOFEST yarışmacılarına yardımcı olan bir destek asistanısın. " +
        "Yalnızca sana verilen KAYNAK METİNLER içindeki bilgiye dayanarak kısa bir yanıt ver. " +
        "Kaynaklarda yer almayan hiçbir bilgiyi uydurma veya tahmin etme. " +
        "Kaynaklar soruyu yanıtlamaya yetmiyorsa bunu açıkça belirt. " +
        "ÖNEMLİ: Yanıtının tamamını yalnızca Türkçe yaz. Başka hiçbir dilden " +
        "tek bir kelime veya karakter bile kullanma.";

    private readonly HttpClient _httpClient = httpClient;
    private readonly ClaudeOptions _options = options.Value;

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

        var request = new ClaudeMessagesRequest(
            _options.Model,
            1024,
            SystemPrompt,
            [new ClaudeMessage("user", userPrompt)],
            true);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/messages") { Content = JsonContent.Create(request) };
        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Anthropic streams Server-Sent Events — each "data: {...}" line is one event; we only
        // care about content_block_delta events, which carry the next slice of text.
        var fullText = new StringBuilder();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (!line.StartsWith("data: ", StringComparison.Ordinal)) continue;

            var json = line["data: ".Length..];
            ClaudeStreamEvent? evt;
            try
            {
                evt = JsonSerializer.Deserialize<ClaudeStreamEvent>(json);
            }
            catch (JsonException)
            {
                continue;
            }

            var text = evt?.Delta?.Text;
            if (evt?.Type == "content_block_delta" && !string.IsNullOrEmpty(text))
            {
                fullText.Append(text);
                if (onChunk is not null)
                {
                    await onChunk(text);
                }
            }
        }

        return fullText.ToString().Trim();
    }

    private record ClaudeMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private record ClaudeMessagesRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("system")] string System,
        [property: JsonPropertyName("messages")] IReadOnlyList<ClaudeMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream);

    private record ClaudeStreamDelta([property: JsonPropertyName("text")] string? Text);

    private record ClaudeStreamEvent(
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("delta")] ClaudeStreamDelta? Delta);
}
