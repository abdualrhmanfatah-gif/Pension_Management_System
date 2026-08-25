import api from "./axios";
import type { ApiResponse } from "@/types/api";

export interface SalaryRecipientDto {
    recipientID: number;
    fileID: number;
    fileNumber: string;
    retireeID?: number;
    agentID?: number;
    entitledID?: number;
    guardianID?: number;
    recipientName: string;
    recipientType: string; // "متقاعد" | "وكيل" | "مستحق" | "وصي"
    recipientPhotoPath?: string | null;
    startDate: string;
    endDate?: string | null;
    isActive: boolean;
    notes?: string | null;
    createdAt: string;
    updatedAt?: string | null;
    pendingReviewStatus?: number;
}

export interface CreateSalaryRecipientDto {
    fileID: number;
    retireeID?: number;
    agentID?: number;
    entitledID?: number;
    guardianID?: number;
    startDate: string;
    notes?: string;
    personalPhoto?: File;
    nationalIdImage?: File;
    // Data completion
    nationalID?: string;
    birthDate?: string;
    socialStatusID?: number;
    healthStatusID?: number;
}

export interface EndSalaryRecipientDto {
    endDate: string;
    notes?: string;
}

export const salaryRecipientApi = {
    getByFile: (fileId: number) => 
        api.get<ApiResponse<SalaryRecipientDto[]>>(`/salary-recipients/by-file/${fileId}`).then(r => r.data),
    
    getById: (id: number) => 
        api.get<ApiResponse<SalaryRecipientDto>>(`/salary-recipients/${id}`).then(r => r.data),
    
    create: (data: CreateSalaryRecipientDto) => {
        const formData = new FormData();
        formData.append("fileID", data.fileID.toString());
        formData.append("startDate", data.startDate + "T00:00:00.000Z");
        if (data.retireeID) formData.append("retireeID", data.retireeID.toString());
        if (data.agentID) formData.append("agentID", data.agentID.toString());
        if (data.entitledID) formData.append("entitledID", data.entitledID.toString());
        if (data.guardianID) formData.append("guardianID", data.guardianID.toString());
        if (data.notes) formData.append("notes", data.notes);
        if (data.personalPhoto) formData.append("personalPhoto", data.personalPhoto);
        if (data.nationalIdImage) formData.append("nationalIdImage", data.nationalIdImage);
        
        // Append data completion fields
        if (data.nationalID) formData.append("nationalID", data.nationalID);
        if (data.birthDate) formData.append("birthDate", data.birthDate + "T00:00:00.000Z");
        if (data.socialStatusID) formData.append("socialStatusID", data.socialStatusID.toString());
        if (data.healthStatusID) formData.append("healthStatusID", data.healthStatusID.toString());

        return api.post<ApiResponse<SalaryRecipientDto>>("/salary-recipients", formData, {
            headers: { "Content-Type": "multipart/form-data" }
        }).then(r => r.data);
    },
    
    end: (id: number, data: EndSalaryRecipientDto) => 
        api.put<ApiResponse<boolean>>(`/salary-recipients/${id}/end`, data).then(r => r.data),

    updatePhoto: (id: number, photo: File) => {
        const formData = new FormData();
        formData.append("photo", photo);
        return api.post<ApiResponse<string>>(`/salary-recipients/${id}/photo`, formData, {
            headers: { "Content-Type": "multipart/form-data" }
        }).then(r => r.data);
    },
};
