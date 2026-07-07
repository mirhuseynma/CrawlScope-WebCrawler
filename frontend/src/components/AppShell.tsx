import type { ReactNode } from "react";
import { Link, NavLink } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { permissions } from "../auth/permissions";

type AppShellProps = {
  children: ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  const { hasPermission, logout, user } = useAuth();

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
          {hasPermission(permissions.adminAccess) && (
            <>
              <NavLink className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to="/admin/overview">
                Overview
              </NavLink>
              <NavLink className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to="/admin/jobs">
                Jobs
              </NavLink>
              <NavLink className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to="/admin/pages">
                Pages
              </NavLink>
              <NavLink className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to="/admin/schedules">
                Schedules
              </NavLink>
              <NavLink className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to="/admin/exports">
                Exports
              </NavLink>
            </>
          )}
        </nav>
        <div className="sidebar-footnote">
          <span>{user?.fullName || user?.userName}</span>
          <strong>{user?.roles.join(", ")}</strong>
          <button className="logout-button" type="button" onClick={logout}>
            Logout
          </button>
        </div>
      </aside>
      <main className="content">{children}</main>
    </div>
  );
}
