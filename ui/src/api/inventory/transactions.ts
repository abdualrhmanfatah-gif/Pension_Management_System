import api from "../axios";

export interface TransactionDto {
    inventoryTransactionID: number;
    inventoryItemID: number;
    itemName: string;
    itemCode: string;
    transactionType: number;
    quantity: number;
    previousQuantity: number;
    newQuantity: number;
    invoiceNumber?: string;
    referenceNumber?: string;
    reason?: string;
    unitCost?: number;
    currency: string;
    withdrawalOrderNumber?: string;
    transactionDate: string;
    performedByName?: string;
    batchID?: string;
    createdAt: string;
}

export interface BulkTransactionItemDto {
    inventoryItemID: number;
    quantity: number;
    unitCost?: number;
}

export interface CreateBulkTransactionDto {
    transactionType: number;
    unitCost?: number;
    currency?: string;
    withdrawalOrderNumber?: string;
    invoiceNumber?: string;
    referenceNumber?: string;
    reason?: string;
    transactionDate: string;
    items: BulkTransactionItemDto[];
}

export const transactionApi = {
    getAll: () =>
        api.get<{ data: TransactionDto[] }>("/inventorytransactions").then(r => r.data.data),

    getByItem: (itemId: number) =>
        api.get<{ data: TransactionDto[] }>(`/inventorytransactions/by-item/${itemId}`).then(r => r.data.data),

    getByWithdrawalOrder: (orderNumber: string) =>
        api.get<{ data: TransactionDto[] }>(`/inventorytransactions/by-withdrawal-order/${encodeURIComponent(orderNumber)}`).then(r => r.data.data),

    getById: (id: number) =>
        api.get(`/inventorytransactions/${id}`).then(r => r.data.data),

    create: (data: unknown) =>
        api.post("/inventorytransactions", data).then(r => r.data),

    createBulk: (data: CreateBulkTransactionDto) =>
        api.post("/inventorytransactions/bulk", data).then(r => r.data),
};
