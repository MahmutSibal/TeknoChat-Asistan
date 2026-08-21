import { useEffect, useState } from "react";
import { faqApi, ticketsApi } from "../../api/resources";
import { useAuth } from "../../context/AuthContext";
import { SupportTicketStatus, type SupportTicket } from "../../types/api";
import { ticketStatusLabels, formatDateTime } from "../../lib/labels";
import { EmptyState, SkeletonList, Spinner } from "../../components/ui";
import { Inbox } from "lucide-react";

export function TicketsPage() {
  const { user } = useAuth();
  const [tickets, setTickets] = useState<SupportTicket[]>([]);
  const [loading, setLoading] = useState(false);
  const [resolutionDrafts, setResolutionDrafts] = useState<Record<number, string>>({});
  const [addToFaqDrafts, setAddToFaqDrafts] = useState<Record<number, boolean>>({});
  const [busyId, setBusyId] = useState<number | null>(null);

  const load = () => {
    setLoading(true);
    ticketsApi
      .listOpen(1, 100)
      .then((res) => setTickets(res.items))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const handleAssignToMe = async (id: number) => {
    if (!user) return;
    setBusyId(id);
    try {
      await ticketsApi.assign(id, user.userId);
      load();
    } finally {
      setBusyId(null);
    }
  };

  const handleResolve = async (id: number) => {
    const resolution = resolutionDrafts[id]?.trim();
    if (!resolution || !user) return;
    const ticket = tickets.find((t) => t.id === id);
    setBusyId(id);
    try {
      await ticketsApi.resolve(id, resolution);
      if ((addToFaqDrafts[id] ?? true) && ticket) {
        await faqApi.create({
          question: ticket.questionText,
          answer: resolution,
          competitionId: ticket.competitionId,
          createdByUserId: user.userId,
          sourceChatQueryId: ticket.chatQueryId,
        });
      }
      load();
    } finally {
      setBusyId(null);
    }
  };

  return (
    <div className="mx-auto max-w-3xl px-4 py-6">
      <h2 className="mb-4 text-lg font-semibold" style={{ color: "var(--color-text)" }}>
        Açık Destek Talepleri
      </h2>

      {loading && <SkeletonList rows={3} rowClassName="h-32" />}
      {!loading && tickets.length === 0 && <EmptyState icon={Inbox} text="Açık destek talebi yok." />}

      <div className="space-y-3">
        {tickets.map((t) => (
          <div key={t.id} className="card-modern p-4">
            <div className="mb-2 flex items-center justify-between">
              <span
                className="rounded-full px-2 py-0.5 text-xs font-medium"
                style={{
                  background: t.status === SupportTicketStatus.Acik ? "#f59e0b" : "#3b82f6",
                  color: "#fff",
                }}
              >
                {ticketStatusLabels[t.status]}
              </span>
              <span className="text-xs" style={{ color: "var(--color-text-muted)" }}>
                {formatDateTime(t.createdAt)}
              </span>
            </div>
            <p className="mb-3 text-sm" style={{ color: "var(--color-text)" }}>
              {t.questionText}
            </p>

            {t.status === SupportTicketStatus.Acik && (
              <button
                onClick={() => handleAssignToMe(t.id)}
                disabled={busyId === t.id}
                className="mb-2 flex items-center gap-2 rounded-lg border px-3 py-1.5 text-xs transition-transform hover:-translate-y-0.5 disabled:opacity-50 disabled:hover:translate-y-0"
                style={{ borderColor: "var(--color-border)", color: "var(--color-text)" }}
              >
                {busyId === t.id && <Spinner size={12} />}
                Bana ata
              </button>
            )}

            <div className="flex gap-2">
              <textarea
                placeholder="Yanıt yazın…"
                value={resolutionDrafts[t.id] ?? ""}
                onChange={(e) => setResolutionDrafts((prev) => ({ ...prev, [t.id]: e.target.value }))}
                className="flex-1 rounded-lg border px-3 py-2 text-sm"
                style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
                rows={2}
              />
              <button
                onClick={() => handleResolve(t.id)}
                disabled={busyId === t.id || !resolutionDrafts[t.id]?.trim()}
                className="flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-medium text-white transition-transform hover:-translate-y-0.5 disabled:opacity-40 disabled:hover:translate-y-0"
                style={{ background: "var(--color-accent)" }}
              >
                {busyId === t.id && <Spinner size={12} />}
                Çöz
              </button>
            </div>
            <label className="mt-2 flex items-center gap-2 text-xs" style={{ color: "var(--color-text-muted)" }}>
              <input
                type="checkbox"
                checked={addToFaqDrafts[t.id] ?? true}
                onChange={(e) => setAddToFaqDrafts((prev) => ({ ...prev, [t.id]: e.target.checked }))}
              />
              Aynı zamanda SSS'e ekle (tekrarlayan sorular için önerilir)
            </label>
          </div>
        ))}
      </div>
    </div>
  );
}
