using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Domain.Entities;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Application.Services;

public class ChatQueryService(
    IUnitOfWork unitOfWork,
    IEmbeddingService embeddingService,
    [FromKeyedServices("ollama")] IAnswerGenerationService ollamaAnswerGenerationService,
    [FromKeyedServices("claude")] IAnswerGenerationService claudeAnswerGenerationService,
    ISystemStatusService systemStatusService,
    IRealtimeNotifier realtimeNotifier,
    ILogger<ChatQueryService> logger) : IChatQueryService
{
    private const double HighConfidenceThreshold = 0.75;
    private const double MediumConfidenceThreshold = 0.6;
    private const double LowConfidenceThreshold = 0.45;

    // Keyword-overlap fallback never claims true semantic understanding, so it caps out at "Orta".
    private const double KeywordMediumThreshold = 0.6;
    private const double KeywordLowThreshold = 0.35;

    private const int MaxCitations = 3;
    private const int ExtractiveAnswerMaxLength = 800;

    private const string InsufficientEvidenceReason =
        "Bu soruyu yanıtlamak için yeterli bilgi bulunamadı. Sorunuz destek ekibine yönlendirildi.";

    private const string AiAndFallbackFailedReason =
        "Yapay zeka asistanına şu anda ulaşılamıyor ve temel arama da yeterli bilgi bulamadı. " +
        "Sorunuz destek ekibine yönlendirildi, en kısa sürede yanıtlanacaktır.";

    private static readonly HashSet<string> TurkishStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "ve", "veya", "ile", "bir", "bu", "şu", "o", "mi", "mı", "mu", "mü",
        "midir", "mıdır", "mudur", "müdür", "de", "da", "ki", "için", "gibi",
        "ne", "nedir", "nasıl", "kaç", "hangi", "var", "yok", "olan", "olarak",
        "çok", "en", "daha", "bana", "sana", "beni", "seni", "diye", "acaba"
    };

    private static readonly char[] TokenSeparators =
        [' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']', '{', '}', '-', '/', '\\'];

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmbeddingService _embeddingService = embeddingService;
    private readonly IAnswerGenerationService _ollamaAnswerGenerationService = ollamaAnswerGenerationService;
    private readonly IAnswerGenerationService _claudeAnswerGenerationService = claudeAnswerGenerationService;
    private readonly ISystemStatusService _systemStatusService = systemStatusService;
    private readonly IRealtimeNotifier _realtimeNotifier = realtimeNotifier;
    private readonly ILogger<ChatQueryService> _logger = logger;

    public async Task<ChatQueryResponseDto> AskAsync(ChatQueryRequestDto dto, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var candidateChunks = await _unitOfWork.Repository<DocumentChunk>().FindAsync(
            c => c.SourceDocument.CompetitionId == dto.CompetitionId
                && (dto.CategoryId == null || c.SourceDocument.CategoryId == dto.CategoryId)
                && c.SourceDocument.IsActive
                && c.SourceDocument.ValidFrom <= now
                && (c.SourceDocument.ValidUntil == null || c.SourceDocument.ValidUntil >= now)
                && c.Embedding != null,
            cancellationToken);

        // The AI model (embeddings/answer generation) is an external dependency that can be
        // unreachable or misconfigured. If it fails, we never fabricate an answer or surface a 500
        // to the competitor — instead we degrade through progressively cheaper tiers (Ollama ->
        // Claude cloud -> dependency-free keyword search) over the same verified chunks. Only when
        // every tier finds nothing do we escalate to a human, and we tell the competitor plainly
        // which tier actually answered.
        List<(DocumentChunk Chunk, double Score)> scored = [];
        var semanticFailed = false;
        try
        {
            var questionEmbedding = await _embeddingService.GetEmbeddingAsync(dto.QuestionText, cancellationToken);
            scored = ScoreChunks(questionEmbedding, candidateChunks)
                .OrderByDescending(s => s.Score)
                .Take(MaxCitations)
                .ToList();
            _systemStatusService.RecordOllamaResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Soru embedding'i alınamadı (AI servisi erişilemez olabilir); temel arama moduna geçiliyor.");
            semanticFailed = true;
            _systemStatusService.RecordOllamaResult(false);
        }

        if (semanticFailed)
        {
            scored = ScoreChunksByKeyword(dto.QuestionText, candidateChunks)
                .OrderByDescending(s => s.Score)
                .Take(MaxCitations)
                .ToList();
        }

        var bestScore = scored.Count > 0 ? scored[0].Score : 0d;
        var confidence = semanticFailed
            ? ToKeywordConfidenceLevel(bestScore)
            : ToConfidenceLevel(bestScore);
        var isEscalated = confidence == ConfidenceLevel.Yetersiz;

        // No evidence clears the bar -> never fabricate an answer, hand off to a human instead.
        // Otherwise, try each generation tier in order — a tier only needs *some* context chunks
        // (semantic or keyword-scored), not Ollama's embeddings specifically, so Claude and the
        // extractive fallback are attempted here too even when retrieval itself fell back.
        string? answerText = null;
        AnswerMode? answerMode = null;
        if (!isEscalated)
        {
            var contextChunks = scored.Select(s => s.Chunk.Content).ToList();
            Func<string, Task>? onChunk = dto.UserId is { } askerId && dto.CorrelationId is { } correlationId
                ? chunk => SafeNotifyAsync(() => _realtimeNotifier.SendAnswerChunkAsync(askerId, correlationId, chunk, isFinal: false, cancellationToken))
                : null;

            try
            {
                answerText = await _ollamaAnswerGenerationService.GenerateAnswerAsync(dto.QuestionText, contextChunks, onChunk, cancellationToken);
                answerMode = AnswerMode.YapayZeka;
                _systemStatusService.RecordOllamaResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ollama cevabı üretilemedi (AI servisi erişilemez olabilir); Claude bulut yapay zekaya geçiliyor.");
                _systemStatusService.RecordOllamaResult(false);

                try
                {
                    answerText = await _claudeAnswerGenerationService.GenerateAnswerAsync(dto.QuestionText, contextChunks, onChunk, cancellationToken);
                    answerMode = AnswerMode.ClaudeBulut;
                    _systemStatusService.RecordClaudeResult(true);
                }
                catch (Exception claudeEx)
                {
                    _logger.LogWarning(claudeEx, "Claude bulut yapay zeka da başarısız oldu; temel arama moduna geçiliyor.");
                    _systemStatusService.RecordClaudeResult(false);
                    answerMode = AnswerMode.TemelArama;
                    answerText = BuildExtractiveAnswer(scored);
                }
            }

            if (onChunk is not null)
            {
                await SafeNotifyAsync(() => _realtimeNotifier.SendAnswerChunkAsync(dto.UserId!.Value, dto.CorrelationId!, string.Empty, isFinal: true, cancellationToken));
            }
        }

        string? escalationReason = null;
        if (isEscalated)
        {
            escalationReason = semanticFailed ? AiAndFallbackFailedReason : InsufficientEvidenceReason;
        }

        var query = new ChatQuery
        {
            UserId = dto.UserId,
            CompetitionId = dto.CompetitionId,
            CategoryId = dto.CategoryId,
            QuestionText = dto.QuestionText,
            ConfidenceLevel = confidence,
            IsEscalated = isEscalated,
            AnswerText = answerText,
            EscalationReason = isEscalated ? escalationReason : null,
            AnswerMode = isEscalated ? null : answerMode
        };

        await _unitOfWork.Repository<ChatQuery>().AddAsync(query, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var citationDtos = new List<CitationDto>();
        if (!isEscalated)
        {
            var sourceIds = scored.Select(s => s.Chunk.SourceDocumentId).Distinct().ToList();
            var sources = await _unitOfWork.Repository<SourceDocument>().FindAsync(d => sourceIds.Contains(d.Id), cancellationToken);
            var titleById = sources.ToDictionary(s => s.Id, s => s.Title);

            foreach (var s in scored)
            {
                var citation = new QuerySourceCitation
                {
                    ChatQueryId = query.Id,
                    SourceDocumentId = s.Chunk.SourceDocumentId,
                    RelevanceScore = s.Score
                };
                await _unitOfWork.Repository<QuerySourceCitation>().AddAsync(citation, cancellationToken);
                citationDtos.Add(new CitationDto(s.Chunk.SourceDocumentId, titleById.GetValueOrDefault(s.Chunk.SourceDocumentId, string.Empty), s.Score));
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var ticket = new SupportTicket
            {
                ChatQueryId = query.Id,
                Status = SupportTicketStatus.Acik
            };
            await _unitOfWork.Repository<SupportTicket>().AddAsync(ticket, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await SafeNotifyAsync(() => _realtimeNotifier.NotifyNewTicketAsync(ticket.Id, query.QuestionText, query.CompetitionId, cancellationToken));
        }

        return new ChatQueryResponseDto(
            query.Id, query.QuestionText, query.AnswerText, query.ConfidenceLevel, query.IsEscalated, citationDtos,
            isEscalated ? SupportTicketStatus.Acik : null, null, query.EscalationReason, query.AnswerMode);
    }

    /// <summary>SignalR is a UX nicety layered on top of the core flow — a push failure (no
    /// listener, transient transport error) must never fail the request or trigger escalation.</summary>
    private async Task SafeNotifyAsync(Func<Task> notify)
    {
        try
        {
            await notify();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gerçek zamanlı bildirim gönderilemedi (yok sayılıyor).");
        }
    }

    public Task<IReadOnlyList<ChatQueryResponseDto>> GetHistoryAsync(int competitionId, CancellationToken cancellationToken = default) =>
        BuildHistoryAsync(q => q.CompetitionId == competitionId, cancellationToken);

    public Task<IReadOnlyList<ChatQueryResponseDto>> GetMyHistoryAsync(int userId, int competitionId, CancellationToken cancellationToken = default) =>
        BuildHistoryAsync(q => q.CompetitionId == competitionId && q.UserId == userId, cancellationToken);

    private async Task<IReadOnlyList<ChatQueryResponseDto>> BuildHistoryAsync(
        Expression<Func<ChatQuery, bool>> predicate, CancellationToken cancellationToken)
    {
        var queries = await _unitOfWork.Repository<ChatQuery>().FindAsync(predicate, cancellationToken);
        var result = new List<ChatQueryResponseDto>();

        var escalatedQueryIds = queries.Where(q => q.IsEscalated).Select(q => q.Id).ToList();
        var tickets = escalatedQueryIds.Count > 0
            ? await _unitOfWork.Repository<SupportTicket>().FindAsync(t => escalatedQueryIds.Contains(t.ChatQueryId), cancellationToken)
            : [];
        var ticketByQueryId = tickets.ToDictionary(t => t.ChatQueryId);

        foreach (var q in queries.OrderByDescending(q => q.CreatedAt))
        {
            var citations = await _unitOfWork.Repository<QuerySourceCitation>().FindAsync(c => c.ChatQueryId == q.Id, cancellationToken);
            var sourceIds = citations.Select(c => c.SourceDocumentId).Distinct().ToList();
            var sources = await _unitOfWork.Repository<SourceDocument>().FindAsync(d => sourceIds.Contains(d.Id), cancellationToken);
            var titleById = sources.ToDictionary(s => s.Id, s => s.Title);

            var citationDtos = citations
                .Select(c => new CitationDto(c.SourceDocumentId, titleById.GetValueOrDefault(c.SourceDocumentId, string.Empty), c.RelevanceScore))
                .ToList();

            ticketByQueryId.TryGetValue(q.Id, out var ticket);

            result.Add(new ChatQueryResponseDto(
                q.Id, q.QuestionText, q.AnswerText, q.ConfidenceLevel, q.IsEscalated, citationDtos,
                ticket?.Status, ticket?.Resolution, q.EscalationReason, q.AnswerMode));
        }

        return result;
    }

    private static ConfidenceLevel ToConfidenceLevel(double score) => score switch
    {
        >= HighConfidenceThreshold => ConfidenceLevel.Yuksek,
        >= MediumConfidenceThreshold => ConfidenceLevel.Orta,
        >= LowConfidenceThreshold => ConfidenceLevel.Dusuk,
        _ => ConfidenceLevel.Yetersiz
    };

    private static ConfidenceLevel ToKeywordConfidenceLevel(double score) => score switch
    {
        >= KeywordMediumThreshold => ConfidenceLevel.Orta,
        >= KeywordLowThreshold => ConfidenceLevel.Dusuk,
        _ => ConfidenceLevel.Yetersiz
    };

    /// <summary>Dependency-free retrieval used when the AI embedding model is unreachable — scores
    /// chunks by the fraction of meaningful question words they contain, no external calls involved.</summary>
    private static IEnumerable<(DocumentChunk Chunk, double Score)> ScoreChunksByKeyword(
        string question, IReadOnlyList<DocumentChunk> chunks)
    {
        var questionTokens = Tokenize(question);
        if (questionTokens.Count == 0) yield break;

        foreach (var chunk in chunks)
        {
            var chunkTokens = Tokenize(chunk.Content);
            if (chunkTokens.Count == 0) continue;

            var overlap = questionTokens.Intersect(chunkTokens).Count();
            if (overlap == 0) continue;

            yield return (chunk, (double)overlap / questionTokens.Count);
        }
    }

    private static HashSet<string> Tokenize(string text)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        return text.ToLower(culture)
            .Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !TurkishStopWords.Contains(w))
            .ToHashSet();
    }

    /// <summary>Builds an answer without a generation model — the best-matching source text, verbatim
    /// and trimmed, used by the keyword fallback and whenever AI generation fails after retrieval succeeded.</summary>
    private static string? BuildExtractiveAnswer(IReadOnlyList<(DocumentChunk Chunk, double Score)> scored)
    {
        if (scored.Count == 0) return null;
        var text = scored[0].Chunk.Content.Trim();
        return text.Length > ExtractiveAnswerMaxLength ? text[..ExtractiveAnswerMaxLength] + "…" : text;
    }

    private static IEnumerable<(DocumentChunk Chunk, double Score)> ScoreChunks(float[] questionEmbedding, IReadOnlyList<DocumentChunk> chunks)
    {
        foreach (var chunk in chunks)
        {
            if (chunk.Embedding is null) continue;

            float[] chunkEmbedding;
            try
            {
                chunkEmbedding = JsonSerializer.Deserialize<float[]>(chunk.Embedding) ?? [];
            }
            catch (JsonException)
            {
                continue;
            }

            if (chunkEmbedding.Length == 0 || chunkEmbedding.Length != questionEmbedding.Length) continue;

            yield return (chunk, CosineSimilarity(questionEmbedding, chunkEmbedding));
        }
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0) return 0;
        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
