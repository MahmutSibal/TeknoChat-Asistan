import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { competitionsApi } from "../api/resources";
import type { Competition } from "../types/api";
import { useAuth } from "./AuthContext";

interface CompetitionContextValue {
  competitions: Competition[];
  selectedCompetitionId: number | null;
  setSelectedCompetitionId: (id: number) => void;
  loading: boolean;
  reload: () => Promise<void>;
}

const CompetitionContext = createContext<CompetitionContextValue | undefined>(undefined);

const STORAGE_KEY = "teknofest_selected_competition";

export function CompetitionProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();
  const [competitions, setCompetitions] = useState<Competition[]>([]);
  const [selectedCompetitionId, setSelectedCompetitionIdState] = useState<number | null>(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? Number(stored) : null;
  });
  const [loading, setLoading] = useState(false);

  const setSelectedCompetitionId = (id: number) => {
    setSelectedCompetitionIdState(id);
    localStorage.setItem(STORAGE_KEY, String(id));
  };

  const reload = async () => {
    setLoading(true);
    try {
      const res = await competitionsApi.list(1, 100);
      setCompetitions(res.items);
      if (res.items.length > 0 && !res.items.some((c) => c.id === selectedCompetitionId)) {
        setSelectedCompetitionId(res.items[0].id);
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isAuthenticated) {
      reload();
    } else {
      setCompetitions([]);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isAuthenticated]);

  return (
    <CompetitionContext.Provider value={{ competitions, selectedCompetitionId, setSelectedCompetitionId, loading, reload }}>
      {children}
    </CompetitionContext.Provider>
  );
}

export function useCompetitions(): CompetitionContextValue {
  const ctx = useContext(CompetitionContext);
  if (!ctx) throw new Error("useCompetitions, CompetitionProvider içinde kullanılmalı");
  return ctx;
}
