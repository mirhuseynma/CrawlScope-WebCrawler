import { request } from "./httpClient";
import type { AdminOverview } from "../types/crawlJob";

export function getAdminOverview() {
  return request<AdminOverview>("/api/AdminOverview");
}
