import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { AuthCard, FormField, PrimaryButton, ErrorText } from "../../components/AuthCard";
import { authApi } from "../../api/resources";
import { ApiError } from "../../api/client";

export function ForgotPasswordPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [sent, setSent] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await authApi.forgotPassword({ email });
      setSent(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "İşlem başarısız oldu.");
    } finally {
      setLoading(false);
    }
  };

  if (sent) {
    return (
      <AuthCard title="Kod Gönderildi" subtitle="E-postanızı kontrol edin">
        <p className="mb-4 text-sm" style={{ color: "var(--color-text-muted)" }}>
          {email} adresine bir sıfırlama kodu gönderdik.
        </p>
        <PrimaryButton onClick={() => navigate("/reset-password", { state: { email } })}>
          Kodu Girmeye Devam Et
        </PrimaryButton>
      </AuthCard>
    );
  }

  return (
    <AuthCard title="Şifremi Unuttum" subtitle="E-posta adresinizi girin">
      {error && <ErrorText>{error}</ErrorText>}
      <form onSubmit={handleSubmit}>
        <FormField label="E-posta" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        <PrimaryButton type="submit" disabled={loading}>
          {loading ? "Gönderiliyor…" : "Sıfırlama Kodu Gönder"}
        </PrimaryButton>
      </form>
      <p className="mt-6 text-center text-sm" style={{ color: "var(--color-text-muted)" }}>
        <Link to="/login" className="font-medium" style={{ color: "var(--color-accent)" }}>
          Girişe dön
        </Link>
      </p>
    </AuthCard>
  );
}
