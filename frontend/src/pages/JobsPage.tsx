import { FormEvent, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { analyzeUrl, cancelCrawlJob, createCrawlJob, deleteCrawlJob, getCrawlJobs, startCrawlJob, toggleCrawlJobImportance } from "../api/crawlJobsApi";
import { PaginationControls } from "../components/PaginationControls";
import { StatusBadge } from "../components/StatusBadge";
import type { CrawlJob, CreateCrawlJobRequest, PagedResult } from "../types/crawlJob";

const initialFormState: CreateCrawlJobRequest = {
  targetUrl: "https://example.com",
  maxDepth: 0,
  maxPages: 1,
  stayWithinDomain: true,
  crawlType: "Fast",
};

const emptyJobsPage: PagedResult<CrawlJob> = {
  items: [],
  pageNumber: 1,
  pageSize: 5,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

export function JobsPage() {
  const [jobsPage, setJobsPage] = useState<PagedResult<CrawlJob>>(emptyJobsPage);
  const [form, setForm] = useState<CreateCrawlJobRequest>(initialFormState);
  const [jobsSearch, setJobsSearch] = useState("");
  const [jobsStatus, setJobsStatus] = useState("");
  const [importantOnly, setImportantOnly] = useState(false);
  const [jobsPageNumber, setJobsPageNumber] = useState(1);
  const [jobsPageSize, setJobsPageSize] = useState(5);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showAnalysisModal, setShowAnalysisModal] = useState(false);
  const [analysisReason, setAnalysisReason] = useState("");
  const [pendingJobRequest, setPendingJobRequest] = useState<CreateCrawlJobRequest | null>(null);
  const [activeJobId, setActiveJobId] = useState<string | null>(null);
  const [deletingJobId, setDeletingJobId] = useState<string | null>(null);
  const [updatingImportantJobId, setUpdatingImportantJobId] = useState<string | null>(null);
  const [cancelingJobId, setCancelingJobId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const totals = useMemo(
    () => ({
      jobs: jobsPage.totalCount,
      crawled: jobsPage.items.reduce((sum, job) => sum + job.pagesCrawled, 0),
      failed: jobsPage.items.reduce((sum, job) => sum + job.pagesFailed, 0),
      important: jobsPage.items.filter((job) => job.isImportant).length,
    }),
    [jobsPage],
  );

  async function loadJobs(pageNumber = jobsPageNumber, isBackground = false) {
    if (!isBackground) {
      setIsLoading(true);
    }
    setError(null);

    try {
      const data = await getCrawlJobs({
        search: jobsSearch,
        status: jobsStatus,
        importantOnly,
        pageNumber,
        pageSize: jobsPageSize,
      });
      setJobsPage(data);
      setJobsPageNumber(data.pageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load crawl jobs.");
    } finally {
      if (!isBackground) {
        setIsLoading(false);
      }
    }
  }

  useEffect(() => {
    void loadJobs(1);
  }, []);

  useEffect(() => {
    const hasActiveJobs = jobsPage.items.some(
      (job) => job.status === "Pending" || job.status === "InProgress"
    );
    if (hasActiveJobs) {
      const interval = setInterval(() => {
        void loadJobs(jobsPageNumber, true);
      }, 3000);
      return () => clearInterval(interval);
    }
  }, [jobsPage.items, jobsPageNumber]);

  async function handleCreateJob(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
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
      
      await createCrawlJob(form);
      setForm(initialFormState);
      await loadJobs(1);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to analyze or create crawl job.");
    } finally {
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
      await createCrawlJob(requestToSubmit);
      setForm(initialFormState);
      setPendingJobRequest(null);
      await loadJobs(1);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to create crawl job.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleStartJob(id: string) {
    setActiveJobId(id);
    setError(null);

    try {
      await startCrawlJob(id);
      await loadJobs(jobsPageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to start crawl job.");
    } finally {
      setActiveJobId(null);
    }
  }

  async function handleDeleteJob(job: CrawlJob) {
    const confirmed = window.confirm(`Delete crawl job for ${job.targetUrl}? This will remove its pages, logs, and exports.`);

    if (!confirmed) {
      return;
    }

    setDeletingJobId(job.id);
    setError(null);

    try {
      await deleteCrawlJob(job.id);
      await loadJobs(jobsPageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to delete crawl job.");
    } finally {
      setDeletingJobId(null);
    }
  }

  async function handleCancelJob(job: CrawlJob) {
    const confirmed = window.confirm(`Cancel crawl job for ${job.targetUrl}?`);

    if (!confirmed) {
      return;
    }

    setCancelingJobId(job.id);
    setError(null);

    try {
      await cancelCrawlJob(job.id);
      await loadJobs(jobsPageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to cancel crawl job.");
    } finally {
      setCancelingJobId(null);
    }
  }

  async function handleToggleImportance(job: CrawlJob) {
    setUpdatingImportantJobId(job.id);
    setError(null);

    try {
      await toggleCrawlJobImportance(job.id);
      await loadJobs(jobsPageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to update job importance.");
    } finally {
      setUpdatingImportantJobId(null);
    }
  }

  function applyJobFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void loadJobs(1);
  }

  return (
    <section className="page-section" id="jobs">
      <div className="section-header">
        <div>
          <p className="eyebrow">Crawl operations</p>
          <h2>Crawl Jobs</h2>
        </div>
        <button className="icon-button" type="button" onClick={() => void loadJobs(jobsPageNumber)} title="Refresh jobs">
          Refresh
        </button>
      </div>

      <div className="metric-grid">
        <div className="metric-card">
          <span>Total jobs</span>
          <strong>{totals.jobs}</strong>
        </div>
        <div className="metric-card">
          <span>Pages crawled on page</span>
          <strong>{totals.crawled}</strong>
        </div>
        <div className="metric-card">
          <span>Failed pages on page</span>
          <strong>{totals.failed}</strong>
        </div>
        <div className="metric-card">
          <span>Important on page</span>
          <strong>{totals.important}</strong>
        </div>
      </div>

      <div className="workspace-grid">
        <form className="panel create-form" onSubmit={(event) => void handleCreateJob(event)}>
          <div className="create-form-header">
            <p className="eyebrow">New crawl</p>
            <h3>Create job</h3>
          </div>

          <label className="create-url-field">
            Target URL
            <input
              type="url"
              value={form.targetUrl}
              onChange={(event) => setForm((current) => ({ ...current, targetUrl: event.target.value }))}
              required
            />
          </label>

          <div className="create-options-row two-cols">
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

          <div className="create-actions">
            <label className="checkbox-row">
              <input
                type="checkbox"
                checked={form.stayWithinDomain}
                onChange={(event) => setForm((current) => ({ ...current, stayWithinDomain: event.target.checked }))}
              />
              Stay within domain
            </label>

            <button className="primary-button create-submit" type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Creating..." : "Create crawl job"}
            </button>
          </div>
        </form>

        <div className="panel table-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">Recent activity</p>
              <h3>Jobs list</h3>
            </div>
          </div>

          <form className="filter-bar" onSubmit={applyJobFilters}>
            <input
              aria-label="Search jobs"
              placeholder="Search URL"
              value={jobsSearch}
              onChange={(event) => setJobsSearch(event.target.value)}
            />
            <select aria-label="Filter by status" value={jobsStatus} onChange={(event) => setJobsStatus(event.target.value)}>
              <option value="">All statuses</option>
              <option value="Pending">Pending</option>
              <option value="InProgress">In progress</option>
              <option value="Completed">Completed</option>
              <option value="Failed">Failed</option>
              <option value="Canceled">Canceled</option>
            </select>
            <select
              aria-label="Jobs page size"
              value={jobsPageSize}
              onChange={(event) => setJobsPageSize(Number(event.target.value))}
            >
              <option value={5}>5</option>
              <option value={10}>10</option>
              <option value={25}>25</option>
              <option value={50}>50</option>
            </select>
            <label className="filter-check">
              <input
                type="checkbox"
                checked={importantOnly}
                onChange={(event) => setImportantOnly(event.target.checked)}
              />
              Important
            </label>
            <button className="secondary-button" type="submit">
              Apply
            </button>
          </form>

          {error && <div className="alert">{error}</div>}

          {isLoading ? (
            <div className="empty-state">Loading jobs...</div>
          ) : jobsPage.items.length === 0 ? (
            <div className="empty-state">No crawl jobs match the current filters.</div>
          ) : (
            <>
              {/* ── Desktop Table ── */}
              <div className="table-scroll jobs-desktop-table">
                <table className="jobs-table">
                  <thead>
                    <tr>
                      <th>Target</th>
                      <th>Watch</th>
                      <th>Status</th>
                      <th>Depth</th>
                      <th>Pages</th>
                      <th>Created</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {jobsPage.items.map((job) => (
                      <tr key={job.id}>
                        <td>
                          <div className="url-cell" title={job.targetUrl}>
                            {job.targetUrl}
                          </div>
                        </td>
                        <td>
                          <button
                            className={`watch-button${job.isImportant ? " is-active" : ""}`}
                            type="button"
                            onClick={() => void handleToggleImportance(job)}
                            disabled={updatingImportantJobId === job.id}
                            title={job.isImportant ? "Remove from important jobs" : "Mark as important"}
                          >
                            {job.isImportant ? "Important" : "Watch"}
                          </button>
                        </td>
                        <td><StatusBadge status={job.status} /></td>
                        <td>{job.maxDepth}</td>
                        <td>{job.pagesCrawled}/{job.maxPages}</td>
                        <td>
                          <span className="date-cell">{new Date(job.createdAt).toLocaleString()}</span>
                        </td>
                        <td>
                          <div className="button-group jobs-actions">
                            <Link className="secondary-link-button" to={`/admin/jobs/${job.id}`}>View</Link>
                            <button
                              className="secondary-button"
                              type="button"
                              onClick={() => void handleStartJob(job.id)}
                              disabled={job.status !== "Pending" || activeJobId === job.id}
                            >
                              {activeJobId === job.id ? "Starting..." : "Start"}
                            </button>
                            {(job.status === "Pending" || job.status === "InProgress") && (
                              <button
                                className="danger-button"
                                type="button"
                                onClick={() => void handleCancelJob(job)}
                                disabled={cancelingJobId === job.id}
                              >
                                {cancelingJobId === job.id ? "Canceling..." : "Cancel"}
                              </button>
                            )}
                            <button
                              className="danger-button"
                              type="button"
                              onClick={() => void handleDeleteJob(job)}
                              disabled={job.status === "InProgress" || deletingJobId === job.id}
                            >
                              {deletingJobId === job.id ? "Deleting..." : "Delete"}
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* ── Mobile Cards ── */}
              <div className="jobs-mobile-list">
                {jobsPage.items.map((job) => (
                  <div className="job-mob-card" key={job.id}>
                    {/* Header: URL + Watch */}
                    <div className="job-mob-header">
                      <span className="job-mob-url" title={job.targetUrl}>{job.targetUrl}</span>
                      <button
                        className={`watch-button${job.isImportant ? " is-active" : ""}`}
                        type="button"
                        onClick={() => void handleToggleImportance(job)}
                        disabled={updatingImportantJobId === job.id}
                        title={job.isImportant ? "Remove from important jobs" : "Mark as important"}
                      >
                        {job.isImportant ? "★ Important" : "☆ Watch"}
                      </button>
                    </div>

                    {/* Stats row: Status / Depth / Pages */}
                    <div className="job-mob-stats">
                      <div className="job-mob-stat">
                        <StatusBadge status={job.status} />
                      </div>
                      <div className="job-mob-stat">
                        <span className="stat-label">Depth</span>
                        <span className="stat-value">{job.maxDepth}</span>
                      </div>
                      <div className="job-mob-stat">
                        <span className="stat-label">Pages</span>
                        <span className="stat-value">{job.pagesCrawled}/{job.maxPages}</span>
                      </div>
                    </div>

                    {/* Date */}
                    <div className="job-mob-meta">
                      <span className="meta-icon">🕐</span>
                      {new Date(job.createdAt).toLocaleString()}
                    </div>

                    {/* Action buttons */}
                    <div className="job-mob-actions">
                      <Link className="secondary-link-button" to={`/admin/jobs/${job.id}`}>
                        View
                      </Link>
                      <button
                        className="secondary-button"
                        type="button"
                        onClick={() => void handleStartJob(job.id)}
                        disabled={job.status !== "Pending" || activeJobId === job.id}
                      >
                        {activeJobId === job.id ? "Starting..." : "Start"}
                      </button>
                      {(job.status === "Pending" || job.status === "InProgress") && (
                        <button
                          className="danger-button"
                          type="button"
                          onClick={() => void handleCancelJob(job)}
                          disabled={cancelingJobId === job.id}
                        >
                          {cancelingJobId === job.id ? "Canceling..." : "Cancel"}
                        </button>
                      )}
                      <button
                        className="danger-button"
                        type="button"
                        onClick={() => void handleDeleteJob(job)}
                        disabled={job.status === "InProgress" || deletingJobId === job.id}
                      >
                        {deletingJobId === job.id ? "Deleting..." : "Delete"}
                      </button>
                    </div>
                  </div>
                ))}
              </div>

              <PaginationControls
                label="Jobs"
                page={jobsPage}
                onPageChange={(pageNumber) => void loadJobs(pageNumber)}
              />
            </>
          )}
        </div>
      </div>

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
    </section>
  );
}
