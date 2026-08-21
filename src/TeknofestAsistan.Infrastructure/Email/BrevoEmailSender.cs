using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.Infrastructure.Email;

/// <summary>Sends transactional email via Brevo's REST API (https://api.brevo.com) — no SMTP
/// server to run or configure, just an API key and a verified sender address.</summary>
public class BrevoEmailSender(HttpClient httpClient, IOptions<BrevoOptions> options, ILogger<BrevoEmailSender> logger) : IEmailSender
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly BrevoOptions _options = options.Value;
    private readonly ILogger<BrevoEmailSender> _logger = logger;

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var request = new BrevoEmailRequest(
            new BrevoContact(_options.SenderName, _options.SenderEmail),
            [new BrevoContact(toName, toEmail)],
            subject,
            htmlBody);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v3/smtp/email");
        httpRequest.Headers.Add("api-key", _options.ApiKey);
        httpRequest.Headers.Add("Accept", "application/json");
        httpRequest.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Brevo e-posta gönderimi başarısız: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException("E-posta gönderilemedi. Lütfen daha sonra tekrar deneyin.");
        }
    }

    private record BrevoContact(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("email")] string Email);

    private record BrevoEmailRequest(
        [property: JsonPropertyName("sender")] BrevoContact Sender,
        [property: JsonPropertyName("to")] IReadOnlyList<BrevoContact> To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("htmlContent")] string HtmlContent);
}
