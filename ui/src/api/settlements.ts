import api from "./axios";
import type { PagedResult } from "@/types/api";

export interface SettlementDecisionDto {
    sourceRowsCount: number;
    hasFailureReport: boolean;
    decisionID: number;
    batchID: number;
    decisionNumber: string;
    decisionDate: string;
    decisionType: string | null;
    notes: string | null;
    itemCount: number;
    totalAmount: number;
    createdByUserName: string | null;
    createdAt: string;
}

export interface SettlementItemDto {
    settlementItemID: number;
    serialNumber: number;
    militaryNumber: string;
    retireeFullName: string;
    retireeID: number | null;
    currentRankName: string;
    processedRankName: string;
    processedRankSalary: number;
    decisionNumber: string | null;
    decisionID: number | null;
    notes: string | null;
    createdAt: string;
}

export interface LinkSettlementItemDto {
    settlementItemID: number;
    retireeID: number;
}

export interface ImportSettlementExcelDto {
    file: File;
    decisionNumber: string;
    decisionDate?: string;
    decisionType?: string;
    notes?: string;
}

export const settlementApi = {
    getAllDecisions: (params?: { page?: number; pageSize?: number; searchTerm?: string }) =>
        api.get<{ data: PagedResult<SettlementDecisionDto> }>("/settlements/decisions", { params }).then((res) => res.data.data),

    getDecision: (id: number) =>
        api.get<{ data: SettlementDecisionDto }>(`/settlements/decisions/${id}`).then((res) => res.data),

    getDecisionItems: (id: number, params?: { page?: number; pageSize?: number; searchTerm?: string }) =>
        api.get<{ data: PagedResult<SettlementItemDto> }>(`/settlements/decisions/${id}/items`, { params }).then((res) => res.data.data),

    linkItem: (data: LinkSettlementItemDto) =>
        api.put<{ data: boolean }>("/settlements/items/link", data).then((res) => res.data),

    deleteDecision: (id: number) =>
        api.delete<{ data: boolean }>(`/settlements/decisions/${id}`).then((res) => res.data),

    exportDecision: (id: number) =>
        api.get(`/settlements/decisions/${id}/export`, { responseType: "blob" }),

    getFailureReport: (id: number) =>
        api.get(`/settlements/decisions/${id}/failures-report`, { responseType: "blob" }),

    exportFailures: (id: number, file: File) => {
        const formData = new FormData();
        formData.append("file", file);
        return api.post(`/settlements/decisions/${id}/export-failures`, formData, {
            responseType: "blob",
            headers: { "Content-Type": "multipart/form-data" },
        });
    },

    uploadDecision: (formData: FormData) =>
        api.post("/settlements/decisions/upload", formData, {
            headers: { "Content-Type": "multipart/form-data" },
        }).then(r => r.data),

    exportFailuresForFile: (file: File) => {
        const formData = new FormData();
        formData.append("file", file);
        return api.post("/settlements/decisions/export-failures", formData, {
            responseType: "blob",
            headers: { "Content-Type": "multipart/form-data" },
        });
    },

    importCorrection: (batchId: number, data: ImportSettlementExcelDto) => {
        const formData = new FormData();
        formData.append("file", data.file);
        formData.append("decisionNumber", data.decisionNumber);
        if (data.decisionDate) formData.append("decisionDate", data.decisionDate);
        if (data.decisionType) formData.append("decisionType", data.decisionType);
        if (data.notes) formData.append("notes", data.notes);

        return api.post<{ data: number }>(`/settlements/batches/${batchId}/import`, formData, {
            headers: { "Content-Type": "multipart/form-data" },
        }).then((res) => res.data);
    },
};
