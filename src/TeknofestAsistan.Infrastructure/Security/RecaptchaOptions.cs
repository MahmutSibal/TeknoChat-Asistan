namespace TeknofestAsistan.Infrastructure.Security;

public class RecaptchaOptions
{
    public const string SectionName = "Recaptcha";

    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Shared secret that lets a local `npm run dev` frontend skip real verification.
    /// Only ever emitted by Vite when `import.meta.env.DEV` is true, which is dead-code-eliminated
    /// out of production builds — a real deployed frontend bundle never contains this value.</summary>
    public string DevBypassToken { get; set; } = string.Empty;
}
