namespace TeknofestAsistan.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "TeknofestAsistan";
    public string Audience { get; set; } = "TeknofestAsistan";
    public int ExpiryMinutes { get; set; } = 120;
}
