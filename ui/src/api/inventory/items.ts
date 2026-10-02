import api from "../axios";

export interface ItemDto {
    inventoryItemID: number;
    itemName: string;
    itemCode: string;
    description?: string;
    unit: string;
    quantity: number;
    minimumQuantity: number;
    location?: string;
    condition: number;
    status: number;
    serialNumber?: string;
    notes?: string;
    createdAt: string;
    updatedAt?: string;
}

export const itemApi = {
    getAll: () =>
        api.get<{ data: ItemDto[] }>("/inventoryitems").then(r => r.data.data),

    getById: (id: number) =>
        api.get(`/inventoryitems/${id}`).then(r => r.data.data),

    create: (data: unknown) =>
        api.post("/inventoryitems", data).then(r => r.data),

    update: (id: number, data: unknown) =>
        api.put(`/inventoryitems/${id}`, data).then(r => r.data),

    delete: (id: number) =>
        api.delete(`/inventoryitems/${id}`).then(r => r.data),
};
