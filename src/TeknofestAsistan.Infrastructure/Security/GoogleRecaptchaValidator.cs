using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.Infrastructure.Security;

public class GoogleRecaptchaValidator(HttpClient httpClient, IOptions<RecaptchaOptions> options, ILogger<GoogleRecaptchaValidator> logger)
    : IRecaptchaValidator
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly RecaptchaOptions _options = options.Value;
    private readonly ILogger<GoogleRecaptchaValidator> _logger = logger;

    public async Task<bool> ValidateAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        if (!string.IsNullOrEmpty(_options.DevBypassToken) && token == _options.DevBypassToken)
        {
            return true;
        }

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = _options.SecretKey,
                ["response"] = token
            });

            var response = await _httpClient.PostAsync("/recaptcha/api/siteverify", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RecaptchaSiteVerifyResponse>(cancellationToken: cancellationToken);
            return result?.Success ?? false;
        }
        catch (Exception ex)
        {
            // Google's verification endpoint being unreachable must fail closed (reject the
            // request) — treating an outage as "valid" would defeat the whole point of this check.
            _logger.LogWarning(ex, "reCAPTCHA doğrulaması yapılamadı.");
            return false;
        }
    }

    private record RecaptchaSiteVerifyResponse([property: JsonPropertyName("success")] bool Success);
}
