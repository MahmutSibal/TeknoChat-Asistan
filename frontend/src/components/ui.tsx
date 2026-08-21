import { Loader2, type LucideIcon } from "lucide-react";

export function Skeleton({ className = "" }: { className?: string }) {
  return <div className={`animate-pulse rounded-lg ${className}`} style={{ background: "var(--color-border)" }} />;
}

export function SkeletonList({ rows = 3, rowClassName = "h-16" }: { rows?: number; rowClassName?: string }) {
  return (
    <div className="space-y-2">
      {Array.from({ length: rows }).map((_, i) => (
        <Skeleton key={i} className={`w-full ${rowClassName}`} />
      ))}
    </div>
  );
}

export function Spinner({ size = 14, className = "" }: { size?: number; className?: string }) {
  return <Loader2 size={size} className={`animate-spin ${className}`} aria-hidden />;
}

export function EmptyState({ icon: Icon, text }: { icon: LucideIcon; text: string }) {
  return (
    <div className="flex flex-col items-center gap-2 rounded-2xl border border-dashed px-6 py-12 text-center" style={{ borderColor: "var(--color-border)" }}>
      <Icon size={28} strokeWidth={1.5} style={{ color: "var(--color-text-muted)" }} aria-hidden />
      <p className="text-sm" style={{ color: "var(--color-text-muted)" }}>
        {text}
      </p>
    </div>
  );
}
