import { useEffect, useState, useRef } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { request } from "../api/httpClient";

export function ConfirmEmailPage() {
  const [searchParams] = useSearchParams();
  const userId = searchParams.get("userId") || "";
  const token = searchParams.get("token") || "";

  const [status, setStatus] = useState<"loading" | "success" | "error">("loading");
  const [errorMessage, setErrorMessage] = useState("");
  const hasAttempted = useRef(false);

  useEffect(() => {
    if (!userId || !token) {
      setStatus("error");
      setErrorMessage("Invalid confirmation link. Missing parameters.");
      return;
    }

    if (hasAttempted.current) return;
    hasAttempted.current = true;

    const confirmEmail = async () => {
      try {
        await request<{ message?: string }>(`/api/auth/confirm-email?userId=${encodeURIComponent(userId)}&token=${encodeURIComponent(token)}`);
        
        setStatus("success");
      } catch (error) {
        setStatus("error");
        setErrorMessage(error instanceof Error ? error.message : "Confirmation failed.");
      }
    };

    void confirmEmail();
  }, [userId, token]);

  return (
    <main className="auth-shell">
      <section className="auth-panel auth-panel-compact" aria-label="Email Confirmation">
        <div className="auth-copy">
          <p className="eyebrow">CrawlScope Security</p>
          <h1>Account Verification</h1>
          <p>We are verifying your email address.</p>
        </div>

        <div className="auth-form auth-message-panel">
          {status === "loading" && (
            <div className="alert" style={{ background: "transparent", color: "var(--text-color)" }}>
              Please wait while we confirm your email...
            </div>
          )}

          {status === "success" && (
            <>
              <div className="alert" style={{ background: "rgba(34, 197, 94, 0.1)", color: "#22c55e", borderColor: "#22c55e" }}>
                Your email has been successfully verified! You can now access all features.
              </div>
              <Link className="primary-button auth-submit" to="/login">
                Login to your account
              </Link>
            </>
          )}

          {status === "error" && (
            <>
              <div className="alert">{errorMessage}</div>
              <Link className="secondary-button auth-submit" to="/login">
                Return to login
              </Link>
            </>
          )}
        </div>
      </section>
    </main>
  );
}
