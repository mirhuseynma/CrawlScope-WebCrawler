import { FormEvent, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getCrawlJobs } from "../api/crawlJobsApi";
import { PaginationControls } from "../components/PaginationControls";
import { StatusBadge } from "../components/StatusBadge";
import { UserTopbar } from "../components/UserTopbar";
import type { CrawlJob, PagedResult } from "../types/crawlJob";

const emptyReportsPage: PagedResult<CrawlJob> = {
  items: [],
  pageNumber: 1,
  pageSize: 5,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

export function UserReportsPage() {
  const [reportsPage, setReportsPage] = useState<PagedResult<CrawlJob>>(emptyReportsPage);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function loadReports(nextPageNumber = pageNumber) {
    setIsLoading(true);
    setError(null);

    try {
      const data = await getCrawlJobs({
        search,
        status,
        pageNumber: nextPageNumber,
        pageSize: 5,
      });
      setReportsPage(data);
      setPageNumber(data.pageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load reports.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadReports(1);
  }, []);

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void loadReports(1);
  }

  return (
    <main className="user-shell">
      <UserTopbar />

      <section className="user-reports">
        <div className="section-header">
          <div>
            <p className="eyebrow">Private workspace</p>
            <h2>My reports</h2>
          </div>
        </div>

        <form className="filter-bar user-reports-filter" onSubmit={applyFilters}>
          <input
            aria-label="Search reports"
            placeholder="Search target URL"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <select aria-label="Filter reports by status" value={status} onChange={(event) => setStatus(event.target.value)}>
            <option value="">All statuses</option>
            <option value="Pending">Pending</option>
            <option value="InProgress">In progress</option>
            <option value="Completed">Completed</option>
            <option value="Failed">Failed</option>
            <option value="Canceled">Canceled</option>
          </select>
          <button className="secondary-button" type="submit">
            Apply
          </button>
        </form>

        {error && <div className="alert">{error}</div>}

        {isLoading ? (
          <div className="empty-state">Loading your reports...</div>
        ) : reportsPage.items.length === 0 ? (
          <div className="empty-state">No reports yet. Start a crawl to create your first report.</div>
        ) : (
          <>
            <div className="report-card-list">
              {reportsPage.items.map((report) => (
                <article className="report-card" key={report.id}>
                  <div>
                    <StatusBadge status={report.status} />
                    <h3>{report.targetUrl}</h3>
                    <p>
                      {report.pagesCrawled}/{report.maxPages} pages crawled / depth {report.maxDepth}
                    </p>
                  </div>
                  <div className="report-card-actions">
                    <span>{new Date(report.createdAt).toLocaleString()}</span>
                    <Link className="secondary-link-button" to={`/reports/${report.id}`}>
                      Open report
                    </Link>
                  </div>
                </article>
              ))}
            </div>
            <PaginationControls
              label="Reports"
              page={reportsPage}
              onPageChange={(nextPageNumber) => void loadReports(nextPageNumber)}
            />
          </>
        )}
      </section>
    </main>
  );
}
