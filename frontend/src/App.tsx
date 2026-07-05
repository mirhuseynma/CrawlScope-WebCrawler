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

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<AuthPage />} />
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <UserCrawlerPage />
            </ProtectedRoute>
          }
        />
        <Route path="/admin" element={<Navigate to="/jobs" replace />} />
        <Route
          path="/jobs"
          element={
            <ProtectedRoute permission={permissions.adminAccess}>
              <AppShell>
                <JobsPage />
              </AppShell>
            </ProtectedRoute>
          }
        />
        <Route
          path="/jobs/:id"
          element={
            <ProtectedRoute permission={permissions.adminAccess}>
              <AppShell>
                <JobDetailsPage />
              </AppShell>
            </ProtectedRoute>
          }
        />
        <Route
          path="/pages"
          element={
            <ProtectedRoute permission={permissions.adminAccess}>
              <AppShell>
                <PagesPage />
              </AppShell>
            </ProtectedRoute>
          }
        />
        <Route
          path="/schedules"
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
