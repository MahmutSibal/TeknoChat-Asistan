namespace TeknofestAsistan.Domain.Enums;

/// <summary>Which RAG path produced the answer — lets the competitor see when the system degraded
/// from full AI generation to the dependency-free backend fallback (e.g. Ollama unreachable).</summary>
public enum AnswerMode
{
    YapayZeka = 0,
    TemelArama = 1
}
