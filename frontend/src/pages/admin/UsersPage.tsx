import { useEffect, useState } from "react";
import { usersApi } from "../../api/resources";
import { ApiError } from "../../api/client";
import { UserRole, type AppUser } from "../../types/api";
import { roleLabels } from "../../context/AuthContext";
import { SkeletonList, Spinner } from "../../components/ui";

export function UsersPage() {
  const [users, setUsers] = useState<AppUser[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState<UserRole>(UserRole.IcerikYoneticisi);

  const load = () => {
    setLoading(true);
    usersApi
      .list(1, 100)
      .then((res) => setUsers(res.items))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setCreating(true);
    try {
      await usersApi.create({ fullName, email, password, role });
      setFullName("");
      setEmail("");
      setPassword("");
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Kullanıcı oluşturulamadı.");
    } finally {
      setCreating(false);
    }
  };

  return (
    <div className="mx-auto max-w-3xl px-4 py-6">
      <h2 className="mb-4 text-lg font-semibold" style={{ color: "var(--color-text)" }}>
        Kullanıcılar
      </h2>

      {error && (
        <p className="mb-3 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-950/40 dark:text-red-400">
          {error}
        </p>
      )}

      <form onSubmit={handleCreate} className="card-modern mb-6 grid grid-cols-1 gap-2 p-4 sm:grid-cols-2">
        <input
          required
          placeholder="Ad Soyad"
          value={fullName}
          onChange={(e) => setFullName(e.target.value)}
          className="rounded-lg border px-3 py-2 text-sm"
          style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
        />
        <input
          required
          type="email"
          placeholder="E-posta"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className="rounded-lg border px-3 py-2 text-sm"
          style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
        />
        <input
          required
          type="password"
          minLength={8}
          placeholder="Geçici şifre"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          className="rounded-lg border px-3 py-2 text-sm"
          style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
        />
        <select
          value={role}
          onChange={(e) => setRole(Number(e.target.value) as UserRole)}
          className="rounded-lg border px-3 py-2 text-sm"
          style={{ borderColor: "var(--color-border)", background: "var(--color-bg)", color: "var(--color-text)" }}
        >
          <option value={UserRole.IcerikYoneticisi}>İçerik Yöneticisi</option>
          <option value={UserRole.DestekEkibi}>Destek Ekibi</option>
          <option value={UserRole.SistemYoneticisi}>Sistem Yöneticisi</option>
        </select>
        <button
          type="submit"
          disabled={creating}
          className="flex items-center justify-center gap-2 rounded-lg px-4 py-2 text-sm font-medium text-white transition-transform hover:-translate-y-0.5 disabled:opacity-50 disabled:hover:translate-y-0 sm:col-span-2"
          style={{ background: "var(--color-accent)" }}
        >
          {creating && <Spinner size={12} />}
          Hesap Oluştur
        </button>
      </form>

      {loading ? (
        <SkeletonList rows={4} rowClassName="h-14" />
      ) : (
        <div className="space-y-2">
          {users.map((u) => (
            <div key={u.id} className="card-modern card-hover flex items-center justify-between p-3">
              <div>
                <p className="text-sm font-medium" style={{ color: "var(--color-text)" }}>
                  {u.fullName}
                </p>
                <p className="text-xs" style={{ color: "var(--color-text-muted)" }}>
                  {u.email}
                </p>
              </div>
              <span
                className="rounded-full px-2 py-0.5 text-xs"
                style={{ background: "var(--color-bg-subtle)", color: "var(--color-text-muted)" }}
              >
                {roleLabels[u.role]}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
