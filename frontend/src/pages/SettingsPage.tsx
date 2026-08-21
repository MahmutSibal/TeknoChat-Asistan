import { useState } from "react";
import { getNotificationsEnabled, setNotificationsEnabled } from "../lib/settings";

export function SettingsPage() {
  const [notifications, setNotifications] = useState(getNotificationsEnabled());

  const toggle = () => {
    const next = !notifications;
    setNotifications(next);
    setNotificationsEnabled(next);
  };

  return (
    <div className="mx-auto max-w-2xl px-4 py-6">
      <h2 className="mb-6 text-lg font-semibold" style={{ color: "var(--color-text)" }}>
        Ayarlar
      </h2>

      <div className="rounded-xl border p-4" style={{ borderColor: "var(--color-border)" }}>
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm font-medium" style={{ color: "var(--color-text)" }}>
              Anlık Bildirimler
            </p>
            <p className="text-xs" style={{ color: "var(--color-text-muted)" }}>
              Destek talebi çözüldüğünde veya yeni bir talep oluştuğunda ekranda bildirim göster.
            </p>
          </div>
          <button
            onClick={toggle}
            role="switch"
            aria-checked={notifications}
            className="relative h-6 w-11 shrink-0 rounded-full transition-colors"
            style={{ background: notifications ? "var(--color-accent)" : "var(--color-border)" }}
          >
            <span
              className="absolute top-0.5 h-5 w-5 rounded-full bg-white transition-transform"
              style={{ transform: notifications ? "translateX(22px)" : "translateX(2px)" }}
            />
          </button>
        </div>
      </div>

      <p className="mt-4 text-xs" style={{ color: "var(--color-text-muted)" }}>
        Tema (açık/koyu) tarayıcınızın sistem ayarını otomatik takip eder.
      </p>
    </div>
  );
}
