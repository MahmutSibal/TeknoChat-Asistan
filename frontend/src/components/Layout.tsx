import { useEffect, useRef, useState } from "react";
import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import {
  MessageSquare,
  History,
  FileText,
  Trophy,
  Ticket,
  HelpCircle,
  Users,
  BarChart3,
  User,
  Settings,
  LogOut,
  ChevronLeft,
  ChevronRight,
  ChevronDown,
  Menu,
  X,
  type LucideIcon,
} from "lucide-react";
import { useAuth, roleLabels } from "../context/AuthContext";
import { CompetitionProvider, useCompetitions } from "../context/CompetitionContext";
import { SignalRProvider } from "../context/SignalRContext";
import { ToastContainer } from "./ToastContainer";
import { UserRole, type SystemStatus } from "../types/api";
import { useSlidingIndicator } from "../lib/useSlidingIndicator";
import { useHideUrlBar } from "../lib/useHideUrlBar";
import { systemApi } from "../api/resources";

const STATUS_POLL_INTERVAL_MS = 60_000;

interface NavItem {
  to: string;
  label: string;
  icon: LucideIcon;
  roles: UserRole[] | "all";
}

const NAV_ITEMS: NavItem[] = [
  { to: "/chat", label: "Sohbet", icon: MessageSquare, roles: [UserRole.Yarismaci] },
  { to: "/my-history", label: "Geçmişim", icon: History, roles: [UserRole.Yarismaci] },
  { to: "/documents", label: "Dokümanlar", icon: FileText, roles: [UserRole.IcerikYoneticisi, UserRole.SistemYoneticisi] },
  { to: "/competitions", label: "Yarışmalar", icon: Trophy, roles: [UserRole.IcerikYoneticisi, UserRole.SistemYoneticisi] },
  { to: "/tickets", label: "Destek Talepleri", icon: Ticket, roles: [UserRole.DestekEkibi] },
  { to: "/faq", label: "SSS", icon: HelpCircle, roles: [UserRole.DestekEkibi, UserRole.IcerikYoneticisi, UserRole.SistemYoneticisi] },
  { to: "/users", label: "Kullanıcılar", icon: Users, roles: [UserRole.SistemYoneticisi] },
  { to: "/analytics", label: "Analiz", icon: BarChart3, roles: [UserRole.SistemYoneticisi] },
];

const ACCOUNT_ITEMS: NavItem[] = [
  { to: "/profile", label: "Profil", icon: User, roles: "all" },
  { to: "/settings", label: "Ayarlar", icon: Settings, roles: "all" },
];

const COLLAPSE_KEY = "teknofest_sidebar_collapsed";
const ICON_SIZE = 18;

