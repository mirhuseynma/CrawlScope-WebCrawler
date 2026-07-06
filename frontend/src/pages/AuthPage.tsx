import { FormEvent, useMemo, useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

type AuthMode = "login" | "register";

export function AuthPage() {
  const [mode, setMode] = useState<AuthMode>("login");
  const [emailOrUserName, setEmailOrUserName] = useState("");
  const [email, setEmail] = useState("");
  const [userName, setUserName] = useState("");
  const [fullName, setFullName] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { loginUser, registerUser, status } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname ?? "/";

  const title = useMemo(() => (mode === "login" ? "Welcome back" : "Create your crawler account"), [mode]);

  if (status === "authenticated") {
    return <Navigate to="/" replace />;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      if (mode === "login") {
        await loginUser({ emailOrUserName, password });
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
          <p>Run focused crawl reports, inspect indexed pages, and export structured results from one secured workspace.</p>
          <div className="auth-proof">
            <span>JWT secured</span>
            <span>Permission aware</span>
            <span>Admin ready</span>
          </div>
        </div>

        <form className="auth-form" onSubmit={(event) => void handleSubmit(event)}>
          <div className="auth-mode-toggle" role="tablist" aria-label="Authentication mode">
            <button className={mode === "login" ? "active" : ""} type="button" onClick={() => setMode("login")}>
              Login
            </button>
            <button className={mode === "register" ? "active" : ""} type="button" onClick={() => setMode("register")}>
              Register
            </button>
          </div>

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
            {isSubmitting ? "Please wait..." : mode === "login" ? "Login" : "Create account"}
          </button>

          {error && <div className="alert">{error}</div>}
        </form>
      </section>
    </main>
  );
}
