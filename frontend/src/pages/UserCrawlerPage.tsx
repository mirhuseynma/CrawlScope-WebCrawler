import { FormEvent, useEffect, useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { analyzeUrl, createCrawlJob, startCrawlJob } from "../api/crawlJobsApi";
import { useAuth } from "../auth/AuthContext";
import { UserTopbar } from "../components/UserTopbar";
import type { CreateCrawlJobRequest } from "../types/crawlJob";

const initialFormState: CreateCrawlJobRequest = {
  targetUrl: "https://example.com",
  maxDepth: 1,
  maxPages: 5,
  stayWithinDomain: true,
  crawlType: "Fast",
};

export function UserCrawlerPage() {
  const { status } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState<CreateCrawlJobRequest>(initialFormState);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showAnalysisModal, setShowAnalysisModal] = useState(false);
  const [analysisReason, setAnalysisReason] = useState("");
  const [pendingJobRequest, setPendingJobRequest] = useState<CreateCrawlJobRequest | null>(null);

  const crawlScopeLabel = useMemo(() => (form.stayWithinDomain ? "Domain only" : "External links allowed"), [form.stayWithinDomain]);
  const isAuthenticated = status === "authenticated";
  const isCheckingSession = status === "checking";
  const maxAllowedDepth = isAuthenticated ? 10 : 0;
  const maxAllowedPages = isAuthenticated ? 500 : 1;

  useEffect(() => {
    if (isAuthenticated) {
      return;
    }

    setForm((current) => ({
      ...current,
      maxDepth: 0,
      maxPages: 1,
    }));
  }, [isAuthenticated]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!isAuthenticated) {
      navigate("/login", { state: { from: { pathname: "/" } } });
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const analysis = await analyzeUrl(form.targetUrl);
      if (analysis.recommendedType === "Dynamic" && form.crawlType !== "Dynamic") {
        setAnalysisReason(analysis.recommendationReason);
        setPendingJobRequest(form);
        setShowAnalysisModal(true);
        setIsSubmitting(false);
        return;
      }
      
      const jobId = await createCrawlJob(form);
      await startCrawlJob(jobId);
      navigate(`/reports/${jobId}`);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to analyze or start crawl.");
      setIsSubmitting(false);
    }
  }

  async function handleConfirmDynamicCrawl(useDynamic: boolean) {
    if (!pendingJobRequest) return;
    
    setShowAnalysisModal(false);
    setIsSubmitting(true);
    setError(null);

    try {
      const requestToSubmit: CreateCrawlJobRequest = {
        ...pendingJobRequest,
        crawlType: useDynamic ? "Dynamic" : "Fast"
      };
      const jobId = await createCrawlJob(requestToSubmit);
      await startCrawlJob(jobId);
      navigate(`/reports/${jobId}`);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to start crawl.");
      setIsSubmitting(false);
    }
  }

  return (
    <main className="user-shell">
      <UserTopbar />

      <section className="user-workspace">
        <div className="user-intro">
          <p className="eyebrow">Crawler studio</p>
          <h1>Turn public pages into crawl reports</h1>
          <p className="user-summary">Collect page titles, links, status codes, and content snapshots from a single focused workspace.</p>
          <div className="user-stat-strip" aria-label="Crawler highlights">
            <div>
              <strong>{isAuthenticated ? "Private" : "1 page"}</strong>
              <span>{isAuthenticated ? "reports" : "guest preview"}</span>
            </div>
            <div>
              <strong>{isAuthenticated ? "Live" : "Depth 0"}</strong>
              <span>{isAuthenticated ? "crawl run" : "limited scope"}</span>
            </div>
            <div>
              <strong>CSV/JSON</strong>
              <span>{isAuthenticated ? "export ready" : "after login"}</span>
            </div>
          </div>
        </div>

        <div className="crawl-studio">
          <form className="user-crawl-panel" onSubmit={(event) => void handleSubmit(event)}>
            <div className="panel-heading">
              <div>
                <p className="eyebrow">New report</p>
                <h2>Start a crawl</h2>
              </div>
            </div>

            <label>
              Target URL
              <input
                type="url"
                value={form.targetUrl}
                onChange={(event) => setForm((current) => ({ ...current, targetUrl: event.target.value }))}
                required
              />
            </label>

            <div className="form-row">
              <label>
                Max depth
                <input
                  type="number"
                  min="0"
                  max={maxAllowedDepth}
                  value={form.maxDepth}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      maxDepth: Math.min(Number(event.target.value), maxAllowedDepth),
                    }))
                  }
                  required
                />
              </label>
              <label>
                Max pages
                <input
                  type="number"
                  min="1"
                  max={maxAllowedPages}
                  value={form.maxPages}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      maxPages: Math.min(Number(event.target.value), maxAllowedPages),
                    }))
                  }
                  required
                />
              </label>
            </div>

            <div className="form-row">
              <label>
                Crawl mode
                <select
                  value={form.crawlType}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      crawlType: event.target.value as CreateCrawlJobRequest["crawlType"],
                    }))
                  }
                >
                  <option value="Fast">Standard crawl</option>
                  <option value="Dynamic">Browser crawl</option>
                </select>
              </label>

              <label className="checkbox-row" style={{ marginTop: '22px' }}>
                <input
                  type="checkbox"
                  checked={form.stayWithinDomain}
                  onChange={(event) => setForm((current) => ({ ...current, stayWithinDomain: event.target.checked }))}
                />
                Stay within domain
              </label>
            </div>

            <div className="crawl-review">
              <span>{crawlScopeLabel}</span>
              <span>{form.maxDepth} depth</span>
              <span>{form.maxPages} pages</span>
              <span>{form.crawlType === "Dynamic" ? "Browser" : "Standard"}</span>
            </div>

            <button className="primary-button user-submit-button" type="submit" disabled={isSubmitting || isCheckingSession}>
              {isCheckingSession ? "Checking session..." : isSubmitting ? "Crawling..." : isAuthenticated ? "Start crawl" : "Login to start"}
            </button>

            {!isAuthenticated && (
              <div className="guest-limit-callout">
                Guest mode is limited to a one-page preview. Login to run crawls, save reports, and export results.
              </div>
            )}

            {error && <div className="alert">{error}</div>}
          </form>

          <aside className="report-preview" aria-label="Crawl report preview">
            <div className="report-window-bar">
              <span></span>
              <span></span>
              <span></span>
            </div>
            <div className="report-preview-header">
              <p className="eyebrow">Report preview</p>
              <h2>Structured output</h2>
            </div>
            <div className="preview-score">
              <strong>{form.maxPages}</strong>
              <span>pages in this run</span>
            </div>
            <div className="preview-list">
              <div>
                <span>Titles</span>
                <strong>Extracted</strong>
              </div>
              <div>
                <span>Links</span>
                <strong>Internal / external</strong>
              </div>
              <div>
                <span>Snapshots</span>
                <strong>Searchable</strong>
              </div>
              <div>
                <span>Export</span>
                <strong>CSV / JSON</strong>
              </div>
            </div>
          </aside>
        </div>
      </section>

      <section className="user-insights">
        <div className="section-header">
          <div>
            <p className="eyebrow">Report workspace</p>
            <h2>What CrawlScope prepares for you</h2>
          </div>
        </div>

        <div className="insight-grid">
          <article className="insight-card">
            <span>01</span>
            <h3>Scope control</h3>
            <p>Choose depth, page limit, and whether the crawler should stay inside the target domain.</p>
          </article>
          <article className="insight-card">
            <span>02</span>
            <h3>Readable report</h3>
            <p>Review page titles, status codes, response times, link counts, and captured content previews.</p>
          </article>
          <article className="insight-card">
            <span>03</span>
            <h3>Portable output</h3>
            <p>Export the result as CSV or JSON when you need to continue analysis outside the app.</p>
          </article>
        </div>
      </section>


      {showAnalysisModal && (
        <div className="modal-backdrop">
          <div className="modal">
            <div className="modal-header">
              <h2>Smart Probe: Issue Detected</h2>
              <button className="icon-button" onClick={() => setShowAnalysisModal(false)}>
                &times;
              </button>
            </div>
            <div className="modal-body">
              <p>
                Our quick analysis detected an issue with the target URL:
                <br />
                <strong>{analysisReason}</strong>
              </p>
              <p>
                It is highly recommended to use the <strong>Dynamic (Playwright)</strong> mode to bypass this protection and render the page properly. Note that Dynamic mode may be slower.
              </p>
              <p>Do you want to switch to Dynamic mode?</p>
            </div>
            <div className="modal-footer">
              <button className="secondary-button" onClick={() => handleConfirmDynamicCrawl(false)}>
                No, use Fast mode
              </button>
              <button className="primary-button" onClick={() => handleConfirmDynamicCrawl(true)}>
                Yes, use Dynamic mode
              </button>
            </div>
          </div>
        </div>
      )}
    </main>
  );
}
