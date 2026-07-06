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

      <nav className="user-nav" aria-label="User navigation">
        <NavLink className={({ isActive }) => (isActive ? "user-nav-link active" : "user-nav-link")} to="/" end>
          New crawl
        </NavLink>
        {isAuthenticated && (
          <NavLink className={({ isActive }) => (isActive ? "user-nav-link active" : "user-nav-link")} to="/reports">
            My reports
          </NavLink>
        )}
      </nav>

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
              <button className="user-menu-action" type="button" onClick={logout}>
                Sign out
              </button>
            </div>
          </details>
        ) : (
          <Link className="secondary-link-button topbar-action" to="/login">
            Login
          </Link>
        )}
      </div>
    </header>
  );
}
