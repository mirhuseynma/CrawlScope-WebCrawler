import { FormEvent, Fragment, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { cancelCrawlJob, exportCrawlJob, getBrokenLinks, getCrawledPages, getCrawlJob, getCrawlLogs } from "../api/crawlJobsApi";
import { PaginationControls } from "../components/PaginationControls";
import { StatusBadge } from "../components/StatusBadge";
import type { BrokenLink, CrawledPage, CrawlJobDetails, CrawlLog, PagedResult } from "../types/crawlJob";

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

const emptyBrokenLinksPage: PagedResult<BrokenLink> = {
  items: [],
  pageNumber: 1,
  pageSize: 5,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

type JobDetailsPageProps = {
  variant?: "admin" | "user";
};

function getBrokenLinkAction(link: BrokenLink) {
  if (link.statusCode === 404) {
    return "Fix the source link, restore the target page, or add a 301 redirect.";
  }

  if (link.statusCode === 401 || link.statusCode === 403) {
    return "Review access rules or remove private links from public pages.";
  }

  if (link.statusCode && link.statusCode >= 500) {
    return "Check the target server logs, deployment health, and upstream services.";
  }

  if (!link.statusCode) {
    return "Check DNS, SSL, timeout, firewall, or network availability.";
  }

  if (link.statusCode >= 300 && link.statusCode < 400) {
    return "Verify redirect target and update the source link if the destination changed.";
  }

  return "Review the target URL and confirm it should remain linked.";
}

export function JobDetailsPage({ variant = "admin" }: JobDetailsPageProps) {
  const { id } = useParams<{ id: string }>();
  const isUserReport = variant === "user";
  const [job, setJob] = useState<CrawlJobDetails | null>(null);
  const [pagesPage, setPagesPage] = useState<PagedResult<CrawledPage>>(emptyPagesPage);
  const [logsPage, setLogsPage] = useState<PagedResult<CrawlLog>>(emptyLogsPage);
  const [brokenLinksPage, setBrokenLinksPage] = useState<PagedResult<BrokenLink>>(emptyBrokenLinksPage);
  const [pagesSearch, setPagesSearch] = useState("");
  const [pagesStatusCode, setPagesStatusCode] = useState("");
  const [pagesDepthLevel, setPagesDepthLevel] = useState("");
  const [pagesPageNumber, setPagesPageNumber] = useState(1);
  const [brokenSearch, setBrokenSearch] = useState("");
  const [brokenStatusCode, setBrokenStatusCode] = useState("");
  const [brokenScope, setBrokenScope] = useState("");
  const [brokenPageNumber, setBrokenPageNumber] = useState(1);
  const [logsLevel, setLogsLevel] = useState("");
  const [logsPageNumber, setLogsPageNumber] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [exportingFormat, setExportingFormat] = useState<"Csv" | "Json" | null>(null);
  const [expandedPageIds, setExpandedPageIds] = useState<Set<string>>(new Set());
  const [error, setError] = useState<string | null>(null);
  const [isCanceling, setIsCanceling] = useState(false);

  async function handleCancel() {
    if (!id) return;
    if (!window.confirm("Are you sure you want to cancel this crawl job?")) {
      return;
    }
    setIsCanceling(true);
    setError(null);
    try {
      await cancelCrawlJob(id);
      void loadDetails(pagesPageNumber, logsPageNumber, brokenPageNumber, false);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to cancel crawl job.");
    } finally {
      setIsCanceling(false);
    }
  }

  async function loadDetails(pageNumber = pagesPageNumber, logPageNumber = logsPageNumber, brokenPage = brokenPageNumber, isBackground = false) {
    if (!id) {
      return;
    }

    if (!isBackground) {
      setIsLoading(true);
    }
    setError(null);

    try {
      const [loadedJob, loadedPages, loadedLogs, loadedBrokenLinks] = await Promise.all([
        getCrawlJob(id),
        getCrawledPages(id, {
          search: pagesSearch,
          statusCode: pagesStatusCode === "" ? undefined : Number(pagesStatusCode),
          depthLevel: pagesDepthLevel === "" ? undefined : Number(pagesDepthLevel),
          pageNumber,
          pageSize: 5,
        }),
        isUserReport
          ? Promise.resolve(emptyLogsPage)
          : getCrawlLogs(id, {
              level: logsLevel,
              pageNumber: logPageNumber,
              pageSize: 20,
            }),
        getBrokenLinks(id, {
          search: brokenSearch,
          statusCode: brokenStatusCode === "" ? undefined : Number(brokenStatusCode),
          externalOnly: brokenScope === "" ? undefined : brokenScope === "external",
          pageNumber: brokenPage,
          pageSize: 5,
        }),
      ]);

      setJob(loadedJob);
      setPagesPage(loadedPages);
      setLogsPage(loadedLogs);
      setBrokenLinksPage(loadedBrokenLinks);
      setPagesPageNumber(loadedPages.pageNumber);
      setLogsPageNumber(loadedLogs.pageNumber);
      setBrokenPageNumber(loadedBrokenLinks.pageNumber);
      setExpandedPageIds(new Set());
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load crawl job details.");
    } finally {
      if (!isBackground) {
        setIsLoading(false);
      }
    }
  }

  useEffect(() => {
    void loadDetails(1, 1, 1);
  }, [id]);

  useEffect(() => {
    if (job?.status === "Pending" || job?.status === "InProgress") {
      const interval = setInterval(() => {
        void loadDetails(pagesPageNumber, logsPageNumber, brokenPageNumber, true);
      }, 3000);
      return () => clearInterval(interval);
    }
  }, [job?.status, pagesPageNumber, logsPageNumber, brokenPageNumber]);

  function applyDetailFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void loadDetails(1, 1, 1);
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
        <Link className="secondary-link-button" to={isUserReport ? "/" : "/admin/jobs"}>
          {isUserReport ? "Back to crawler" : "Back to jobs"}
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
          <div className="metric-card">
            <span>Broken links</span>
            <strong>{brokenLinksPage.totalCount}</strong>
          </div>
        </div>
      )}

      <section className="panel broken-links-panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">Site health audit</p>
            <h3>Broken links</h3>
          </div>
        </div>

        <form className="filter-bar" onSubmit={applyDetailFilters}>
          <input
            aria-label="Search broken links"
            placeholder="Search source, target, or anchor"
            value={brokenSearch}
            onChange={(event) => setBrokenSearch(event.target.value)}
          />
          <input
            aria-label="Filter broken links by status code"
            placeholder="Status code"
            type="number"
            value={brokenStatusCode}
            onChange={(event) => setBrokenStatusCode(event.target.value)}
          />
          <select aria-label="Filter broken links by scope" value={brokenScope} onChange={(event) => setBrokenScope(event.target.value)}>
            <option value="">Internal and external</option>
            <option value="internal">Internal only</option>
            <option value="external">External only</option>
          </select>
          <button className="secondary-button" type="submit">
            Apply
          </button>
        </form>

        {isLoading ? (
          <div className="empty-state">Checking broken links...</div>
        ) : brokenLinksPage.items.length === 0 ? (
          <div className="empty-state">No broken links detected for this crawl.</div>
        ) : (
          <>
            <div className="broken-link-list">
              {brokenLinksPage.items.map((link) => (
                <article className="broken-link-card" key={link.id}>
                  <div className="broken-link-status">
                    <span>{link.statusCode ?? "Network"}</span>
                    <strong>{link.isExternal ? "External" : "Internal"}</strong>
                  </div>
                  <div className="broken-link-content">
                    <div>
                      <span>Source page</span>
                      <strong title={link.sourceUrl}>{link.sourceUrl}</strong>
                    </div>
                    <div>
                      <span>Broken target</span>
                      <strong title={link.targetUrl}>{link.targetUrl}</strong>
                    </div>
                    <div className="broken-link-meta">
                      <span>Anchor: {link.anchorText || "No anchor text"}</span>
                      <span>Depth {link.depthLevel}</span>
                      <span>{link.responseTimeMs === null ? "No response time" : `${link.responseTimeMs} ms`}</span>
                      <span>{new Date(link.detectedAt).toLocaleString()}</span>
                    </div>
                    <div className="broken-link-action">
                      <span>{link.errorMessage || "HTTP request failed."}</span>
                      <strong>{getBrokenLinkAction(link)}</strong>
                    </div>
                  </div>
                </article>
              ))}
            </div>
            <PaginationControls
              label="Broken links"
              page={brokenLinksPage}
              onPageChange={(pageNumber) => void loadDetails(pagesPageNumber, logsPageNumber, pageNumber)}
            />
          </>
        )}
      </section>

      <section className="panel detail-panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">Crawled pages</p>
            <h3>Pages</h3>
          </div>
          <div className="detail-actions">
            {(job?.status === "Pending" || job?.status === "InProgress") && (
              <button
                className="danger-button"
                type="button"
                onClick={() => void handleCancel()}
                disabled={isCanceling}
              >
                {isCanceling ? "Canceling..." : "Cancel Job"}
              </button>
            )}
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
            {/* ── Desktop Table ── */}
            <div className="table-scroll pages-desktop-table">
              <table className="pages-table">
                <thead>
                  <tr>
                    <th>URL & Title</th>
                    <th>Status</th>
                    <th>Depth</th>
                    <th>Links</th>
                    <th>Response</th>
                    <th className="actions-header">Snapshot</th>
                  </tr>
                </thead>
                <tbody>
                  {pagesPage.items.map((page) => {
                    const isExpanded = expandedPageIds.has(page.id);
                    const hasContent = Boolean(page.contentPreview && page.contentPreview.trim().length > 0);

                    return (
                      <Fragment key={page.id}>
                        <tr className={`page-table-row${isExpanded ? " is-expanded-parent" : ""}`}>
                          <td data-label="URL & Title">
                            <div className="page-title">{page.title || "Untitled page"}</div>
                            <a href={page.url} target="_blank" rel="noreferrer" className="page-url-link">
                              {page.url}
                            </a>
                            {hasContent && !isExpanded && (
                              <p className="content-preview-snippet">
                                {page.contentPreview}
                              </p>
                            )}
                          </td>
                          <td data-label="Status">
                            <span className={`status-code-pill status-${page.statusCode && page.statusCode >= 200 && page.statusCode < 300 ? "success" : page.statusCode && page.statusCode >= 400 ? "error" : "default"}`}>
                              {page.statusCode ?? "-"}
                            </span>
                          </td>
                          <td data-label="Depth">
                            <span className="depth-badge">{page.depthLevel}</span>
                          </td>
                          <td data-label="Links">
                            <div className="links-pill-group">
                              <span className="links-pill int">{page.internalLinksCount} int</span>
                              <span className="links-pill ext">{page.externalLinksCount} ext</span>
                            </div>
                          </td>
                          <td data-label="Response">
                            <span className="response-time-cell">{page.responseTimeMs === null ? "-" : `${page.responseTimeMs} ms`}</span>
                          </td>
                          <td data-label="Snapshot">
                            {hasContent ? (
                              <button
                                className={`snapshot-toggle-btn${isExpanded ? " is-active" : ""}`}
                                type="button"
                                onClick={() => toggleContentPreview(page.id)}
                                aria-expanded={isExpanded}
                              >
                                <span>{isExpanded ? "Close" : "View"}</span>
                                <svg
                                  className={`chevron-icon${isExpanded ? " is-rotated" : ""}`}
                                  width="14"
                                  height="14"
                                  viewBox="0 0 24 24"
                                  fill="none"
                                  stroke="currentColor"
                                  strokeWidth="2.5"
                                >
                                  <polyline points="6 9 12 15 18 9" />
                                </svg>
                              </button>
                            ) : (
                              <span className="no-content-label">No preview</span>
                            )}
                          </td>
                        </tr>
                        {isExpanded && page.contentPreview && (
                          <tr className="page-detail-expand-row">
                            <td colSpan={6} data-label="Content Preview" className="page-detail-expand-cell">
                              <div className="page-snapshot-card">
                                <div className="snapshot-card-header">
                                  <div className="snapshot-title">
                                    <svg className="snapshot-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                                      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
                                      <polyline points="14 2 14 8 20 8" />
                                      <line x1="16" y1="13" x2="8" y2="13" />
                                      <line x1="16" y1="17" x2="8" y2="17" />
                                      <polyline points="10 9 9 9 8 9" />
                                    </svg>
                                    <span>Extracted Content Snapshot</span>
                                    <span className="snapshot-badge">{page.contentPreview.length} characters</span>
                                  </div>
                                  <button
                                    className="copy-snapshot-btn"
                                    type="button"
                                    onClick={() => void navigator.clipboard.writeText(page.contentPreview || "")}
                                  >
                                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                                      <rect x="9" y="9" width="13" height="13" rx="2" ry="2" />
                                      <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
                                    </svg>
                                    <span>Copy Text</span>
                                  </button>
                                </div>
                                <div className="snapshot-body">
                                  <pre className="snapshot-text">{page.contentPreview}</pre>
                                </div>
                              </div>
                            </td>
                          </tr>
                        )}
                      </Fragment>
                    );
                  })}
                </tbody>
              </table>
            </div>

            {/* ── Mobile Crawled Pages Card List ── */}
            <div className="pages-mobile-list">
              {pagesPage.items.map((page) => {
                const isExpanded = expandedPageIds.has(page.id);
                const hasContent = Boolean(page.contentPreview && page.contentPreview.trim().length > 0);

                return (
                  <div className="page-mob-card" key={page.id}>
                    {/* Header: Title & Link */}
                    <div className="page-mob-header">
                      <div className="page-title">{page.title || "Untitled page"}</div>
                      <a href={page.url} target="_blank" rel="noreferrer" className="page-url-link">
                        {page.url}
                      </a>
                      {hasContent && !isExpanded && (
                        <p className="content-preview-snippet">{page.contentPreview}</p>
                      )}
                    </div>

                    {/* Stats Grid: Status | Depth | Links | Response */}
                    <div className="page-mob-stats">
                      <div className="page-mob-stat">
                        <span className="stat-label">Status</span>
                        <span className={`status-code-pill status-${page.statusCode && page.statusCode >= 200 && page.statusCode < 300 ? "success" : page.statusCode && page.statusCode >= 400 ? "error" : "default"}`}>
                          {page.statusCode ?? "-"}
                        </span>
                      </div>
                      <div className="page-mob-stat">
                        <span className="stat-label">Depth</span>
                        <span className="depth-badge">{page.depthLevel}</span>
                      </div>
                      <div className="page-mob-stat">
                        <span className="stat-label">Links</span>
                        <div className="links-pill-group">
                          <span className="links-pill int">{page.internalLinksCount}</span>
                          <span className="links-pill ext">{page.externalLinksCount}</span>
                        </div>
                      </div>
                      <div className="page-mob-stat">
                        <span className="stat-label">Response</span>
                        <span className="response-time-cell">{page.responseTimeMs === null ? "-" : `${page.responseTimeMs}ms`}</span>
                      </div>
                    </div>

                    {/* Snapshot Action */}
                    {hasContent && (
                      <div className="page-mob-action">
                        <button
                          className={`snapshot-toggle-btn${isExpanded ? " is-active" : ""}`}
                          type="button"
                          onClick={() => toggleContentPreview(page.id)}
                          aria-expanded={isExpanded}
                        >
                          <span>{isExpanded ? "Close Snapshot" : "View Snapshot"}</span>
                          <svg
                            className={`chevron-icon${isExpanded ? " is-rotated" : ""}`}
                            width="14"
                            height="14"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2.5"
                          >
                            <polyline points="6 9 12 15 18 9" />
                          </svg>
                        </button>
                      </div>
                    )}

                    {/* Expanded Snapshot Drawer in Mobile Card */}
                    {isExpanded && page.contentPreview && (
                      <div className="page-mob-snapshot">
                        <div className="page-snapshot-card">
                          <div className="snapshot-card-header">
                            <div className="snapshot-title">
                              <svg className="snapshot-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
                                <polyline points="14 2 14 8 20 8" />
                                <line x1="16" y1="13" x2="8" y2="13" />
                                <line x1="16" y1="17" x2="8" y2="17" />
                                <polyline points="10 9 9 9 8 9" />
                              </svg>
                              <span>Extracted Snapshot</span>
                              <span className="snapshot-badge">{page.contentPreview.length} chars</span>
                            </div>
                            <button
                              className="copy-snapshot-btn"
                              type="button"
                              onClick={() => void navigator.clipboard.writeText(page.contentPreview || "")}
                            >
                              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                                <rect x="9" y="9" width="13" height="13" rx="2" ry="2" />
                                <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
                              </svg>
                              <span>Copy</span>
                            </button>
                          </div>
                          <div className="snapshot-body">
                            <pre className="snapshot-text">{page.contentPreview}</pre>
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
            <PaginationControls
              label="Pages"
              page={pagesPage}
              onPageChange={(pageNumber) => void loadDetails(pageNumber, logsPageNumber)}
            />
          </>
        )}
      </section>

      {!isUserReport && (
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
      )}
    </section>
  );
}
