namespace TeknofestAsistan.Application.Interfaces;

public record GoogleProfile(string Email, bool EmailVerified, string FullName);

public interface IGoogleTokenValidator
{
    /// <exception cref="InvalidOperationException">Token is invalid, expired, or its audience
    /// doesn't match our Google OAuth client — never trust an unvalidated token.</exception>
    Task<GoogleProfile> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
