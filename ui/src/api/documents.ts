import api from "./axios";
import type {
    DocumentItem,
    MissingDocumentInfo,
    DocumentTypeLookup,
} from "@/types/retirement";
import type { ApiResponse } from "@/types/api";

export const documentsApi = {
    getByFileId(fileId: number): Promise<DocumentItem[]> {
        return api
            .get<ApiResponse<DocumentItem[]>>(`/documents/by-file/${fileId}`)
            .then((r) => r.data.data);
    },

    getById(id: number): Promise<DocumentItem> {
        return api
            .get<ApiResponse<DocumentItem>>(`/documents/${id}`)
            .then((r) => r.data.data);
    },

    delete(id: number): Promise<boolean> {
        return api
            .delete<ApiResponse<boolean>>(`/documents/${id}`)
            .then((r) => r.data.data);
    },

    upload(formData: FormData): Promise<DocumentItem> {
        return api
            .post<ApiResponse<DocumentItem>>("/documents", formData, {
                headers: { "Content-Type": "multipart/form-data" },
                timeout: 300000,
            })
            .then((r) => r.data.data);
    },

    update(id: number, formData: FormData): Promise<DocumentItem> {
        return api
            .put<ApiResponse<DocumentItem>>(`/documents/${id}`, formData, {
                headers: { "Content-Type": "multipart/form-data" },
            })
            .then((r) => r.data.data);
    },

    review(
        id: number,
        dto: {
            pendingReviewStatus: number;
            notes?: string;
            rejectionReason?: string;
        }
    ): Promise<boolean> {
        return api
            .put<ApiResponse<boolean>>(`/documents/${id}/review`, dto)
            .then((r) => r.data.data);
    },

    getTypes(): Promise<DocumentTypeLookup[]> {
        return api
            .get<ApiResponse<DocumentTypeLookup[]>>("/lookups/document-types")
            .then((r) => r.data.data);
    },

    getTypesByOwner(ownerType: string): Promise<DocumentTypeLookup[]> {
        return api
            .get<ApiResponse<{ documentTypeID: number; typeName: string }[]>>(
                `/document-types/by-owner/${ownerType}`
            )
            .then((r) =>
                r.data.data.map((t) => ({ id: t.documentTypeID, name: t.typeName }))
            );
    },

    getMissingByFileId(fileId: number): Promise<MissingDocumentInfo[]> {
        return api
            .get<ApiResponse<MissingDocumentInfo[]>>(
                `/missing-documents/by-file/${fileId}`
            )
            .then((r) => r.data.data);
    },

    getAutoDetectedMissing(fileId: number): Promise<MissingDocumentInfo[]> {
        return api
            .get<ApiResponse<MissingDocumentInfo[]>>(
                `/missing-documents/auto-detected/by-file/${fileId}`
            )
            .then((r) => r.data.data);
    },

    fulfillMissing(id: number): Promise<boolean> {
        return api
            .put<ApiResponse<boolean>>(`/missing-documents/${id}/fulfill`)
            .then((r) => r.data.data);
    },

    createMissing(data: {
        fileID: number;
        documentTypeID: number;
        agentID?: number;
        guardianID?: number;
        entitledID?: number;
    }): Promise<number> {
        return api
            .post<ApiResponse<{ missingDocumentID: number }>>(
                "/missing-documents",
                data
            )
            .then((r) => r.data.data.missingDocumentID);
    },

    getArchiveStats(): Promise<{
        totalDocuments: number;
        verifiedDocuments: number;
        pendingReview: number;
        expiredDocuments: number;
        filesWithMissingDocuments: number;
        totalMissingDocuments: number;
        pendingMissingDocuments: number;
        pledgedMissingDocuments: number;
    }> {
        return api
            .get<ApiResponse<{
                totalDocuments: number;
                verifiedDocuments: number;
                pendingReview: number;
                expiredDocuments: number;
                filesWithMissingDocuments: number;
                totalMissingDocuments: number;
                pendingMissingDocuments: number;
                pledgedMissingDocuments: number;
            }>>("/documents/archive-stats")
            .then((r) => r.data.data);
    },

    getArchiveDocuments(filters: Record<string, unknown>): Promise<{
        items: DocumentItem[];
        totalCount: number;
        page: number;
        pageSize: number;
    }> {
        return api
            .get<ApiResponse<{
                items: DocumentItem[];
                totalCount: number;
                page: number;
                pageSize: number;
            }>>("/documents", { params: filters })
            .then((r) => r.data.data);
    },

    getReviewDocuments(filters: Record<string, unknown>): Promise<{
        items: DocumentItem[];
        totalCount: number;
        page: number;
        pageSize: number;
    }> {
        return api
            .get<ApiResponse<{
                items: DocumentItem[];
                totalCount: number;
                page: number;
                pageSize: number;
            }>>("/documents", { params: filters })
            .then((r) => r.data.data);
    },
};
