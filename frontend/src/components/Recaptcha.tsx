import { useEffect, useRef } from "react";

const SITE_KEY = import.meta.env.VITE_RECAPTCHA_SITE_KEY;

// Read from .env.local (gitignored, never committed) so the repo's source never contains the
// literal secret. Only ever referenced inside `import.meta.env.DEV`, which Vite dead-code-
// eliminates from `npm run build` output — a production bundle never contains this branch either
// way. Must match Recaptcha:DevBypassToken in the backend's (also gitignored) appsettings.json.
const DEV_BYPASS_TOKEN = import.meta.env.VITE_RECAPTCHA_DEV_BYPASS_TOKEN;

interface Props {
  onToken: (token: string | null) => void;
}

export function Recaptcha({ onToken }: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const rendered = useRef(false);

  useEffect(() => {
    if (import.meta.env.DEV) {
      onToken(DEV_BYPASS_TOKEN);
      return;
    }

    let cancelled = false;

    const tryInit = () => {
      if (cancelled || rendered.current) return;
      if (typeof window.grecaptcha?.render !== "function" || !containerRef.current) {
        setTimeout(tryInit, 150);
        return;
      }
      window.grecaptcha.render(containerRef.current, {
        sitekey: SITE_KEY,
        callback: (token) => onToken(token),
        "expired-callback": () => onToken(null),
      });
      rendered.current = true;
    };

    tryInit();
    return () => {
      cancelled = true;
    };
  }, [onToken]);

  if (import.meta.env.DEV) {
    return (
      <p className="text-center text-xs" style={{ color: "var(--color-text-muted)" }}>
        (Yerel geliştirme modu — reCAPTCHA atlandı)
      </p>
    );
  }

  return <div ref={containerRef} className="flex justify-center" />;
}
