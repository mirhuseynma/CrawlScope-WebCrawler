import { FormEvent, useMemo, useState } from "react";
import { Link, Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { permissions } from "../auth/permissions";

type AuthMode = "login" | "register";
type AuthPageProps = {
  variant?: "user" | "admin";
};

export function AuthPage({ variant = "user" }: AuthPageProps) {
  const [mode, setMode] = useState<AuthMode>("login");
  const [emailOrUserName, setEmailOrUserName] = useState("");
  const [email, setEmail] = useState("");
  const [userName, setUserName] = useState("");
  const [fullName, setFullName] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { hasPermission, loginUser, logout, registerUser, status, user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const isAdminLogin = variant === "admin";
  const defaultRedirectPath = isAdminLogin ? "/admin/jobs" : "/";
  const routeState = location.state as { from?: { pathname?: string }; reason?: string } | null;
  const from = routeState?.from?.pathname ?? defaultRedirectPath;
  const accessNotice =
    routeState?.reason === "forbidden"
      ? "Your account is signed in, but it does not have permission to access the admin workspace."
      : null;

  const title = useMemo(() => {
    if (isAdminLogin) {
      return "Admin sign in";
    }

    return mode === "login" ? "Welcome back" : "Create your crawler account";
  }, [isAdminLogin, mode]);

  if (status === "authenticated" && isAdminLogin && !hasPermission(permissions.adminAccess)) {
    return (
      <main className="auth-shell">
        <section className="auth-panel auth-panel-compact" aria-label="Admin access required">
          <div className="auth-copy">
            <p className="eyebrow">CrawlScope</p>
            <h1>Admin access required</h1>
            <p>
              {user?.email || user?.userName} is signed in, but this account does not have permission to open the admin workspace.
            </p>
            <div className="auth-proof">
              <span>Permission required</span>
              <span>Current role: {user?.roles.join(", ") || "User"}</span>
            </div>
          </div>

          <div className="auth-form auth-message-panel">
            <div className="alert">Use an admin account or return to the user workspace.</div>
            <Link className="primary-button auth-submit" to="/">
              Go to user workspace
            </Link>
            <button className="secondary-button auth-submit" type="button" onClick={logout}>
              Sign out
            </button>
          </div>
        </section>
      </main>
    );
  }

  if (status === "authenticated") {
    return <Navigate to={defaultRedirectPath} replace />;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      if (mode === "login" || isAdminLogin) {
        const authenticatedUser = await loginUser({ emailOrUserName, password });

        if (isAdminLogin && !authenticatedUser.permissions.includes(permissions.adminAccess)) {
          logout();
          setError("This account does not have admin access.");
          return;
        }
      } else {
        if (password !== confirmPassword) {
          setError("Passwords do not match.");
          return;
        }

        await registerUser({ email, userName, fullName: fullName || undefined, password, confirmPassword });
      }

      navigate(from, { replace: true });
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Authentication failed.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="auth-shell">
      <section className="auth-panel" aria-label="Authentication">
        <div className="auth-copy">
          <p className="eyebrow">CrawlScope</p>
          <h1>{title}</h1>
          <p>
            {isAdminLogin
              ? "Access crawl operations, schedules, exports, and system-wide reporting from the secured admin workspace."
              : "Run focused crawl reports, inspect indexed pages, and export structured results from one secured workspace."}
          </p>
          <div className="auth-proof">
            {isAdminLogin ? (
              <>
                <span>Admin workspace</span>
                <span>Permission required</span>
                <span>JWT secured</span>
              </>
            ) : (
              <>
                <span>JWT secured</span>
                <span>Private reports</span>
                <span>Export ready</span>
              </>
            )}
          </div>
        </div>

        <form className="auth-form" onSubmit={(event) => void handleSubmit(event)}>
          {!isAdminLogin && (
            <div className="auth-mode-toggle" role="tablist" aria-label="Authentication mode">
              <button className={mode === "login" ? "active" : ""} type="button" onClick={() => setMode("login")}>
                Login
              </button>
              <button className={mode === "register" ? "active" : ""} type="button" onClick={() => setMode("register")}>
                Register
              </button>
            </div>
          )}

          {mode === "login" ? (
            <label>
              Email or username
              <input
                value={emailOrUserName}
                onChange={(event) => setEmailOrUserName(event.target.value)}
                autoComplete="username"
                required
              />
            </label>
          ) : (
            <>
              <label>
                Full name
                <input
                  value={fullName}
                  onChange={(event) => setFullName(event.target.value)}
                  autoComplete="name"
                  maxLength={120}
                  required
                />
              </label>
              <label>
                Username
                <input
                  value={userName}
                  onChange={(event) => setUserName(event.target.value)}
                  autoComplete="username"
                  minLength={3}
                  maxLength={60}
                  required
                />
              </label>
              <label>
                Email
                <input
                  type="email"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  autoComplete="email"
                  maxLength={160}
                  required
                />
              </label>
            </>
          )}

          <label>
            Password
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete={mode === "login" ? "current-password" : "new-password"}
              minLength={mode === "register" ? 8 : undefined}
              required
            />
          </label>

          {mode === "register" && (
            <label>
              Confirm password
              <input
                type="password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                autoComplete="new-password"
                minLength={8}
                required
              />
            </label>
          )}

          <button className="primary-button auth-submit" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Please wait..." : mode === "login" || isAdminLogin ? "Login" : "Create account"}
          </button>

          {accessNotice && <div className="alert">{accessNotice}</div>}
          {error && <div className="alert">{error}</div>}
        </form>
      </section>
    </main>
  );
}
