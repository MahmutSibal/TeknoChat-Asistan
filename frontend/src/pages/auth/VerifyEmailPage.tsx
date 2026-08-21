import { useState } from "react";
import { useLocation, Link } from "react-router-dom";
import { AuthCard, FormField, PrimaryButton, ErrorText } from "../../components/AuthCard";
import { authApi } from "../../api/resources";
import { ApiError } from "../../api/client";
import { setAuthToken } from "../../api/client";

export function VerifyEmailPage() {
  const location = useLocation();
  const initialEmail = (location.state as { email?: string } | null)?.email ?? "";

  const [email, setEmail] = useState(initialEmail);
  const [code, setCode] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [resending, setResending] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const res = await authApi.verifyEmail({ email, code });
      setAuthToken(res.token);
      window.location.href = "/";
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Doğrulama başarısız oldu.");
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async () => {
    setError(null);
    setInfo(null);
    setResending(true);
    try {
      await authApi.resendVerification({ email });
      setInfo("Yeni kod e-postanıza gönderildi.");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Kod gönderilemedi.");
    } finally {
      setResending(false);
    }
  };

  return (
    <AuthCard title="E-posta Doğrulama" subtitle="E-postanıza gönderilen 6 haneli kodu girin">
      {error && <ErrorText>{error}</ErrorText>}
      {info && (
        <p className="mb-3 rounded-lg bg-green-50 px-3 py-2 text-sm text-green-700 dark:bg-green-950/40 dark:text-green-400">
          {info}
        </p>
      )}
      <form onSubmit={handleSubmit}>
        <FormField label="E-posta" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        <FormField
          label="Doğrulama Kodu"
          required
          maxLength={6}
          inputMode="numeric"
          value={code}
          onChange={(e) => setCode(e.target.value)}
        />
        <PrimaryButton type="submit" disabled={loading}>
          {loading ? "Doğrulanıyor…" : "Doğrula"}
        </PrimaryButton>
      </form>

      <button
        onClick={handleResend}
        disabled={resending || !email}
        className="mt-4 w-full text-center text-sm disabled:opacity-50"
        style={{ color: "var(--color-accent)" }}
      >
        {resending ? "Gönderiliyor…" : "Kodu tekrar gönder"}
      </button>

      <p className="mt-6 text-center text-sm" style={{ color: "var(--color-text-muted)" }}>
        <Link to="/login" className="font-medium" style={{ color: "var(--color-accent)" }}>
          Girişe dön
        </Link>
      </p>
    </AuthCard>
  );
}
