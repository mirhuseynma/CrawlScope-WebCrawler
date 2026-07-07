import { request, requestBlob } from "./httpClient";
import type { ExportFile, ExportFilesQuery, PagedResult } from "../types/crawlJob";

function toQueryString(params: Record<string, string | number | undefined>) {
  const searchParams = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== "") {
      searchParams.set(key, String(value));
    }
  });

  const queryString = searchParams.toString();
  return queryString ? `?${queryString}` : "";
}

export function getExportFiles(query: ExportFilesQuery) {
  return request<PagedResult<ExportFile>>(
    `/api/ExportFile${toQueryString({
      search: query.search,
      format: query.format,
      pageNumber: query.pageNumber,
      pageSize: query.pageSize,
    })}`,
  );
}

export function downloadExportFile(id: string) {
  return requestBlob(`/api/ExportFile/${id}/download`);
}

export function deleteExportFileRecord(id: string) {
  return request<void>(`/api/ExportFile/${id}`, {
    method: "DELETE",
  });
}
