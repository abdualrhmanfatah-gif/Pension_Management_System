import api from "../axios";

export interface CustodyDto {
    inventoryCustodyID: number;
    assetID: number;
    assetName: string;
    assetNumber: string;
    employeeID: number;
    employeeName: string;
    quantity: number;
    assignedDate: string;
    returnedDate?: string;
    status: number;
    assignedByName?: string;
    returnedToName?: string;
    notes?: string;
    createdAt: string;
}

export const custodyApi = {
    getAll: () =>
        api.get<{ data: CustodyDto[] }>("/inventorycustody").then(r => r.data.data),

    getActive: () =>
        api.get<{ data: CustodyDto[] }>("/inventorycustody/active").then(r => r.data.data),

    getByEmployee: (employeeId: number) =>
        api.get(`/inventorycustody/by-employee/${employeeId}`).then(r => r.data.data),

    getById: (id: number) =>
        api.get(`/inventorycustody/${id}`).then(r => r.data.data),

    create: (data: unknown) =>
        api.post("/inventorycustody", data).then(r => r.data),

    return: (id: number, data: unknown) =>
        api.put(`/inventorycustody/${id}/return`, data).then(r => r.data),

    delete: (id: number) =>
        api.delete(`/inventorycustody/${id}`).then(r => r.data),
};
