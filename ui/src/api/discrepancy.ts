<<<<<<< HEAD
import type { ApiResponse, PagedResult } from "@/types/api";

import api from "./axios";
=======
import api from "./axios";
import type { ApiResponse, PagedResult } from "@/types/api";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export interface RetireeDiscrepancy {
  approvalItemID?: number;
  retireeID?: number;
  fileID?: number;
  militaryNumber?: string;
  nationalID?: string;
  approvalFullName?: string;
  approvalRankName?: string;
  approvalRetirementDate?: string;
  approvalUnitName?: string;
  gender?: number;
  salary?: number;
  categoryName?: string;
  linkNumber?: string;
  retireeFullName?: string;
  isProcessed: boolean;
  fileNumber?: string;
  fileRankName?: string;
  fileRetirementDate?: string;
  fileStatusName?: string;
  isNameMismatch: boolean;
  isRankMismatch: boolean;
  isDateMismatch: boolean;
  hasAnyDiscrepancy: boolean;
  currentStage: string;
}

export interface DiscrepancyFilter {
  searchTerm?: string;
  onlyMismatches?: boolean;
  stage?: string;
  unitName?: string;
  rankName?: string;
  categoryName?: string;
  gender?: number;
  processingStatus?: string;
  fileStatus?: string;
  pageNumber: number;
  pageSize: number;
}

export interface NameValuePair {
  name: string;
  count: number;
}

export interface KashfReportSummary {
  totalCount: number;
  processedCount: number;
  pendingCount: number;
  fileCreatedCount: number;
  byUnit: NameValuePair[];
  byRank: NameValuePair[];
  byCategory: NameValuePair[];
  byGender: NameValuePair[];
}

export const discrepancyApi = {
  getReport: (params: DiscrepancyFilter) =>
    api
      .get<ApiResponse<PagedResult<RetireeDiscrepancy>>>("/discrepancy/report", { params })
      .then((r) => r.data.data),

  getKashfReport: (params: DiscrepancyFilter) =>
    api
      .get<ApiResponse<PagedResult<RetireeDiscrepancy>>>("/discrepancy/kashf-report", { params })
      .then((r) => r.data.data),

  getKashfReportSummary: (params: DiscrepancyFilter) =>
    api
      .get<ApiResponse<KashfReportSummary>>("/discrepancy/kashf-report/summary", { params })
      .then((r) => r.data.data),

  exportKashfReport: (params: DiscrepancyFilter, format: "excel" | "pdf") =>
    api.get(`/discrepancy/kashf-report/export`, {
      params: { ...params, format },
      responseType: "blob",
    }),
};
