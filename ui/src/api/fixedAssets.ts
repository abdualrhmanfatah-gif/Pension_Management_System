import type { ApiResponse } from "@/types/api";

import api from "./axios";

// ═══════════════════════════════════════
// Types
// ═══════════════════════════════════════

export interface AssetTypeDto {
    assetTypeID: number;
    typeName: string;
    typeCode: string;
    description?: string;
    isActive: boolean;
    assetCount: number;
}

export interface AssetCategoryDto {
    categoryID: number;
    categoryName: string;
    categoryCode?: string;
    description?: string;
    assetTypeID: number;
    assetTypeName?: string;
    usefulLifeMonths: number;
    depreciationRate: number;
    salvagePercent: number;
    assetAccountID?: number;
    assetAccountName?: string;
    assetAccountCode?: string;
    accumDeprAccountID?: number;
    accumDeprAccountName?: string;
    deprExpenseAccountID?: number;
    deprExpenseAccountName?: string;
    isActive: boolean;
    assetCount: number;
}

export interface FixedAssetDto {
    assetID: number;
    assetNumber: string;
    assetName: string;
    description?: string;
    categoryID: number;
    categoryName?: string;
    assetTypeID: number;
    assetTypeName?: string;
    serialNumber?: string;
    purchaseDate: string;
    inServiceDate: string;
    unitPrice: number;
    acquiredCost: number;
    salvageValue: number;
    currencyCode: string;
    quantity: number;
    usefulLifeMonths: number;
    depreciationRate: number;
    depreciationMethod: number;
    remainingUsefulLifeMonths?: number | null;
    valuationMethod: number;
    valuationNotes?: string;
    accumulatedDepreciation: number;
    netBookValue: number;
    lastDepreciationDate?: string;
    location?: string;
    custodianEmployeeID?: number;
    custodianEmployeeName?: string;
    assetAccountID: number;
    assetAccountName?: string;
    assetAccountCode?: string;
    accumDeprAccountID: number;
    deprExpenseAccountID: number;
    status: number;
    disposalDate?: string;
    disposalReason?: string;
    createdAt: string;
    updatedAt?: string;
    itDetail?: AssetITDetailDto;
    vehicleDetail?: AssetVehicleDetailDto;
    buildingDetail?: AssetBuildingDetailDto;
    landDetail?: AssetLandDetailDto;
    furnitureDetail?: AssetFurnitureDetailDto;
}

export interface AssetITDetailDto {
    model?: string;
    manufacturer?: string;
    processorType?: string;
    processorSpeed?: string;
    ramSize?: string;
    hardDiskType?: string;
    hardDiskSize?: string;
    gpuModel?: string;
    screenSize?: string;
    screenResolution?: string;
    operatingSystem?: string;
    hasWebcam: boolean;
    hasWifi: boolean;
    hasBluetooth: boolean;
    ipAddress?: string;
    macAddress?: string;
    warrantyExpiry?: string;
    notes?: string;
}

export interface AssetVehicleDetailDto {
    plateNumber?: string;
    plateLetter1?: string;
    plateLetter2?: string;
    plateLetter3?: string;
    engineNumber?: string;
    chassisNumber?: string;
    transmissionType?: string;
    fuelType?: string;
    cylinderCount?: number;
    engineCapacity?: string;
    horsePower?: number;
    wheelCount: number;
    tireSize?: string;
    color?: string;
    modelYear?: number;
    registrationExpiry?: string;
    insuranceExpiry?: string;
    mileage: number;
    hasGps: boolean;
    notes?: string;
}

export interface AssetBuildingDetailDto {
    areaSqm?: number;
    builtAreaSqm?: number;
    floorCount: number;
    ownershipDocNumber?: string;
    licenseNumber?: string;
    notes?: string;
}

export interface AssetLandDetailDto {
    plotNumber?: string;
    landAreaSqm?: number;
    ownershipDocNumber?: string;
    ownershipDocDate?: string;
    notes?: string;
}

export interface AssetFurnitureDetailDto {
    material?: string;
    color?: string;
    notes?: string;
}

export interface DepreciationScheduleDto {
    scheduleID: number;
    assetID: number;
    assetNumber?: string;
    assetName?: string;
    periodYear: number;
    periodMonth: number;
    periodStartDate: string;
    periodEndDate: string;
    depreciationAmount: number;
    accumulatedDepreciation: number;
    netBookValue: number;
    isPosted: boolean;
    postedDate?: string;
    createdAt: string;
}

// ═══════════════════════════════════════
// Constants
// ═══════════════════════════════════════

export const VALUATION_METHODS: Record<number, string> = {
    0: "سعر الشراء",
    1: "القيمة العادلة",
    2: "القيمة المقدرة",
    3: "تكلفة الإعادة",
};

// ═══════════════════════════════════════
// API
// ═══════════════════════════════════════

export const fixedAssetApi = {
    getAll: () =>
        api.get<{ data: FixedAssetDto[] }>("/fixedassets").then(r => r.data.data),

    getById: (id: number) =>
        api.get(`/fixedassets/${id}`).then(r => r.data.data),

    getByType: (typeId: number) =>
        api.get(`/fixedassets/by-type/${typeId}`).then(r => r.data.data),

    getTypes: () =>
        api.get<{ data: AssetTypeDto[] }>("/fixedassets/types").then(r => r.data.data),

    getCategories: (typeId?: number) =>
        api.get<{ data: AssetCategoryDto[] }>("/fixedassets/categories", { params: { typeId } }).then(r => r.data.data),

    createCategory: (data: unknown) =>
        api.post("/fixedassets/categories", data).then(r => r.data),

    updateCategory: (id: number, data: unknown) =>
        api.put(`/fixedassets/categories/${id}`, data).then(r => r.data),

    deleteCategory: (id: number) =>
        api.delete(`/fixedassets/categories/${id}`).then(r => r.data),

    toggleCategoryActive: (id: number) =>
        api.patch(`/fixedassets/categories/${id}/toggle-active`).then(r => r.data),

    resetCategories: () =>
        api.post("/fixedassets/categories/reset").then(r => r.data),

    create: (data: unknown) =>
        api.post("/fixedassets", data).then(r => r.data),

    update: (id: number, data: unknown) =>
        api.put(`/fixedassets/${id}`, data).then(r => r.data),

    delete: (id: number) =>
        api.delete(`/fixedassets/${id}`).then(r => r.data),

    getSchedule: (id: number) =>
        api.get(`/fixedassets/${id}/schedule`).then(r => r.data.data),

    runYearlyDepreciation: (year: number): Promise<ApiResponse<boolean>> =>
        api.post(`/fixedassets/depreciation/yearly/run`, null, { params: { year } }).then(r => r.data),

    postYearlyDepreciation: (year: number): Promise<ApiResponse<boolean>> =>
        api.post(`/fixedassets/depreciation/yearly/post`, null, { params: { year } }).then(r => r.data),
};
