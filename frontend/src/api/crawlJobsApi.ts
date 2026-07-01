import { request } from "./httpClient";
import type { CrawlJob, CreateCrawlJobRequest } from "../types/crawlJob";

export function getCrawlJobs() {
  return request<CrawlJob[]>("/api/CrawlJob");
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
