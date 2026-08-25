namespace TeknofestAsistan.Domain.Enums;

/// <summary>Which RAG tier produced the answer — lets the competitor see when the system degraded
/// from the primary local AI to the cloud fallback, or all the way to the dependency-free backend
/// search (e.g. both AI tiers unreachable).</summary>
public enum AnswerMode
{
    YapayZeka = 0,
    TemelArama = 1,
    ClaudeBulut = 2
}
