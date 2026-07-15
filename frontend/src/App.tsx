import type { ReactElement } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext";
import { permissions } from "./auth/permissions";
import { AppShell } from "./components/AppShell";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { UserTopbar } from "./components/UserTopbar";
import { JobDetailsPage } from "./pages/JobDetailsPage";
import { JobsPage } from "./pages/JobsPage";
import { PagesPage } from "./pages/PagesPage";
import { SchedulesPage } from "./pages/SchedulesPage";
import { AuthPage } from "./pages/AuthPage";
import { UserCrawlerPage } from "./pages/UserCrawlerPage";
import { UserReportsPage } from "./pages/UserReportsPage";
import { ExportsPage } from "./pages/ExportsPage";
import { AdminOverviewPage } from "./pages/AdminOverviewPage";
import { RolesPage } from "./pages/RolesPage";
import { UsersPage } from "./pages/UsersPage";
import { ForgotPasswordPage } from "./pages/ForgotPasswordPage";
import { ResetPasswordPage } from "./pages/ResetPasswordPage";
import { ConfirmEmailPage } from "./pages/ConfirmEmailPage";

type AdminRoute = {
  path: string;
  element: ReactElement;
  requiredPermissions: string[];
};

const adminRoutes: AdminRoute[] = [
  {
    path: "/admin/overview",
    element: <AdminOverviewPage />,
    requiredPermissions: [permissions.adminAccess],
  },
  {
    path: "/admin/jobs",
    element: <JobsPage />,
    requiredPermissions: [permissions.adminAccess, permissions.crawlJobsView],
  },
  {
    path: "/admin/jobs/:id",
    element: <JobDetailsPage variant="admin" />,
    requiredPermissions: [permissions.adminAccess, permissions.crawlJobsView],
  },
  {
    path: "/admin/pages",
    element: <PagesPage />,
    requiredPermissions: [permissions.adminAccess, permissions.crawledPagesView],
  },
  {
    path: "/admin/schedules",
    element: <SchedulesPage />,
    requiredPermissions: [permissions.adminAccess, permissions.schedulesView],
  },
  {
    path: "/admin/exports",
    element: <ExportsPage />,
    requiredPermissions: [permissions.adminAccess, permissions.crawlJobsExport],
  },
  {
    path: "/admin/users",
    element: <UsersPage />,
    requiredPermissions: [permissions.adminAccess, permissions.usersView],
  },
  {
    path: "/admin/roles",
    element: <RolesPage />,
    requiredPermissions: [permissions.adminAccess, permissions.rolesView],
  },
];

function renderAdminRoute({ element, path, requiredPermissions }: AdminRoute) {
  return (
    <Route
      key={path}
      path={path}
      element={
        <ProtectedRoute loginPath="/admin/login" permissions={requiredPermissions}>
          <AppShell>{element}</AppShell>
        </ProtectedRoute>
      }
    />
  );
}

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<AuthPage variant="user" mode="login" />} />
        <Route path="/register" element={<AuthPage variant="user" mode="register" />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/confirm-email" element={<ConfirmEmailPage />} />
        <Route path="/admin/login" element={<AuthPage variant="admin" />} />
        <Route path="/" element={<UserCrawlerPage />} />
        <Route
          path="/reports"
          element={
            <ProtectedRoute>
              <UserReportsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/reports/:id"
          element={
            <ProtectedRoute>
              <main className="user-shell">
                <UserTopbar />
                <JobDetailsPage variant="user" />
              </main>
            </ProtectedRoute>
          }
        />
        <Route path="/admin" element={<Navigate to="/admin/overview" replace />} />
        {adminRoutes.map(renderAdminRoute)}
      </Routes>
    </AuthProvider>
  );
}
