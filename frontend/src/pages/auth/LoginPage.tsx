import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { AuthCard, FormField, PrimaryButton, ErrorText } from "../../components/AuthCard";
import { GoogleSignInButton } from "../../components/GoogleSignInButton";
import { Recaptcha } from "../../components/Recaptcha";
import { useAuth } from "../../context/AuthContext";
import { ApiError } from "../../api/client";

export function LoginPage() {
  const { login, loginWithGoogle } = useAuth();
  const navigate = useNavigate();
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
      await login(email, password, recaptchaToken);
      navigate("/");
    } catch (err) {
      if (err instanceof ApiError && err.status === 403) {
        navigate("/verify-email", { state: { email } });
        return;
      }
      setError(err instanceof ApiError ? err.message : "Giriş yapılamadı.");
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
      setError(err instanceof ApiError ? err.message : "Google ile giriş başarısız oldu.");
    }
  };

  return (
    <AuthCard title="Giriş Yap" subtitle="TEKNOFEST Yarışmacı Asistanı">
      {error && <ErrorText>{error}</ErrorText>}
      <form onSubmit={handleSubmit}>
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
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <div className="mb-3 text-right">
          <Link to="/forgot-password" className="text-xs" style={{ color: "var(--color-accent)" }}>
            Şifremi unuttum
          </Link>
        </div>
        <div className="mb-3">
          <Recaptcha onToken={setRecaptchaToken} />
        </div>
        <PrimaryButton type="submit" disabled={loading || !recaptchaToken}>
          {loading ? "Giriş yapılıyor…" : "Giriş Yap"}
        </PrimaryButton>
      </form>

      <div className="my-4 flex items-center gap-3">
        <div className="h-px flex-1" style={{ background: "var(--color-border)" }} />
        <span className="text-xs" style={{ color: "var(--color-text-muted)" }}>
          veya
        </span>
        <div className="h-px flex-1" style={{ background: "var(--color-border)" }} />
      </div>

      <GoogleSignInButton onToken={handleGoogle} text="signin_with" />

      <p className="mt-6 text-center text-sm" style={{ color: "var(--color-text-muted)" }}>
        Hesabınız yok mu?{" "}
        <Link to="/register" className="font-medium" style={{ color: "var(--color-accent)" }}>
          Kayıt olun
        </Link>
      </p>
    </AuthCard>
  );
}
