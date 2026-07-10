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

  const userInitial = (user?.fullName || user?.userName || "A")[0]?.toUpperCase() ?? "A";

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
          <Link className="nav-item nav-item-ghost" to="/" onClick={closeMenu}>
            ← User view
          </Link>

          {hasPermission(permissions.adminAccess) && (
            <>
              <div className="admin-nav-section">
                <span className="admin-nav-label">Crawl Engine</span>
                <NavLink
                  className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")}
                  to="/admin/overview"
                  onClick={closeMenu}
                >
                  <span className="nav-icon">⊞</span>Overview
                </NavLink>
                {hasPermission(permissions.crawlJobsView) && (
                  <NavLink
                    className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")}
                    to="/admin/jobs"
                    onClick={closeMenu}
                  >
                    <span className="nav-icon">◈</span>Jobs
                  </NavLink>
                )}
                {hasPermission(permissions.crawledPagesView) && (
                  <NavLink
                    className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")}
                    to="/admin/pages"
                    onClick={closeMenu}
                  >
                    <span className="nav-icon">◎</span>Pages
                  </NavLink>
                )}
                {hasPermission(permissions.schedulesView) && (
                  <NavLink
                    className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")}
                    to="/admin/schedules"
                    onClick={closeMenu}
                  >
                    <span className="nav-icon">◷</span>Schedules
                  </NavLink>
                )}
                {hasPermission(permissions.crawlJobsExport) && (
                  <NavLink
                    className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")}
                    to="/admin/exports"
                    onClick={closeMenu}
                  >
                    <span className="nav-icon">↓</span>Exports
                  </NavLink>
                )}
              </div>

              {(hasPermission(permissions.usersView) || hasPermission(permissions.rolesView)) && (
                <div className="admin-nav-section">
                  <span className="admin-nav-label">Access Control</span>
                  {hasPermission(permissions.usersView) && (
                    <NavLink
                      className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")}
                      to="/admin/users"
                      onClick={closeMenu}
                    >
                      <span className="nav-icon">◉</span>Users
                    </NavLink>
                  )}
                  {hasPermission(permissions.rolesView) && (
                    <NavLink
                      className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")}
                      to="/admin/roles"
                      onClick={closeMenu}
                    >
                      <span className="nav-icon">◐</span>Roles
                    </NavLink>
                  )}
                </div>
              )}
            </>
          )}
        </nav>

        <div className="sidebar-footnote">
          <div className="sidebar-user">
            <span className="sidebar-user-avatar">{userInitial}</span>
            <div className="sidebar-user-info">
              <strong>{user?.fullName || user?.userName}</strong>
              <small>{user?.roles.join(", ")}</small>
            </div>
          </div>
          <button className="logout-button" type="button" onClick={logout}>
            Logout
          </button>
        </div>
      </aside>

      <main className="content">{children}</main>
    </div>
  );
}
