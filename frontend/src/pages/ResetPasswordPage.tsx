import { FormEvent, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { request } from "../api/httpClient";

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const email = searchParams.get("email") || "";
  const token = searchParams.get("token") || "";

  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  if (!email || !token) {
    return (
      <main className="auth-shell">
        <section className="auth-panel auth-panel-compact" aria-label="Invalid Link">
          <div className="auth-copy">
            <p className="eyebrow">CrawlScope</p>
            <h1>Invalid link</h1>
            <p>The password reset link is invalid or has expired.</p>
          </div>
          <div className="auth-form auth-message-panel">
            <Link className="primary-button auth-submit" to="/forgot-password">
              Request new link
            </Link>
          </div>
        </section>
      </main>
    );
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setIsSubmitting(true);

    try {
      await request<{ message?: string }>("/api/auth/reset-password", {
        method: "POST",
        body: {
          email,
          token,
          newPassword: password
        }
      });
      
      setSuccess(true);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "An unexpected error occurred.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="auth-shell">
      <section className="auth-panel auth-panel-compact" aria-label="Create New Password">
        <div className="auth-copy">
          <p className="eyebrow">CrawlScope Security</p>
          <h1>Create new password</h1>
          <p>Please enter your new password below to regain access to your workspace.</p>
        </div>

        {success ? (
          <div className="auth-form auth-message-panel">
            <div className="alert" style={{ background: "rgba(34, 197, 94, 0.1)", color: "#22c55e", borderColor: "#22c55e" }}>
              Your password has been successfully reset!
            </div>
            <Link className="primary-button auth-submit" to="/login">
              Login to your account
            </Link>
          </div>
        ) : (
          <form className="auth-form" onSubmit={(event) => void handleSubmit(event)}>
            <label>
              New Password
              <input
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                autoComplete="new-password"
                minLength={8}
                required
              />
            </label>
            <label>
              Confirm Password
              <input
                type="password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                autoComplete="new-password"
                minLength={8}
                required
              />
            </label>

            <button className="primary-button auth-submit" type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Resetting..." : "Reset password"}
            </button>

            {error && <div className="alert">{error}</div>}
          </form>
        )}
      </section>
    </main>
  );
}
