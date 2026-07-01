import { FormEvent, useEffect, useMemo, useState } from "react";
import { createCrawlJob, getCrawlJobs, startCrawlJob } from "../api/crawlJobsApi";
import { StatusBadge } from "../components/StatusBadge";
import type { CrawlJob, CreateCrawlJobRequest } from "../types/crawlJob";

const initialFormState: CreateCrawlJobRequest = {
  targetUrl: "https://example.com",
  maxDepth: 0,
  maxPages: 1,
  stayWithinDomain: true,
};

export function JobsPage() {
  const [jobs, setJobs] = useState<CrawlJob[]>([]);
  const [form, setForm] = useState<CreateCrawlJobRequest>(initialFormState);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [activeJobId, setActiveJobId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const totals = useMemo(
    () => ({
      jobs: jobs.length,
      crawled: jobs.reduce((sum, job) => sum + job.pagesCrawled, 0),
      failed: jobs.reduce((sum, job) => sum + job.pagesFailed, 0),
    }),
    [jobs],
  );

  async function loadJobs() {
    setIsLoading(true);
    setError(null);

    try {
      const data = await getCrawlJobs();
      setJobs(data);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load crawl jobs.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadJobs();
  }, []);

  async function handleCreateJob(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      await createCrawlJob(form);
      setForm(initialFormState);
      await loadJobs();
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
      await loadJobs();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to start crawl job.");
    } finally {
      setActiveJobId(null);
    }
  }

  return (
    <section className="page-section" id="jobs">
      <div className="section-header">
        <div>
          <p className="eyebrow">Crawl operations</p>
          <h2>Crawl Jobs</h2>
        </div>
        <button className="icon-button" type="button" onClick={() => void loadJobs()} title="Refresh jobs">
          Refresh
        </button>
      </div>

      <div className="metric-grid">
        <div className="metric-card">
          <span>Total jobs</span>
          <strong>{totals.jobs}</strong>
        </div>
        <div className="metric-card">
          <span>Pages crawled</span>
          <strong>{totals.crawled}</strong>
        </div>
        <div className="metric-card">
          <span>Failed pages</span>
          <strong>{totals.failed}</strong>
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

          {error && <div className="alert">{error}</div>}

          {isLoading ? (
            <div className="empty-state">Loading jobs...</div>
          ) : jobs.length === 0 ? (
            <div className="empty-state">No crawl jobs yet.</div>
          ) : (
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Target</th>
                    <th>Status</th>
                    <th>Depth</th>
                    <th>Pages</th>
                    <th>Created</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  {jobs.map((job) => (
                    <tr key={job.id}>
                      <td>
                        <div className="url-cell">{job.targetUrl}</div>
                      </td>
                      <td>
                        <StatusBadge status={job.status} />
                      </td>
                      <td>{job.maxDepth}</td>
                      <td>
                        {job.pagesCrawled}/{job.maxPages}
                      </td>
                      <td>{new Date(job.createdAt).toLocaleString()}</td>
                      <td>
                        <button
                          className="secondary-button"
                          type="button"
                          onClick={() => void handleStartJob(job.id)}
                          disabled={job.status !== "Pending" || activeJobId === job.id}
                        >
                          {activeJobId === job.id ? "Starting..." : "Start"}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </section>
  );
}
