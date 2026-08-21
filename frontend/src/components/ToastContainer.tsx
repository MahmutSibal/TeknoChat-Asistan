import { useEffect } from "react";
import { useAuth } from "../context/AuthContext";
import { useSignalR, useToasts } from "../context/SignalRContext";
import { getNotificationsEnabled } from "../lib/settings";
import { UserRole } from "../types/api";

export function ToastContainer() {
  const { user } = useAuth();
  const { onTicketResolved, onNewTicket } = useSignalR();
  const { toasts, push } = useToasts();

  useEffect(() => {
    if (!user) return;

    const unsubs: (() => void)[] = [];

    if (user.role === UserRole.Yarismaci) {
      unsubs.push(
        onTicketResolved(() => {
          if (getNotificationsEnabled()) push("Destek ekibi bir sorunuzu yanıtladı. Geçmişim sayfasından görebilirsiniz.");
        }),
      );
    }

    if (user.role === UserRole.DestekEkibi || user.role === UserRole.SistemYoneticisi) {
      unsubs.push(
        onNewTicket((e) => {
          if (getNotificationsEnabled()) push(`Yeni destek talebi: "${e.questionText}"`);
        }),
      );
    }

    return () => unsubs.forEach((fn) => fn());
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user]);

  if (toasts.length === 0) return null;

  return (
    <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-2">
      {toasts.map((t) => (
        <div
          key={t.id}
          className="max-w-xs rounded-lg border px-4 py-3 text-sm shadow-lg"
          style={{ background: "var(--color-bg)", borderColor: "var(--color-accent)", color: "var(--color-text)" }}
        >
          {t.text}
        </div>
      ))}
    </div>
  );
}
