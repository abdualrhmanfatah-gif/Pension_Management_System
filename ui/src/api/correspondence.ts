import api from "@/api/axios";
import type { HrDocumentDto } from "@/api/hr";
import type { ApiResponse } from "@/types/api";
import type { 
  CorrespondenceDetailsResponse,
  CorrespondenceListResponse,
  CorrespondenceTypeListResponse,
  CreateCorrespondenceDto, 
  CreateExternalPartyDto, 
  ExternalPartyListResponse,
  ExternalPartyResponse,
  UpdateCorrespondenceDto, 
  UpdateExternalPartyDto} from "@/types/correspondence";

/**
 * خدمات سجل الصادر والوارد (Correspondence Register)
 */
export const correspondenceApi = {
  // ─── Correspondences ─────────────────────────────────────
  
  getAll: (params: { direction?: number; typeId?: number; page?: number; pageSize?: number; search?: string }) =>
    api.get<CorrespondenceListResponse>('/Correspondence', { params }).then(r => r.data),
    
  getById: (id: number) =>
    api.get<CorrespondenceDetailsResponse>(`/Correspondence/${id}`).then(r => r.data),
    
  create: (data: CreateCorrespondenceDto) =>
    api.post<CorrespondenceDetailsResponse>('/Correspondence', data).then(r => r.data),
    
  update: (id: number, data: UpdateCorrespondenceDto) =>
    api.put<CorrespondenceDetailsResponse>(`/Correspondence/${id}`, data).then(r => r.data),
    
  delete: (id: number) =>
    api.delete<ApiResponse<boolean>>(`/Correspondence/${id}`).then(r => r.data),
  
  // ─── External Parties ────────────────────────────────────
  
  getExternalParties: () =>
    api.get<ExternalPartyListResponse>('/Correspondence/external-parties').then(r => r.data),
    
  createExternalParty: (data: CreateExternalPartyDto) =>
    api.post<ExternalPartyResponse>('/Correspondence/external-parties', data).then(r => r.data),
    
  updateExternalParty: (id: number, data: UpdateExternalPartyDto) =>
    api.put<ExternalPartyResponse>(`/Correspondence/external-parties/${id}`, data).then(r => r.data),
    
  deleteExternalParty: (id: number) =>
    api.delete<ApiResponse<boolean>>(`/Correspondence/external-parties/${id}`).then(r => r.data),
    
  // ─── Correspondence Types (Lookup) ──────────────────────
  
  getTypes: () =>
    api.get<CorrespondenceTypeListResponse>('/Correspondence/types').then(r => r.data),

  // ─── Documents ──────────────────────────────────────────
  
  getDocuments: (correspondenceId: number) =>
    api.get<ApiResponse<HrDocumentDto[]>>(`/Correspondence/${correspondenceId}/documents`).then(r => r.data),
    
  uploadDocument: (correspondenceId: number, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post<ApiResponse<HrDocumentDto>>(`/Correspondence/${correspondenceId}/documents`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    }).then(r => r.data);
  },
  
  deleteDocument: (correspondenceId: number, documentId: number) =>
    api.delete<ApiResponse<boolean>>(`/Correspondence/${correspondenceId}/documents/${documentId}`).then(r => r.data),
};
