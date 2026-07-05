import { Navigate, Route, Routes } from "react-router-dom";
import { AppShell } from "./components/AppShell";
import { JobDetailsPage } from "./pages/JobDetailsPage";
import { JobsPage } from "./pages/JobsPage";
import { PagesPage } from "./pages/PagesPage";
import { SchedulesPage } from "./pages/SchedulesPage";
import { UserCrawlerPage } from "./pages/UserCrawlerPage";

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<UserCrawlerPage />} />
      <Route path="/admin" element={<Navigate to="/jobs" replace />} />
      <Route
        path="/jobs"
        element={
          <AppShell>
            <JobsPage />
          </AppShell>
        }
      />
      <Route
        path="/jobs/:id"
        element={
          <AppShell>
            <JobDetailsPage />
          </AppShell>
        }
      />
      <Route
        path="/pages"
        element={
          <AppShell>
            <PagesPage />
          </AppShell>
        }
      />
      <Route
        path="/schedules"
        element={
          <AppShell>
            <SchedulesPage />
          </AppShell>
        }
      />
    </Routes>
  );
}
