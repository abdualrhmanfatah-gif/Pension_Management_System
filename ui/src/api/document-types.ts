import api from "./axios";

export interface DocumentTypeDto {
    documentTypeID: number;
    typeName: string;
    ownerType: number | null;
    isRequired: boolean;
    hasExpiry: boolean;
    maxFileSize: number | null;
    allowedExtensions: string | null;
    isActive: boolean;
}

export const documentTypesApi = {
    getAll: () =>
        api.get<{ data: DocumentTypeDto[] }>("/document-types").then(r => r.data.data || []),

    create: (data: any) =>
        api.post("/document-types", data),

    update: (id: number, data: any) =>
        api.put(`/document-types/${id}`, data),

    delete: (id: number) =>
        api.delete(`/document-types/${id}`),
};
