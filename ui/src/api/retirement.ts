import type { ReactivateEntitledPersonRequest, RecordEntitledPersonDeathRequest, StopEntitledPersonRequest } from "@/lib/entitledPersonRequests";
import { entitledPersonMultipart, reactivateEntitledPersonForm, recordEntitledPersonDeathForm, stopEntitledPersonForm } from "@/lib/entitledPersonRequests";
import type { ApiResponse, PagedResult } from "@/types/api";
import type {
    AgentSearchResult,
    AgentSummary,
    ChangeLogItem,
    EntitledPersonSearchResult,
    EntitledPersonSummary,
    ExemptionItem,
    FileReviewDiff,
    FileStatus,
    FinancialEntitlement,
    GuardianSearchResult,
    GuardianSummary,
    LookupDto,
    NoteItem,
    PaymentBookItem,
    PendingReviewFile,
    PledgeItem,
    RecipientItem,
    RetirementFileDetail,
    RetirementFilesPagedResult,
    SalaryPaymentBatch,
    StatusTransition,
    SuspensionItem} from "@/types/retirement";

import api from "./axios";
import { documentsApi } from "./documents";
import type { SettlementItemDto } from "./settlements";

function normalizeAgentPhoto<T extends { personalPhotoPath?: string | null }>(item: T & {
    PersonalPhotoPath?: string | null;
    personalPhoto?: string | null;
    PersonalPhoto?: string | null;
    photoPath?: string | null;
    PhotoPath?: string | null;
}): T {
    return {
        ...item,
        personalPhotoPath: item.personalPhotoPath ?? item.PersonalPhotoPath ?? item.personalPhoto ?? item.PersonalPhoto ?? item.photoPath ?? item.PhotoPath ?? null,
    } as T;
}

const normalizeEntitledPhoto = normalizeAgentPhoto;

// ─── Report DTOs ────────────────────────────────────────

export interface RetirementFileReportFilter {
    searchTerm?: string;
    statusId?: number;
    rankId?: number;
    gender?: number;
    branchId?: number;
    governorate?: string;
    reviewStatus?: number;
    socialStatusId?: number;
    pageNumber: number;
    pageSize: number;
}

export interface RetirementFileReportRow {
    fileID: number;
    fileNumber?: string;
    retireeName?: string;
    militaryNumber?: string;
    nationalID?: string;
    rankName?: string;
    branchName?: string;
    governorateName?: string;
    statusName?: string;
    gender?: number;
    socialStatusName?: string;
    healthStatusName?: string;
    retirementDate?: string;
    fileOpenDate?: string;
    missingDocumentsCount: number;
    reviewStatusName?: string;
    isAlive: boolean;
}

export interface NameValuePair {
    name: string;
    count: number;
}

export interface RetirementFileReportSummary {
    totalCount: number;
    activeCount: number;
    suspendedCount: number;
    closedCount: number;
    incompleteCount: number;
    pendingReviewCount: number;
    aliveCount: number;
    deceasedCount: number;
    byRank: NameValuePair[];
    byBranch: NameValuePair[];
    byGovernorate: NameValuePair[];
    byGender: NameValuePair[];
    byStatus: NameValuePair[];
    bySocialStatus: NameValuePair[];
}

// ─── DTOs ──────────────────────────────────────────────

export interface CreateRecipientDto {
    fileID: number;
    retireeID?: number;
    agentID?: number;
    entitledID?: number;
    guardianID?: number;
    startDate: string;
    notes?: string;
}

export interface EndRecipientDto {
    endDate: string;
    notes?: string;
    reason?: string;
}

export interface CreateSuspensionDto {
    fileID: number;
    suspensionReason: string;
    suspensionSource: string;
}

export interface EndSuspensionDto {
    endReason: string;
}

export interface CreatePledgeDto {
    fileID: number;
    missingDocumentIDs: number[];
    deadline: string;
    pledgedByName: string;
    pledgedByType: string;
    reason?: string;
}

export interface CreateExemptionDto {
    fileID: number;
    missingDocumentIDs: number[];
    directiveSource: string;
    exemptionDate: string;
    notes?: string;
}

export interface CreateMissingDocumentDto {
    fileID: number;
    documentTypeID: number;
    entityType?: string;
    entityName?: string;
    agentID?: number;
    guardianID?: number;
    entitledID?: number;
}

