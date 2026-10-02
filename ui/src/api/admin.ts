import api from "./axios";

export interface SettingItem {
    settingID: number;
    settingKey: string;
    settingValue: string;
    description: string | null;
    category: string | null;
    updatedByUser: string | null;
    createdAt: string;
    updatedAt: string | null;
}

export const adminApi = {
    getCategories: () =>
        api.get<{ data: string[] }>("/admin/settings/categories").then(r => r.data.data || []),

    getSettings: (category?: string) => {
        const params = category ? `?category=${encodeURIComponent(category)}` : "";
        return api.get<{ data: SettingItem[] }>(`/admin/settings${params}`).then(r => r.data.data || []);
    },

    updateSetting: (id: number, value: string) =>
        api.put(`/admin/settings/${id}`, { value }),

    createSetting: (data: { key: string; value: string; description?: string; category?: string }) =>
        api.post("/admin/settings", data),

    deleteSetting: (id: number) =>
        api.delete(`/admin/settings/${id}`),
};
