import { Navigate, Route, Routes } from "react-router-dom";
import { AppShell } from "./components/AppShell";
import { JobDetailsPage } from "./pages/JobDetailsPage";
import { JobsPage } from "./pages/JobsPage";
import { PagesPage } from "./pages/PagesPage";
import { SchedulesPage } from "./pages/SchedulesPage";

export default function App() {
  return (
    <AppShell>
      <Routes>
        <Route path="/" element={<Navigate to="/jobs" replace />} />
        <Route path="/jobs" element={<JobsPage />} />
        <Route path="/jobs/:id" element={<JobDetailsPage />} />
        <Route path="/pages" element={<PagesPage />} />
        <Route path="/schedules" element={<SchedulesPage />} />
      </Routes>
    </AppShell>
  );
}