function CompetitionSelector() {
  const { competitions, selectedCompetitionId, setSelectedCompetitionId } = useCompetitions();
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const handleOutsideClick = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener("mousedown", handleOutsideClick);
    return () => document.removeEventListener("mousedown", handleOutsideClick);
  }, [open]);

  if (competitions.length === 0) return null;

  const selected = competitions.find((c) => c.id === selectedCompetitionId);

  return (
    <div ref={containerRef} className="relative w-full">
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        className="flex w-full items-center justify-between gap-2 rounded-lg border px-3 py-2 text-left text-sm"
        style={{
          borderColor: "var(--color-sidebar-border)",
          background: "rgba(255,255,255,0.06)",
          color: "var(--color-sidebar-text)",
        }}
      >
        <span className="truncate">{selected?.name ?? "Yarışma seçin"}</span>
        <ChevronDown
          size={14}
          className="shrink-0"
          style={{ transform: open ? "rotate(180deg)" : undefined, transition: "transform 0.15s ease" }}
        />
      </button>
      {open && (
        <div
          className="absolute top-full left-0 z-50 mt-1 max-h-64 w-full overflow-y-auto rounded-lg border py-1 shadow-lg"
          style={{ borderColor: "var(--color-sidebar-border)", background: "#14293e" }}
        >
          {competitions.map((c) => (
            <button
              key={c.id}
              type="button"
              onClick={() => {
                setSelectedCompetitionId(c.id);
                setOpen(false);
              }}
              className="block w-full truncate px-3 py-2 text-left text-sm hover:bg-white/10"
              style={{
                color: "#eef3f6",
                background: c.id === selectedCompetitionId ? "var(--color-accent)" : "transparent",
              }}
            >
              {c.name}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

/// Polled, not pushed — a slow-changing status doesn't need SignalR, and the backend caches
/// results heavily (see SystemStatusService) so this never adds meaningful traffic to Ollama/Claude.
function SystemStatusIndicators() {
  const [status, setStatus] = useState<SystemStatus | null>(null);

  useEffect(() => {
    let cancelled = false;
    const load = () => {
      systemApi
        .status()
        .then((s) => {
          if (!cancelled) setStatus(s);
        })
        .catch(() => {});
    };
    load();
    const interval = setInterval(load, STATUS_POLL_INTERVAL_MS);
    return () => {
      cancelled = true;
      clearInterval(interval);
    };
  }, []);

  if (!status) return null;

  const items: { label: string; active: boolean }[] = [
    { label: "Ollama", active: status.ollama },
    { label: "Claude Bulut", active: status.claudeBulut },
    { label: "Temel Arama", active: status.temelArama },
  ];

  return (
    <div className="mb-2 flex flex-wrap gap-x-3 gap-y-1 px-2">
      {items.map((item) => (
        <span
          key={item.label}
          className="flex items-center gap-1 text-[10px]"
          title={item.active ? `${item.label} şu anda aktif` : `${item.label} şu anda erişilemiyor`}
          style={{ color: "var(--color-sidebar-text-muted)" }}
        >
          <span
            className="h-1.5 w-1.5 shrink-0 rounded-full"
            style={{ background: item.active ? "#22c55e" : "#6b7280" }}
          />
          {item.label}
        </span>
      ))}
    </div>
  );
}

function SidebarContent({
  collapsed,
  onToggle,
  onNavigate,
}: {
  collapsed: boolean;
  onToggle: () => void;
  onNavigate?: () => void;
}) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const items = user ? NAV_ITEMS.filter((item) => item.roles !== "all" && item.roles.includes(user.role)) : [];
  const activeItem = items.find((item) => item.to === location.pathname);
  const { containerRef, registerItem, rect, showAt, reset } = useSlidingIndicator(activeItem?.to ?? null, "y");

  if (!user) return null;

  const renderLink = (item: NavItem, withRef: boolean) => {
    const Icon = item.icon;
    return (
      <NavLink
        key={item.to}
        to={item.to}
        ref={withRef ? registerItem(item.to) : undefined}
        onMouseEnter={withRef ? () => showAt(item.to) : undefined}
        onClick={onNavigate}
        title={collapsed ? item.label : undefined}
        className={`relative flex items-center gap-3 rounded-xl px-3 py-2 text-sm transition-colors ${!withRef ? "hover:bg-white/[0.07]" : ""}`}
        style={({ isActive }) => ({
          fontWeight: isActive ? 500 : 400,
          color: isActive ? "#fff" : "var(--color-sidebar-text)",
          background: !withRef && isActive ? "var(--color-accent)" : "transparent",
          justifyContent: collapsed ? "center" : "flex-start",
        })}
      >
        <Icon size={ICON_SIZE} strokeWidth={1.75} aria-hidden />
        {!collapsed && <span>{item.label}</span>}
      </NavLink>
    );
  };

  return (
    <div className="flex h-full flex-col p-3">
      <div className="mb-4 flex items-center justify-between px-1">
        {!collapsed && (
          <div>
            <h1 className="text-lg font-semibold" style={{ color: "var(--color-sidebar-text)" }}>
              TeknoChat
            </h1>
            <p className="text-xs" style={{ color: "var(--color-sidebar-text-muted)" }}>
              {roleLabels[user.role]}
            </p>
          </div>
        )}
        {!collapsed && (
          <button
            onClick={onToggle}
            title={onNavigate ? "Kapat" : "Menüyü daralt"}
            className="rounded-lg p-1.5 hover:opacity-70"
            style={{ color: "var(--color-sidebar-text-muted)" }}
          >
            {onNavigate ? <X size={16} /> : <ChevronLeft size={16} />}
          </button>
        )}
      </div>
      {collapsed && (
        <button
          onClick={onToggle}
          title="Menüyü genişlet"
          className="mb-4 flex justify-center rounded-lg p-1.5 hover:opacity-70"
          style={{ color: "var(--color-sidebar-text-muted)" }}
        >
          <ChevronRight size={16} />
        </button>
      )}

      {!collapsed && (
        <div className="mb-4">
          <CompetitionSelector />
        </div>
      )}

      <nav
        ref={containerRef as React.RefObject<HTMLElement>}
        onMouseLeave={reset}
        className="relative flex-1 space-y-1"
      >
        {rect && (
          <span
            className="absolute left-0 w-full rounded-xl transition-[transform,height] duration-300 ease-out"
            style={{
              height: rect.size,
              transform: `translateY(${rect.offset}px)`,
              background: "var(--color-accent)",
            }}
          />
        )}
        {items.map((item) => renderLink(item, true))}
      </nav>

      <div className="space-y-1 border-t pt-2" style={{ borderColor: "var(--color-sidebar-border)" }}>
        {ACCOUNT_ITEMS.map((item) => renderLink(item, false))}
      </div>

      <div className="border-t pt-3" style={{ borderColor: "var(--color-sidebar-border)" }}>
        {!collapsed && <SystemStatusIndicators />}
        {!collapsed && (
          <p className="truncate px-2 text-xs" style={{ color: "var(--color-sidebar-text-muted)" }}>
            {user.fullName} · {user.email}
          </p>
        )}
        <button
          onClick={() => {
            logout();
            navigate("/login");
          }}
          title={collapsed ? "Çıkış yap" : undefined}
          className="mt-2 flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm hover:bg-white/[0.07]"
          style={{ color: "var(--color-sidebar-text-muted)", justifyContent: collapsed ? "center" : "flex-start" }}
        >
          <LogOut size={ICON_SIZE} strokeWidth={1.75} aria-hidden />
          {!collapsed && <span>Çıkış yap</span>}
        </button>
      </div>
    </div>
  );
}

export function Layout() {
  const [collapsed, setCollapsed] = useState(() => localStorage.getItem(COLLAPSE_KEY) === "true");
  const [mobileOpen, setMobileOpen] = useState(false);
  const location = useLocation();
  useHideUrlBar();

  useEffect(() => setMobileOpen(false), [location.pathname]);

  const toggle = () => {
    setCollapsed((prev) => {
      const next = !prev;
      localStorage.setItem(COLLAPSE_KEY, String(next));
      return next;
    });
  };

  return (
    <CompetitionProvider>
      <SignalRProvider>
        <div className="flex h-screen w-screen flex-col overflow-hidden sm:flex-row">
          <div
            className="flex items-center justify-between border-b px-4 py-3 sm:hidden"
            style={{ background: "var(--color-sidebar)", borderColor: "var(--color-sidebar-border)" }}
          >
            <span className="font-semibold" style={{ color: "var(--color-sidebar-text)" }}>
              TeknoChat
            </span>
            <button
              onClick={() => setMobileOpen(true)}
              className="rounded-lg p-1.5"
              style={{ color: "var(--color-sidebar-text)" }}
              aria-label="Menüyü aç"
            >
              <Menu size={22} />
            </button>
          </div>

          <aside
            className={`hidden shrink-0 border-r transition-all sm:block ${collapsed ? "w-16" : "w-64"}`}
            style={{ background: "var(--color-sidebar)", borderColor: "var(--color-sidebar-border)" }}
          >
            <SidebarContent collapsed={collapsed} onToggle={toggle} />
          </aside>

          {mobileOpen && (
            <div className="fixed inset-0 z-50 sm:hidden">
              <div className="absolute inset-0 bg-black/50" onClick={() => setMobileOpen(false)} aria-hidden />
              <aside className="absolute top-0 left-0 h-full w-72" style={{ background: "var(--color-sidebar)" }}>
                <SidebarContent collapsed={false} onToggle={() => setMobileOpen(false)} onNavigate={() => setMobileOpen(false)} />
              </aside>
            </div>
          )}

          <main className="flex-1 overflow-y-auto" style={{ background: "var(--color-bg)" }}>
            <Outlet />
          </main>
        </div>
        <ToastContainer />
      </SignalRProvider>
    </CompetitionProvider>
  );
}
