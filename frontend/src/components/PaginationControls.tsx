import type { PagedResult } from "../types/crawlJob";

type PaginationControlsProps = {
  label: string;
  page: PagedResult<unknown>;
  onPageChange: (pageNumber: number) => void;
};

export function PaginationControls({ label, page, onPageChange }: PaginationControlsProps) {
  return (
    <div className="pagination-row" aria-label={`${label} pagination`}>
      <span className="pagination-summary">{page.totalCount} total</span>
      <span className="pagination-current">
        Page {page.totalPages === 0 ? 0 : page.pageNumber} of {page.totalPages}
      </span>
      <div className="button-group">
        <button
          className="secondary-button"
          type="button"
          onClick={() => onPageChange(page.pageNumber - 1)}
          disabled={!page.hasPreviousPage}
        >
          Previous
        </button>
        <button
          className="secondary-button"
          type="button"
          onClick={() => onPageChange(page.pageNumber + 1)}
          disabled={!page.hasNextPage}
        >
          Next
        </button>
      </div>
    </div>
  );
}
