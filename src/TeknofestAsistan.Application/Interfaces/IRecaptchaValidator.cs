namespace TeknofestAsistan.Application.Interfaces;

public interface IRecaptchaValidator
{
    /// <summary>False for an invalid, expired, or missing token — callers must reject the request
    /// rather than proceed, since this is the only real defense against automated form submission.</summary>
    Task<bool> ValidateAsync(string token, CancellationToken cancellationToken = default);
}
