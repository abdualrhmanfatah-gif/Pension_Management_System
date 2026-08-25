import api from "../axios";

// ═══════════════════════════════════════
// Types
// ═══════════════════════════════════════

export interface AssetStocktakeDto {
  assetStocktakeID: number;
  fiscalYearID: number;
  yearName?: string;
  stocktakeDate: string;
  status: string;
  createdByName?: string;
  verifiedByName?: string;
  verifiedAt?: string;
  notes?: string;
  summary: AssetStocktakeSummaryDto;
  lines: AssetStocktakeLineDto[];
}

export interface AssetStocktakeLineDto {
  stocktakeLineID: number;
  fixedAssetID: number;
  assetNumber: string;
  assetName: string;
  categoryName?: string;
  location?: string;
  netBookValue: number;
  systemQuantity: number;
  actualLocation?: string;
  actualCondition: string;
  isPresent: boolean;
  recordedQuantity: number;
  notes?: string;
}

export interface AssetStocktakeSummaryDto {
  totalAssets: number;
  presentAssets: number;
  missingAssets: number;
  assetsInGoodCondition: number;
  assetsInFairCondition: number;
  assetsInPoorCondition: number;
  totalBookValue: number;
}

export interface SimpleStocktakeDto {
  simpleStocktakeID: number;
  fiscalYearID: number;
  yearName?: string;
  stocktakeDate: string;
  status: string;
  createdByName?: string;
  completedByName?: string;
  completedAt?: string;
  totalItems: number;
  itemsWithVariance: number;
  totalVarianceValue: number;
  journalEntryID?: number;
  journalEntryNumber?: string;
  lines: SimpleStocktakeLineDto[];
}

export interface SimpleStocktakeLineDto {
  lineID: number;
  inventoryItemID: number;
  itemName: string;
  itemCode: string;
  systemQuantity: number;
  physicalQuantity: number;
  variance: number;
  unitCost: number;
  varianceValue: number;
}

export interface FiscalYearDto {
  fiscalYearID: number;
  yearName: string;
  startDate: string;
  endDate: string;
  isClosed: boolean;
}

// ═══════════════════════════════════════
// API Functions
// ═══════════════════════════════════════

export const stocktakeApi = {
  // ═══ السنوات المالية ═══
  getFiscalYears: () =>
    api.get<FiscalYearDto[]>("/stocktake/fiscal-years"),

  // ═══ جرد الأصول ═══
  createAssetStocktake: (fiscalYearID: number) =>
    api.post<AssetStocktakeDto>("/stocktake/asset-stocktake", { fiscalYearID }),

  getAssetStocktake: () =>
    api.get<AssetStocktakeDto>("/stocktake/asset-stocktake/current"),

  getAssetStocktakeByFiscalYear: (fiscalYearId: number) =>
    api.get<AssetStocktakeDto>(`/stocktake/asset-stocktake/year/${fiscalYearId}`),

  getAssetStocktakeDetails: (stocktakeId: number) =>
    api.get<AssetStocktakeDto>(`/stocktake/asset-stocktake/${stocktakeId}/details`),

  getAllAssetStocktakes: (fiscalYearId: number) =>
    api.get<AssetStocktakeDto[]>(`/stocktake/asset-stocktake/all/${fiscalYearId}`),

  recordAssetStocktake: (stocktakeId: number, lines: AssetStocktakeLineDto[]) =>
    api.post(`/stocktake/asset-stocktake/${stocktakeId}/record`, { lines }),

  completeAssetStocktake: (stocktakeId: number) =>
    api.post(`/stocktake/asset-stocktake/${stocktakeId}/complete`),

  // ═══ جرد المخزون ═══
  createSimpleStocktake: () =>
    api.post<SimpleStocktakeDto>("/stocktake/simple-stocktake"),

  getSimpleStocktake: () =>
    api.get<SimpleStocktakeDto>("/stocktake/simple-stocktake/current"),

  getSimpleStocktakeByFiscalYear: (fiscalYearId: number) =>
    api.get<SimpleStocktakeDto>(`/stocktake/simple-stocktake/year/${fiscalYearId}`),

  getSimpleStocktakeDetails: (stocktakeId: number) =>
    api.get<SimpleStocktakeDto>(`/stocktake/simple-stocktake/${stocktakeId}/details`),

  getAllSimpleStocktakes: (fiscalYearId: number) =>
    api.get<SimpleStocktakeDto[]>(`/stocktake/simple-stocktake/all/${fiscalYearId}`),

  recordSimpleStocktake: (stocktakeId: number, actualCounts: Record<number, number>) =>
    api.post(`/stocktake/simple-stocktake/${stocktakeId}/record`, { actualCounts }),

  adjustSimpleStocktake: (stocktakeId: number) =>
    api.post(`/stocktake/simple-stocktake/${stocktakeId}/adjust`),
};
