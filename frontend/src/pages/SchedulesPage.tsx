import { FormEvent, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import {
  createCrawlSchedule,
  deleteCrawlSchedule,
  disableCrawlSchedule,
  enableCrawlSchedule,
  getCrawlSchedules,
} from "../api/crawlSchedulesApi";
import { PaginationControls } from "../components/PaginationControls";
import type { CrawlSchedule, CreateCrawlScheduleRequest, PagedResult } from "../types/crawlJob";

const initialFormState: CreateCrawlScheduleRequest = {
  targetUrl: "https://example.com",
  maxDepth: 0,
  maxPages: 5,
  stayWithinDomain: true,
  intervalMinutes: 60,
};

const emptySchedulesPage: PagedResult<CrawlSchedule> = {
  items: [],
  pageNumber: 1,
  pageSize: 5,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

export function SchedulesPage() {
  const [schedulesPage, setSchedulesPage] = useState<PagedResult<CrawlSchedule>>(emptySchedulesPage);
  const [form, setForm] = useState<CreateCrawlScheduleRequest>(initialFormState);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [activeScheduleId, setActiveScheduleId] = useState<string | null>(null);
  const [deletingScheduleId, setDeletingScheduleId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const totals = useMemo(
    () => ({
      total: schedulesPage.totalCount,
      enabled: schedulesPage.items.filter((schedule) => schedule.isEnabled).length,
      paused: schedulesPage.items.filter((schedule) => !schedule.isEnabled).length,
    }),
    [schedulesPage],
  );

  async function loadSchedules(nextPageNumber = pageNumber) {
    setIsLoading(true);
    setError(null);

    try {
      const data = await getCrawlSchedules({
        search,
        isEnabled: statusFilter === "" ? undefined : statusFilter === "enabled",
        pageNumber: nextPageNumber,
        pageSize,
      });
      setSchedulesPage(data);
      setPageNumber(data.pageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load schedules.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadSchedules(1);
  }, []);

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void loadSchedules(1);
  }

  async function handleCreateSchedule(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      await createCrawlSchedule(form);
      setForm(initialFormState);
      await loadSchedules(1);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to create schedule.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleToggleSchedule(schedule: CrawlSchedule) {
    setActiveScheduleId(schedule.id);
    setError(null);

    try {
      if (schedule.isEnabled) {
        await disableCrawlSchedule(schedule.id);
      } else {
        await enableCrawlSchedule(schedule.id);
      }

      await loadSchedules(pageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to update schedule.");
    } finally {
      setActiveScheduleId(null);
    }
  }

  async function handleDeleteSchedule(schedule: CrawlSchedule) {
    const confirmed = window.confirm(`Delete schedule for ${schedule.targetUrl}?`);

    if (!confirmed) {
      return;
    }

    setDeletingScheduleId(schedule.id);
    setError(null);

    try {
      await deleteCrawlSchedule(schedule.id);
      await loadSchedules(pageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to delete schedule.");
    } finally {
      setDeletingScheduleId(null);
    }
  }

  return (
    <section className="page-section">
      <div className="section-header">
        <div>
          <p className="eyebrow">Periodic crawling</p>
          <h2>Schedules</h2>
        </div>
        <button className="icon-button" type="button" onClick={() => void loadSchedules(pageNumber)} title="Refresh schedules">
          Refresh
        </button>
      </div>

      <div className="metric-grid">
        <div className="metric-card">
          <span>Schedules matching filters</span>
          <strong>{totals.total}</strong>
        </div>
        <div className="metric-card">
          <span>Enabled in current list</span>
          <strong>{totals.enabled}</strong>
        </div>
        <div className="metric-card">
          <span>Paused in current list</span>
          <strong>{totals.paused}</strong>
        </div>
      </div>

      <div className="workspace-grid">
        <form className="panel create-form" onSubmit={(event) => void handleCreateSchedule(event)}>
          <div className="create-form-header">
            <p className="eyebrow">New schedule</p>
            <h3>Create periodic crawl</h3>
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

          <div className="create-options">
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

          <label className="create-interval-field">
            Interval minutes
            <input
              type="number"
              min="1"
              max="10080"
              value={form.intervalMinutes}
              onChange={(event) => setForm((current) => ({ ...current, intervalMinutes: Number(event.target.value) }))}
              required
            />
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
              {isSubmitting ? "Creating..." : "Create schedule"}
            </button>
          </div>
        </form>

        <div className="panel table-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">Background jobs</p>
              <h3>Schedule list</h3>
            </div>
          </div>

          {error && <div className="alert">{error}</div>}

          <form className="filter-bar" onSubmit={applyFilters}>
            <input
              aria-label="Search schedules"
              placeholder="Search target URL"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
            />
            <select aria-label="Filter schedule status" value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
              <option value="">All statuses</option>
              <option value="enabled">Enabled</option>
              <option value="paused">Paused</option>
            </select>
            <select aria-label="Schedules page size" value={pageSize} onChange={(event) => setPageSize(Number(event.target.value))}>
              <option value={5}>5</option>
              <option value={10}>10</option>
              <option value={25}>25</option>
              <option value={50}>50</option>
            </select>
            <button className="secondary-button" type="submit">
              Apply
            </button>
          </form>

          {isLoading ? (
            <div className="empty-state">Loading schedules...</div>
          ) : schedulesPage.items.length === 0 ? (
            <div className="empty-state">No schedules match the current filters.</div>
          ) : (
            <>
              <div className="schedule-list">
                {schedulesPage.items.map((schedule) => (
                  <article className="schedule-card" key={schedule.id}>
                    <div className="schedule-summary">
                      <div className="schedule-main">
                        <div className="schedule-target" title={schedule.targetUrl}>
                          {schedule.targetUrl}
                        </div>
                        <span className="meta-line">
                          Next run: {new Date(schedule.nextRunAt).toLocaleString()}
                        </span>
                      </div>

                      <span className={`status-badge ${schedule.isEnabled ? "status-completed" : "status-cancelled"}`}>
                        {schedule.isEnabled ? "Enabled" : "Paused"}
                      </span>
                    </div>

                    <div className="schedule-chip-row">
                      <span>{schedule.intervalMinutes} min interval</span>
                      <span>depth {schedule.maxDepth}</span>
                      <span>{schedule.maxPages} pages max</span>
                      <span>{schedule.stayWithinDomain ? "domain only" : "any domain"}</span>
                      <span>last run: {schedule.lastRunAt ? new Date(schedule.lastRunAt).toLocaleString() : "-"}</span>
                    </div>

                    <div className="schedule-actions">
                      {schedule.lastCrawlJobId && (
                        <Link className="secondary-link-button" to={`/admin/jobs/${schedule.lastCrawlJobId}`}>
                          Last job
                        </Link>
                      )}
                      <button
                        className="secondary-button"
                        type="button"
                        onClick={() => void handleToggleSchedule(schedule)}
                        disabled={activeScheduleId === schedule.id || deletingScheduleId === schedule.id}
                      >
                        {activeScheduleId === schedule.id ? "Updating..." : schedule.isEnabled ? "Pause" : "Enable"}
                      </button>
                      <button
                        className="danger-button"
                        type="button"
                        onClick={() => void handleDeleteSchedule(schedule)}
                        disabled={activeScheduleId === schedule.id || deletingScheduleId === schedule.id}
                      >
                        {deletingScheduleId === schedule.id ? "Deleting..." : "Delete"}
                      </button>
                    </div>
                  </article>
                ))}
              </div>
              <PaginationControls
                label="Schedules"
                page={schedulesPage}
                onPageChange={(nextPageNumber) => void loadSchedules(nextPageNumber)}
              />
            </>
          )}
        </div>
      </div>
    </section>
  );
}
