/**
 * أنواع الاستجابة المشتركة لجميع واجهات API
 * Canonical API response types — single source of truth
 */

export interface ApiResponse<T> {
  isSuccess: boolean;
  data: T;
  message?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages?: number;
  hasPrevious?: boolean;
  hasNext?: boolean;
  nextCursor?: number | null;
  hasMore?: boolean;
}

export interface ApiError {
  message: string;
  errors?: string[];
  statusCode: number;
}
