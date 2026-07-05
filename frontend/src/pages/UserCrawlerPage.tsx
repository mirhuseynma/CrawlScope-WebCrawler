import { FormEvent, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { createCrawlJob, getAllCrawledPages, startCrawlJob } from "../api/crawlJobsApi";
import type { CrawledPage, CreateCrawlJobRequest, PagedResult } from "../types/crawlJob";

const initialFormState: CreateCrawlJobRequest = {
  targetUrl: "https://example.com",
  maxDepth: 1,
  maxPages: 5,
  stayWithinDomain: true,
};

const emptyPagesPage: PagedResult<CrawledPage> = {
  items: [],
  pageNumber: 1,
  pageSize: 5,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

export function UserCrawlerPage() {
  const [form, setForm] = useState<CreateCrawlJobRequest>(initialFormState);
  const [recentPages, setRecentPages] = useState<PagedResult<CrawledPage>>(emptyPagesPage);
  const [createdJobId, setCreatedJobId] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoadingRecent, setIsLoadingRecent] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const crawlScopeLabel = useMemo(() => (form.stayWithinDomain ? "Domain only" : "External links allowed"), [form.stayWithinDomain]);
  const successfulRecentPages = useMemo(
    () => recentPages.items.filter((page) => page.statusCode && page.statusCode >= 200 && page.statusCode < 300).length,
    [recentPages.items],
  );

  async function loadRecentPages() {
    setIsLoadingRecent(true);

    try {
      const data = await getAllCrawledPages({
        pageNumber: 1,
        pageSize: 5,
      });
      setRecentPages(data);
    } catch {
      setRecentPages(emptyPagesPage);
    } finally {
      setIsLoadingRecent(false);
    }
  }

  useEffect(() => {
    void loadRecentPages();
  }, []);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setCreatedJobId(null);
    setError(null);

    try {
      const jobId = await createCrawlJob(form);
      await startCrawlJob(jobId);
      setCreatedJobId(jobId);
      await loadRecentPages();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to start crawl.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="user-shell">
      <header className="user-topbar">
        <Link className="brand-link" to="/">
          CrawlScope
        </Link>
        <Link className="secondary-link-button" to="/jobs">
          Admin panel
        </Link>
      </header>

      <section className="user-workspace">
        <div className="user-intro">
          <p className="eyebrow">Crawler studio</p>
          <h1>Turn public pages into crawl reports</h1>
          <p className="user-summary">Collect page titles, links, status codes, and content snapshots from a single focused workspace.</p>
          <div className="user-stat-strip" aria-label="Crawler highlights">
            <div>
              <strong>{recentPages.totalCount}</strong>
              <span>indexed pages</span>
            </div>
            <div>
              <strong>{successfulRecentPages}</strong>
              <span>healthy recent</span>
            </div>
            <div>
              <strong>CSV/JSON</strong>
              <span>export ready</span>
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
                  max="10"
                  value={form.maxDepth}
                  onChange={(event) => setForm((current) => ({ ...current, maxDepth: Number(event.target.value) }))}
                  required
                />
              </label>
              <label>
                Max pages
                <input
                  type="number"
                  min="1"
                  max="500"
                  value={form.maxPages}
                  onChange={(event) => setForm((current) => ({ ...current, maxPages: Number(event.target.value) }))}
                  required
                />
              </label>
            </div>

            <label className="checkbox-row">
              <input
                type="checkbox"
                checked={form.stayWithinDomain}
                onChange={(event) => setForm((current) => ({ ...current, stayWithinDomain: event.target.checked }))}
              />
              Stay within domain
            </label>

            <div className="crawl-review">
              <span>{crawlScopeLabel}</span>
              <span>{form.maxDepth} depth</span>
              <span>{form.maxPages} pages</span>
            </div>

            <button className="primary-button user-submit-button" type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Crawling..." : "Start crawl"}
            </button>

            {error && <div className="alert">{error}</div>}
            {createdJobId && (
              <div className="success-callout">
                <strong>Crawl started</strong>
                <Link to={`/jobs/${createdJobId}`}>Open result</Link>
              </div>
            )}
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
              <strong>{successfulRecentPages}</strong>
              <span>healthy recent pages</span>
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

      <section className="recent-results">
        <div className="section-header">
          <div>
            <p className="eyebrow">Recent results</p>
            <h2>Latest crawled pages</h2>
          </div>
          <Link className="secondary-link-button" to="/pages">
            View all
          </Link>
        </div>

        {isLoadingRecent ? (
          <div className="empty-state">Loading recent pages...</div>
        ) : recentPages.items.length === 0 ? (
          <div className="empty-state">No crawled pages yet.</div>
        ) : (
          <div className="result-grid">
            {recentPages.items.map((page) => (
              <article className="result-card" key={page.id}>
                <div>
                  <span
                    className={`status-badge ${
                      page.statusCode && page.statusCode >= 400 ? "status-failed" : "status-completed"
                    }`}
                  >
                    {page.statusCode ?? "No status"}
                  </span>
                </div>
                <h3>{page.title || "Untitled page"}</h3>
                <p>{page.url}</p>
                <div className="result-meta">
                  <span>Depth {page.depthLevel}</span>
                  <span>
                    {page.internalLinksCount} internal / {page.externalLinksCount} external
                  </span>
                </div>
                <Link className="text-button" to={`/jobs/${page.crawlJobId}`}>
                  Open job
                </Link>
              </article>
            ))}
          </div>
        )}
      </section>
    </main>
  );
}
