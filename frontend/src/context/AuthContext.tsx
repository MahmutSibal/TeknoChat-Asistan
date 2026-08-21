import { createContext, useContext, useState, type ReactNode } from "react";
import { authApi } from "../api/resources";
import { setAuthToken, getAuthToken } from "../api/client";
import { UserRole } from "../types/api";

interface CurrentUser {
  userId: number;
  fullName: string;
  email: string;
  role: UserRole;
}

interface AuthContextValue {
  user: CurrentUser | null;
  isAuthenticated: boolean;
  login: (email: string, password: string, recaptchaToken: string) => Promise<void>;
  loginWithGoogle: (idToken: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const USER_STORAGE_KEY = "teknofest_user";

function loadStoredUser(): CurrentUser | null {
  if (!getAuthToken()) return null;
  const raw = localStorage.getItem(USER_STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as CurrentUser;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(loadStoredUser());

  const applyAuthResponse = (res: { userId: number; fullName: string; email: string; role: UserRole; token: string }) => {
    setAuthToken(res.token);
    const currentUser: CurrentUser = { userId: res.userId, fullName: res.fullName, email: res.email, role: res.role };
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(currentUser));
    setUser(currentUser);
  };

  const login = async (email: string, password: string, recaptchaToken: string) => {
    const res = await authApi.login({ email, password, recaptchaToken });
    applyAuthResponse(res);
  };

  const loginWithGoogle = async (idToken: string) => {
    const res = await authApi.google(idToken);
    applyAuthResponse(res);
  };

  const logout = () => {
    setAuthToken(null);
    localStorage.removeItem(USER_STORAGE_KEY);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, login, loginWithGoogle, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth, AuthProvider içinde kullanılmalı");
  return ctx;
}

export const roleLabels: Record<UserRole, string> = {
  [UserRole.Yarismaci]: "Yarışmacı",
  [UserRole.IcerikYoneticisi]: "İçerik Yöneticisi",
  [UserRole.DestekEkibi]: "Destek Ekibi",
  [UserRole.SistemYoneticisi]: "Sistem Yöneticisi",
};
