import { FormEvent, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { deleteExportFileRecord, downloadExportFile, getExportFiles } from "../api/exportFilesApi";
import { PaginationControls } from "../components/PaginationControls";
import type { ExportFile, PagedResult } from "../types/crawlJob";

const emptyExportsPage: PagedResult<ExportFile> = {
  items: [],
  pageNumber: 1,
  pageSize: 5,
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false,
};

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

function saveBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

export function ExportsPage() {
  const [exportsPage, setExportsPage] = useState<PagedResult<ExportFile>>(emptyExportsPage);
  const [search, setSearch] = useState("");
  const [format, setFormat] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [isLoading, setIsLoading] = useState(true);
  const [activeExportId, setActiveExportId] = useState<string | null>(null);
  const [deletingExportId, setDeletingExportId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const totals = useMemo(
    () => ({
      exports: exportsPage.totalCount,
      csv: exportsPage.items.filter((exportFile) => exportFile.format === "Csv").length,
      json: exportsPage.items.filter((exportFile) => exportFile.format === "Json").length,
    }),
    [exportsPage],
  );

  async function loadExports(nextPageNumber = pageNumber) {
    setIsLoading(true);
    setError(null);

    try {
      const data = await getExportFiles({
        search,
        format,
        pageNumber: nextPageNumber,
        pageSize,
      });

      setExportsPage(data);
      setPageNumber(data.pageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to load export history.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadExports(1);
  }, []);

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void loadExports(1);
  }

  async function handleDownload(exportFile: ExportFile) {
    setActiveExportId(exportFile.id);
    setError(null);

    try {
      const blob = await downloadExportFile(exportFile.id);
      saveBlob(blob, exportFile.fileName);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to download export.");
    } finally {
      setActiveExportId(null);
    }
  }

  async function handleDelete(exportFile: ExportFile) {
    const confirmed = window.confirm(`Delete export ${exportFile.fileName}? This will remove the saved file too.`);

    if (!confirmed) {
      return;
    }

    setDeletingExportId(exportFile.id);
    setError(null);

    try {
      await deleteExportFileRecord(exportFile.id);
      await loadExports(pageNumber);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Failed to delete export.");
    } finally {
      setDeletingExportId(null);
    }
  }

  return (
    <section className="page-section">
      <div className="section-header">
        <div>
          <p className="eyebrow">Export center</p>
          <h2>Exports</h2>
        </div>
        <button className="icon-button" type="button" onClick={() => void loadExports(pageNumber)} title="Refresh exports">
          Refresh
        </button>
      </div>

      <div className="metric-grid">
        <div className="metric-card">
          <span>Total exports</span>
          <strong>{totals.exports}</strong>
        </div>
        <div className="metric-card">
          <span>CSV on page</span>
          <strong>{totals.csv}</strong>
        </div>
        <div className="metric-card">
          <span>JSON on page</span>
          <strong>{totals.json}</strong>
        </div>
      </div>

      <section className="panel table-panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">Saved artifacts</p>
            <h3>Export history</h3>
          </div>
        </div>

        <form className="filter-bar export-filter" onSubmit={applyFilters}>
          <input
            aria-label="Search exports"
            placeholder="Search file or target URL"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <select aria-label="Filter by export format" value={format} onChange={(event) => setFormat(event.target.value)}>
            <option value="">All formats</option>
            <option value="Csv">CSV</option>
            <option value="Json">JSON</option>
          </select>
          <select aria-label="Exports page size" value={pageSize} onChange={(event) => setPageSize(Number(event.target.value))}>
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
          <div className="empty-state">Loading exports...</div>
        ) : exportsPage.items.length === 0 ? (
          <div className="empty-state">No exports match the current filters.</div>
        ) : (
          <>
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>File</th>
                    <th>Format</th>
                    <th>Size</th>
                    <th>Job</th>
                    <th>Created</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {exportsPage.items.map((exportFile) => (
                    <tr key={exportFile.id}>
                      <td data-label="File">
                        <div className="page-title">{exportFile.fileName}</div>
                      </td>
                      <td data-label="Format">
                        <span className="format-pill">{exportFile.format.toUpperCase()}</span>
                      </td>
                      <td data-label="Size">{formatFileSize(exportFile.fileSizeBytes)}</td>
                      <td data-label="Job">
                        <div className="url-cell compact-url-cell">{exportFile.crawlJobTargetUrl}</div>
                      </td>
                      <td data-label="Created">
                        <span className="date-cell">{new Date(exportFile.createdAt).toLocaleString()}</span>
                      </td>
                      <td data-label="Actions">
                        <div className="button-group">
                          <button
                            className="secondary-button"
                            type="button"
                            onClick={() => void handleDownload(exportFile)}
                            disabled={activeExportId === exportFile.id}
                          >
                            {activeExportId === exportFile.id ? "Downloading..." : "Download"}
                          </button>
                          <Link className="secondary-link-button" to={`/admin/jobs/${exportFile.crawlJobId}`}>
                            Open job
                          </Link>
                          <button
                            className="danger-button"
                            type="button"
                            onClick={() => void handleDelete(exportFile)}
                            disabled={deletingExportId === exportFile.id}
                          >
                            {deletingExportId === exportFile.id ? "Deleting..." : "Delete"}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <PaginationControls label="Exports" page={exportsPage} onPageChange={(nextPage) => void loadExports(nextPage)} />
          </>
        )}
      </section>
    </section>
  );
}
