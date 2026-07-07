import { useState, type ReactNode } from "react";
import { Link, NavLink } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { permissions } from "../auth/permissions";

type AppShellProps = {
  children: ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  const { hasPermission, logout, user } = useAuth();
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  function closeMenu() {
    setIsMenuOpen(false);
  }

  return (
    <div className="app-shell">
      <aside className={`sidebar${isMenuOpen ? " is-open" : ""}`}>
        <div className="sidebar-brand">
          <div className="sidebar-brand-lockup">
            <span className="brand-mark">CS</span>
            <div>
              <p className="eyebrow">CrawlScope</p>
              <h1>Admin</h1>
            </div>
          </div>
          <button
            className="mobile-menu-button"
            type="button"
            aria-label={isMenuOpen ? "Close admin menu" : "Open admin menu"}
            aria-expanded={isMenuOpen}
            onClick={() => setIsMenuOpen((current) => !current)}
          >
            <span />
            <span />
            <span />
          </button>
        </div>
        <nav className="nav-list" aria-label="Primary navigation">
          <Link className="nav-item" to="/" onClick={closeMenu}>
            User view
          </Link>
          {hasPermission(permissions.adminAccess) && (
            <>
              <NavLink
                className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")}
                to="/admin/overview"
                onClick={closeMenu}
              >
                Overview
              </NavLink>
              <NavLink className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to="/admin/jobs" onClick={closeMenu}>
                Jobs
              </NavLink>
              <NavLink className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to="/admin/pages" onClick={closeMenu}>
                Pages
              </NavLink>
              <NavLink
                className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")}
                to="/admin/schedules"
                onClick={closeMenu}
              >
                Schedules
              </NavLink>
              <NavLink
                className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")}
                to="/admin/exports"
                onClick={closeMenu}
              >
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
