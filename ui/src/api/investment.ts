import api from "./axios";
import type { ApiResponse } from "@/types/api";

export interface InvestmentLookupDto {
    id: number;
    name: string;
    code?: string;
    description?: string;
    isCredit?: boolean;
}

export interface PortfolioDto {
    portfolioID: number;
    name: string;
    description?: string;
    managerUserID: number;
    managerName: string;
    totalTargetCapital: number;
    createdAt: string;
}

export interface PortfolioSummaryDto {
    portfolioID: number;
    name: string;
    managerName: string;
    projectCount: number;
    totalTargetCapital: number;
    totalPlannedBudget: number;
    totalActualSpent: number;
    occupancyRate: number;
}

export interface InvestmentStatsDto {
    totalPortfolios: number;
    totalAssetsValue: number;
    activeLeaseContracts: number;
    openMaintenanceRequests: number;
    pendingApprovalsCount: number;
}

export interface CreateProjectDto {
    portfolioID: number;
    typeID: number;
    name: string;
    plannedBudget: number;
    expectedROI: number;
    startDate: string;
    endDate?: string;
    description?: string;
}

export interface UpdateProjectDto extends CreateProjectDto {
    status: string;
}

export interface ProjectDto {
    projectID: number;
    portfolioID: number;
    portfolioName: string;
    typeID: number;
    typeName: string;
    name: string;
    status: string;
    plannedBudget: number;
    expectedROI: number;
    startDate: string;
    endDate?: string;
}

export interface ProjectSummaryDto {
    projectID: number;
    name: string;
    status: string;
    plannedBudget: number;
    totalActualSpent: number;
    propertyCount: number;
    progressPercentage: number;
}

export interface PropertyDto {
    propertyID: number;
    projectID?: number;
    projectName?: string;
    propertyTypeID: number;
    propertyTypeName: string;
    propertyName: string;
    locationCity?: string;
    totalArea: number;
    initialValue: number;
    currentMarketValue: number;
    mainPhotoPath?: string;
    googleMapsUrl?: string;
    status: string;
    titleDeedNumber?: string;
}

export interface PropertyDetailDto extends PropertyDto {
    address?: string;
    latitude?: number;
    longitude?: number;
    photosJson?: string;
    ownershipDeedPath?: string;
    acquisitionDate: string;
    description?: string;
    units: PropertyUnitDto[];
    valuations: ValuationDto[];
}

export interface PropertyUnitDto {
    unitID: number;
    propertyID: number;
    propertyName?: string;
    unitTypeID: number;
    unitTypeName: string;
    unitNumber: string;
    floor?: string;
    area: number;
    status: string;
    mainPhotoPath?: string;
}

export interface TenantDto {
    tenantID: number;
    fullName: string;
    tenantType?: string;
    nationalIDOrCR: string;
    phone?: string;
    ratingScore: number;
    activeContractsCount: number;
}

export interface LeaseContractDto {
    contractID: number;
    unitID: number;
    unitNumber: string;
    propertyName: string;
    tenantID: number;
    tenantName: string;
    startDate: string;
    endDate: string;
    totalContractValue: number;
    paymentFrequency?: string;
    status: string;
    signedContractPath?: string;
}

export interface PaymentScheduleDto {
    scheduleID: number;
    contractID: number;
    dueDate: string;
    expectedAmount: number;
    paidAmount: number;
    status: string;
}

export interface UpdatePaymentStatusDto {
    dueDate: string;
    expectedAmount: number;
    paidAmount: number;
    status: string;
}

export interface TransactionDto {
    txnID: number;
    txnTypeID: number;
    txnTypeName: string;
    isCredit: boolean;
    projectID?: number;
    projectName?: string;
    sourceType?: string;
    txnDate: string;
    amount: number;
    referenceNumber?: string;
}

export interface CreateTransactionDto {
    txnTypeID: number;
    projectID?: number | null;
    isCredit?: boolean;
    sourceType?: string;
    txnDate: string;
    amount: number;
    referenceNumber?: string;
}

export interface ValuationDto {
    valuationID: number;
    propertyID: number;
    valuationDate: string;
    marketValue: number;
    valuedBy?: string;
    valuationReportPath?: string;
}

export interface DepreciationRecordDto {
    deprecID: number;
    propertyID: number;
    fiscalYearID: number;
    yearName: string;
    depreciationRate: number;
    depreciationAmount: number;
    netBookValue: number;
}

