import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { AuthCard, FormField, PrimaryButton, ErrorText } from "../../components/AuthCard";
import { GoogleSignInButton } from "../../components/GoogleSignInButton";
import { Recaptcha } from "../../components/Recaptcha";
import { useAuth } from "../../context/AuthContext";
import { authApi } from "../../api/resources";
import { ApiError } from "../../api/client";

export function RegisterPage() {
  const { loginWithGoogle } = useAuth();
  const navigate = useNavigate();
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [recaptchaToken, setRecaptchaToken] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!recaptchaToken) return;
    setError(null);
    setLoading(true);
    try {
      await authApi.register({ fullName, email, password, recaptchaToken });
      navigate("/verify-email", { state: { email } });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Kayıt olunamadı.");
    } finally {
      setLoading(false);
    }
  };

  const handleGoogle = async (idToken: string) => {
    setError(null);
    try {
      await loginWithGoogle(idToken);
      navigate("/");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Google ile kayıt başarısız oldu.");
    }
  };

  return (
    <AuthCard title="Kayıt Ol" subtitle="Yarışmacı hesabı oluşturun">
      {error && <ErrorText>{error}</ErrorText>}
      <form onSubmit={handleSubmit}>
        <FormField label="Ad Soyad" required value={fullName} onChange={(e) => setFullName(e.target.value)} />
        <FormField
          label="E-posta"
          type="email"
          required
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
        <FormField
          label="Şifre"
          type="password"
          required
          minLength={8}
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <div className="mb-3">
          <Recaptcha onToken={setRecaptchaToken} />
        </div>
        <PrimaryButton type="submit" disabled={loading || !recaptchaToken}>
          {loading ? "Kayıt olunuyor…" : "Kayıt Ol"}
        </PrimaryButton>
      </form>

      <div className="my-4 flex items-center gap-3">
        <div className="h-px flex-1" style={{ background: "var(--color-border)" }} />
        <span className="text-xs" style={{ color: "var(--color-text-muted)" }}>
          veya
        </span>
        <div className="h-px flex-1" style={{ background: "var(--color-border)" }} />
      </div>

      <GoogleSignInButton onToken={handleGoogle} text="signup_with" />

      <p className="mt-6 text-center text-sm" style={{ color: "var(--color-text-muted)" }}>
        Zaten hesabınız var mı?{" "}
        <Link to="/login" className="font-medium" style={{ color: "var(--color-accent)" }}>
          Giriş yapın
        </Link>
      </p>
    </AuthCard>
  );
}
