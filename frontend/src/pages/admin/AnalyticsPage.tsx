import { useEffect, useState } from "react";
import { analyticsApi } from "../../api/resources";
import { useCompetitions } from "../../context/CompetitionContext";
import type { CompetitionAnalytics } from "../../types/api";
import { confidenceLabels, confidenceColors } from "../../lib/labels";
import { Skeleton } from "../../components/ui";

function StatCard({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="card-modern card-hover p-4">
      <p className="text-2xl font-semibold" style={{ color: "var(--color-text)" }}>
        {value}
      </p>
      <p className="text-xs" style={{ color: "var(--color-text-muted)" }}>
        {label}
      </p>
    </div>
  );
}

export function AnalyticsPage() {
  const { selectedCompetitionId } = useCompetitions();
  const [data, setData] = useState<CompetitionAnalytics | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!selectedCompetitionId) return;
    setLoading(true);
    analyticsApi
      .competition(selectedCompetitionId)
      .then(setData)
      .finally(() => setLoading(false));
  }, [selectedCompetitionId]);

  if (loading || !data) {
    return (
      <div className="mx-auto max-w-3xl space-y-8 px-4 py-6">
        <Skeleton className="h-6 w-40" />
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-20" />
          ))}
        </div>
        <Skeleton className="h-32" />
      </div>
    );
  }

  const maxConfidenceCount = Math.max(1, ...data.confidenceDistribution.map((b) => b.count));
  const maxTopQuestionCount = Math.max(1, ...data.topQuestions.map((t) => t.count));

  return (
    <div className="mx-auto max-w-3xl px-4 py-6 space-y-8">
      <h2 className="text-lg font-semibold" style={{ color: "var(--color-text)" }}>
        Analiz Paneli
      </h2>

      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        <StatCard label="Toplam Soru" value={data.totalQuestions} />
        <StatCard label="Yönlendirilen" value={data.escalatedCount} />
        <StatCard label="Yönlendirme Oranı" value={`%${data.escalationRatePercent}`} />
        <StatCard label="Açık Talep" value={data.openSupportTickets} />
      </div>

      <section>
        <h3 className="mb-3 text-sm font-medium" style={{ color: "var(--color-text)" }}>
          Güven Seviyesi Dağılımı
        </h3>
        <div className="space-y-2">
          {data.confidenceDistribution.map((bucket) => (
            <div key={bucket.level} className="flex items-center gap-3">
              <span className="w-16 text-xs" style={{ color: "var(--color-text-muted)" }}>
                {confidenceLabels[bucket.level]}
              </span>
              <div className="h-3 flex-1 overflow-hidden rounded-full" style={{ background: "var(--color-bg-subtle)" }}>
                <div
                  className="h-full rounded-full"
                  style={{
                    width: `${(bucket.count / maxConfidenceCount) * 100}%`,
                    background: confidenceColors[bucket.level],
                  }}
                />
              </div>
              <span className="w-6 text-right text-xs" style={{ color: "var(--color-text-muted)" }}>
                {bucket.count}
              </span>
            </div>
          ))}
        </div>
      </section>

      <section>
        <h3 className="mb-3 text-sm font-medium" style={{ color: "var(--color-text)" }}>
          Sık Sorulan Konular
        </h3>
        <div className="space-y-2">
          {data.topQuestions.map((q, i) => (
            <div key={i}>
              <div className="mb-1 flex justify-between text-xs">
                <span style={{ color: "var(--color-text)" }}>{q.questionText}</span>
                <span style={{ color: "var(--color-text-muted)" }}>{q.count}</span>
              </div>
              <div className="h-2 overflow-hidden rounded-full" style={{ background: "var(--color-bg-subtle)" }}>
                <div
                  className="h-full rounded-full"
                  style={{ width: `${(q.count / maxTopQuestionCount) * 100}%`, background: "var(--color-accent)" }}
                />
              </div>
            </div>
          ))}
          {data.topQuestions.length === 0 && (
            <p style={{ color: "var(--color-text-muted)" }}>Henüz soru sorulmadı.</p>
          )}
        </div>
      </section>
    </div>
  );
}