export interface MaintenanceRequestDto {
    requestID: number;
    propertyID: number;
    propertyName: string;
    unitID?: number | null;
    unitNumber?: string;
    tenantID?: number | null;
    tenantName?: string;
    priority?: string;
    description: string;
    status: string;
    reportedDate: string;
}

export interface WorkOrderDto {
    workOrderID: number;
    requestID: number;
    description?: string;
    status?: string;
    assignedContractor?: string;
    estimatedCost: number;
    actualCost: number;
    completionDate?: string;
}

export interface RiskAssessmentDto {
    riskID: number;
    projectID: number;
    riskType?: string;
    riskDescription?: string;
    riskScore: number;
    mitigationPlan?: string;
    responsiblePerson?: string;
    assessedDate: string;
}

export interface CreateRiskDto {
    projectID: number;
    riskType?: string;
    riskDescription?: string;
    riskScore: number;
    mitigationPlan?: string;
    responsiblePerson?: string;
    assessedDate: string;
}

export interface InvestmentPlanDto {
    planID: number;
    projectID: number;
    fiscalYearID: number;
    yearName?: string;
    plannedAmount: number;
    actualSpent: number;
}

export interface AccountDto {
    accountID: number;
    accountCode: string;
    accountName: string;
    accountType: string;
    parentAccountID?: number;
    balance: number;
    isActive: boolean;
}

export interface CreateAccountDto {
    accountCode: string;
    accountName: string;
    accountType: string;
    parentAccountID?: number;
}

export interface UpdateAccountDto extends CreateAccountDto {
    isActive: boolean;
}

export interface JournalEntryDto {
    entryID: number;
    txnID?: number;
    accountID: number;
    accountName: string;
    accountCode: string;
    debit: number;
    credit: number;
    entryDate: string;
    description?: string;
}

export interface CreateJournalEntryDto {
    txnID?: number | null;
    accountID: number;
    debit: number;
    credit: number;
    entryDate: string;
    description?: string;
}

export interface ApprovalRequestDto {
    approvalID: number;
    entityType: string;
    entityID: number;
    requestedByUserID: number;
    requestedByUserName: string;
    workflowStage?: string;
    status: string;
    createdAt: string;
}

export interface CreateApprovalDto {
    entityType: string;
    entityID: number;
    workflowStage?: string;
}

export interface UpdateApprovalDto {
    status: string;
    decisionNote?: string;
}

export interface InvestmentDocumentDto {
    documentID: number;
    documentTypeID: number;
    documentTypeName: string;
    fileName: string;
    filePath?: string;
    fileExtension?: string;
    fileSize?: number;
    notes?: string;
    uploadedByUser?: string;
    createdAt: string;
    // Polymorphic parent IDs
    retirementFileID?: number;
    retireeID?: number;
    entitledPersonID?: number;
    agentID?: number;
    guardianID?: number;
    pledgeID?: number;
    exemptionID?: number;
    investmentProjectID?: number;
    investmentPropertyID?: number;
    propertyUnitID?: number;
    leaseContractID?: number;
    tenantID?: number;
}

export interface UploadInvestmentDocumentDto {
    documentTypeID: number;
    file: File;
    notes?: string;
    // Polymorphic parent IDs
    retirementFileID?: number;
    retireeID?: number;
    entitledPersonID?: number;
    agentID?: number;
    guardianID?: number;
    pledgeID?: number;
    exemptionID?: number;
    investmentProjectID?: number;
    investmentPropertyID?: number;
    propertyUnitID?: number;
    leaseContractID?: number;
    tenantID?: number;
}

export interface CreatePortfolioDto {
    name: string;
    description?: string;
    managerUserID: number;
    totalTargetCapital: number;
}

export interface UpdatePortfolioDto extends CreatePortfolioDto {
    status: string;
}

export interface CreatePropertyDto {
    projectID?: number;
    propertyTypeID: number;
    propertyName: string;
    locationCity?: string;
    address?: string;
    totalArea: number;
    initialValue: number;
    currentMarketValue: number;
    googleMapsUrl?: string;
    titleDeedNumber?: string;
}

export interface UpdatePropertyDto extends CreatePropertyDto {
    status: string;
}

export interface CreateInvestmentPlanDto {
    projectID: number;
    fiscalYearID: number;
    plannedAmount: number;
    actualSpent?: number;
}

export interface UpdateInvestmentPlanDto extends CreateInvestmentPlanDto {
    planID: number;
}

// ═══════════════════════════════════════
// API CLIENT
// ═══════════════════════════════════════

