namespace TeknofestAsistan.Infrastructure.AI;

public class ClaudeOptions
{
    public const string SectionName = "Claude";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-haiku-4-5-20251001";
}
