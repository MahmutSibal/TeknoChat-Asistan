using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using TeknofestAsistan.Application.Interfaces;

namespace TeknofestAsistan.Infrastructure.Security;

public class GoogleTokenValidator(IOptions<GoogleOptions> options) : IGoogleTokenValidator
{
    private readonly GoogleOptions _options = options.Value;

    public async Task<GoogleProfile> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_options.ClientId]
            });
        }
        catch (InvalidJwtException ex)
        {
            throw new InvalidOperationException("Google kimlik doğrulaması geçersiz.", ex);
        }

        var fullName = payload.Name ?? $"{payload.GivenName} {payload.FamilyName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = payload.Email;
        }

        return new GoogleProfile(payload.Email, payload.EmailVerified, fullName);
    }
}
