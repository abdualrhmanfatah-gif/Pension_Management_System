import type { ApiResponse, PagedResult } from "@/types/api";
import type { CreateSalaryBatchDto,SalaryBatch, SalaryBatchItem } from "@/types/salaries";

import api from "./axios";

export const salariesApi = {
    getAll: (params: { page: number; pageSize: number; status?: string }) => 
        api.get<ApiResponse<PagedResult<SalaryBatch>>>("/salary-batches", { params }).then(r => r.data.data),
    
    getById: (id: number) => 
        api.get<ApiResponse<SalaryBatch>>(`/salary-batches/${id}`).then(r => r.data.data),
    
    getItems: (batchId: number, params: { page: number; pageSize: number }) => 
        api.get<ApiResponse<PagedResult<SalaryBatchItem>>>(`/salary-batches/${batchId}/items`, { params }).then(r => r.data.data),
    
    create: (data: CreateSalaryBatchDto) => 
        api.post<ApiResponse<SalaryBatch>>("/salary-batches", data).then(r => r.data.data),
    
    approve: (id: number) => 
        api.put<ApiResponse<boolean>>(`/salary-batches/${id}/approve`).then(r => r.data),
    
    delete: (id: number) => 
        api.delete<ApiResponse<boolean>>(`/salary-batches/${id}`).then(r => r.data),
    
    generate: (id: number) => 
        api.post<ApiResponse<{ generatedCount: number; skippedCount: number; totalAmount: number; skippedItems: { fileNumber: string; retireeFullName: string; reason: string }[] }>>(`/salary-batches/${id}/generate`).then(r => r.data),
    
    exportItems: (id: number) => 
        api.get(`/salary-batches/${id}/export`, { responseType: "blob" }),
    
    importResults: (id: number, file: File) => {
        const formData = new FormData();
        formData.append("file", file);
        return api.post<ApiResponse<{ updatedCount: number; skippedCount: number; errors: string[] }>>(`/salary-batches/${id}/import`, formData, {
            headers: { "Content-Type": "multipart/form-data" },
        }).then(r => r.data);
    },
};
