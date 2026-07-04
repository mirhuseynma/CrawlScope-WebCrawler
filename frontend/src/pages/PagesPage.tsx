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
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Page</th>
                    <th>Job</th>
                    <th>Status</th>
                    <th>Depth</th>
                    <th>Links</th>
                    <th>Crawled</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {pagesPage.items.map((page) => {
                    const isExpanded = expandedPageIds.has(page.id);
                    const hasExpandableContent = Boolean(page.contentPreview && page.contentPreview.length > 160);

                    return (
                      <tr key={page.id}>
                        <td data-label="Page">
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
                        <td data-label="Job">
                          <div className="url-cell compact-url-cell">{page.crawlJobTargetUrl}</div>
                        </td>
                        <td data-label="Status">{page.statusCode ?? "-"}</td>
                        <td data-label="Depth">{page.depthLevel}</td>
                        <td data-label="Links">
                          {page.internalLinksCount} internal / {page.externalLinksCount} external
                        </td>
                        <td data-label="Crawled">
                          <span className="date-cell">{new Date(page.crawledAt).toLocaleString()}</span>
                        </td>
                        <td data-label="Actions">
                          <div className="button-group">
                            <Link className="secondary-link-button" to={`/jobs/${page.crawlJobId}`}>
                              Open job
                            </Link>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
            <PaginationControls label="Pages" page={pagesPage} onPageChange={(nextPage) => void loadPages(nextPage)} />
          </>
        )}
      </section>
    </section>
  );
}
