import { request } from "./httpClient";
import type { CrawlSchedule, CrawlSchedulesQuery, CreateCrawlScheduleRequest, PagedResult } from "../types/crawlJob";

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

export function getCrawlSchedules(query: CrawlSchedulesQuery) {
  return request<PagedResult<CrawlSchedule>>(
    `/api/CrawlSchedule${toQueryString({
      search: query.search,
      isEnabled: query.isEnabled === undefined ? undefined : String(query.isEnabled),
      pageNumber: query.pageNumber,
      pageSize: query.pageSize,
    })}`,
  );
}

export function createCrawlSchedule(payload: CreateCrawlScheduleRequest) {
  return request<string>("/api/CrawlSchedule", {
    method: "POST",
    body: payload,
  });
}

export function enableCrawlSchedule(id: string) {
  return request<void>(`/api/CrawlSchedule/${id}/enable`, {
    method: "PATCH",
  });
}

export function disableCrawlSchedule(id: string) {
  return request<void>(`/api/CrawlSchedule/${id}/disable`, {
    method: "PATCH",
  });
}

export function deleteCrawlSchedule(id: string) {
  return request<void>(`/api/CrawlSchedule/${id}`, {
    method: "DELETE",
  });
}
