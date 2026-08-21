import type { ReactNode } from "react";

export function AuthCard({ title, subtitle, children }: { title: string; subtitle?: string; children: ReactNode }) {
  return (
    <div
      className="flex min-h-screen items-center justify-center px-4"
      style={{ background: "var(--color-bg-subtle)" }}
    >
      <div
        className="w-full max-w-sm rounded-3xl border p-8"
        style={{
          background: "var(--color-bg)",
          borderColor: "var(--color-border)",
          boxShadow: "0 24px 48px -12px rgba(0,0,0,0.12), 0 4px 16px rgba(0,0,0,0.04)",
        }}
      >
        <img src="/logo.png" alt="TeknoChat" className="mb-4 h-16 object-contain" />
        <h1 className="text-xl font-semibold" style={{ color: "var(--color-text)" }}>
          {title}
        </h1>
        {subtitle && (
          <p className="mt-1 text-sm" style={{ color: "var(--color-text-muted)" }}>
            {subtitle}
          </p>
        )}
        <div className="mt-6">{children}</div>
      </div>
    </div>
  );
}

export function FormField({
  label,
  ...props
}: { label: string } & React.InputHTMLAttributes<HTMLInputElement>) {
  return (
    <label className="mb-3 block text-sm">
      <span className="mb-1 block font-medium" style={{ color: "var(--color-text)" }}>
        {label}
      </span>
      <input
        {...props}
        className="w-full rounded-lg border px-3 py-2 text-sm outline-none focus:ring-2"
        style={{
          borderColor: "var(--color-border)",
          background: "var(--color-bg)",
          color: "var(--color-text)",
        }}
      />
    </label>
  );
}

export function PrimaryButton({
  children,
  ...props
}: { children: ReactNode } & React.ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <button
      {...props}
      className="w-full rounded-xl px-4 py-2.5 text-sm font-medium text-white transition-all hover:-translate-y-0.5 hover:opacity-90 disabled:opacity-50 disabled:hover:translate-y-0"
      style={{ background: "var(--color-accent)" }}
    >
      {children}
    </button>
  );
}

export function ErrorText({ children }: { children: ReactNode }) {
  return (
    <p className="mb-3 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-950/40 dark:text-red-400">
      {children}
    </p>
  );
}
