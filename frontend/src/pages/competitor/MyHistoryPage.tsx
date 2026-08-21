import { useEffect, useState } from "react";
import { chatApi } from "../../api/resources";
import { useCompetitions } from "../../context/CompetitionContext";
import { AiBubble, UserBubble } from "../../components/ChatBubble";
import type { ChatQueryResponse } from "../../types/api";
import { EmptyState, SkeletonList } from "../../components/ui";
import { History } from "lucide-react";

export function MyHistoryPage() {
  const { selectedCompetitionId } = useCompetitions();
  const [items, setItems] = useState<ChatQueryResponse[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!selectedCompetitionId) return;
    setLoading(true);
    chatApi
      .myHistory(selectedCompetitionId)
      .then(setItems)
      .finally(() => setLoading(false));
  }, [selectedCompetitionId]);

  return (
    <div className="mx-auto max-w-3xl px-4 py-6">
      <h2 className="mb-4 text-lg font-semibold" style={{ color: "var(--color-text)" }}>
        Geçmiş Sorularım
      </h2>

      {loading && <SkeletonList rows={3} rowClassName="h-24" />}
      {!loading && items.length === 0 && <EmptyState icon={History} text="Henüz bir soru sormadınız." />}

      <div className="space-y-4">
        {items.map((item) => (
          <div key={item.id} className="bubble-enter space-y-2">
            <UserBubble text={item.questionText} />
            <AiBubble response={item} />
          </div>
        ))}
      </div>
    </div>
  );
}
