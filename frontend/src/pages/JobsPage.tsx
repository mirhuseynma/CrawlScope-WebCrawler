import { FormEvent, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { createCrawlJob, deleteCrawlJob, getCrawlJobs, startCrawlJob, toggleCrawlJobImportance } from "../api/crawlJobsApi";
import { PaginationControls } from "../components/PaginationControls";
import { StatusBadge } from "../components/StatusBadge";
import type { CrawlJob, CreateCrawlJobRequest, PagedResult } from "../types/crawlJob";

const initialFormState: CreateCrawlJobRequest = {
  targetUrl: "https://example.com",
  maxDepth: 0,
  maxPages: 1,
  stayWithinDomain: true,
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
  const [activeJobId, setActiveJobId] = useState<string | null>(null);
  const [deletingJobId, setDeletingJobId] = useState<string | null>(null);
  const [updatingImportantJobId, setUpdatingImportantJobId] = useState<string | null>(null);
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

  async function loadJobs(pageNumber = jobsPageNumber) {
    setIsLoading(true);
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
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadJobs(1);
  }, []);

  async function handleCreateJob(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      await createCrawlJob(form);
      setForm(initialFormState);
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
          <div>
            <p className="eyebrow">New crawl</p>
            <h3>Create job</h3>
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

          <button className="primary-button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Creating..." : "Create crawl job"}
          </button>
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
              <div className="table-scroll">
                <table>
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
                        <td data-label="Target">
                          <div className="url-cell">{job.targetUrl}</div>
                        </td>
                        <td data-label="Watch">
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
                        <td data-label="Status">
                          <StatusBadge status={job.status} />
                        </td>
                        <td data-label="Depth">{job.maxDepth}</td>
                        <td data-label="Pages">
                          {job.pagesCrawled}/{job.maxPages}
                        </td>
                        <td data-label="Created">
                          <span className="date-cell">{new Date(job.createdAt).toLocaleString()}</span>
                        </td>
                        <td data-label="Actions">
                          <div className="button-group">
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
              <PaginationControls
                label="Jobs"
                page={jobsPage}
                onPageChange={(pageNumber) => void loadJobs(pageNumber)}
              />
            </>
          )}
        </div>
      </div>
    </section>
  );
}
