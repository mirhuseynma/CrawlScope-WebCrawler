import { FormEvent, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import {
  createCrawlSchedule,
  deleteCrawlSchedule,
  disableCrawlSchedule,
  enableCrawlSchedule,
  getCrawlSchedules,
} from "../api/crawlSchedulesApi";
import type { CrawlSchedule, CreateCrawlScheduleRequest } from "../types/crawlJob";

const initialFormState: CreateCrawlScheduleRequest = {
  targetUrl: "https://example.com",
  maxDepth: 0,
  maxPages: 5,
  stayWithinDomain: true,
  intervalMinutes: 60,
};

export function SchedulesPage() {
  const [schedules, setSchedules] = useState<CrawlSchedule[]>([]);
  const [form, setForm] = useState<CreateCrawlScheduleRequest>(initialFormState);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [activeScheduleId, setActiveScheduleId] = useState<string | null>(null);
  const [deletingScheduleId, setDeletingScheduleId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const totals = useMemo(
    () => ({
      total: schedules.length,
      enabled: schedules.filter((schedule) => schedule.isEnabled).length,
      paused: schedules.filter((schedule) => !schedule.isEnabled).length,
    }),
    [schedules],
  );

  async function loadSchedules() {
    setIsLoading(true);
    setError(null);

    try {
      const data = await getCrawlSchedules();
      setSchedules(data);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load schedules.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadSchedules();
  }, []);

  async function handleCreateSchedule(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      await createCrawlSchedule(form);
      setForm(initialFormState);
      await loadSchedules();
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

      await loadSchedules();
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
      await loadSchedules();
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
        <button className="icon-button" type="button" onClick={() => void loadSchedules()} title="Refresh schedules">
          Refresh
        </button>
      </div>

      <div className="metric-grid">
        <div className="metric-card">
          <span>Total schedules</span>
          <strong>{totals.total}</strong>
        </div>
        <div className="metric-card">
          <span>Enabled</span>
          <strong>{totals.enabled}</strong>
        </div>
        <div className="metric-card">
          <span>Paused</span>
          <strong>{totals.paused}</strong>
        </div>
      </div>

      <div className="workspace-grid">
        <form className="panel create-form" onSubmit={(event) => void handleCreateSchedule(event)}>
          <div>
            <p className="eyebrow">New schedule</p>
            <h3>Create periodic crawl</h3>
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

          <label>
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

          <label className="checkbox-row">
            <input
              type="checkbox"
              checked={form.stayWithinDomain}
              onChange={(event) => setForm((current) => ({ ...current, stayWithinDomain: event.target.checked }))}
            />
            Stay within domain
          </label>

          <button className="primary-button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Creating..." : "Create schedule"}
          </button>
        </form>

        <div className="panel table-panel">
          <div className="panel-heading">
            <div>
              <p className="eyebrow">Background jobs</p>
              <h3>Schedule list</h3>
            </div>
          </div>

          {error && <div className="alert">{error}</div>}

          {isLoading ? (
            <div className="empty-state">Loading schedules...</div>
          ) : schedules.length === 0 ? (
            <div className="empty-state">No periodic crawl schedules have been created.</div>
          ) : (
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Target</th>
                    <th>Status</th>
                    <th>Interval</th>
                    <th>Scope</th>
                    <th>Next run</th>
                    <th>Last run</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {schedules.map((schedule) => (
                    <tr key={schedule.id}>
                      <td data-label="Target">
                        <div className="url-cell">{schedule.targetUrl}</div>
                        <span className="meta-line">
                          depth {schedule.maxDepth} / {schedule.maxPages} pages
                        </span>
                      </td>
                      <td data-label="Status">
                        <span className={`status-badge ${schedule.isEnabled ? "status-completed" : "status-cancelled"}`}>
                          {schedule.isEnabled ? "Enabled" : "Paused"}
                        </span>
                      </td>
                      <td data-label="Interval">{schedule.intervalMinutes} min</td>
                      <td data-label="Scope">{schedule.stayWithinDomain ? "Domain only" : "Any domain"}</td>
                      <td data-label="Next run">
                        <span className="date-cell">{new Date(schedule.nextRunAt).toLocaleString()}</span>
                      </td>
                      <td data-label="Last run">
                        {schedule.lastRunAt ? (
                          <span className="date-cell">{new Date(schedule.lastRunAt).toLocaleString()}</span>
                        ) : (
                          "-"
                        )}
                      </td>
                      <td data-label="Actions">
                        <div className="button-group">
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
                            {activeScheduleId === schedule.id
                              ? "Updating..."
                              : schedule.isEnabled
                                ? "Pause"
                                : "Enable"}
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
