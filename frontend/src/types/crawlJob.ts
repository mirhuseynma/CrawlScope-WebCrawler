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
  type: string;
  maxDepth: number;
  maxPages: number;
  pagesFound: number;
  pagesCrawled: number;
  pagesFailed: number;
  isImportant: boolean;
  createdAt: string;
};

export type CrawlJobDetails = {
  id: string;
  targetUrl: string;
  status: CrawlJobStatus | string;
  type: string;
  maxDepth: number;
  maxPages: number;
  pagesFound: number;
  pagesFailed: number;
  isImportant: boolean;
  createdAt: string;
  startedAt: string | null;
  finishedAt: string | null;
  errorMessage: string | null;
};

export type CrawledPage = {
  id: string;
  crawlJobId: string;
  crawlJobTargetUrl: string;
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

export type BrokenLink = {
  id: string;
  crawlJobId: string;
  sourceUrl: string;
  targetUrl: string;
  anchorText: string | null;
  isExternal: boolean;
  depthLevel: number;
  statusCode: number | null;
  responseTimeMs: number | null;
  errorMessage: string | null;
  detectedAt: string;
};

export type ExportFile = {
  id: string;
  crawlJobId: string;
  crawlJobTargetUrl: string;
  format: "Csv" | "Json" | string;
  fileName: string;
  fileSizeBytes: number;
  createdAt: string;
};

export type CreateCrawlJobRequest = {
  targetUrl: string;
  maxDepth: number;
  maxPages: number;
  stayWithinDomain: boolean;
  crawlType: string;
};

export type AnalyzeUrlResult = {
  recommendedType: "Fast" | "Dynamic" | string;
  recommendationReason: string;
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

export type CrawlSchedulesQuery = {
  search?: string;
  isEnabled?: boolean;
  pageNumber: number;
  pageSize: number;
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
  importantOnly?: boolean;
  pageNumber: number;
  pageSize: number;
};

export type ExportFilesQuery = {
  search?: string;
  format?: string;
  pageNumber: number;
  pageSize: number;
};

export type AdminOverviewTotals = {
  totalJobs: number;
  pendingJobs: number;
  inProgressJobs: number;
  completedJobs: number;
  failedJobs: number;
  canceledJobs: number;
  importantJobs: number;
  totalPages: number;
  failedPages: number;
  totalExports: number;
  totalExportSizeBytes: number;
};

export type AdminStatusCount = {
  status: string;
  count: number;
};

export type AdminOverviewJob = {
  id: string;
  targetUrl: string;
  status: CrawlJobStatus | string;
  pagesCrawled: number;
  pagesFailed: number;
  isImportant: boolean;
  createdAt: string;
};

export type AdminOverviewExport = {
  id: string;
  crawlJobId: string;
  crawlJobTargetUrl: string;
  format: "Csv" | "Json" | string;
  fileName: string;
  fileSizeBytes: number;
  createdAt: string;
};

export type AdminOverview = {
  totals: AdminOverviewTotals;
  statusDistribution: AdminStatusCount[];
  recentJobs: AdminOverviewJob[];
  recentExports: AdminOverviewExport[];
  problemJobs: AdminOverviewJob[];
};

export type CrawledPagesQuery = {
  search?: string;
  statusCode?: number;
  depthLevel?: number;
  pageNumber: number;
  pageSize: number;
};

export type BrokenLinksQuery = {
  search?: string;
  statusCode?: number;
  externalOnly?: boolean;
  pageNumber: number;
  pageSize: number;
};

export type CrawlLogsQuery = {
  level?: string;
  pageNumber: number;
  pageSize: number;
};