// ─── API Client ────────────────────────────────────────

export const retirementApi = {
    // الملفات
    getFiles: (params: {
        page: number;
        pageSize: number;
        search?: string;
        statusId?: number;
        cursor?: number | null;
        categoryId?: number;
        rankId?: number;
        governorate?: string;
        gender?: number;
    }) =>
        api.get<ApiResponse<RetirementFilesPagedResult>>("/retirement-files", { params }).then(r => r.data.data),

    getFile: (id: number) =>
        api.get<ApiResponse<RetirementFileDetail>>(`/retirement-files/${id}`).then(r => r.data.data),

    // الحالات
    getStatuses: () =>
        api.get<ApiResponse<FileStatus[]>>("/file-statuses").then(r => r.data.data),

    getAllowedStatuses: (fileId: number) =>
        api.get<ApiResponse<FileStatus[]>>(`/retirement-files/${fileId}/allowed-statuses`).then(r => r.data.data),

    // العمليات
    createFile: (data: unknown) =>
        api.post<ApiResponse<number>>("/retirement-files", data).then(r => r.data),

    deleteFile: (id: number) =>
        api.delete<ApiResponse<boolean>>(`/retirement-files/${id}`).then(r => r.data),

    updateFile: (id: number, data: Partial<RetirementFileDetail>) =>
        api.put<ApiResponse<boolean>>(`/retirement-files/${id}`, data).then(r => r.data),

    // مستلمو المعاشات
    getRecipients: (fileId: number) =>
        api.get<ApiResponse<RecipientItem[]>>(`/salary-recipients/by-file/${fileId}`).then(r => r.data.data),

    createRecipient: (data: CreateRecipientDto) =>
        api.post<ApiResponse<number>>("/salary-recipients", data).then(r => r.data),

    endRecipient: (id: number, data: EndRecipientDto) =>
        api.put<ApiResponse<boolean>>(`/salary-recipients/${id}/end`, data).then(r => r.data),

    // إيقافات المعاش
    getSuspensions: (fileId: number) =>
        api.get<ApiResponse<SuspensionItem[]>>(`/suspensions/by-file/${fileId}`).then(r => r.data.data),

    getAllSuspensions: (params: { page: number; pageSize: number; status?: string; fromDate?: string; toDate?: string; search?: string }) =>
        api.get<ApiResponse<PagedResult<SuspensionItem>>>("/suspensions", { params }).then(r => r.data.data),

    createSuspension: (data: CreateSuspensionDto) =>
        api.post<ApiResponse<number>>("/suspensions", data).then(r => r.data),

    endSuspension: (id: number, data: EndSuspensionDto) =>
        api.put<ApiResponse<boolean>>(`/suspensions/${id}/end`, data).then(r => r.data),

    // الوكلاء
    getAgentById: (id: number) =>
        api.get<ApiResponse<AgentSummary>>(`/agents/${id}`).then(r => r.data.data),

    getAgents: (fileId: number): Promise<AgentSummary[]> =>
        api.get<ApiResponse<AgentSummary[]>>(`/agents/by-file/${fileId}`).then(r => (r.data.data || []).map(normalizeAgentPhoto) as AgentSummary[]),

    searchAgents: (query: string, limit = 10): Promise<AgentSearchResult[]> =>
        api.get<ApiResponse<AgentSearchResult[]>>("/agents/search", { params: { q: query, limit } }).then(r => (r.data.data || []).map(normalizeAgentPhoto) as AgentSearchResult[]),

    deactivateAgent: (id: number) =>
        api.put<ApiResponse<boolean>>(`/agents/${id}/deactivate`).then(r => r.data),

    deleteAgent: (id: number) =>
        api.delete<ApiResponse<boolean>>(`/agents/${id}`).then(r => r.data),

    updateAgent: (id: number, formData: FormData) =>
        api.put<ApiResponse<AgentSummary>>(`/agents/${id}`, formData, {
            headers: { "Content-Type": "multipart/form-data" }
        }).then(r => r.data),

    uploadAgentPhoto: (id: number, photo: File) => {
        const formData = new FormData();
        formData.append("photo", photo);
        return api.post<ApiResponse<string>>(`/agents/${id}/photo`, formData, {
            headers: { "Content-Type": "multipart/form-data" }
        }).then(r => r.data);
    },

    // الورثة والمستحقون
    getEntitledPersonById: (id: number) =>
        api.get<ApiResponse<EntitledPersonSummary>>(`/entitled-persons/${id}`).then(r => r.data.data),

    getEntitledPersons: (fileId: number) =>
        api.get<ApiResponse<EntitledPersonSummary[]>>(`/entitled-persons/by-file/${fileId}`).then(r => (r.data.data || []).map(normalizeEntitledPhoto) as EntitledPersonSummary[]),

    searchEntitledPersons: (query: string, limit = 10): Promise<EntitledPersonSearchResult[]> =>
        api.get<ApiResponse<EntitledPersonSearchResult[]>>("/entitled-persons/search", { params: { q: query, limit } }).then(r => (r.data.data || []).map(normalizeEntitledPhoto) as EntitledPersonSearchResult[]),

    updateEntitledPerson: (id: number, formData: FormData) =>
        api.put<ApiResponse<EntitledPersonSummary>>(`/entitled-persons/${id}`, formData, {
            headers: { "Content-Type": "multipart/form-data" }
        }).then(r => r.data),

    createEntitledPerson: (fileId: number, formData: FormData) => {
        formData.set("fileID", String(fileId));
        return api.post<ApiResponse<EntitledPersonSummary>>(
            `/entitled-persons`,
            formData,
            {
                headers: { "Content-Type": "multipart/form-data" },
            }
        ).then(r => r.data);
    },

    stopEntitledPerson: (id: number, data: StopEntitledPersonRequest) =>
        api.put<ApiResponse<boolean>>(`/entitled-persons/${id}/stop`, stopEntitledPersonForm(data), entitledPersonMultipart).then(r => r.data),

    reactivateEntitledPerson: (id: number, data: ReactivateEntitledPersonRequest) =>
        api.put<ApiResponse<boolean>>(`/entitled-persons/${id}/reactivate`, reactivateEntitledPersonForm(data), entitledPersonMultipart).then(r => r.data),

    recordEntitledPersonDeath: (id: number, data: RecordEntitledPersonDeathRequest) =>
        api.post<ApiResponse<boolean>>(`/entitled-persons/${id}/death`, recordEntitledPersonDeathForm(data), entitledPersonMultipart).then(r => r.data),

    deleteEntitledPerson: (id: number) =>
        api.delete<ApiResponse<boolean>>(`/entitled-persons/${id}`).then(r => r.data),

    // الأوصياء
    getGuardianById: (id: number) =>
        api.get<ApiResponse<GuardianSummary>>(`/guardians/${id}`).then(r => r.data.data),

    getGuardians: (fileId: number) =>
        api.get<ApiResponse<GuardianSummary[]>>(`/guardians/by-file/${fileId}`).then(r => r.data.data),

    searchGuardians: (query: string, limit = 10): Promise<GuardianSearchResult[]> =>
        api.get<ApiResponse<GuardianSearchResult[]>>("/guardians/search", { params: { q: query, limit } }).then(r => (r.data.data || []).map(normalizeAgentPhoto) as GuardianSearchResult[]),

    endGuardianship: (id: number, data: { reason: string }) =>
        api.put<ApiResponse<boolean>>(`/guardians/${id}/end`, data).then(r => r.data),

    updateGuardian: (id: number, formData: FormData) =>
        api.put<ApiResponse<GuardianSummary>>(`/guardians/${id}`, formData, {
            headers: { "Content-Type": "multipart/form-data" }
        }).then(r => r.data),

    // الملاحظات
    getNotes: (fileId: number) =>
        api.get<ApiResponse<NoteItem[]>>(`/file-notes/by-file/${fileId}`).then(r => r.data.data),

    createNote: (data: { fileID: number; content: string }) =>
        api.post<ApiResponse<number>>("/file-notes", data).then(r => r.data),

    deleteNote: (id: number) =>
        api.delete<ApiResponse<boolean>>(`/file-notes/${id}`).then(r => r.data),

    // الوثائق والمستندات — delegate to documentsApi
    getDocuments: documentsApi.getByFileId,
    deleteDocument: (id: number) => documentsApi.delete(id).then(() => ({ isSuccess: true } as ApiResponse<boolean>)),
    getDocumentTypes: documentsApi.getTypes,

    getSocialStatuses: () =>
        api.get<ApiResponse<LookupDto[]>>("/lookups/social-statuses").then(r => r.data.data),

    getHealthStatuses: () =>
        api.get<ApiResponse<LookupDto[]>>("/lookups/health-statuses").then(r => r.data.data),

    // الوثائق الناقصة — delegate to documentsApi
    getMissingDocuments: documentsApi.getMissingByFileId,
    getAutoDetectedMissing: documentsApi.getAutoDetectedMissing,
    fulfillMissingDocument: (id: number) => documentsApi.fulfillMissing(id).then(() => ({ isSuccess: true } as ApiResponse<boolean>)),

    // الاستحقاقات المالية
    getFinancialEntitlements: (retireeId: number) =>
        api.get<ApiResponse<FinancialEntitlement[]>>(`/financial-entitlements/by-retiree/${retireeId}`).then(r => r.data.data),

    // سجلات المعاشات
    getSalaryPayments: (fileId: number) =>
        api.get<ApiResponse<SalaryPaymentBatch[]>>(`/salary-batches/by-file/${fileId}`).then(r => r.data.data),

    // دفاتر الصرف
    getPaymentBooks: (fileId: number) =>
        api.get<ApiResponse<PaymentBookItem[]>>(`/payment-books/by-file/${fileId}`).then(r => r.data.data),

    createPaymentBook: (formData: FormData) =>
        api.post<ApiResponse<number>>("/payment-books", formData, {
            headers: { "Content-Type": "multipart/form-data" }
        }).then(r => r.data),

    updatePaymentBook: (id: number, data: unknown) =>
        api.put<ApiResponse<boolean>>(`/payment-books/${id}`, data).then(r => r.data),

    deletePaymentBook: (id: number) =>
        api.delete<ApiResponse<boolean>>(`/payment-books/${id}`).then(r => r.data),

    checkBookNumber: (bookNumber: string) =>
        api.get<ApiResponse<boolean>>("/payment-books/check-book-number", { params: { bookNumber } }).then(r => r.data.data),

    // سجل التغييرات
    getChangeLog: (fileId: number) =>
        api.get<ApiResponse<ChangeLogItem[]>>(`/retirement-files/${fileId}/change-log`).then(r => r.data.data),

    // التعهدات والإعفاءات
    getPledges: (fileId: number) =>
        api.get<ApiResponse<PledgeItem[]>>(`/document-pledges/by-file/${fileId}`).then(r => r.data.data),

    getPledge: (id: number) =>
        api.get<ApiResponse<PledgeItem>>(`/document-pledges/${id}`).then(r => r.data.data),

    getExemptions: (fileId: number) =>
        api.get<ApiResponse<ExemptionItem[]>>(`/document-exemptions/by-file/${fileId}`).then(r => r.data.data),

    getExemption: (id: number) =>
        api.get<ApiResponse<ExemptionItem>>(`/document-exemptions/${id}`).then(r => r.data.data),

    createPledge: (data: CreatePledgeDto) =>
        api.post<ApiResponse<PledgeItem>>("/document-pledges", data).then(r => r.data),

    updatePledge: (id: number, data: CreatePledgeDto) =>
        api.put<ApiResponse<boolean>>(`/document-pledges/${id}`, data).then(r => r.data),

    deletePledge: (id: number) =>
        api.delete<ApiResponse<boolean>>(`/document-pledges/${id}`).then(r => r.data),

    fulfillPledge: (id: number) =>
        api.put<ApiResponse<boolean>>(`/document-pledges/${id}/fulfill`).then(r => r.data),

    createExemption: (data: CreateExemptionDto) =>
        api.post<ApiResponse<number>>("/document-exemptions", data).then(r => r.data),

    updateExemption: (id: number, data: CreateExemptionDto) =>
        api.put<ApiResponse<boolean>>(`/document-exemptions/${id}`, data).then(r => r.data),

    deleteExemption: (id: number) =>
        api.delete<ApiResponse<boolean>>(`/document-exemptions/${id}`).then(r => r.data),

    createMissingDocument: (data: CreateMissingDocumentDto) =>
        documentsApi.createMissing(data).then((id) => ({ data: { missingDocumentID: id }, isSuccess: true } as ApiResponse<{ missingDocumentID: number }>)),

    // الحالات والمراجعة
    getStatusTransitions: (fileId: number) =>
        api.get<ApiResponse<StatusTransition[]>>(`/retirement-files/${fileId}/status-transitions`).then(r => r.data.data),

    updateFileStatus: (fileId: number, statusId: number, reason?: string) =>
        api.put<ApiResponse<boolean>>(`/retirement-files/${fileId}/status`, null, {
            params: { newStatusId: statusId, reason },
        }).then(r => r.data),

    handleDeath: (fileId: number, deathDate: string) =>
        api.post<ApiResponse<boolean>>(`/retirement-files/${fileId}/death`, null, {
            params: { deathDate },
        }).then(r => r.data),

    correctLifeStatus: (fileId: number, data: { reason: string; healthStatusID: number }) =>
        api.post<ApiResponse<boolean>>(`/retirement-files/${fileId}/correct-life-status`, data).then(r => r.data),

    getSettlements: (retireeId: number) =>
        api.get<ApiResponse<SettlementItemDto[]>>(`/settlements/by-retiree/${retireeId}`).then(r => r.data.data),

    // ═══════════════════════════════════════════════════════
    // FILE REVIEW (مراجعة التعديلات)
    // ═══════════════════════════════════════════════════════

    getPendingReviews: () =>
        api.get<ApiResponse<PendingReviewFile[]>>("/retirement-files/pending-review").then(r => r.data.data),

    getReviewDiff: (fileId: number) =>
        api.get<ApiResponse<FileReviewDiff>>(`/retirement-files/${fileId}/review-diff`).then(r => r.data.data),

    approveReview: (fileId: number, notes?: string) =>
        api.post<ApiResponse<boolean>>(`/retirement-files/${fileId}/review/approve`, null, {
            params: { notes }
        }).then(r => r.data),

    rejectReview: (fileId: number, reason: string) =>
        api.post<ApiResponse<boolean>>(`/retirement-files/${fileId}/review/reject`, null, {
            params: { reason }
        }).then(r => r.data),

    reinstateEntitlement: (fileId: number) =>
        api.post<ApiResponse<boolean>>("/retirement-files/" + fileId + "/reinstate-entitlement").then(r => r.data),

    requestReview: (fileId: number) =>
        api.post<ApiResponse<boolean>>(`/retirement-files/${fileId}/request-review`).then(r => r.data),

    refreshAfterUpload: (fileId: number, documentIds: number[]) =>
        api.post<ApiResponse<{
            processedDocuments: number;
            skippedDocumentIds: number[];
            fulfilledMissingCount: number;
            missingDocumentsCount: number;
            reviewRequested: boolean;
        }>>(`/retirement-files/${fileId}/refresh-after-upload`, { documentIds }).then(r => r.data),

    approveReviewItem: (itemId: number, notes?: string) =>
        api.post<ApiResponse<boolean>>(`/retirement-files/review-items/${itemId}/approve`, null, {
            params: { notes }
        }).then(r => r.data),

    rejectReviewItem: (itemId: number, reason: string) =>
        api.post<ApiResponse<boolean>>(`/retirement-files/review-items/${itemId}/reject`, null, {
            params: { reason }
        }).then(r => r.data),

    acknowledgeEntityRejection: (entityType: string, entityId: number) =>
        api.post<ApiResponse<boolean>>(`/filereview/entities/${entityType}/${entityId}/acknowledge-rejection`).then(r => r.data),

    // ═══════════════════════════════════════════════════════
    // REPORTS
    // ═══════════════════════════════════════════════════════

    getReport: (params: RetirementFileReportFilter) =>
        api.get<ApiResponse<{ items: RetirementFileReportRow[]; totalCount: number; page: number; pageSize: number; totalPages: number }>>("/retirement-files/report", { params }).then(r => r.data.data),

    getReportSummary: (params: RetirementFileReportFilter) =>
        api.get<ApiResponse<RetirementFileReportSummary>>("/retirement-files/report/summary", { params }).then(r => r.data.data),

    exportReport: (params: RetirementFileReportFilter, format: "excel" | "pdf") =>
        api.get("/retirement-files/report/export", {
            params: { ...params, format },
            responseType: "blob",
        }),
};
