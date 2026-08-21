import { useEffect, useState } from "react";
import { categoriesApi, competitionsApi } from "../../api/resources";
import { useCompetitions } from "../../context/CompetitionContext";
import { useAuth } from "../../context/AuthContext";
import { ApiError } from "../../api/client";
import { UserRole, type Category } from "../../types/api";
import { EmptyState, Spinner } from "../../components/ui";
import { Tags } from "lucide-react";

export function CompetitionsAdminPage() {
  const { competitions, selectedCompetitionId, setSelectedCompetitionId, reload } = useCompetitions();
  const { user } = useAuth();
  const isSistemYoneticisi = user?.role === UserRole.SistemYoneticisi;

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [creatingCompetition, setCreatingCompetition] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [categories, setCategories] = useState<Category[]>([]);
  const [categoryName, setCategoryName] = useState("");
  const [creatingCategory, setCreatingCategory] = useState(false);

  const loadCategories = () => {
    if (!selectedCompetitionId) return;
    categoriesApi.listByCompetition(selectedCompetitionId).then(setCategories);
  };

  useEffect(loadCategories, [selectedCompetitionId]);

  const handleCreateCompetition = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setCreatingCompetition(true);
    try {
      await competitionsApi.create({ name, description: description || undefined });
      setName("");
      setDescription("");
      await reload();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Yarışma oluşturulamadı.");
    } finally {
      setCreatingCompetition(false);
    }
  };

  const handleCreateCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedCompetitionId) return;
    setError(null);
    setCreatingCategory(true);
    try {
      await categoriesApi.create({ competitionId: selectedCompetitionId, name: categoryName });
      setCategoryName("");
      loadCategories();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Kategori oluşturulamadı.");
    } finally {
      setCreatingCategory(false);
    }
  };

  return (
    <div className="mx-auto max-w-3xl px-4 py-6 space-y-8">
      {error && (
        <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-950/40 dark:text-red-400">
          {error}
        </p>
      )}

      {isSistemYoneticisi && (
        <section>
          <h2 className="mb-3 text-lg font-semibold" style={{ color: "var(--color-text)" }}>
            Yeni Yarışma
          </h2>
          <form onSubmit={handleCreateCompetition} className="flex flex-col gap-2 sm:flex-row">
            <input
              required
              placeholder="Yarışma adı"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="flex-1 rounded-lg border px-3 py-2 text-sm"
              style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
            />
            <input
              placeholder="Açıklama (opsiyonel)"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="flex-1 rounded-lg border px-3 py-2 text-sm"
              style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
            />
            <button
              type="submit"
              disabled={creatingCompetition}
              className="flex items-center justify-center gap-2 rounded-lg px-4 py-2 text-sm font-medium text-white transition-transform hover:-translate-y-0.5 disabled:opacity-50 disabled:hover:translate-y-0"
              style={{ background: "var(--color-accent)" }}
            >
              {creatingCompetition && <Spinner size={12} />}
              Oluştur
            </button>
          </form>
        </section>
      )}

      <section>
        <h2 className="mb-3 text-lg font-semibold" style={{ color: "var(--color-text)" }}>
          Yarışmalar
        </h2>
        <div className="space-y-2">
          {competitions.map((c) => (
            <button
              key={c.id}
              onClick={() => setSelectedCompetitionId(c.id)}
              className="card-modern card-hover block w-full p-3 text-left text-sm"
              style={{
                borderColor: c.id === selectedCompetitionId ? "var(--color-accent)" : "var(--color-border)",
                color: "var(--color-text)",
              }}
            >
              <span className="font-medium">{c.name}</span>
              {c.description && (
                <span className="ml-2" style={{ color: "var(--color-text-muted)" }}>
                  {c.description}
                </span>
              )}
            </button>
          ))}
        </div>
      </section>

      {selectedCompetitionId && (
        <section>
          <h2 className="mb-3 text-lg font-semibold" style={{ color: "var(--color-text)" }}>
            Kategoriler
          </h2>
          <form onSubmit={handleCreateCategory} className="mb-3 flex gap-2">
            <input
              required
              placeholder="Kategori adı"
              value={categoryName}
              onChange={(e) => setCategoryName(e.target.value)}
              className="flex-1 rounded-lg border px-3 py-2 text-sm"
              style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
            />
            <button
              type="submit"
              disabled={creatingCategory}
              className="flex items-center gap-2 rounded-lg border px-4 py-2 text-sm transition-transform hover:-translate-y-0.5 disabled:opacity-50 disabled:hover:translate-y-0"
              style={{ borderColor: "var(--color-border)", color: "var(--color-text)" }}
            >
              {creatingCategory && <Spinner size={12} />}
              Ekle
            </button>
          </form>
          <div className="flex flex-wrap gap-2">
            {categories.map((cat) => (
              <span
                key={cat.id}
                className="rounded-full border px-3 py-1 text-xs"
                style={{ borderColor: "var(--color-border)", color: "var(--color-text)" }}
              >
                {cat.name}
              </span>
            ))}
            {categories.length === 0 && <EmptyState icon={Tags} text="Kategori yok." />}
          </div>
        </section>
      )}
    </div>
  );
}
