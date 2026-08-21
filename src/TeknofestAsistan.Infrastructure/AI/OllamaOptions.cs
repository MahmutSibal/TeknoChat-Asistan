namespace TeknofestAsistan.Infrastructure.AI;

public class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ChatModel { get; set; } = "qwen2.5:7b";
    public string EmbeddingModel { get; set; } = "qwen2.5:7b";
}
