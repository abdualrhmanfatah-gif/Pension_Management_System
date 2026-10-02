import api from "./axios";

export interface BranchDto {
    pensionBranchID: number;
    branchName: string;
    governorate: string | null;
    address: string | null;
    phone: string | null;
    managerName: string | null;
    managerPhone: string | null;
    isActive: boolean;
    usersCount: number;
    createdAt: string;
}

export const branchesApi = {
    getAll: () =>
        api.get<{ data: BranchDto[] }>("/branches").then(r => r.data.data || []),

    create: (data: any) =>
        api.post("/branches", data),

    update: (id: number, data: any) =>
        api.put(`/branches/${id}`, data),

    delete: (id: number) =>
        api.delete(`/branches/${id}`),

    toggleActive: (id: number) =>
        api.patch(`/branches/${id}/toggle-active`),
};
