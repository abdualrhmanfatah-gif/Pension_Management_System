import api from "./axios";
import type { ApiResponse, PagedResult } from "@/types/api";

// ═══════════════════════════════════════
// TYPES
// ═══════════════════════════════════════

export type VoucherType = 1 | 2; // 1=صرف, 2=قبض
export type VoucherStatus = 1 | 2 | 3; // 1=مسودة, 2=مرحّل, 3=ملغي

export interface VoucherDto {
  voucherID: number;
  voucherNumber: string;
  voucherType: VoucherType;
  voucherTypeName: string;
  status: VoucherStatus;
  statusName: string;
  voucherDate: string;
  payeeName: string | null;
  payerName: string | null;
  amount: number;
  description: string;
  debitAccountID: number;
  debitAccountCode: string;
  debitAccountName: string;
  creditAccountID: number;
  creditAccountCode: string;
  creditAccountName: string;
  chequeNumber: string | null;
  referenceNumber: string | null;
  referralSource: string | null;
  journalEntryID: number | null;
  journalEntryNumber: string | null;
  createdByUserName: string;
  createdAt: string;
}

export interface CreateVoucherDto {
  voucherType: VoucherType;
  voucherDate: string;
  payeeName?: string;
  payerName?: string;
  amount: number;
  description: string;
  debitAccountID: number;
  creditAccountID: number;
  chequeNumber?: string;
  referenceNumber?: string;
  referralSource?: string;
}

export interface VoucherFilter {
  voucherType?: VoucherType;
  status?: VoucherStatus;
  fromDate?: string;
  toDate?: string;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface ImportErrorDto {
  rowNumber: number;
  message: string;
}

export interface ImportVoucherResultDto {
  totalRows: number;
  successCount: number;
  skipCount: number;
  errors: ImportErrorDto[];
  createdVoucherIds: number[];
}

// ═══════════════════════════════════════
// API
// ═══════════════════════════════════════

export const vouchersApi = {
  list: (filter?: VoucherFilter) =>
    api.get<ApiResponse<PagedResult<VoucherDto>>>("/accounting/vouchers", { params: filter }).then((r) => r.data.data),

  getById: (id: number) =>
    api.get<ApiResponse<VoucherDto>>(`/accounting/vouchers/${id}`).then((r) => r.data.data),

  create: (dto: CreateVoucherDto) =>
    api.post<ApiResponse<VoucherDto>>("/accounting/vouchers", dto).then((r) => r.data),

  update: (id: number, dto: Partial<CreateVoucherDto>) =>
    api.put<ApiResponse<VoucherDto>>(`/accounting/vouchers/${id}`, dto).then((r) => r.data),

  delete: (id: number) =>
    api.delete<ApiResponse<boolean>>(`/accounting/vouchers/${id}`).then((r) => r.data),

  post: (id: number) =>
    api.post<ApiResponse<VoucherDto>>(`/accounting/vouchers/${id}/post`).then((r) => r.data),

  cancel: (id: number) =>
    api.post<ApiResponse<VoucherDto>>(`/accounting/vouchers/${id}/cancel`).then((r) => r.data),

  nextNumber: (type: VoucherType, date?: string) =>
    api.get<ApiResponse<string>>("/accounting/vouchers/next-number", { params: { type, date } }).then((r) => r.data.data),

  import: (file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return api.post<ApiResponse<ImportVoucherResultDto>>("/accounting/vouchers/import", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    }).then((r) => r.data);
  },
};
