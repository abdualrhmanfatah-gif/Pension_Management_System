import type { ApiResponse, PagedResult } from "@/types/api";

export type { ApiResponse, PagedResult };

export interface ArchiveDocument {
    documentId: number;
    fileName: string;
    fileNumber?: string;
    documentTypeName: string;
    documentTypeId: number;
    ownerType: number;
    ownerTypeName: string;
    uploadedBy: string;
    createdAt: string;
    expiryDate?: string;
    verified: boolean;
    pendingReviewStatus: number;
    filePath: string;
    fileSize?: number;
    mimeType?: string;
    notes?: string;
}

export interface ArchiveFilters {
    ownerType?: number;
    documentTypeId?: number;
    dateFrom?: string;
    dateTo?: string;
    verified?: boolean;
    searchTerm?: string;
    page: number;
    pageSize: number;
}

export interface ArchiveStats {
    totalDocuments: number;
    verifiedDocuments: number;
    pendingReview: number;
    expiredDocuments: number;
    filesWithMissingDocuments: number;
    totalMissingDocuments: number;
    pendingMissingDocuments: number;
    pledgedMissingDocuments: number;
}

export const OWNER_TYPES = {
    RETIREMENT_FILE: 1,
    RETIREE: 2,
    ENTITLED_PERSON: 3,
    AGENT: 4,
    GUARDIAN: 5,
    DOCUMENT_PLEDGE: 6,
    DOCUMENT_EXEMPTION: 7,
    FILE_NOTE: 8,
    PENSION_SUSPENSION: 9,
    SALARY_PAYMENT_BATCH: 10,
    MINISTRY_APPROVAL_BATCH: 11,
    SETTLEMENT_DECISION: 12,
    SETTLEMENTS_BATCH: 13,
    SAVED_REPORT: 14,
    INVESTMENT_PROJECT: 15,
    INVESTMENT_PROPERTY: 16,
    PROPERTY_UNIT: 17,
    LEASE_CONTRACT: 18,
    TENANT: 19,
    INVESTMENT_PORTFOLIO: 20,
    MAINTENANCE_REQUEST: 21,
    WORK_ORDER: 22,
    INVESTMENT_FINANCE: 23,
    INVESTMENT_GOVERNANCE: 24,
    PROPERTY_VALUATION: 25,
    FEASIBILITY_STUDY: 26,
    PROPERTY_DEPRECIATION: 27,
    LEASE_PAYMENT: 28,
    OPERATIONAL_EXPENSE: 30,
    INVESTMENT_PLAN: 31,
    INVESTMENT_TXN: 32,
    RISK_ASSESSMENT: 33,
    EMPLOYEE: 40,
    CORRESPONDENCE: 41,
    SERVICE_REQUEST: 50,
    TICKET_MESSAGE: 51,
    CITIZEN_ACCOUNT: 52,
} as const;

export const PENDING_REVIEW_STATUS = {
    NONE: 0,
    PENDING: 1,
    APPROVED: 2,
    REJECTED: 3,
} as const;

export const OWNER_TYPE_LABELS: Record<number, string> = {
    [OWNER_TYPES.RETIREMENT_FILE]: "ملفات التقاعد",
    [OWNER_TYPES.RETIREE]: "المتقاعدون",
    [OWNER_TYPES.ENTITLED_PERSON]: "المستحقون",
    [OWNER_TYPES.AGENT]: "الوكلاء",
    [OWNER_TYPES.GUARDIAN]: "الأوصياء",
    [OWNER_TYPES.DOCUMENT_PLEDGE]: "التعهدات",
    [OWNER_TYPES.DOCUMENT_EXEMPTION]: "الإعفاءات",
    [OWNER_TYPES.INVESTMENT_PROJECT]: "مشاريع الاستثمار",
    [OWNER_TYPES.INVESTMENT_PROPERTY]: "عقارات الاستثمار",
    [OWNER_TYPES.EMPLOYEE]: "الموظفون",
    [OWNER_TYPES.CORRESPONDENCE]: "المراسلات",
};

// ─── Document Review Types ──────────────────────────────

export interface DocumentReviewItem {
    documentId: number;
    fileName: string;
    documentTypeName: string;
    documentTypeId: number;
    ownerType: number;
    ownerTypeName: string;
    uploadedBy: string;
    createdAt: string;
    expiryDate?: string;
    verified: boolean;
    pendingReviewStatus: number;
    filePath: string;
    fileSize?: number;
    mimeType?: string;
    notes?: string;
    retirementFileId?: number;
    fileNumber?: string;
    retireeName?: string;
    entityName?: string;
    entityType?: string;
    documentNumber?: string;
    reviewStatus: number;
    reviewNotes?: string;
    reviewedBy?: string;
    reviewedAt?: string;
    rejectionReason?: string;
}

export interface DocumentReviewFilters {
    ownerType?: number;
    documentTypeId?: number;
    reviewStatus?: number;
    branchId?: number;
    dateFrom?: string;
    dateTo?: string;
    searchTerm?: string;
    page: number;
    pageSize: number;
}

export interface DocumentReviewStats {
    totalDocuments: number;
    pendingReview: number;
    approvedDocuments: number;
    rejectedDocuments: number;
    expiredDocuments: number;
}

export interface DocumentReviewDto {
    reviewStatus: number;
    reviewNotes: string;
    rejectionReason?: string;
}

export const REVIEW_STATUS = {
    NONE: 0,
    PENDING: 1,
    APPROVED: 2,
    REJECTED: 3,
} as const;

export const REVIEW_STATUS_LABELS: Record<number, string> = {
    [REVIEW_STATUS.NONE]: "بدون فحص",
    [REVIEW_STATUS.PENDING]: "بانتظار الفحص",
    [REVIEW_STATUS.APPROVED]: "معتمدة",
    [REVIEW_STATUS.REJECTED]: "مرفوضة",
};