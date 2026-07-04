export type CrawlJobStatus = "Pending" | "InProgress" | "Completed" | "Failed" | "Cancelled";

export type PagedResult<T> = {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
};

export type CrawlJob = {
  id: string;
  targetUrl: string;
  status: CrawlJobStatus | string;
  maxDepth: number;
  maxPages: number;
  pagesFound: number;
  pagesCrawled: number;
  pagesFailed: number;
  createdAt: string;
};

export type CrawlJobDetails = {
  id: string;
  targetUrl: string;
  status: CrawlJobStatus | string;
  maxDepth: number;
  maxPages: number;
  pagesFound: number;
  pagesFailed: number;
  createdAt: string;
  startedAt: string | null;
  finishedAt: string | null;
  errorMessage: string | null;
};

export type CrawledPage = {
  id: string;
  url: string;
  title: string | null;
  contentPreview: string | null;
  statusCode: number | null;
  depthLevel: number;
  crawledAt: string;
  responseTimeMs: number | null;
  internalLinksCount: number;
  externalLinksCount: number;
};

export type CrawlLog = {
  id: string;
  level: string;
  message: string;
  createdAt: string;
};

export type CreateCrawlJobRequest = {
  targetUrl: string;
  maxDepth: number;
  maxPages: number;
  stayWithinDomain: boolean;
};

export type CrawlSchedule = {
  id: string;
  targetUrl: string;
  maxDepth: number;
  maxPages: number;
  stayWithinDomain: boolean;
  intervalMinutes: number;
  isEnabled: boolean;
  createdAt: string;
  nextRunAt: string;
  lastRunAt: string | null;
  lastCrawlJobId: string | null;
};

export type CreateCrawlScheduleRequest = {
  targetUrl: string;
  maxDepth: number;
  maxPages: number;
  stayWithinDomain: boolean;
  intervalMinutes: number;
};

export type CrawlJobsQuery = {
  search?: string;
  status?: string;
  pageNumber: number;
  pageSize: number;
};

export type CrawledPagesQuery = {
  search?: string;
  statusCode?: number;
  depthLevel?: number;
  pageNumber: number;
  pageSize: number;
};

export type CrawlLogsQuery = {
  level?: string;
  pageNumber: number;
  pageSize: number;
};
