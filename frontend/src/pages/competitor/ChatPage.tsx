import { useEffect, useRef, useState } from "react";
import { chatApi } from "../../api/resources";
import { ApiError } from "../../api/client";
import { useCompetitions } from "../../context/CompetitionContext";
import { useSignalR } from "../../context/SignalRContext";
import { UserBubble, AiBubble } from "../../components/ChatBubble";
import type { ChatQueryResponse } from "../../types/api";

interface Turn {
  question: string;
  correlationId: string;
  response: ChatQueryResponse | null;
  streamingText: string;
  loading: boolean;
}

export function ChatPage() {
  const { selectedCompetitionId, competitions } = useCompetitions();
  const { onAnswerChunk } = useSignalR();
  const [turns, setTurns] = useState<Turn[]>([]);
  const [input, setInput] = useState("");
  const [error, setError] = useState<string | null>(null);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [turns]);

  useEffect(
    () =>
      onAnswerChunk(({ correlationId, chunk, isFinal }) => {
        if (isFinal) return;
        setTurns((prev) => {
          const idx = prev.findIndex((t) => t.correlationId === correlationId);
          if (idx === -1) return prev;
          const copy = [...prev];
          copy[idx] = { ...copy[idx], streamingText: copy[idx].streamingText + chunk };
          return copy;
        });
      }),
    [onAnswerChunk],
  );

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!input.trim() || !selectedCompetitionId) return;

    const question = input.trim();
    const correlationId = crypto.randomUUID();
    setInput("");
    setError(null);
    setTurns((prev) => [...prev, { question, correlationId, response: null, streamingText: "", loading: true }]);

    try {
      const response = await chatApi.ask({ competitionId: selectedCompetitionId, questionText: question, correlationId });
      setTurns((prev) => {
        const copy = [...prev];
        const idx = copy.findIndex((t) => t.correlationId === correlationId);
        if (idx !== -1) copy[idx] = { ...copy[idx], response, loading: false };
        return copy;
      });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Soru gönderilemedi.");
      setTurns((prev) => prev.filter((t) => t.correlationId !== correlationId));
    }
  };

  if (competitions.length === 0) {
    return (
      <div className="flex h-full items-center justify-center p-6 text-center" style={{ color: "var(--color-text-muted)" }}>
        Henüz aktif bir yarışma bulunmuyor.
      </div>
    );
  }

  return (
    <div className="mx-auto flex h-full max-w-3xl flex-col">
      <div className="flex-1 space-y-4 overflow-y-auto px-4 py-6">
        {turns.length === 0 && (
          <div className="mt-16 text-center" style={{ color: "var(--color-text-muted)" }}>
            <p className="text-lg font-medium" style={{ color: "var(--color-text)" }}>
              Şartname veya kılavuzla ilgili sorunuzu yazın
            </p>
            <p className="mt-1 text-sm">Yanıtlar yalnızca doğrulanmış kaynaklara dayanır.</p>
          </div>
        )}

        {turns.map((turn) => (
          <div key={turn.correlationId} className="bubble-enter space-y-3">
            <UserBubble text={turn.question} />
            {turn.loading ? (
              turn.streamingText ? (
                <div className="flex justify-start">
                  <div
                    className="max-w-[80%] rounded-2xl rounded-bl-sm px-4 py-3 text-sm whitespace-pre-wrap"
                    style={{ background: "var(--color-bubble-ai)", color: "var(--color-text)" }}
                  >
                    {turn.streamingText}
                    <span className="animate-pulse">▌</span>
                  </div>
                </div>
              ) : (
                <div className="flex justify-start">
                  <div
                    className="rounded-2xl rounded-bl-sm px-4 py-3 text-sm"
                    style={{ background: "var(--color-bubble-ai)", color: "var(--color-text-muted)" }}
                  >
                    Yanıt hazırlanıyor…
                  </div>
                </div>
              )
            ) : (
              turn.response && <AiBubble response={turn.response} />
            )}
          </div>
        ))}
        <div ref={bottomRef} />
      </div>

      {error && (
        <p className="mx-4 mb-2 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-950/40 dark:text-red-400">
          {error}
        </p>
      )}

      <form onSubmit={handleSend} className="border-t p-4" style={{ borderColor: "var(--color-border)" }}>
        <div className="flex gap-2">
          <input
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Sorunuzu yazın…"
            className="flex-1 rounded-full border px-4 py-2.5 text-sm outline-none focus:ring-2"
            style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
          />
          <button
            type="submit"
            disabled={!input.trim()}
            className="rounded-full px-5 py-2.5 text-sm font-medium text-white disabled:opacity-40"
            style={{ background: "var(--color-accent)" }}
          >
            Gönder
          </button>
        </div>
      </form>
    </div>
  );
}
