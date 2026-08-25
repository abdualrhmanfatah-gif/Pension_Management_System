import api from "./axios";

export interface RankDto {
    rankID: number;
    rankName: string;
    rankLevel: number;
    baseSalary: number;
    createdAt: string;
}

export const ranksApi = {
    getAll: () =>
        api.get<{ data: RankDto[] }>("/ranks").then(r => r.data.data || []),

    create: (data: any) =>
        api.post("/ranks", data),

    update: (id: number, data: any) =>
        api.put(`/ranks/${id}`, data),

    delete: (id: number) =>
        api.delete(`/ranks/${id}`),
};
