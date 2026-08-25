using Microsoft.Extensions.Options;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.Infrastructure.AI;

/// <summary>Singleton so the cache survives across requests. Live checks are heavily cached and
/// mostly superseded by real usage results (see Record*Result) — this deliberately avoids polling
/// Ollama/Claude on every sidebar render, which would add background traffic (and, for Claude,
/// real cost) for a feature that's purely informational.</summary>
public class SystemStatusService(
    IHttpClientFactory httpClientFactory,
    IOptions<OllamaOptions> ollamaOptions,
    IOptions<ClaudeOptions> claudeOptions) : ISystemStatusService
{
    private static readonly TimeSpan OllamaCacheDuration = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan ClaudeCacheDuration = TimeSpan.FromMinutes(5);

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly OllamaOptions _ollamaOptions = ollamaOptions.Value;
    private readonly ClaudeOptions _claudeOptions = claudeOptions.Value;

    private readonly SemaphoreSlim _ollamaLock = new(1, 1);
    private readonly SemaphoreSlim _claudeLock = new(1, 1);

    private DateTime _ollamaLastCheck = DateTime.MinValue;
    private volatile bool _ollamaLastResult;
    private DateTime _claudeLastCheck = DateTime.MinValue;
    private volatile bool _claudeLastResult;

    public void RecordOllamaResult(bool success)
    {
        _ollamaLastResult = success;
        _ollamaLastCheck = DateTime.UtcNow;
    }

    public void RecordClaudeResult(bool success)
    {
        _claudeLastResult = success;
        _claudeLastCheck = DateTime.UtcNow;
    }

    public async Task<SystemStatusDto> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var ollama = await GetOllamaStatusAsync(cancellationToken);
        var claude = await GetClaudeStatusAsync(cancellationToken);
        return new SystemStatusDto(ollama, claude, TemelArama: true);
    }

    private async Task<bool> GetOllamaStatusAsync(CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow - _ollamaLastCheck < OllamaCacheDuration) return _ollamaLastResult;

        await _ollamaLock.WaitAsync(cancellationToken);
        try
        {
            if (DateTime.UtcNow - _ollamaLastCheck < OllamaCacheDuration) return _ollamaLastResult;

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                var response = await client.GetAsync($"{_ollamaOptions.BaseUrl}/api/tags", cancellationToken);
                RecordOllamaResult(response.IsSuccessStatusCode);
            }
            catch
            {
                RecordOllamaResult(false);
            }
            return _ollamaLastResult;
        }
        finally
        {
            _ollamaLock.Release();
        }
    }

    private async Task<bool> GetClaudeStatusAsync(CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow - _claudeLastCheck < ClaudeCacheDuration) return _claudeLastResult;
        if (string.IsNullOrEmpty(_claudeOptions.ApiKey)) return false;

        await _claudeLock.WaitAsync(cancellationToken);
        try
        {
            if (DateTime.UtcNow - _claudeLastCheck < ClaudeCacheDuration) return _claudeLastResult;

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.anthropic.com/v1/models");
                request.Headers.Add("x-api-key", _claudeOptions.ApiKey);
                request.Headers.Add("anthropic-version", "2023-06-01");
                var response = await client.SendAsync(request, cancellationToken);
                RecordClaudeResult(response.IsSuccessStatusCode);
            }
            catch
            {
                RecordClaudeResult(false);
            }
            return _claudeLastResult;
        }
        finally
        {
            _claudeLock.Release();
        }
    }
}
