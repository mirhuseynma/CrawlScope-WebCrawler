import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { getAdminOverview } from "../api/adminOverviewApi";
import { StatusBadge } from "../components/StatusBadge";
import type { AdminOverview } from "../types/crawlJob";

function formatNumber(value: number) {
  return new Intl.NumberFormat().format(value);
}

function formatFileSize(bytes: number) {
  if (bytes <= 0) {
    return "-";
  }

  const units = ["B", "KB", "MB", "GB"];
  let size = bytes;
  let unitIndex = 0;

  while (size >= 1024 && unitIndex < units.length - 1) {
    size /= 1024;
    unitIndex += 1;
  }

  return `${size.toFixed(unitIndex === 0 ? 0 : 1)} ${units[unitIndex]}`;
}

function calculatePercent(value: number, total: number) {
  if (total <= 0) {
    return 0;
  }

  return Math.round((value / total) * 100);
}

export function AdminOverviewPage() {
  const [overview, setOverview] = useState<AdminOverview | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const completionRate = useMemo(() => {
    if (!overview) {
      return 0;
    }

    return calculatePercent(overview.totals.completedJobs, overview.totals.totalJobs);
  }, [overview]);

  async function loadOverview() {
    setIsLoading(true);
    setError(null);

    try {
      const data = await getAdminOverview();
      setOverview(data);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load admin overview.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadOverview();
  }, []);

  if (isLoading) {
    return (
      <section className="page-section">
        <div className="empty-state">Loading admin overview...</div>
      </section>
    );
  }

  if (!overview) {
    return (
      <section className="page-section">
        {error && <div className="alert">{error}</div>}
        <div className="empty-state">Overview data is not available.</div>
      </section>
    );
  }

  return (
    <section className="page-section overview-page">
      <div className="section-header overview-hero">
        <div>
          <p className="eyebrow">Operations overview</p>
          <h2>Admin Overview</h2>
          <p className="section-subtitle">Live crawl health, export activity, and jobs that need attention.</p>
        </div>
        <button className="icon-button" type="button" onClick={() => void loadOverview()} title="Refresh overview">
          Refresh
        </button>
      </div>

      {error && <div className="alert">{error}</div>}

      <div className="overview-grid">
        <div className="overview-card overview-card-primary">
          <span>Completion rate</span>
          <strong>{completionRate}%</strong>
          <div className="overview-progress">
            <span style={{ width: `${completionRate}%` }} />
          </div>
        </div>
        <div className="overview-card">
          <span>Total jobs</span>
          <strong>{formatNumber(overview.totals.totalJobs)}</strong>
        </div>
        <div className="overview-card">
          <span>Crawled pages</span>
          <strong>{formatNumber(overview.totals.totalPages)}</strong>
        </div>
        <div className="overview-card">
          <span>Exports</span>
          <strong>{formatNumber(overview.totals.totalExports)}</strong>
          <small>{formatFileSize(overview.totals.totalExportSizeBytes)}</small>
        </div>
        <div className="overview-card">
          <span>Important jobs</span>
          <strong>{formatNumber(overview.totals.importantJobs)}</strong>
        </div>
        <div className="overview-card danger-metric">
          <span>Failed pages</span>
          <strong>{formatNumber(overview.totals.failedPages)}</strong>
        </div>
      </div>

      <section className="panel overview-health-panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">Status distribution</p>
            <h3>Crawl health</h3>
          </div>
        </div>
        <div className="status-stack">
          {overview.statusDistribution.length === 0 ? (
            <div className="empty-state">No crawl jobs yet.</div>
          ) : (
            overview.statusDistribution.map((status) => {
              const percent = calculatePercent(status.count, overview.totals.totalJobs);

              return (
                <div className="status-row" key={status.status}>
                  <div>
                    <StatusBadge status={status.status} />
                    <strong>{formatNumber(status.count)}</strong>
                  </div>
                  <div className="status-bar">
                    <span style={{ width: `${percent}%` }} />
                  </div>
                </div>
              );
            })
          )}
        </div>
      </section>

      <div className="overview-columns wide">
        <section className="panel table-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">Recent activity</p>
              <h3>Latest jobs</h3>
            </div>
            <Link className="secondary-link-button" to="/admin/jobs">
              View all
            </Link>
          </div>
          {overview.recentJobs.length === 0 ? (
            <div className="empty-state">No jobs have been created yet.</div>
          ) : (
            <div className="overview-list">
              {overview.recentJobs.map((job) => (
                <Link className="overview-list-item" to={`/admin/jobs/${job.id}`} key={job.id}>
                  <div>
                    <strong>{job.targetUrl}</strong>
                    <span>{new Date(job.createdAt).toLocaleString()}</span>
                  </div>
                  <StatusBadge status={job.status} />
                </Link>
              ))}
            </div>
          )}
        </section>

        <div className="overview-side-stack">
          <section className="panel table-panel">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">Attention</p>
                <h3>Problem jobs</h3>
              </div>
            </div>
            {overview.problemJobs.length === 0 ? (
              <div className="empty-state">No failed jobs or failed pages detected.</div>
            ) : (
              <div className="overview-list">
                {overview.problemJobs.map((job) => (
                  <Link className="overview-list-item problem" to={`/admin/jobs/${job.id}`} key={job.id}>
                    <div>
                      <strong>{job.targetUrl}</strong>
                      <span>
                        {job.pagesFailed} failed / {job.pagesCrawled} crawled
                      </span>
                    </div>
                    <StatusBadge status={job.status} />
                  </Link>
                ))}
              </div>
            )}
          </section>

          <section className="panel quick-actions-panel">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">Shortcuts</p>
                <h3>Quick actions</h3>
              </div>
            </div>
            <div className="quick-action-grid">
              <Link className="quick-action" to="/admin/jobs">
                Create crawl job
              </Link>
              <Link className="quick-action" to="/admin/pages">
                Inspect pages
              </Link>
              <Link className="quick-action" to="/admin/exports">
                Manage exports
              </Link>
              <Link className="quick-action" to="/admin/schedules">
                Review schedules
              </Link>
            </div>
          </section>
        </div>
      </div>

      <section className="panel table-panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">Saved artifacts</p>
            <h3>Latest exports</h3>
          </div>
          <Link className="secondary-link-button" to="/admin/exports">
            View all
          </Link>
        </div>
        {overview.recentExports.length === 0 ? (
          <div className="empty-state">No export files have been created yet.</div>
        ) : (
          <div className="overview-list">
            {overview.recentExports.map((exportFile) => (
              <Link className="overview-list-item" to={`/admin/jobs/${exportFile.crawlJobId}`} key={exportFile.id}>
                <div>
                  <strong>{exportFile.fileName}</strong>
                  <span>{exportFile.crawlJobTargetUrl}</span>
                </div>
                <span className="format-pill">{exportFile.format.toUpperCase()}</span>
              </Link>
            ))}
          </div>
        )}
      </section>
    </section>
  );
}
