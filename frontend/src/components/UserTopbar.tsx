import { Link, NavLink, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useEffect, useRef, useState } from "react";

export function UserTopbar() {
  const { logout, status, user } = useAuth();
  const isAuthenticated = status === "authenticated";
  const detailsRef = useRef<HTMLDetailsElement>(null);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const location = useLocation();

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (detailsRef.current && detailsRef.current.open && !detailsRef.current.contains(event.target as Node)) {
        detailsRef.current.open = false;
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  // Close mobile menu on route change
  useEffect(() => {
    setIsMobileMenuOpen(false);
  }, [location.pathname]);

  const closeMenu = () => {
    if (detailsRef.current) {
      detailsRef.current.open = false;
    }
    setIsMobileMenuOpen(false);
  };

  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
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
    <header className="user-topbar">
      <div className="topbar-brand-area">
        <Link className="brand-link" to="/">
          <span className="brand-copy">
            <strong>CrawlScope</strong>
            <small>Web crawler</small>
          </span>
        </Link>
        <button 
          className={`mobile-menu-toggle ${isMobileMenuOpen ? "open" : ""}`} 
          onClick={toggleMobileMenu}
          aria-label="Toggle menu"
          aria-expanded={isMobileMenuOpen}
        >
          <span className="hamburger-line"></span>
          <span className="hamburger-line"></span>
          <span className="hamburger-line"></span>
        </button>
      </div>

      <span className="topbar-spacer" aria-hidden="true" />

      <div className={`user-account-menu ${isMobileMenuOpen ? "mobile-open" : ""}`}>
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
