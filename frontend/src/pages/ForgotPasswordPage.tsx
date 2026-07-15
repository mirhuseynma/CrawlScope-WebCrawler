import { FormEvent, useState } from "react";
import { Link } from "react-router-dom";
import { request } from "../api/httpClient";

export function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await request<{ message?: string }>("/api/auth/forgot-password", { method: "POST", body: { email } });
      setSuccess(true);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "An unexpected error occurred.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="auth-shell">
      <section className="auth-panel auth-panel-compact" aria-label="Forgot Password">
        <div className="auth-copy">
          <p className="eyebrow">CrawlScope Security</p>
          <h1>Reset your password</h1>
          <p>
            Enter your email address and we'll send you a link to securely reset your password.
          </p>
          <div className="auth-proof">
            <span>Secure connection</span>
            <span>Encrypted token</span>
          </div>
        </div>

        {success ? (
          <div className="auth-form auth-message-panel">
            <div className="alert" style={{ background: "rgba(34, 197, 94, 0.1)", color: "#22c55e", borderColor: "#22c55e" }}>
              If an account with that email exists, we have sent a password reset link. Please check your inbox.
            </div>
            <Link className="primary-button auth-submit" to="/login">
              Return to login
            </Link>
          </div>
        ) : (
          <form className="auth-form" onSubmit={(event) => void handleSubmit(event)}>
            <label>
              Email address
              <input
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                autoComplete="email"
                required
              />
            </label>

            <button className="primary-button auth-submit" type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Sending..." : "Send reset link"}
            </button>

            {error && <div className="alert">{error}</div>}

            <div className="auth-route-switch">
              <span>Remembered your password?</span>
              <Link to="/login">Return to login</Link>
            </div>
          </form>
        )}
      </section>
    </main>
  );
}
