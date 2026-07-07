import { FormEvent, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { getAllCrawledPages } from "../api/crawlJobsApi";
import { PaginationControls } from "../components/PaginationControls";
import type { CrawledPage, PagedResult } from "../types/crawlJob";

const emptyPagesPage: PagedResult<CrawledPage> = {
  items: [],
  pageNumber: 1,
  pageSize: 5,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

function formatPageContent(content?: string | null) {
  if (!content) {
    return "";
  }

  return content
    .replace(/\s+/g, " ")
    .replace(/([.!?])(?=[A-Z])/g, "$1 ")
    .replace(/([a-z])(?=[A-Z][a-z])/g, "$1 ")
    .trim();
}

function getStatusTone(statusCode?: number | null) {
  if (!statusCode) {
    return "unknown";
  }

  if (statusCode >= 200 && statusCode < 300) {
    return "ok";
  }

  if (statusCode >= 300 && statusCode < 400) {
    return "redirect";
  }

  if (statusCode >= 400) {
    return "failed";
  }

  return "unknown";
}

function getStatusLabel(statusCode?: number | null) {
  if (!statusCode) {
    return "No response";
  }

  if (statusCode >= 200 && statusCode < 300) {
    return "Successful";
  }

  if (statusCode >= 300 && statusCode < 400) {
    return "Redirect";
  }

  if (statusCode >= 400) {
    return "Needs attention";
  }

  return "Unknown";
}

export function PagesPage() {
  const [pagesPage, setPagesPage] = useState<PagedResult<CrawledPage>>(emptyPagesPage);
  const [search, setSearch] = useState("");
  const [statusCode, setStatusCode] = useState("");
  const [depthLevel, setDepthLevel] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [expandedPageIds, setExpandedPageIds] = useState<Set<string>>(new Set());
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const totals = useMemo(
    () => ({
      pages: pagesPage.totalCount,
      ok: pagesPage.items.filter((page) => page.statusCode && page.statusCode >= 200 && page.statusCode < 300).length,
      failed: pagesPage.items.filter((page) => page.statusCode && page.statusCode >= 400).length,
    }),
    [pagesPage],
  );

  async function loadPages(nextPageNumber = pageNumber) {
    setIsLoading(true);
    setError(null);

    try {
      const data = await getAllCrawledPages({
        search,
        statusCode: statusCode === "" ? undefined : Number(statusCode),
        depthLevel: depthLevel === "" ? undefined : Number(depthLevel),
        pageNumber: nextPageNumber,
        pageSize,
      });

      setPagesPage(data);
      setPageNumber(data.pageNumber);
      setExpandedPageIds(new Set());
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load crawled pages.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadPages(1);
  }, []);

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void loadPages(1);
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

  return (
    <section className="page-section">
      <div className="section-header">
        <div>
          <p className="eyebrow">Content aggregator</p>
          <h2>Crawled Pages</h2>
        </div>
        <button className="icon-button" type="button" onClick={() => void loadPages(pageNumber)} title="Refresh pages">
          Refresh
        </button>
      </div>

      <div className="metric-grid">
        <div className="metric-card">
          <span>Total pages</span>
          <strong>{totals.pages}</strong>
        </div>
        <div className="metric-card">
          <span>Successful on page</span>
          <strong>{totals.ok}</strong>
        </div>
        <div className="metric-card">
          <span>Failed on page</span>
          <strong>{totals.failed}</strong>
        </div>
      </div>

      <section className="panel table-panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">Search and filter</p>
            <h3>All crawled content</h3>
          </div>
        </div>

        <form className="filter-bar pages-filter" onSubmit={applyFilters}>
          <input
            aria-label="Search crawled pages"
            placeholder="Search URL, title, or content"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <input
            aria-label="Filter by status code"
            placeholder="Status code"
            type="number"
            value={statusCode}
            onChange={(event) => setStatusCode(event.target.value)}
          />
          <input
            aria-label="Filter by depth"
            placeholder="Depth"
            type="number"
            value={depthLevel}
            onChange={(event) => setDepthLevel(event.target.value)}
          />
          <select aria-label="Pages page size" value={pageSize} onChange={(event) => setPageSize(Number(event.target.value))}>
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={25}>25</option>
            <option value={50}>50</option>
          </select>
          <button className="secondary-button" type="submit">
            Apply
          </button>
        </form>

        {error && <div className="alert">{error}</div>}

        {isLoading ? (
          <div className="empty-state">Loading crawled pages...</div>
        ) : pagesPage.items.length === 0 ? (
          <div className="empty-state">No crawled pages match the current filters.</div>
        ) : (
          <>
            <div className="pages-list">
              {pagesPage.items.map((page) => {
                const isExpanded = expandedPageIds.has(page.id);
                const readableContent = formatPageContent(page.contentPreview);
                const hasExpandableContent = readableContent.length > 90;
                const statusTone = getStatusTone(page.statusCode);
                const statusLabel = getStatusLabel(page.statusCode);

                return (
                  <article className={`crawled-page-card status-${statusTone}`} key={page.id}>
                    <div className="page-feed-marker" aria-hidden="true">
                      <span>{page.statusCode ?? "-"}</span>
                    </div>
                    <div className="crawled-page-body">
                      <div className="crawled-page-header">
                        <div className="crawled-page-main">
                          <div className="crawled-page-kicker">
                            <span className={`http-status-pill ${statusTone}`}>{statusLabel}</span>
                            <span>Status {page.statusCode ?? "-"}</span>
                            <span>Depth {page.depthLevel}</span>
                          </div>
                          <div className="page-title" title={page.title || "Untitled page"}>
                            {page.title || "Untitled page"}
                          </div>
                          <div className="url-cell" title={page.url}>
                            {page.url}
                          </div>
                        </div>
                        <Link className="secondary-link-button page-card-action" to={`/admin/jobs/${page.crawlJobId}`}>
                          Open job
                        </Link>
                      </div>

                      <div className="page-meta-grid">
                        <div>
                          <span>Links</span>
                          <strong>
                            {page.internalLinksCount} internal / {page.externalLinksCount} external
                          </strong>
                        </div>
                        <div>
                          <span>Crawled</span>
                          <strong>{new Date(page.crawledAt).toLocaleString()}</strong>
                        </div>
                        <div>
                          <span>Job</span>
                          <strong title={page.crawlJobTargetUrl}>{page.crawlJobTargetUrl}</strong>
                        </div>
                      </div>

                      <div className="page-content-section">
                        <span>Content preview</span>
                        <div className={`page-content-box${readableContent ? "" : " is-empty"}${isExpanded ? " is-expanded" : ""}`}>
                          {readableContent || "No content snapshot captured."}
                        </div>
                        {hasExpandableContent && (
                          <button
                            className="text-button page-more-button"
                            type="button"
                            onClick={() => toggleContentPreview(page.id)}
                            aria-expanded={isExpanded}
                          >
                            {isExpanded ? "Show less" : "Read more"}
                          </button>
                        )}
                      </div>
                    </div>
                  </article>
                );
              })}
            </div>
            <PaginationControls label="Pages" page={pagesPage} onPageChange={(nextPage) => void loadPages(nextPage)} />
          </>
        )}
      </section>
    </section>
  );
}
