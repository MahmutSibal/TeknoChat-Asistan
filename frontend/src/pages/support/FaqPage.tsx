import { useEffect, useState } from "react";
import { faqApi } from "../../api/resources";
import { useCompetitions } from "../../context/CompetitionContext";
import { useAuth } from "../../context/AuthContext";
import { ApiError } from "../../api/client";
import type { FaqEntry } from "../../types/api";
import { EmptyState, SkeletonList, Spinner } from "../../components/ui";
import { HelpCircle } from "lucide-react";

export function FaqPage() {
  const { selectedCompetitionId } = useCompetitions();
  const { user } = useAuth();
  const [entries, setEntries] = useState<FaqEntry[]>([]);
  const [loading, setLoading] = useState(false);
  const [question, setQuestion] = useState("");
  const [answer, setAnswer] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);

  const load = () => {
    if (!selectedCompetitionId) return;
    setLoading(true);
    faqApi
      .list(selectedCompetitionId)
      .then((res) => setEntries(res.items))
      .finally(() => setLoading(false));
  };

  useEffect(load, [selectedCompetitionId]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedCompetitionId || !user) return;
    setError(null);
    setCreating(true);
    try {
      await faqApi.create({
        question,
        answer,
        competitionId: selectedCompetitionId,
        createdByUserId: user.userId,
      });
      setQuestion("");
      setAnswer("");
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "SSS eklenemedi.");
    } finally {
      setCreating(false);
    }
  };

  return (
    <div className="mx-auto max-w-3xl px-4 py-6">
      <h2 className="mb-4 text-lg font-semibold" style={{ color: "var(--color-text)" }}>
        Sık Sorulan Sorular
      </h2>

      {error && (
        <p className="mb-3 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-950/40 dark:text-red-400">
          {error}
        </p>
      )}

      <form onSubmit={handleCreate} className="card-modern mb-6 space-y-2 p-4">
        <input
          required
          placeholder="Soru"
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
          className="w-full rounded-lg border px-3 py-2 text-sm"
          style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
        />
        <textarea
          required
          placeholder="Cevap"
          value={answer}
          onChange={(e) => setAnswer(e.target.value)}
          rows={3}
          className="w-full rounded-lg border px-3 py-2 text-sm"
          style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
        />
        <button
          type="submit"
          disabled={creating}
          className="flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-medium text-white transition-transform hover:-translate-y-0.5 disabled:opacity-50 disabled:hover:translate-y-0"
          style={{ background: "var(--color-accent)" }}
        >
          {creating && <Spinner size={12} />}
          Ekle
        </button>
      </form>

      {loading ? (
        <SkeletonList rows={3} />
      ) : (
        <div className="space-y-3">
          {entries.map((e) => (
            <div key={e.id} className="card-modern card-hover p-4">
              <p className="text-sm font-medium" style={{ color: "var(--color-text)" }}>
                {e.question}
              </p>
              <p className="mt-1 text-sm" style={{ color: "var(--color-text-muted)" }}>
                {e.answer}
              </p>
            </div>
          ))}
          {entries.length === 0 && <EmptyState icon={HelpCircle} text="SSS kaydı yok." />}
        </div>
      )}
    </div>
  );
}
