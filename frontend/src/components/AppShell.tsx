import type { ReactNode } from "react";
import { Link, NavLink } from "react-router-dom";

type AppShellProps = {
  children: ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <span className="brand-mark">CS</span>
          <div>
            <p className="eyebrow">CrawlScope</p>
            <h1>Admin</h1>
          </div>
        </div>
        <nav className="nav-list" aria-label="Primary navigation">
          <Link className="nav-item" to="/">
            User view
          </Link>
          <NavLink className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to="/jobs">
            Jobs
          </NavLink>
          <NavLink className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to="/pages">
            Pages
          </NavLink>
          <NavLink className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to="/schedules">
            Schedules
          </NavLink>
        </nav>
        <div className="sidebar-footnote">
          <span>Operations console</span>
          <strong>Live crawler control</strong>
        </div>
      </aside>
      <main className="content">{children}</main>
    </div>
  );
}
