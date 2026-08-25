import api from "./axios";

export interface BankDto {
    bankID: number;
    bankName: string;
    bankCode?: string;
    accountID?: number | null;
    accountCode?: string;
    accountName?: string;
    isActive: boolean;
    batchCount: number;
    createdAt: string;
}

export const bankApi = {
    getAll: () =>
        api.get<{ data: BankDto[] }>("/banks").then(r => r.data.data),

    getById: (id: number) =>
        api.get(`/banks/${id}`).then(r => r.data.data),

    create: (data: unknown) =>
        api.post("/banks", data).then(r => r.data),

    update: (id: number, data: unknown) =>
        api.put(`/banks/${id}`, data).then(r => r.data),

    delete: (id: number) =>
        api.delete(`/banks/${id}`).then(r => r.data),

    toggleActive: (id: number) =>
        api.patch(`/banks/${id}/toggle-active`).then(r => r.data),
};
