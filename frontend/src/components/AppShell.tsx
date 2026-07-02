import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";

type AppShellProps = {
  children: ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div>
          <p className="eyebrow">CrawlScope</p>
          <h1>Dashboard</h1>
        </div>
        <nav className="nav-list" aria-label="Primary navigation">
          <NavLink className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to="/jobs">
            Jobs
          </NavLink>
          <a className="nav-item" href="#schedules">
            Schedules
          </a>
          <a className="nav-item" href="#exports">
            Exports
          </a>
        </nav>
      </aside>
      <main className="content">{children}</main>
    </div>
  );
}
