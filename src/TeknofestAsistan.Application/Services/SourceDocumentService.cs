using System.Text.Json;
using Microsoft.Extensions.Logging;
using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Application.Dtos;
using TeknofestAsistan.Application.Interfaces;
using TeknofestAsistan.Domain.Entities;
using TeknofestAsistan.Domain.Enums;

namespace TeknofestAsistan.Application.Services;

public class SourceDocumentService(
    IUnitOfWork unitOfWork,
    IEmbeddingService embeddingService,
    IDocumentTextExtractor documentTextExtractor,
    ILogger<SourceDocumentService> logger) : ISourceDocumentService
{
    private const int ChunkSize = 800;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmbeddingService _embeddingService = embeddingService;
    private readonly IDocumentTextExtractor _documentTextExtractor = documentTextExtractor;
    private readonly ILogger<SourceDocumentService> _logger = logger;

    public async Task<PagedResultDto<SourceDocumentDto>> GetAllAsync(
        int competitionId, int? categoryId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        (pageNumber, pageSize) = Paging.Normalize(pageNumber, pageSize);
        var (items, totalCount) = await _unitOfWork.Repository<SourceDocument>().FindPagedAsync(
            d => d.CompetitionId == competitionId && (categoryId == null || d.CategoryId == categoryId),
            pageNumber, pageSize, cancellationToken);
        return new PagedResultDto<SourceDocumentDto>(items.Select(ToDto).ToList(), pageNumber, pageSize, totalCount);
    }

    public async Task<SourceDocumentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Repository<SourceDocument>().GetByIdAsync(id, cancellationToken);
        return document is null ? null : ToDto(document);
    }

    public Task<SourceDocumentDto> CreateAsync(CreateSourceDocumentDto dto, CancellationToken cancellationToken = default) =>
        CreateInternalAsync(
            dto.Title, dto.DocumentType, dto.CompetitionId, dto.CategoryId, dto.FileName,
            dto.Content, dto.UploadedByUserId, dto.ValidFrom, dto.ValidUntil, cancellationToken);

    public async Task<SourceDocumentDto> CreateFromFileAsync(
        UploadSourceDocumentDto dto, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        if (!_documentTextExtractor.CanExtract(fileName))
        {
            throw new NotSupportedException($"'{fileName}' dosya türü desteklenmiyor. Desteklenen türler: PDF, DOCX, TXT.");
        }

        var content = await _documentTextExtractor.ExtractTextAsync(fileStream, fileName, cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Dosyadan metin çıkarılamadı (dosya boş veya taranmış bir görüntü olabilir).");
        }

        return await CreateInternalAsync(
            dto.Title, dto.DocumentType, dto.CompetitionId, dto.CategoryId, fileName,
            content, dto.UploadedByUserId, dto.ValidFrom, dto.ValidUntil, cancellationToken);
    }

    private async Task<SourceDocumentDto> CreateInternalAsync(
        string title, SourceDocumentType documentType, int competitionId, int? categoryId,
        string? fileName, string content, int uploadedByUserId, DateTime? validFrom, DateTime? validUntil,
        CancellationToken cancellationToken)
    {
        var document = new SourceDocument
        {
            Title = title,
            DocumentType = documentType,
            CompetitionId = competitionId,
            CategoryId = categoryId,
            FileName = fileName,
            Content = content,
            UploadedByUserId = uploadedByUserId,
            ValidFrom = validFrom ?? DateTime.UtcNow,
            ValidUntil = validUntil,
            IsActive = true,
            Version = 1
        };

        document.Chunks = SplitIntoChunks(content)
            .Select((text, index) => new DocumentChunk { ChunkIndex = index, Content = text })
            .ToList();

        // Chunking is plain deterministic text splitting above; the AI model's job is to turn each
        // chunk into a semantic embedding so retrieval in ChatQueryService can do meaning-based search.
        foreach (var chunk in document.Chunks)
        {
            try
            {
                var vector = await _embeddingService.GetEmbeddingAsync(chunk.Content, cancellationToken);
                chunk.Embedding = JsonSerializer.Serialize(vector);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Chunk {ChunkIndex} için embedding üretilemedi (Ollama erişilemez olabilir); doküman embedding olmadan kaydedilecek.", chunk.ChunkIndex);
            }
        }

        await _unitOfWork.Repository<SourceDocument>().AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(document);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<SourceDocument>();
        var document = await repository.GetByIdAsync(id, cancellationToken);
        if (document is null) return false;

        document.IsActive = false;
        document.UpdatedAt = DateTime.UtcNow;
        repository.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> ReembedMissingChunksAsync(int? competitionId, CancellationToken cancellationToken = default)
    {
        var chunkRepository = _unitOfWork.Repository<DocumentChunk>();

        var missingChunks = competitionId is null
            ? await chunkRepository.FindAsync(c => c.Embedding == null, cancellationToken)
            : await chunkRepository.FindAsync(c => c.Embedding == null && c.SourceDocument.CompetitionId == competitionId, cancellationToken);

        var fixedCount = 0;
        foreach (var chunk in missingChunks)
        {
            try
            {
                var vector = await _embeddingService.GetEmbeddingAsync(chunk.Content, cancellationToken);
                chunk.Embedding = JsonSerializer.Serialize(vector);
                chunkRepository.Update(chunk);
                fixedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Chunk {ChunkId} yeniden embed edilemedi.", chunk.Id);
            }
        }

        if (fixedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return fixedCount;
    }

    private static IEnumerable<string> SplitIntoChunks(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            yield break;
        }

        var paragraphs = content.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries);
        var buffer = string.Empty;

        foreach (var paragraph in paragraphs)
        {
            var trimmed = paragraph.Trim();
            if (trimmed.Length == 0) continue;

            if (buffer.Length + trimmed.Length > ChunkSize && buffer.Length > 0)
            {
                yield return buffer;
                buffer = string.Empty;
            }

            buffer = buffer.Length == 0 ? trimmed : $"{buffer}\n\n{trimmed}";

            while (buffer.Length > ChunkSize)
            {
                yield return buffer[..ChunkSize];
                buffer = buffer[ChunkSize..];
            }
        }

        if (buffer.Length > 0)
        {
            yield return buffer;
        }
    }

    private static SourceDocumentDto ToDto(SourceDocument d) => new(
        d.Id, d.Title, d.DocumentType, d.CompetitionId, d.CategoryId, d.FileName,
        d.UploadedByUserId, d.ValidFrom, d.ValidUntil, d.IsActive, d.Version, d.CreatedAt);
}
