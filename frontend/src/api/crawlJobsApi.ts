import { request, requestBlob } from "./httpClient";
import type {
  CrawledPage,
  CrawledPagesQuery,
  CrawlJob,
  CrawlJobDetails,
  CrawlJobsQuery,
  CrawlLog,
  CrawlLogsQuery,
  CreateCrawlJobRequest,
  PagedResult,
} from "../types/crawlJob";

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

export function getCrawlJobs(query: CrawlJobsQuery) {
  return request<PagedResult<CrawlJob>>(
    `/api/CrawlJob${toQueryString({
      search: query.search,
      status: query.status,
      importantOnly: query.importantOnly ? "true" : undefined,
      pageNumber: query.pageNumber,
      pageSize: query.pageSize,
    })}`,
  );
}

export function getCrawlJob(id: string) {
  return request<CrawlJobDetails>(`/api/CrawlJob/${id}`);
}

export function createCrawlJob(payload: CreateCrawlJobRequest) {
  return request<string>("/api/CrawlJob", {
    method: "POST",
    body: payload,
  });
}

export function startCrawlJob(id: string) {
  return request<void>(`/api/CrawlJob/${id}/start`, {
    method: "POST",
  });
}

export function deleteCrawlJob(id: string) {
  return request<void>(`/api/CrawlJob/${id}`, {
    method: "DELETE",
  });
}

export function toggleCrawlJobImportance(id: string) {
  return request<{ isImportant: boolean }>(`/api/CrawlJob/${id}/importance`, {
    method: "PATCH",
  });
}

export function getCrawledPages(id: string, query: CrawledPagesQuery) {
  return request<PagedResult<CrawledPage>>(
    `/api/CrawlJob/${id}/pages${toQueryString({
      search: query.search,
      statusCode: query.statusCode,
      depthLevel: query.depthLevel,
      pageNumber: query.pageNumber,
      pageSize: query.pageSize,
    })}`,
  );
}

export function getAllCrawledPages(query: CrawledPagesQuery) {
  return request<PagedResult<CrawledPage>>(
    `/api/CrawlJob/pages${toQueryString({
      search: query.search,
      statusCode: query.statusCode,
      depthLevel: query.depthLevel,
      pageNumber: query.pageNumber,
      pageSize: query.pageSize,
    })}`,
  );
}

export function getCrawlLogs(id: string, query: CrawlLogsQuery) {
  return request<PagedResult<CrawlLog>>(
    `/api/CrawlJob/${id}/logs${toQueryString({
      level: query.level,
      pageNumber: query.pageNumber,
      pageSize: query.pageSize,
    })}`,
  );
}

export async function exportCrawlJob(id: string, format: "Csv" | "Json") {
  return requestBlob(`/api/CrawlJob/${id}/export?format=${format}`, {
    method: "POST",
  });
}

export function deleteExportFile(id: string) {
  return request<void>(`/api/CrawlJob/exports/${id}`, {
    method: "DELETE",
  });
}
