import { request } from "./httpClient";
import type { CrawlSchedule, CreateCrawlScheduleRequest } from "../types/crawlJob";

export function getCrawlSchedules() {
  return request<CrawlSchedule[]>("/api/CrawlSchedule");
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
