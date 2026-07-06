import { Navigate, Route, Routes } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext";
import { permissions } from "./auth/permissions";
import { AppShell } from "./components/AppShell";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { JobDetailsPage } from "./pages/JobDetailsPage";
import { JobsPage } from "./pages/JobsPage";
import { PagesPage } from "./pages/PagesPage";
import { SchedulesPage } from "./pages/SchedulesPage";
import { AuthPage } from "./pages/AuthPage";
import { UserCrawlerPage } from "./pages/UserCrawlerPage";
import { UserReportsPage } from "./pages/UserReportsPage";

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<AuthPage />} />
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
                <JobDetailsPage variant="user" />
              </main>
            </ProtectedRoute>
          }
        />
        <Route path="/admin" element={<Navigate to="/admin/jobs" replace />} />
        <Route
          path="/admin/jobs"
          element={
            <ProtectedRoute permission={permissions.adminAccess}>
              <AppShell>
                <JobsPage />
              </AppShell>
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/jobs/:id"
          element={
            <ProtectedRoute permission={permissions.adminAccess}>
              <AppShell>
                <JobDetailsPage variant="admin" />
              </AppShell>
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/pages"
          element={
            <ProtectedRoute permission={permissions.adminAccess}>
              <AppShell>
                <PagesPage />
              </AppShell>
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/schedules"
          element={
            <ProtectedRoute permission={permissions.adminAccess}>
              <AppShell>
                <SchedulesPage />
              </AppShell>
            </ProtectedRoute>
          }
        />
      </Routes>
    </AuthProvider>
  );
}
