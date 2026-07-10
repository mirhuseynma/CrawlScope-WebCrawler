import { Link, NavLink } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export function UserTopbar() {
  const { logout, status, user } = useAuth();
  const isAuthenticated = status === "authenticated";
  const displayName = user?.fullName || user?.userName || "User";
  const initials = displayName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();

  return (
    <header className="user-topbar">
      <Link className="brand-link" to="/">
        <span className="brand-copy">
          <strong>CrawlScope</strong>
          <small>Web crawler</small>
        </span>
      </Link>

      <span className="topbar-spacer" aria-hidden="true" />

      <div className="user-account-menu">
        {isAuthenticated ? (
          <details className="user-menu">
            <summary className="user-identity">
              <span className="user-avatar" aria-hidden="true">
                {initials || "U"}
              </span>
              <span className="user-menu-copy">
                <strong>{displayName}</strong>
                <small>Account</small>
              </span>
            </summary>
            <div className="user-menu-panel">
              <div className="user-menu-summary">
                <strong>{displayName}</strong>
                <span>Signed in</span>
              </div>
              <nav className="user-menu-nav" aria-label="Workspace navigation">
                <NavLink className={({ isActive }) => (isActive ? "user-menu-action active" : "user-menu-action")} to="/" end>
                  New crawl
                </NavLink>
                <NavLink className={({ isActive }) => (isActive ? "user-menu-action active" : "user-menu-action")} to="/reports">
                  My reports
                </NavLink>
              </nav>
              <button className="user-menu-action" type="button" onClick={logout}>
                Sign out
              </button>
            </div>
          </details>
        ) : (
          <div className="topbar-auth-actions">
            <Link className="secondary-link-button topbar-action" to="/login">
              Login
            </Link>
            <Link className="primary-button topbar-action" to="/register">
              Create account
            </Link>
          </div>
        )}
      </div>
    </header>
  );
}
