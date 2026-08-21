import { AlertTriangle, CheckCircle2, FileText, Search } from "lucide-react";
import type { ChatQueryResponse } from "../types/api";
import { AnswerMode, ConfidenceLevel } from "../types/api";
import { confidenceLabels, confidenceColors, ticketStatusLabels, answerModeLabels } from "../lib/labels";

export function UserBubble({ text }: { text: string }) {
  return (
    <div className="flex justify-end">
      <div
        className="max-w-[75%] rounded-2xl rounded-br-sm px-4 py-2.5 text-sm text-white"
        style={{ background: "var(--color-bubble-user)" }}
      >
        {text}
      </div>
    </div>
  );
}

export function AiBubble({ response }: { response: ChatQueryResponse }) {
  return (
    <div className="flex justify-start">
      <div
        className="max-w-[80%] rounded-2xl rounded-bl-sm px-4 py-3 text-sm"
        style={{ background: "var(--color-bubble-ai)", color: "var(--color-text)" }}
      >
        {response.isEscalated ? (
          <div>
            <p className="mb-1 flex items-center gap-1.5 font-medium" style={{ color: "#f59e0b" }}>
              <AlertTriangle size={15} strokeWidth={2} aria-hidden />
              Destek ekibine yönlendirildi
            </p>
            <p style={{ color: "var(--color-text-muted)" }}>{response.escalationReason}</p>
            {response.supportTicketStatus !== null && response.supportTicketStatus !== undefined && (
              <p className="mt-2 text-xs" style={{ color: "var(--color-text-muted)" }}>
                Talep durumu: {ticketStatusLabels[response.supportTicketStatus]}
              </p>
            )}
            {response.supportResolution && (
              <div className="mt-2 rounded-lg border-l-2 pl-3" style={{ borderColor: "var(--color-accent)" }}>
                <p className="flex items-center gap-1.5 text-xs font-medium" style={{ color: "var(--color-text)" }}>
                  <CheckCircle2 size={13} strokeWidth={2} aria-hidden />
                  Destek ekibinin cevabı:
                </p>
                <p style={{ color: "var(--color-text)" }}>{response.supportResolution}</p>
              </div>
            )}
          </div>
        ) : (
          <div>
            {response.answerMode === AnswerMode.TemelArama && (
              <p
                className="mb-2 flex items-center gap-1.5 rounded-lg px-2 py-1 text-xs font-medium"
                style={{ background: "rgba(245, 158, 11, 0.12)", color: "#b45309" }}
              >
                <Search size={13} strokeWidth={2} aria-hidden />
                Yapay zeka şu anda ulaşılamıyor — bu yanıt, doğrulanmış kaynaklarda doğrudan arama ile bulundu.
              </p>
            )}
            <p className="whitespace-pre-wrap">{response.answerText}</p>

            <div className="mt-3 flex flex-wrap items-center gap-2">
              <span
                className="rounded-full px-2 py-0.5 text-xs font-medium text-white"
                style={{ background: confidenceColors[response.confidenceLevel] }}
              >
                Güven: {confidenceLabels[response.confidenceLevel]}
              </span>
              {response.answerMode !== null && (
                <span
                  className="rounded-full px-2 py-0.5 text-xs font-medium"
                  style={{ background: "var(--color-bg)", color: "var(--color-text-muted)" }}
                >
                  {answerModeLabels[response.answerMode]}
                </span>
              )}
            </div>

            {response.citations.length > 0 && (
              <div className="mt-2 space-y-1">
                {response.citations.map((c) => (
                  <div
                    key={c.sourceDocumentId}
                    className="flex items-center gap-1.5 rounded-lg px-2 py-1 text-xs"
                    style={{ background: "var(--color-bg)", color: "var(--color-text-muted)" }}
                  >
                    <FileText size={13} strokeWidth={1.75} aria-hidden />
                    {c.sourceTitle} · %{Math.round(c.relevanceScore * 100)} ilgili
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export function ConfidenceBadge({ level }: { level: ConfidenceLevel }) {
  return (
    <span
      className="rounded-full px-2 py-0.5 text-xs font-medium text-white"
      style={{ background: confidenceColors[level] }}
    >
      {confidenceLabels[level]}
    </span>
  );
}
