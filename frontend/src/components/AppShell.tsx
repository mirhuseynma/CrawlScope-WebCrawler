import { useState, type ReactNode } from "react";
import { Link, NavLink } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { permissions } from "../auth/permissions";

type AppShellProps = {
  children: ReactNode;
};

type NavItem = {
  label: string;
  path: string;
  icon: ReactNode;
  permission: string;
};

type NavSection = {
  label: string;
  items: NavItem[];
};

const Icons = {
  Overview: () => (<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect width="7" height="9" x="3" y="3" rx="1"/><rect width="7" height="5" x="14" y="3" rx="1"/><rect width="7" height="9" x="14" y="12" rx="1"/><rect width="7" height="5" x="3" y="16" rx="1"/></svg>),
  Jobs: () => (<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M22 12h-2.48a2 2 0 0 0-1.93 1.46l-2.35 8.36a.25.25 0 0 1-.48 0L9.24 2.18a.25.25 0 0 0-.48 0l-2.35 8.36A2 2 0 0 1 4.48 12H2"/></svg>),
  Pages: () => (<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/><path d="M2 12h20"/></svg>),
  Schedules: () => (<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M21 7.5V6a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h3.5"/><path d="M16 2v4"/><path d="M8 2v4"/><path d="M3 10h5"/><path d="M17.5 17.5 16 16.3V14"/><circle cx="16" cy="16" r="6"/></svg>),
  Exports: () => (<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M4 14.899A7 7 0 1 1 15.71 8h1.79a4.5 4.5 0 0 1 2.5 8.242"/><path d="M12 12v9"/><path d="m8 17 4 4 4-4"/></svg>),
  Users: () => (<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>),
  Roles: () => (<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2-1 4-2 7-2 2.5 0 4.5 1 7 2a1 1 0 0 1 1 1v7z"/></svg>),
  LogOut: () => (<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" x2="9" y1="12" y2="12"/></svg>)
};

const navSections: NavSection[] = [
  {
    label: "Crawl Engine",
    items: [
      { label: "Overview", path: "/admin/overview", icon: <Icons.Overview />, permission: permissions.adminAccess },
      { label: "Jobs", path: "/admin/jobs", icon: <Icons.Jobs />, permission: permissions.crawlJobsView },
      { label: "Pages", path: "/admin/pages", icon: <Icons.Pages />, permission: permissions.crawledPagesView },
      { label: "Schedules", path: "/admin/schedules", icon: <Icons.Schedules />, permission: permissions.schedulesView },
      { label: "Exports", path: "/admin/exports", icon: <Icons.Exports />, permission: permissions.crawlJobsExport },
    ],
  },
  {
    label: "Access Control",
    items: [
      { label: "Users", path: "/admin/users", icon: <Icons.Users />, permission: permissions.usersView },
      { label: "Roles", path: "/admin/roles", icon: <Icons.Roles />, permission: permissions.rolesView },
    ],
  },
];

export function AppShell({ children }: AppShellProps) {
  const { hasPermission, logout, user } = useAuth();
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  function closeMenu() {
    setIsMenuOpen(false);
  }

  const userInitial = (user?.fullName || user?.userName || "A")[0]?.toUpperCase() ?? "A";
  const visibleSections = navSections
    .map((section) => ({
      ...section,
      items: section.items.filter((item) => hasPermission(item.permission)),
    }))
    .filter((section) => section.items.length > 0);

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
            Back to user view
          </Link>

          {hasPermission(permissions.adminAccess) &&
            visibleSections.map((section) => (
              <div className="admin-nav-section" key={section.label}>
                <span className="admin-nav-label">{section.label}</span>
                {section.items.map((item) => (
                  <NavLink
                    className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")}
                    key={item.path}
                    to={item.path}
                    onClick={closeMenu}
                  >
                    <span className="nav-icon" aria-hidden="true">
                      {item.icon}
                    </span>
                    {item.label}
                  </NavLink>
                ))}
              </div>
            ))}
        </nav>

        <div className="sidebar-footnote">
          <div className="sidebar-user">
            <span className="sidebar-user-avatar">{userInitial}</span>
            <div className="sidebar-user-info">
              <strong>{user?.fullName || user?.userName}</strong>
              <small>{user?.roles.join(", ")}</small>
            </div>
          </div>
          <button className="logout-button" type="button" onClick={logout} aria-label="Logout" title="Logout">
            <Icons.LogOut />
          </button>
        </div>
      </aside>

      <main className="content">{children}</main>
    </div>
  );
}
