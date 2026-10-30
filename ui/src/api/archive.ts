<<<<<<< HEAD
import type { ApiResponse, PagedResult } from "@/types/api";
import type { ArchiveDocument, ArchiveFilters, DocumentReviewDto, DocumentReviewFilters, DocumentReviewItem } from "@/types/archive";

import api from "./axios";
=======
import api from "./axios";
import type { ArchiveDocument, ArchiveFilters, DocumentReviewDto, DocumentReviewFilters, DocumentReviewItem } from "@/types/archive";
import type { ApiResponse, PagedResult } from "@/types/api";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

const OWNER_TYPE_LABELS: Record<number, string> = {
    1: "ملفات التقاعد",
    2: "المتقاعدون",
    3: "المستحقون",
    4: "الوكلاء",
    5: "الأوصياء",
    6: "التعهدات",
    7: "الإعفاءات",
    8: "ملفات الملاحظات",
    9: "إيقاف المعاشات",
    10: "دفعات الرواتب",
    11: "كشوفات الإحاله",
    12: "قرارات التسوية",
    13: "دفعات التسويات",
    14: "التقارير المحفوظة",
    15: "مشاريع الاستثمار",
    16: "عقارات الاستثمار",
    17: "وحدات العقارات",
    18: "عقود التأجير",
    19: "المستأجرون",
    20: "المحافظ الاستثمارية",
    21: "طلبات الصيانة",
    22: "أوامر العمل",
    23: "المالية الاستثمارية",
    24: "الحوكمة الاستثمارية",
    25: "تقييم العقارات",
    26: "دراسات الجدوى",
    27: "إهلاك العقارات",
    28: "دفعات الإيجار",
    30: "المصروف التشغيلي",
    31: "الخطط الاستثمارية",
    32: "المعاملات الاستثمارية",
    33: "تقييم المخاطر",
    40: "الموظفون",
    41: "المراسلات",
    50: "طلبات الخدمة",
    51: "رسائل التذاكر",
    52: "حساب المواطن",
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mapToArchiveDocument(doc: any): ArchiveDocument {
    return {
        documentId: doc.documentID,
        fileName: doc.fileName,
        fileNumber: doc.fileNumber,
        documentTypeName: doc.documentTypeName,
        documentTypeId: doc.documentTypeID,
        ownerType: doc.ownerType,
        ownerTypeName: OWNER_TYPE_LABELS[doc.ownerType] || `قسم (${doc.ownerType})`,
        uploadedBy: doc.uploadedByUser,
        createdAt: doc.createdAt,
        expiryDate: doc.expiryDate,
        verified: doc.verified,
        pendingReviewStatus: doc.pendingReviewStatus,
        filePath: doc.filePath,
        fileSize: doc.fileSize,
        mimeType: doc.mimeType,
        notes: doc.notes,
    };
}

export const archiveApi = {
    getDocuments: (filters: ArchiveFilters) =>
        api.get<ApiResponse<PagedResult<ArchiveDocument>>>("/documents", { params: filters })
            .then((r) => {
                const data = r.data.data;
                return {
                    items: data.items.map(mapToArchiveDocument),
                    totalCount: data.totalCount,
                    page: data.page,
                    pageSize: data.pageSize,
                };
            }),

    getDocument: (id: number) =>
        api.get<ApiResponse<ArchiveDocument>>(`/documents/${id}`)
            .then((r) => mapToArchiveDocument(r.data.data)),

    getStats: () =>
        api.get<ApiResponse<{
            totalDocuments: number;
            verifiedDocuments: number;
            pendingReview: number;
            expiredDocuments: number;
        }>>("/documents/archive-stats")
            .then((r) => r.data.data),

    getDocumentTypes: () =>
        api.get<ApiResponse<{ documentTypeID: number; typeName: string }[]>>("/document-types")
            .then((r) =>
                r.data.data.map((t) => ({
                    documentTypeId: t.documentTypeID,
                    typeName: t.typeName,
                }))
            ),

    // ─── Document Review ──────────────────────────────────

    getReviewDocuments: (filters: DocumentReviewFilters) =>
        api.get<ApiResponse<PagedResult<DocumentReviewItem>>>("/documents", { params: filters })
            .then((r) => {
                const data = r.data.data;
                return {
                    items: data.items.map(mapToReviewDocument),
                    totalCount: data.totalCount,
                    page: data.page,
                    pageSize: data.pageSize,
                };
            }),

    getReviewStats: () =>
        api.get<ApiResponse<{
            totalDocuments: number;
            pendingReview: number;
            approvedDocuments: number;
            rejectedDocuments: number;
            expiredDocuments: number;
        }>>("/documents/archive-stats")
            .then((r) => r.data.data),

    reviewDocument: (documentId: number, dto: DocumentReviewDto) =>
        api.put<ApiResponse<boolean>>(`/documents/${documentId}/review`, {
            pendingReviewStatus: dto.reviewStatus,
            notes: dto.reviewNotes,
            rejectionReason: dto.rejectionReason,
        }).then((r) => r.data.isSuccess),
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mapToReviewDocument(doc: any): DocumentReviewItem {
    return {
        documentId: doc.documentID,
        fileName: doc.fileName,
        documentTypeName: doc.documentTypeName,
        documentTypeId: doc.documentTypeID,
        ownerType: doc.ownerType,
        ownerTypeName: OWNER_TYPE_LABELS[doc.ownerType] || `قسم (${doc.ownerType})`,
        uploadedBy: doc.uploadedByUser,
        createdAt: doc.createdAt,
        expiryDate: doc.expiryDate,
        verified: doc.verified,
        pendingReviewStatus: doc.pendingReviewStatus,
        filePath: doc.filePath,
        fileSize: doc.fileSize,
        mimeType: doc.mimeType,
        notes: doc.notes,
        retirementFileId: doc.fileID,
        fileNumber: doc.fileNumber,
        retireeName: doc.retireeName,
        entityName: doc.entityName,
        entityType: doc.entityType,
        documentNumber: doc.documentNumber,
        reviewStatus: doc.pendingReviewStatus ?? 0,
        reviewNotes: doc.reviewNotes,
        reviewedBy: doc.reviewedBy,
        reviewedAt: doc.reviewedAt,
        rejectionReason: doc.rejectionReason,
    };
}
