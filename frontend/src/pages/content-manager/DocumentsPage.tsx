import { useEffect, useState } from "react";
import { documentsApi } from "../../api/resources";
import { useCompetitions } from "../../context/CompetitionContext";
import { useAuth } from "../../context/AuthContext";
import { ApiError } from "../../api/client";
import { SourceDocumentType, type SourceDocument } from "../../types/api";
import { formatDateTime } from "../../lib/labels";
import { EmptyState, SkeletonList, Spinner } from "../../components/ui";
import { FileX } from "lucide-react";

const documentTypeLabels: Record<SourceDocumentType, string> = {
  [SourceDocumentType.Sartname]: "Şartname",
  [SourceDocumentType.Kilavuz]: "Kılavuz",
  [SourceDocumentType.Sss]: "SSS",
  [SourceDocumentType.Diger]: "Diğer",
};

export function DocumentsPage() {
  const { selectedCompetitionId } = useCompetitions();
  const { user } = useAuth();

  const [documents, setDocuments] = useState<SourceDocument[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  const [title, setTitle] = useState("");
  const [docType, setDocType] = useState<SourceDocumentType>(SourceDocumentType.Sartname);
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [reembedding, setReembedding] = useState(false);

  const load = () => {
    if (!selectedCompetitionId) return;
    setLoading(true);
    documentsApi
      .list(selectedCompetitionId)
      .then((res) => setDocuments(res.items))
      .finally(() => setLoading(false));
  };

  useEffect(load, [selectedCompetitionId]);

  const handleUpload = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedCompetitionId || !user || !file) return;
    setError(null);
    setInfo(null);
    setUploading(true);
    try {
      await documentsApi.uploadFile(
        {
          Title: title,
          DocumentType: docType,
          CompetitionId: selectedCompetitionId,
          UploadedByUserId: user.userId,
        },
        file,
      );
      setTitle("");
      setFile(null);
      (document.getElementById("file-input") as HTMLInputElement | null)?.value != null &&
        ((document.getElementById("file-input") as HTMLInputElement).value = "");
      setInfo("Doküman yüklendi ve işleniyor.");
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Yükleme başarısız oldu.");
    } finally {
      setUploading(false);
    }
  };

  const handleDeactivate = async (id: number) => {
    await documentsApi.deactivate(id);
    load();
  };

  const handleReembed = async () => {
    if (!selectedCompetitionId) return;
    setReembedding(true);
    setInfo(null);
    try {
      const res = await documentsApi.reembedMissing(selectedCompetitionId);
      setInfo(`${res.chunksFixed} parça yeniden işlendi.`);
      load();
    } finally {
      setReembedding(false);
    }
  };

  return (
    <div className="mx-auto max-w-4xl px-4 py-6">
      <div className="mb-6 flex items-center justify-between">
        <h2 className="text-lg font-semibold" style={{ color: "var(--color-text)" }}>
          Dokümanlar
        </h2>
        <button
          onClick={handleReembed}
          disabled={reembedding}
          className="flex items-center gap-2 rounded-lg border px-3 py-1.5 text-sm transition-transform hover:-translate-y-0.5 disabled:opacity-50 disabled:hover:translate-y-0"
          style={{ borderColor: "var(--color-border)", color: "var(--color-text)" }}
        >
          {reembedding && <Spinner />}
          {reembedding ? "İşleniyor…" : "Eksik embedding'leri yenile"}
        </button>
      </div>

      {error && (
        <p className="mb-3 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-950/40 dark:text-red-400">
          {error}
        </p>
      )}
      {info && (
        <p className="mb-3 rounded-lg bg-green-50 px-3 py-2 text-sm text-green-700 dark:bg-green-950/40 dark:text-green-400">
          {info}
        </p>
      )}

      <form onSubmit={handleUpload} className="card-modern mb-6 space-y-3 p-4">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <input
            required
            placeholder="Başlık"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="rounded-lg border px-3 py-2 text-sm"
            style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
          />
          <select
            value={docType}
            onChange={(e) => setDocType(Number(e.target.value) as SourceDocumentType)}
            className="rounded-lg border px-3 py-2 text-sm"
            style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
          >
            {Object.entries(documentTypeLabels).map(([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ))}
          </select>
        </div>
        <input
          id="file-input"
          required
          type="file"
          accept=".pdf,.docx,.txt"
          onChange={(e) => setFile(e.target.files?.[0] ?? null)}
          className="w-full text-sm"
          style={{ color: "var(--color-text)" }}
        />
        <p className="text-xs" style={{ color: "var(--color-text-muted)" }}>
          Desteklenen türler: PDF, DOCX, TXT
        </p>
        <button
          type="submit"
          disabled={uploading}
          className="flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-medium text-white transition-transform hover:-translate-y-0.5 disabled:opacity-50 disabled:hover:translate-y-0"
          style={{ background: "var(--color-accent)" }}
        >
          {uploading && <Spinner />}
          {uploading ? "Yükleniyor…" : "Yükle"}
        </button>
      </form>

      {loading ? (
        <SkeletonList rows={3} />
      ) : (
        <div className="space-y-2">
          {documents.map((doc) => (
            <div key={doc.id} className="card-modern card-hover flex items-center justify-between p-3">
              <div>
                <p className="text-sm font-medium" style={{ color: "var(--color-text)" }}>
                  {doc.title}{" "}
                  {!doc.isActive && (
                    <span className="ml-2 rounded-full bg-gray-200 px-2 py-0.5 text-xs text-gray-600 dark:bg-gray-700 dark:text-gray-300">
                      Pasif
                    </span>
                  )}
                </p>
                <p className="text-xs" style={{ color: "var(--color-text-muted)" }}>
                  {documentTypeLabels[doc.documentType]} · v{doc.version} · {formatDateTime(doc.createdAt)}
                </p>
              </div>
              {doc.isActive && (
                <button
                  onClick={() => handleDeactivate(doc.id)}
                  className="text-xs"
                  style={{ color: "var(--color-text-muted)" }}
                >
                  Pasife al
                </button>
              )}
            </div>
          ))}
          {documents.length === 0 && <EmptyState icon={FileX} text="Henüz doküman yok." />}
        </div>
      )}
    </div>
  );
}
