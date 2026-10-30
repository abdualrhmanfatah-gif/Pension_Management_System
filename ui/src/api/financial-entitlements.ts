<<<<<<< HEAD
import type { ApiResponse } from "@/types/api";

import api from "./axios";
=======
import api from "./axios";
import type { ApiResponse } from "@/types/api";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export interface FinancialEntitlementDto {
    entitlementID: number;
    retireeID: number;
    retireeFullName: string | null;
    amount: number;
    effectiveDate: string;
    endDate: string | null;
    isActive: boolean;
    notes: string | null;
    createdAt: string;
    fileID: number | null;
    fileNumber: string | null;
    militaryNumber: string | null;
    phone: string | null;
    agentName: string | null;
}

export interface CreateFinancialEntitlementDto {
    retireeID: number;
    amount: number;
    effectiveDate: string;
    notes?: string;
}

export interface EndEntitlementRequest {
    endDate: string;
}

export const financialEntitlementsApi = {
    getByRetiree: (retireeId: number) => 
        api.get<ApiResponse<FinancialEntitlementDto[]>>(`/financial-entitlements/by-retiree/${retireeId}`).then(r => r.data),
    
    search: (query: string) => 
        api.get<ApiResponse<FinancialEntitlementDto[]>>("/financial-entitlements/search", { params: { q: query } }).then(r => r.data),
    
    getActive: (retireeId: number) => 
        api.get<ApiResponse<FinancialEntitlementDto>>(`/financial-entitlements/active/${retireeId}`).then(r => r.data),
    
    getById: (id: number) => 
        api.get<ApiResponse<FinancialEntitlementDto>>(`/financial-entitlements/${id}`).then(r => r.data),
    
    create: (data: CreateFinancialEntitlementDto) => 
        api.post<ApiResponse<FinancialEntitlementDto>>("/financial-entitlements", data).then(r => r.data),
    
    end: (id: number, request: EndEntitlementRequest) => 
        api.put<ApiResponse<boolean>>(`/financial-entitlements/${id}/end`, request).then(r => r.data),
};
