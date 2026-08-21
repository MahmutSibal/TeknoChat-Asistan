import { useState } from "react";
import { useAuth, roleLabels } from "../context/AuthContext";
import { authApi } from "../api/resources";
import { ApiError } from "../api/client";

export function ProfilePage() {
  const { user } = useAuth();
  const [step, setStep] = useState<"idle" | "code-sent">("idle");
  const [code, setCode] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  if (!user) return null;

  const handleRequestCode = async () => {
    setError(null);
    setInfo(null);
    setLoading(true);
    try {
      await authApi.forgotPassword({ email: user.email });
      setStep("code-sent");
      setInfo("E-postanıza bir doğrulama kodu gönderildi.");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Kod gönderilemedi.");
    } finally {
      setLoading(false);
    }
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setInfo(null);
    setLoading(true);
    try {
      await authApi.resetPassword({ email: user.email, resetToken: code, newPassword });
      setInfo("Şifreniz güncellendi.");
      setStep("idle");
      setCode("");
      setNewPassword("");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Şifre güncellenemedi.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="mx-auto max-w-2xl px-4 py-6">
      <h2 className="mb-6 text-lg font-semibold" style={{ color: "var(--color-text)" }}>
        Profil
      </h2>

      <div className="mb-8 rounded-xl border p-4" style={{ borderColor: "var(--color-border)" }}>
        <dl className="space-y-2 text-sm">
          <div className="flex justify-between">
            <dt style={{ color: "var(--color-text-muted)" }}>Ad Soyad</dt>
            <dd style={{ color: "var(--color-text)" }}>{user.fullName}</dd>
          </div>
          <div className="flex justify-between">
            <dt style={{ color: "var(--color-text-muted)" }}>E-posta</dt>
            <dd style={{ color: "var(--color-text)" }}>{user.email}</dd>
          </div>
          <div className="flex justify-between">
            <dt style={{ color: "var(--color-text-muted)" }}>Rol</dt>
            <dd style={{ color: "var(--color-text)" }}>{roleLabels[user.role]}</dd>
          </div>
        </dl>
      </div>

      <div className="rounded-xl border p-4" style={{ borderColor: "var(--color-border)" }}>
        <h3 className="mb-3 text-sm font-medium" style={{ color: "var(--color-text)" }}>
          Şifre Değiştir
        </h3>

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

        {step === "idle" ? (
          <button
            onClick={handleRequestCode}
            disabled={loading}
            className="rounded-lg border px-4 py-2 text-sm disabled:opacity-50"
            style={{ borderColor: "var(--color-border)", color: "var(--color-text)" }}
          >
            {loading ? "Gönderiliyor…" : "E-postama doğrulama kodu gönder"}
          </button>
        ) : (
          <form onSubmit={handleChangePassword} className="space-y-2">
            <input
              required
              placeholder="E-postanıza gelen kod"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              className="w-full rounded-lg border px-3 py-2 text-sm"
              style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
            />
            <input
              required
              type="password"
              minLength={8}
              placeholder="Yeni şifre"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              className="w-full rounded-lg border px-3 py-2 text-sm"
              style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
            />
            <button
              type="submit"
              disabled={loading}
              className="rounded-lg px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
              style={{ background: "var(--color-accent)" }}
            >
              {loading ? "Güncelleniyor…" : "Şifreyi Güncelle"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}
