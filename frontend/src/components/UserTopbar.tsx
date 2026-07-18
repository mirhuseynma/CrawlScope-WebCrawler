import { Link, NavLink, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useEffect, useRef, useState } from "react";

export function UserTopbar() {
  const { logout, status, user } = useAuth();
  const isAuthenticated = status === "authenticated";
  const detailsRef = useRef<HTMLDetailsElement>(null);
  const topbarRef = useRef<HTMLElement>(null);
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const location = useLocation();

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (detailsRef.current && detailsRef.current.open && !detailsRef.current.contains(event.target as Node)) {
        detailsRef.current.open = false;
      }
      if (isMenuOpen && topbarRef.current && !topbarRef.current.contains(event.target as Node)) {
        setIsMenuOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [isMenuOpen]);

  // Close mobile menu on route change
  useEffect(() => {
    setIsMenuOpen(false);
  }, [location.pathname]);

  const closeMenu = () => {
    if (detailsRef.current) {
      detailsRef.current.open = false;
    }
    setIsMenuOpen(false);
  };

  const displayName = user?.fullName || user?.userName || "User";
  const initials = displayName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();

  return (
    <header className={`user-topbar ${isMenuOpen ? "is-open" : ""}`} ref={topbarRef}>
      <div className="topbar-header">
        <Link className="brand-link" to="/">
          <span className="brand-copy">
            <strong>CrawlScope</strong>
            <small>Web crawler</small>
          </span>
        </Link>
        <button
          className="user-mobile-menu-button"
          type="button"
          aria-label={isMenuOpen ? "Close menu" : "Open menu"}
          aria-expanded={isMenuOpen}
          onClick={() => setIsMenuOpen((current) => !current)}
        >
          <span />
          <span />
          <span />
        </button>
      </div>

      <span className="topbar-spacer" aria-hidden="true" />

      <div className="user-account-menu">
        {isAuthenticated ? (
          <details className="user-menu" ref={detailsRef}>
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
                <NavLink className={({ isActive }) => (isActive ? "user-menu-action active" : "user-menu-action")} to="/" end onClick={closeMenu}>
                  New crawl
                </NavLink>
                <NavLink className={({ isActive }) => (isActive ? "user-menu-action active" : "user-menu-action")} to="/reports" onClick={closeMenu}>
                  My reports
                </NavLink>
              </nav>
              <button className="user-menu-action" type="button" onClick={() => { logout(); closeMenu(); }}>
                Sign out
              </button>
            </div>
          </details>
        ) : (
          <div className="topbar-auth-actions">
            <Link className="secondary-link-button topbar-action" to="/login" onClick={closeMenu}>
              Login
            </Link>
            <Link className="primary-button topbar-action" to="/register" onClick={closeMenu}>
              Create account
            </Link>
          </div>
        )}
      </div>
    </header>
  );
}
