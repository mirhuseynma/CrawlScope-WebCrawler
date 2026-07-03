import { FormEvent, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { exportCrawlJob, getCrawledPages, getCrawlJob, getCrawlLogs } from "../api/crawlJobsApi";
import { PaginationControls } from "../components/PaginationControls";
import { StatusBadge } from "../components/StatusBadge";
import type { CrawledPage, CrawlJobDetails, CrawlLog, PagedResult } from "../types/crawlJob";

const emptyPagesPage: PagedResult<CrawledPage> = {
  items: [],
  pageNumber: 1,
  pageSize: 5,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

const emptyLogsPage: PagedResult<CrawlLog> = {
  items: [],
  pageNumber: 1,
  pageSize: 20,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

export function JobDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const [job, setJob] = useState<CrawlJobDetails | null>(null);
  const [pagesPage, setPagesPage] = useState<PagedResult<CrawledPage>>(emptyPagesPage);
  const [logsPage, setLogsPage] = useState<PagedResult<CrawlLog>>(emptyLogsPage);
  const [pagesSearch, setPagesSearch] = useState("");
  const [pagesStatusCode, setPagesStatusCode] = useState("");
  const [pagesDepthLevel, setPagesDepthLevel] = useState("");
  const [pagesPageNumber, setPagesPageNumber] = useState(1);
  const [logsLevel, setLogsLevel] = useState("");
  const [logsPageNumber, setLogsPageNumber] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [exportingFormat, setExportingFormat] = useState<"Csv" | "Json" | null>(null);
  const [expandedPageIds, setExpandedPageIds] = useState<Set<string>>(new Set());
  const [error, setError] = useState<string | null>(null);

  async function loadDetails(pageNumber = pagesPageNumber, logPageNumber = logsPageNumber) {
    if (!id) {
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const [loadedJob, loadedPages, loadedLogs] = await Promise.all([
        getCrawlJob(id),
        getCrawledPages(id, {
          search: pagesSearch,
          statusCode: pagesStatusCode === "" ? undefined : Number(pagesStatusCode),
          depthLevel: pagesDepthLevel === "" ? undefined : Number(pagesDepthLevel),
          pageNumber,
          pageSize: 5,
        }),
        getCrawlLogs(id, {
          level: logsLevel,
          pageNumber: logPageNumber,
          pageSize: 20,
        }),
      ]);

      setJob(loadedJob);
      setPagesPage(loadedPages);
      setLogsPage(loadedLogs);
      setPagesPageNumber(loadedPages.pageNumber);
      setLogsPageNumber(loadedLogs.pageNumber);
      setExpandedPageIds(new Set());
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load crawl job details.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadDetails(1, 1);
  }, [id]);

  function applyDetailFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void loadDetails(1, 1);
  }

  function toggleContentPreview(pageId: string) {
    setExpandedPageIds((current) => {
      const next = new Set(current);

      if (next.has(pageId)) {
        next.delete(pageId);
      } else {
        next.add(pageId);
      }

      return next;
    });
  }

  async function handleExport(format: "Csv" | "Json") {
    if (!id) {
      return;
    }

    setExportingFormat(format);
    setError(null);

    try {
      const blob = await exportCrawlJob(id, format);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `crawl-${id}.${format.toLowerCase()}`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to export crawl data.");
    } finally {
      setExportingFormat(null);
    }
  }

  return (
    <section className="page-section">
      <div className="section-header">
        <div>
          <p className="eyebrow">Crawl job details</p>
          <h2>{job?.targetUrl ?? "Selected job"}</h2>
        </div>
        <Link className="secondary-link-button" to="/jobs">
          Back to jobs
        </Link>
      </div>

      {error && <div className="alert">{error}</div>}

      {job && (
        <div className="metric-grid">
          <div className="metric-card">
            <span>Status</span>
            <StatusBadge status={job.status} />
          </div>
          <div className="metric-card">
            <span>Pages found</span>
            <strong>{job.pagesFound}</strong>
          </div>
          <div className="metric-card">
            <span>Failed pages</span>
            <strong>{job.pagesFailed}</strong>
          </div>
        </div>
      )}

      <section className="panel detail-panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">Crawled pages</p>
            <h3>Pages</h3>
          </div>
          <div className="detail-actions">
            <button className="secondary-button" type="button" onClick={() => void loadDetails()}>
              Refresh
            </button>
            <button
              className="secondary-button"
              type="button"
              onClick={() => void handleExport("Csv")}
              disabled={exportingFormat !== null}
            >
              {exportingFormat === "Csv" ? "Exporting..." : "Export CSV"}
            </button>
            <button
              className="secondary-button"
              type="button"
              onClick={() => void handleExport("Json")}
              disabled={exportingFormat !== null}
            >
              {exportingFormat === "Json" ? "Exporting..." : "Export JSON"}
            </button>
          </div>
        </div>

        <form className="filter-bar" onSubmit={applyDetailFilters}>
          <input
            aria-label="Search crawled pages"
            placeholder="Search pages"
            value={pagesSearch}
            onChange={(event) => setPagesSearch(event.target.value)}
          />
          <input
            aria-label="Filter by status code"
            placeholder="Status code"
            type="number"
            value={pagesStatusCode}
            onChange={(event) => setPagesStatusCode(event.target.value)}
          />
          <input
            aria-label="Filter by depth"
            placeholder="Depth"
            type="number"
            value={pagesDepthLevel}
            onChange={(event) => setPagesDepthLevel(event.target.value)}
          />
          <button className="secondary-button" type="submit">
            Apply
          </button>
        </form>

        {isLoading ? (
          <div className="empty-state">Loading crawl details...</div>
        ) : pagesPage.items.length === 0 ? (
          <div className="empty-state">No crawled pages match the current filters.</div>
        ) : (
          <>
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>URL</th>
                    <th>Status</th>
                    <th>Depth</th>
                    <th>Links</th>
                    <th>Response</th>
                  </tr>
                </thead>
                <tbody>
                  {pagesPage.items.map((page) => {
                    const isExpanded = expandedPageIds.has(page.id);
                    const hasExpandableContent = Boolean(page.contentPreview && page.contentPreview.length > 160);

                    return (
                      <tr key={page.id}>
                        <td data-label="URL">
                          <div className="page-title">{page.title || "Untitled page"}</div>
                          <div className="url-cell">{page.url}</div>
                          <p
                            className={`content-preview${page.contentPreview ? "" : " is-empty"}${
                              isExpanded ? " is-expanded" : ""
                            }`}
                          >
                            {page.contentPreview || "No content snapshot captured."}
                          </p>
                          {hasExpandableContent && (
                            <button
                              className="text-button"
                              type="button"
                              onClick={() => toggleContentPreview(page.id)}
                              aria-expanded={isExpanded}
                            >
                              {isExpanded ? "Less" : "More"}
                            </button>
                          )}
                        </td>
                        <td data-label="Status">{page.statusCode ?? "-"}</td>
                        <td data-label="Depth">{page.depthLevel}</td>
                        <td data-label="Links">
                          {page.internalLinksCount} internal / {page.externalLinksCount} external
                        </td>
                        <td data-label="Response">{page.responseTimeMs === null ? "-" : `${page.responseTimeMs} ms`}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
            <PaginationControls
              label="Pages"
              page={pagesPage}
              onPageChange={(pageNumber) => void loadDetails(pageNumber, logsPageNumber)}
            />
          </>
        )}
      </section>

      <section className="panel logs-panel">
        <div>
          <p className="eyebrow">Execution logs</p>
          <h3>Logs</h3>
        </div>

        <form className="filter-bar compact-filter" onSubmit={applyDetailFilters}>
          <select aria-label="Filter logs by level" value={logsLevel} onChange={(event) => setLogsLevel(event.target.value)}>
            <option value="">All levels</option>
            <option value="Info">Info</option>
            <option value="Warning">Warning</option>
            <option value="Error">Error</option>
          </select>
          <button className="secondary-button" type="submit">
            Apply
          </button>
        </form>

        {isLoading ? (
          <div className="empty-state">Loading logs...</div>
        ) : logsPage.items.length === 0 ? (
          <div className="empty-state">No logs match the current filters.</div>
        ) : (
          <>
            <div className="log-list">
              {logsPage.items.map((log) => (
                <article className="log-entry" key={log.id}>
                  <span className={`log-level log-${log.level.toLowerCase()}`}>{log.level}</span>
                  <p>{log.message}</p>
                  <time>{new Date(log.createdAt).toLocaleString()}</time>
                </article>
              ))}
            </div>
            <PaginationControls
              label="Logs"
              page={logsPage}
              onPageChange={(pageNumber) => void loadDetails(pagesPageNumber, pageNumber)}
            />
          </>
        )}
      </section>
    </section>
  );
}
