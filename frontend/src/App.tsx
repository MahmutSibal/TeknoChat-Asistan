import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider, useAuth } from "./context/AuthContext";
import { UserRole } from "./types/api";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { Layout } from "./components/Layout";
import { PublicLayout } from "./components/PublicLayout";

import { HomePage } from "./pages/public/HomePage";
import { AboutPage } from "./pages/public/AboutPage";
import { ContactPage } from "./pages/public/ContactPage";
import { SupportInfoPage } from "./pages/public/SupportInfoPage";

import { LoginPage } from "./pages/auth/LoginPage";
import { RegisterPage } from "./pages/auth/RegisterPage";
import { VerifyEmailPage } from "./pages/auth/VerifyEmailPage";
import { ForgotPasswordPage } from "./pages/auth/ForgotPasswordPage";
import { ResetPasswordPage } from "./pages/auth/ResetPasswordPage";

import { ChatPage } from "./pages/competitor/ChatPage";
import { MyHistoryPage } from "./pages/competitor/MyHistoryPage";

import { DocumentsPage } from "./pages/content-manager/DocumentsPage";
import { CompetitionsAdminPage } from "./pages/content-manager/CompetitionsAdminPage";

import { TicketsPage } from "./pages/support/TicketsPage";
import { FaqPage } from "./pages/support/FaqPage";

import { UsersPage } from "./pages/admin/UsersPage";
import { AnalyticsPage } from "./pages/admin/AnalyticsPage";

import { ProfilePage } from "./pages/ProfilePage";
import { SettingsPage } from "./pages/SettingsPage";

const ALL_ROLES: UserRole[] = [UserRole.Yarismaci, UserRole.IcerikYoneticisi, UserRole.DestekEkibi, UserRole.SistemYoneticisi];

function RoleHome({ role }: { role: UserRole }) {
  switch (role) {
    case UserRole.Yarismaci:
      return <Navigate to="/chat" replace />;
    case UserRole.IcerikYoneticisi:
      return <Navigate to="/documents" replace />;
    case UserRole.DestekEkibi:
      return <Navigate to="/tickets" replace />;
    case UserRole.SistemYoneticisi:
      return <Navigate to="/analytics" replace />;
    default:
      return <Navigate to="/login" replace />;
  }
}

/// Public landing page for visitors; signed-in users are sent straight to their role's home.
function RootRoute() {
  const { user } = useAuth();
  if (!user) return <HomePage />;
  return <RoleHome role={user.role} />;
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route element={<PublicLayout />}>
            <Route path="/" element={<RootRoute />} />
            <Route path="/about" element={<AboutPage />} />
            <Route path="/contact" element={<ContactPage />} />
            <Route path="/support-info" element={<SupportInfoPage />} />
          </Route>

          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/verify-email" element={<VerifyEmailPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />

          <Route element={<Layout />}>
            <Route element={<ProtectedRoute allowedRoles={ALL_ROLES} />}>
              <Route path="/profile" element={<ProfilePage />} />
              <Route path="/settings" element={<SettingsPage />} />
            </Route>

            <Route element={<ProtectedRoute allowedRoles={[UserRole.Yarismaci]} />}>
              <Route path="/chat" element={<ChatPage />} />
              <Route path="/my-history" element={<MyHistoryPage />} />
            </Route>

            <Route element={<ProtectedRoute allowedRoles={[UserRole.IcerikYoneticisi, UserRole.SistemYoneticisi]} />}>
              <Route path="/documents" element={<DocumentsPage />} />
              <Route path="/competitions" element={<CompetitionsAdminPage />} />
            </Route>

            <Route element={<ProtectedRoute allowedRoles={[UserRole.DestekEkibi]} />}>
              <Route path="/tickets" element={<TicketsPage />} />
            </Route>

            <Route
              element={
                <ProtectedRoute
                  allowedRoles={[UserRole.DestekEkibi, UserRole.IcerikYoneticisi, UserRole.SistemYoneticisi]}
                />
              }
            >
              <Route path="/faq" element={<FaqPage />} />
            </Route>

            <Route element={<ProtectedRoute allowedRoles={[UserRole.SistemYoneticisi]} />}>
              <Route path="/users" element={<UsersPage />} />
              <Route path="/analytics" element={<AnalyticsPage />} />
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
