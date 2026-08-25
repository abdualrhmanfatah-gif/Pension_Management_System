import api from "./axios";
import type { PagedResult } from "@/types/api";

export interface RetireeListItem {
    retireeID: number;
    nationalID: string;
    fullName: string;
    militaryNumber: string | null;
    linkNumber: string | null;
    phone: string | null;
    isAlive: boolean;
    fileCount: number;
    fileID: number | null;
    governorate?: string | null;
    rankId?: number | null;
    rankName?: string | null;
    categoryId?: number | null;
    gender?: number;
    pendingReviewStatus?: number;
}

export interface RetireeFilters {
    search?: string;
    categoryId?: number;
    rankId?: number;
    gender?: number;
    governorate?: string;
}

export const retireeApi = {
    getAll: (params: { page: number; pageSize: number } & RetireeFilters) =>
        api.get<{ data: PagedResult<RetireeListItem> }>("/retirees", { params }).then(r => r.data.data),

    getById: (id: number) =>
        api.get(`/retirees/${id}`).then(r => r.data.data),

    getStatement: (id: number) =>
        api.get(`/retirees/${id}/statement`).then(r => r.data.data),

    update: (id: number, data: unknown) =>
        api.put(`/retirees/${id}`, data).then(r => r.data),

    delete: (id: number) =>
        api.delete(`/retirees/${id}`).then(r => r.data),

    uploadPhoto: (id: number, file: File) => {
        const formData = new FormData();
        formData.append("photo", file);
        return api.post(`/retirees/${id}/photo`, formData, {
            headers: { "Content-Type": "multipart/form-data" }
        }).then(r => r.data);
    },
};
