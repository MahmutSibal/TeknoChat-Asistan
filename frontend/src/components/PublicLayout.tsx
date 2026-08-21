import { useState } from "react";
import { Link, Outlet, useLocation } from "react-router-dom";
import { Menu, X } from "lucide-react";
import { useAuth } from "../context/AuthContext";
import { useSlidingIndicator } from "../lib/useSlidingIndicator";

const NAV_LINKS = [
  { to: "/about", label: "Hakkında" },
  { to: "/contact", label: "İletişim" },
  { to: "/support-info", label: "Destek" },
];

const ACCOUNT_LINKS = [
  { to: "/login", label: "Giriş Yap" },
  { to: "/register", label: "Kayıt Ol" },
];

function GlassNav() {
  const location = useLocation();
  const activeLink = NAV_LINKS.find((l) => l.to === location.pathname);
  const { containerRef, registerItem, rect, showAt, reset } = useSlidingIndicator(activeLink?.to ?? null, "x");

  return (
    <nav
      ref={containerRef as React.RefObject<HTMLElement>}
      onMouseLeave={reset}
      className="relative hidden gap-1 rounded-2xl border px-2 py-1.5 backdrop-blur-xl sm:flex"
      style={{
        background: "var(--color-nav-glass)",
        borderColor: "var(--color-border)",
        boxShadow: "0 8px 32px rgba(0,0,0,0.08), 0 4px 16px rgba(0,0,0,0.04)",
      }}
    >
      {rect && (
        <span
          className="absolute top-1.5 bottom-1.5 left-0 rounded-xl transition-[transform,width] duration-300 ease-out"
          style={{
            width: rect.size,
            transform: `translateX(${rect.offset}px)`,
            background: "var(--color-accent)",
            opacity: 0.12,
          }}
        />
      )}
      {NAV_LINKS.map((link) => (
        <Link
          key={link.to}
          to={link.to}
          ref={registerItem(link.to)}
          onMouseEnter={() => showAt(link.to)}
          className="relative rounded-xl px-4 py-2 text-sm font-medium transition-colors"
          style={{ color: location.pathname === link.to ? "var(--color-accent)" : "var(--color-text-muted)" }}
        >
          {link.label}
        </Link>
      ))}
    </nav>
  );
}

export function PublicLayout() {
  const { isAuthenticated } = useAuth();
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  return (
    <div className="flex min-h-screen flex-col" style={{ background: "var(--color-bg)", color: "var(--color-text)" }}>
      <header
        className="sticky top-0 z-10 flex items-center justify-between px-6 py-4 backdrop-blur-xl"
        style={{ background: "var(--color-header-glass)", borderBottom: "1px solid var(--color-border)" }}
      >
        <Link to="/" className="flex items-center">
          <img src="/logo.png" alt="TeknoChat" className="h-14 object-contain" />
        </Link>
        <GlassNav />
        <div className="flex items-center gap-2">
          <button
            onClick={() => setMobileNavOpen((v) => !v)}
            className="rounded-lg p-2 sm:hidden"
            style={{ color: "var(--color-text)" }}
            aria-label="Menüyü aç"
          >
            {mobileNavOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
          {isAuthenticated ? (
            <Link
              to="/"
              className="rounded-xl px-4 py-2 text-sm font-medium text-white transition-transform hover:-translate-y-0.5"
              style={{ background: "var(--color-accent)" }}
            >
              Panele Git
            </Link>
          ) : (
            <>
              <Link
                to="/login"
                className="rounded-xl border px-4 py-2 text-sm transition-transform hover:-translate-y-0.5"
                style={{ borderColor: "var(--color-border)", color: "var(--color-text)" }}
              >
                Giriş Yap
              </Link>
              <Link
                to="/register"
                className="rounded-xl px-4 py-2 text-sm font-medium text-white transition-transform hover:-translate-y-0.5"
                style={{ background: "var(--color-accent)" }}
              >
                Kayıt Ol
              </Link>
            </>
          )}
        </div>
      </header>

      {mobileNavOpen && (
        <div
          className="flex flex-col gap-1 border-b px-6 py-3 sm:hidden"
          style={{ background: "var(--color-bg)", borderColor: "var(--color-border)" }}
        >
          {NAV_LINKS.map((link) => (
            <Link
              key={link.to}
              to={link.to}
              onClick={() => setMobileNavOpen(false)}
              className="rounded-lg px-3 py-2 text-sm"
              style={{ color: "var(--color-text)" }}
            >
              {link.label}
            </Link>
          ))}
        </div>
      )}

      <div className="flex-1">
        <Outlet />
      </div>

      <footer className="border-t px-6 py-10" style={{ borderColor: "var(--color-border)", background: "var(--color-bg-subtle)" }}>
        <div className="mx-auto grid max-w-4xl grid-cols-2 gap-8 sm:grid-cols-4">
          <div className="col-span-2 sm:col-span-2">
            <img src="/logo.png" alt="TeknoChat" className="mb-2 h-12 object-contain" />
            <p className="text-xs" style={{ color: "var(--color-text-muted)" }}>
              TEKNOFEST yarışmacıları için yapay zeka destekli, kaynak gösteren soru-cevap asistanı.
            </p>
          </div>

          <div>
            <h3 className="mb-2 text-xs font-semibold uppercase tracking-wide" style={{ color: "var(--color-text)" }}>
              Sayfalar
            </h3>
            <ul className="space-y-1.5 text-xs" style={{ color: "var(--color-text-muted)" }}>
              {NAV_LINKS.map((link) => (
                <li key={link.to}>
                  <Link to={link.to} className="hover:opacity-80">
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          <div>
            <h3 className="mb-2 text-xs font-semibold uppercase tracking-wide" style={{ color: "var(--color-text)" }}>
              Hesap
            </h3>
            <ul className="space-y-1.5 text-xs" style={{ color: "var(--color-text-muted)" }}>
              {ACCOUNT_LINKS.map((link) => (
                <li key={link.to}>
                  <Link to={link.to} className="hover:opacity-80">
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </div>
        </div>

        <div
          className="mx-auto mt-8 max-w-4xl border-t pt-4 text-center text-xs"
          style={{ borderColor: "var(--color-border)", color: "var(--color-text-muted)" }}
        >
          © {new Date().getFullYear()} TeknoChat — TEKNOFEST Yapay Zeka Destekli SSS ve Chatbot Asistanı
        </div>
      </footer>
    </div>
  );
}