export const investmentApi = {
    // Portfolios
    getPortfolios: () => api.get<ApiResponse<PortfolioDto[]>>('/investment/portfolios/all').then(r => r.data.data),
    getPortfolio: (id: number) => api.get<ApiResponse<PortfolioDto>>(`/investment/portfolios/${id}`).then(r => r.data.data),
    getPortfolioSummary: (id: number) => api.get<ApiResponse<PortfolioSummaryDto>>(`/investment/portfolios/summary/${id}`).then(r => r.data.data),
    createPortfolio: (data: CreatePortfolioDto) => api.post<ApiResponse<number>>('/investment/portfolios', data).then(r => r.data.data),
    updatePortfolio: (id: number, data: UpdatePortfolioDto) => api.put<ApiResponse<boolean>>(`/investment/portfolios/${id}`, data).then(r => r.data.data),
    deletePortfolio: (id: number) => api.delete<ApiResponse<boolean>>(`/investment/portfolios/${id}`).then(r => r.data.data),
    getStats: () => api.get<ApiResponse<InvestmentStatsDto>>('/investment/portfolios/stats').then(r => r.data.data),

    // Projects
    getProjects: (portfolioId?: number) => api.get<ApiResponse<ProjectDto[]>>(`/investment/projects/${portfolioId ? `portfolio/${portfolioId}` : 'all'}`).then(r => r.data.data),
    getProjectsByPortfolio: (pId: number) => api.get<ApiResponse<ProjectDto[]>>(`/investment/projects/portfolio/${pId}`).then(r => r.data.data),
    getProject: (id: number) => api.get<ApiResponse<ProjectDto>>(`/investment/projects/${id}`).then(r => r.data.data),
    getProjectSummary: (id: number) => api.get<ApiResponse<ProjectSummaryDto>>(`/investment/projects/summary/${id}`).then(r => r.data.data),
    createProject: (data: CreateProjectDto) => api.post<ApiResponse<number>>('/investment/projects', data).then(r => r.data.data),
    updateProject: (id: number, data: UpdateProjectDto) => api.put<ApiResponse<boolean>>(`/investment/projects/${id}`, data).then(r => r.data.data),
    deleteProject: (id: number) => api.delete<ApiResponse<boolean>>(`/investment/projects/${id}`).then(r => r.data.data),

    // Investment Plans
    getInvestmentPlans: (projectId: number) => api.get<ApiResponse<InvestmentPlanDto[]>>(`/investment/projects/${projectId}/plans`).then(r => r.data.data),
    createInvestmentPlan: (data: CreateInvestmentPlanDto) => api.post<ApiResponse<number>>('/investment/projects/plans', data).then(r => r.data.data),
    updateInvestmentPlan: (id: number, data: UpdateInvestmentPlanDto) => api.put<ApiResponse<boolean>>(`/investment/projects/plans/${id}`, data).then(r => r.data.data),
    deleteInvestmentPlan: (id: number) => api.delete<ApiResponse<boolean>>(`/investment/projects/plans/${id}`).then(r => r.data.data),

    // Properties & Assets
    getProperties: (projectId?: number) => api.get<ApiResponse<PropertyDto[]>>(projectId ? `/investment/properties/list?projectId=${projectId}` : '/investment/properties').then(r => r.data.data),
    getAllProperties: () => api.get<ApiResponse<PropertyDto[]>>('/investment/properties').then(r => r.data.data),
    getProperty: (id: number) => api.get<ApiResponse<PropertyDetailDto>>(`/investment/properties/${id}`).then(r => r.data.data),
    createProperty: (data: CreatePropertyDto) => api.post<ApiResponse<number>>('/investment/properties', data).then(r => r.data.data),
    updateProperty: (id: number, data: UpdatePropertyDto) => api.put<ApiResponse<boolean>>(`/investment/properties/${id}`, data).then(r => r.data.data),
    deleteProperty: (id: number) => api.delete<ApiResponse<boolean>>(`/investment/properties/${id}`).then(r => r.data.data),
    uploadPropertyPhoto: (id: number, file: File) => {
        const fd = new FormData();
        fd.append('file', file);
        return api.post<ApiResponse<string>>(`/investment/properties/${id}/upload-photo`, fd).then(r => r.data.data);
    },

    // Valuations & Depreciation
    getValuations: (pId: number) => api.get<ApiResponse<ValuationDto[]>>(`/investment/properties/${pId}/valuations`).then(r => r.data.data),
    createValuation: (data: unknown) => api.post<ApiResponse<number>>('/investment/properties/valuations', data).then(r => r.data.data),
    deleteValuation: (id: number) => api.delete<ApiResponse<boolean>>(`/investment/properties/valuations/${id}`).then(r => r.data.data),
    
    getDepreciation: (pId: number) => api.get<ApiResponse<DepreciationRecordDto[]>>(`/investment/properties/${pId}/depreciation`).then(r => r.data.data),
    createDepreciation: (data: unknown) => api.post<ApiResponse<number>>('/investment/properties/depreciation', data).then(r => r.data.data),
    deleteDepreciation: (id: number) => api.delete<ApiResponse<boolean>>(`/investment/properties/depreciation/${id}`).then(r => r.data.data),

    // Units
    getUnits: (pId: number) => api.get<ApiResponse<PropertyUnitDto[]>>(`/investment/properties/${pId}/units`).then(r => r.data.data),
    getAllUnits: () => api.get<ApiResponse<PropertyUnitDto[]>>('/investment/properties/units/all').then(r => r.data.data),
    createUnit: (data: unknown) => api.post<ApiResponse<number>>('/investment/properties/units', data).then(r => r.data.data),
    updateUnit: (id: number, data: unknown) => api.put<ApiResponse<boolean>>(`/investment/properties/units/${id}`, data).then(r => r.data.data),
    deleteUnit: (id: number) => api.delete<ApiResponse<boolean>>(`/investment/properties/units/${id}`).then(r => r.data.data),

    // Tenants & Leasing
    getTenants: () => api.get<ApiResponse<TenantDto[]>>('/investment/leasing/tenants').then(r => r.data.data),
    getTenant: (id: number) => api.get<ApiResponse<TenantDto>>(`/investment/leasing/tenants/${id}`).then(r => r.data.data),
    createTenant: (data: unknown) => api.post<ApiResponse<number>>('/investment/leasing/tenants', data).then(r => r.data.data),
    updateTenant: (id: number, data: unknown) => api.put<ApiResponse<boolean>>(`/investment/leasing/tenants/${id}`, data).then(r => r.data.data),
    deleteTenant: (id: number) => api.delete<ApiResponse<boolean>>(`/investment/leasing/tenants/${id}`).then(r => r.data.data),
    getTenantProperties: (id: number) => api.get<ApiResponse<PropertyDto[]>>(`/investment/leasing/tenants/${id}/properties`).then(r => r.data.data),

    // Contracts
    getContracts: (pId?: number) => api.get<ApiResponse<LeaseContractDto[]>>(pId ? `/investment/leasing/properties/${pId}/contracts` : '/investment/leasing/contracts').then(r => r.data.data),
    getContract: (id: number) => api.get<ApiResponse<LeaseContractDto>>(`/investment/leasing/contracts/${id}`).then(r => r.data.data),
    createContract: (data: unknown) => api.post<ApiResponse<number>>('/investment/leasing/contracts', data).then(r => r.data.data),
    updateContract: (id: number, data: unknown) => api.put<ApiResponse<boolean>>(`/investment/leasing/contracts/${id}`, data).then(r => r.data.data),
    deleteContract: (id: number) => api.delete<ApiResponse<boolean>>(`/investment/leasing/contracts/${id}`).then(r => r.data.data),
    uploadSignedContract: (id: number, file: File) => {
        const fd = new FormData();
        fd.append('file', file);
        return api.post<ApiResponse<string>>(`/investment/leasing/contracts/${id}/upload-signed-contract`, fd).then(r => r.data.data);
    },

    // Payments
    getSchedules: (cId?: number) => {
        const url = cId ? `/investment/leasing/contracts/${cId}/schedules` : '/investment/leasing/schedules';
        return api.get<ApiResponse<PaymentScheduleDto[]>>(url).then(r => r.data.data);
    },
    createSchedule: (data: unknown) => api.post<ApiResponse<number>>('/investment/leasing/schedules', data).then(r => r.data.data),
    updatePaymentStatus: (id: number, data: UpdatePaymentStatusDto) => api.put<ApiResponse<boolean>>(`/investment/leasing/schedules/${id}/status`, data).then(r => r.data.data),

    // Maintenance
    getRequests: (pId: number) => api.get<ApiResponse<MaintenanceRequestDto[]>>(`/investment/maintenance/properties/${pId}/requests`).then(r => r.data.data),
    getAllRequests: () => api.get<ApiResponse<MaintenanceRequestDto[]>>('/investment/maintenance/requests/all').then(r => r.data.data),
    getRequest: (id: number) => api.get<ApiResponse<MaintenanceRequestDto>>(`/investment/maintenance/requests/${id}`).then(r => r.data.data),
    createRequest: (data: unknown) => api.post<ApiResponse<number>>('/investment/maintenance/requests', data).then(r => r.data.data),
    updateRequest: (id: number, data: unknown) => api.put<ApiResponse<boolean>>(`/investment/maintenance/requests/${id}`, data).then(r => r.data.data),
    
    getWorkOrders: (rId: number) => api.get<ApiResponse<WorkOrderDto[]>>(`/investment/maintenance/requests/${rId}/work-orders`).then(r => r.data.data),
    createWorkOrder: (data: unknown) => api.post<ApiResponse<number>>('/investment/maintenance/work-orders', data).then(r => r.data.data),
    updateWorkOrder: (id: number, data: unknown) => api.put<ApiResponse<boolean>>(`/investment/maintenance/work-orders/${id}`, data).then(r => r.data.data),
    addMaintenanceCost: (ticketId: number, data: unknown) => api.post<ApiResponse<number>>(`/investment/maintenance/requests/${ticketId}/costs`, data).then(r => r.data.data),

    // Transactions
    getTransactions: (pId?: number) => {
        const url = pId ? `/investment/transactions?projectId=${pId}` : '/investment/transactions';
        return api.get<ApiResponse<TransactionDto[]>>(url).then(r => r.data.data);
    },
    createTransaction: (data: CreateTransactionDto) => api.post<ApiResponse<number>>('/investment/transactions', data).then(r => r.data.data),

    // Lookups
    getBranches: () => api.get<ApiResponse<InvestmentLookupDto[]>>('/lookups/branches').then(r => r.data.data),
    getInvestmentTypes: () => api.get<ApiResponse<InvestmentLookupDto[]>>('/investment/lookups/investment-types').then(r => r.data.data),
    getPropertyTypes: () => api.get<ApiResponse<InvestmentLookupDto[]>>('/investment/lookups/property-types').then(r => r.data.data),
    getUnitTypes: () => api.get<ApiResponse<InvestmentLookupDto[]>>('/investment/lookups/unit-types').then(r => r.data.data),
    getTransactionTypes: () => api.get<ApiResponse<InvestmentLookupDto[]>>('/investment/lookups/transaction-types').then(r => r.data.data),
    
    // General Document Lookups
    getDocumentTypes: () => api.get<ApiResponse<InvestmentLookupDto[]>>('/lookups/document-types').then(r => r.data.data),
    getDocumentTypesByOwner: (ownerType: number) => api.get<ApiResponse<unknown[]>>(`/document-types/by-owner/${ownerType}`).then(r => r.data.data),
    getDocumentReadiness: (ownerType: number, entityId: number) => api.get<ApiResponse<unknown>>(`/investment/documents/readiness/${ownerType}/${entityId}`).then(r => r.data.data),
    
    // CRUD for lookups
    createInvestmentType: (data: unknown) => api.post<ApiResponse<number>>('/investment/lookups/investment-types', data).then(r => r.data.data),
    createPropertyType: (data: unknown) => api.post<ApiResponse<number>>('/investment/lookups/property-types', data).then(r => r.data.data),
    createUnitType: (data: unknown) => api.post<ApiResponse<number>>('/investment/lookups/unit-types', data).then(r => r.data.data),
    createTransactionType: (data: unknown) => api.post<ApiResponse<number>>('/investment/lookups/transaction-types', data).then(r => r.data.data),

    // Approvals
    getApprovals: () => api.get<ApiResponse<ApprovalRequestDto[]>>('/investment/approvals').then(r => r.data.data),
    createApproval: (data: CreateApprovalDto) => api.post<ApiResponse<number>>('/investment/approvals', data).then(r => r.data.data),
    updateApprovalStatus: (id: number, data: UpdateApprovalDto) => api.put<ApiResponse<boolean>>(`/investment/approvals/${id}/status`, data).then(r => r.data.data),

    // Risks
    getRisks: (projectId: number) => api.get<ApiResponse<RiskAssessmentDto[]>>(`/investment/projects/${projectId}/risks`).then(r => r.data.data),
    createRisk: (data: CreateRiskDto) => api.post<ApiResponse<number>>('/investment/projects/risks', data).then(r => r.data.data),
};