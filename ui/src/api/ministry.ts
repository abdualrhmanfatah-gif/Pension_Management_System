<<<<<<< HEAD
import type { ApiResponse, PagedResult } from "@/types/api";

import api from "./axios";
=======
import api from "./axios";
import type { ApiResponse, PagedResult } from "@/types/api";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export interface ForceDashboardStatsDto {
    totalBatches: number;
    processedRecords: number;
    pendingRecords: number;
    reconciliationGaps: number;
}

export interface ForceGlobalSearchDto {
    approvalItemID: number;
    militaryId: string;
    nationalId?: string;
    fullName: string;
    rankName?: string;
    unitName?: string;
    batchName?: string;
    gender?: number;
    status: string;
    isProcessed: boolean;
    retireeID?: number;
}

export interface MinistryApprovalBatchDto {
    batchID: number;
    batchNumber: string;
    batchDate: string;
    receivedDate: string;
    totalCount: number;
    processedCount: number;
    status: string;
    notes?: string;
    importedByUser?: string;
    createdAt: string;
}

export interface MinistryApprovalItemDto {
    approvalItemID: number;
    serialNumber: number;
    militaryNumber: string;
    fullName: string;
    gender?: number;
<<<<<<< HEAD
    rankID?: number;
    rankName?: string;
    categoryID?: number;
=======
    rankName?: string;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    unitName?: string;
    enrollmentDate?: string;
    retirementDate?: string;
    salary?: number;
    categoryName?: string;
    notes?: string;
    linkNumber?: string;
    isProcessed: boolean;
    retireeID?: number;
    batchNumber?: string;
    batchDate?: string;
    processedAt?: string;
    processedByUser?: string;
}

export interface ReconciliationRowDto {
    approvalItemID: number;
    militaryId: string;
    ministryName: string;
    rankName?: string;
    unitName?: string;
    salary?: number;
    dbName?: string;
    dbMilitaryId?: string;
    statusGap: string;
    isActionRequired: boolean;
}

export interface ForceAuditLogDto {
    actionDate: string;
    userName: string;
    actionType: string;
    oldStatus?: string;
    newStatus?: string;
    notes?: string;
}

export const ministryApi = {
    // Analytics & Search
    getDashboardStats: () => 
        api.get<ApiResponse<ForceDashboardStatsDto>>('/ministry-approvals/analytics/dashboard').then(r => r.data),
    searchGlobalForce: (q: string) => 
        api.get<ApiResponse<ForceGlobalSearchDto[]>>(`/ministry-approvals/search?q=${q}`).then(r => r.data),

    // Batches
    getBatches: () => 
        api.get<ApiResponse<MinistryApprovalBatchDto[]>>('/ministry-approvals/batches').then(r => r.data),
    getBatchById: (id: number) => 
        api.get<ApiResponse<MinistryApprovalBatchDto>>(`/ministry-approvals/batches/${id}`).then(r => r.data),
    createBatch: (data: unknown) => 
        api.post<ApiResponse<MinistryApprovalBatchDto>>('/ministry-approvals/batches', data).then(r => r.data),
    deleteBatch: (id: number) => 
        api.delete<ApiResponse<boolean>>(`/ministry-approvals/batches/${id}`).then(r => r.data),
    importExcel: (id: number, file: File) => {
        const formData = new FormData();
        formData.append('File', file);
        return api.post<ApiResponse<boolean>>(`/ministry-approvals/batches/${id}/import`, formData).then(r => r.data);
    },

    // Items
<<<<<<< HEAD
    getBatchItems: (batchId: number, page: number = 1, pageSize: number = 10, q?: string, gender?: number) =>
        api.get<ApiResponse<PagedResult<MinistryApprovalItemDto>>>(`/ministry-approvals/batches/${batchId}/items`, { params: { pageNumber: page, pageSize, q, gender } }).then(r => r.data),
    getAllItems: (page: number = 1, pageSize: number = 20, q?: string, isProcessed?: boolean, gender?: number) =>
        api.get<ApiResponse<PagedResult<MinistryApprovalItemDto>>>('/ministry-approvals/all-items', { params: { pageNumber: page, pageSize, q, isProcessed, gender } }).then(r => r.data),
=======
    getBatchItems: (batchId: number, page: number = 1, pageSize: number = 10, q?: string) => 
        api.get<ApiResponse<PagedResult<MinistryApprovalItemDto>>>(`/ministry-approvals/batches/${batchId}/items?pageNumber=${page}&pageSize=${pageSize}${q ? `&q=${q}` : ''}`).then(r => r.data),
    getAllItems: (page: number = 1, pageSize: number = 20, q?: string, isProcessed?: boolean) => 
        api.get<ApiResponse<PagedResult<MinistryApprovalItemDto>>>(`/ministry-approvals/all-items?pageNumber=${page}&pageSize=${pageSize}${q ? `&q=${q}` : ''}${isProcessed !== undefined ? `&isProcessed=${isProcessed}` : ''}`).then(r => r.data),
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    getItemById: (id: number) => 
        api.get<ApiResponse<MinistryApprovalItemDto>>(`/ministry-approvals/items/${id}`).then(r => r.data),
    addManualItem: (batchId: number, data: unknown) => 
        api.post<ApiResponse<MinistryApprovalItemDto>>(`/ministry-approvals/batches/${batchId}/items`, data).then(r => r.data),
<<<<<<< HEAD
    updateManualItem: (itemId: number, data: unknown) => 
        api.put<ApiResponse<MinistryApprovalItemDto>>(`/ministry-approvals/items/${itemId}`, data).then(r => r.data),
    deleteManualItem: (itemId: number) => 
        api.delete<ApiResponse<boolean>>(`/ministry-approvals/items/${itemId}`).then(r => r.data),
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    processItem: (itemId: number) => 
        api.post<ApiResponse<boolean>>(`/ministry-approvals/items/${itemId}/process`).then(r => r.data),

    // Reconciliation & Bulk
    getReconciliationMatrix: (batchId: number, page: number = 1, pageSize: number = 10) => 
        api.get<ApiResponse<PagedResult<ReconciliationRowDto>>>(`/ministry-approvals/batches/${batchId}/reconciliation?pageNumber=${page}&pageSize=${pageSize}`).then(r => r.data),
    processBulkApprovals: (approvalIds: number[]) => 
        api.post<ApiResponse<boolean>>('/ministry-approvals/bulk-process', { approvalIds }).then(r => r.data),

    // Audit & Export
    getBatchAuditTrail: (batchId: number) => 
        api.get<ApiResponse<ForceAuditLogDto[]>>(`/ministry-approvals/${batchId}/audit-trail`).then(r => r.data),
    exportBatch: (id: number, format: 'excel' | 'pdf') => 
        api.get(`/ministry-approvals/${id}/export?format=${format}`, { responseType: 'blob' }),
};
