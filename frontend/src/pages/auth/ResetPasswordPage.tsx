import { useState } from "react";
import { useLocation, useNavigate, Link } from "react-router-dom";
import { AuthCard, FormField, PrimaryButton, ErrorText } from "../../components/AuthCard";
import { authApi } from "../../api/resources";
import { ApiError } from "../../api/client";

export function ResetPasswordPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const initialEmail = (location.state as { email?: string } | null)?.email ?? "";

  const [email, setEmail] = useState(initialEmail);
  const [resetToken, setResetToken] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [done, setDone] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await authApi.resetPassword({ email, resetToken, newPassword });
      setDone(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Şifre sıfırlanamadı.");
    } finally {
      setLoading(false);
    }
  };

  if (done) {
    return (
      <AuthCard title="Şifre Güncellendi" subtitle="Yeni şifrenizle giriş yapabilirsiniz">
        <PrimaryButton onClick={() => navigate("/login")}>Girişe Git</PrimaryButton>
      </AuthCard>
    );
  }

  return (
    <AuthCard title="Yeni Şifre" subtitle="E-postanıza gelen kodu ve yeni şifrenizi girin">
      {error && <ErrorText>{error}</ErrorText>}
      <form onSubmit={handleSubmit}>
        <FormField label="E-posta" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        <FormField label="Sıfırlama Kodu" required value={resetToken} onChange={(e) => setResetToken(e.target.value)} />
        <FormField
          label="Yeni Şifre"
          type="password"
          required
          minLength={8}
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
        />
        <PrimaryButton type="submit" disabled={loading}>
          {loading ? "Kaydediliyor…" : "Şifreyi Güncelle"}
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
