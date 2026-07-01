export type CrawlJobStatus = "Pending" | "InProgress" | "Completed" | "Failed" | "Cancelled";

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

export type CreateCrawlJobRequest = {
  targetUrl: string;
  maxDepth: number;
  maxPages: number;
  stayWithinDomain: boolean;
};
