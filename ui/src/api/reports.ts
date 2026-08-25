import api from "./axios";
import { retirementApi, type RetirementFileReportFilter, type RetirementFileReportRow, type NameValuePair } from "./retirement";
import type { ApiResponse, PagedResult } from "@/types/api";

// ═══════════════════════════════════════
// DTOs
// ═══════════════════════════════════════

export interface ReportFilter {
    fromDate?: string;
    toDate?: string;
    portfolioId?: number;
    projectId?: number;
    propertyId?: number;
    branchId?: number;
    statusId?: number;
    fiscalYear?: number;
    quarter?: string;
    search?: string;
    pageNumber?: number;
    pageSize?: number;
    rankId?: number;
    gender?: number;
    reviewStatus?: number;
    socialStatusId?: number;
}

export type ReportDataRow = Record<string, string | number | boolean | null | undefined>;

// ═══════════════════════════════════════
// Retirement File Report Types
// ═══════════════════════════════════════

export interface RetirementFileReportSummary {
    totalCount: number;
    activeCount: number;
    suspendedCount: number;
    pendingReviewCount: number;
    byRank: NameValuePair[];
    byBranch: NameValuePair[];
    byGender: NameValuePair[];
    byStatus: NameValuePair[];
    bySocialStatus: NameValuePair[];
}

export interface RetirementFileReportData {
    items: RetirementFileReportRow[];
    totalCount: number;
}

// ═══════════════════════════════════════
// API CLIENT
// ═══════════════════════════════════════

export const reportsApi = {
    // ── Retirement File Reports ──
    getRetirementFileReportSummary: (filter: RetirementFileReportFilter) => 
        retirementApi.getReportSummary(filter),
    getRetirementFileReport: (filter: RetirementFileReportFilter) => 
        retirementApi.getReport(filter),
    exportRetirementFileReport: (filter: RetirementFileReportFilter, format: "excel" | "pdf" = "excel") => 
        retirementApi.exportReport(filter, format),

    // ── Investment ──
    getPortfolioPerformance: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/investment/portfolio-performance', { params: filter }).then(r => r.data),
    exportPortfolioPerformance: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/investment/portfolio-performance/export', filter, undefined, format),

    getOccupancy: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/investment/occupancy', { params: filter }).then(r => r.data),
    exportOccupancy: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/investment/occupancy/export', filter, undefined, format),

    getRentCollection: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/investment/rent-collection', { params: filter }).then(r => r.data),
    exportRentCollection: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/investment/rent-collection/export', filter, undefined, format),

    getContractStatus: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/investment/contract-status', { params: filter }).then(r => r.data),
    exportContractStatus: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/investment/contract-status/export', filter, undefined, format),

    getRiskExposure: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/investment/risk-exposure', { params: filter }).then(r => r.data),
    exportRiskExposure: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/investment/risk-exposure/export', filter, undefined, format),

    getMaintenanceSummary: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/investment/maintenance-summary', { params: filter }).then(r => r.data),
    exportMaintenanceSummary: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/investment/maintenance-summary/export', filter, undefined, format),

    getBudgetVsActual: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/investment/budget-vs-actual', { params: filter }).then(r => r.data),
    exportBudgetVsActual: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/investment/budget-vs-actual/export', filter, undefined, format),

    // ── Retirement ──
    getFileStatus: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/retirement/file-status', { params: filter }).then(r => r.data),
    exportFileStatus: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/retirement/file-status/export', filter, undefined, format),

    getActiveRetirees: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/retirement/active-retirees', { params: filter }).then(r => r.data),
    exportActiveRetirees: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/retirement/active-retirees/export', filter, undefined, format),

    getSuspensions: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/retirement/suspensions', { params: filter }).then(r => r.data),
    exportSuspensions: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/retirement/suspensions/export', filter, undefined, format),

    // ── Financial ──
    getSalaryBatches: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/financial/salary-batches', { params: filter }).then(r => r.data),
    exportSalaryBatches: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/financial/salary-batches/export', filter, undefined, format),

    getSettlements: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/financial/settlements', { params: filter }).then(r => r.data),
    exportSettlements: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/financial/settlements/export', filter, undefined, format),

    getBudgetQuarter: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/financial/fiscal-period', { params: filter }).then(r => r.data),
    exportBudgetQuarter: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/financial/fiscal-period/export', filter, undefined, format),

    // ── Documents ──
    getMissingDocs: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/documents/missing', { params: filter, timeout: 120000 }).then(r => r.data),
    exportMissingDocs: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/documents/missing/export', filter, 120000, format),

    getMissingDocsByPerson: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/documents/missing-by-person', { params: filter, timeout: 120000 }).then(r => r.data),
    exportMissingDocsByPerson: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/documents/missing-by-person/export', filter, 120000, format),

    getExpiredDocs: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/documents/expired', { params: filter, timeout: 120000 }).then(r => r.data),
    exportExpiredDocs: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/documents/expired/export', filter, 120000, format),

    // ── Dependents ──
    getBeneficiaries: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/dependents/beneficiaries', { params: filter }).then(r => r.data),
    exportBeneficiaries: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/dependents/beneficiaries/export', filter, undefined, format),

    // ── Compliance ──
    getMinistryBatches: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/compliance/ministry-batches', { params: filter }).then(r => r.data),
    exportMinistryBatches: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/compliance/ministry-batches/export', filter, undefined, format),

    getPendingApprovals: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/compliance/pending-approvals', { params: filter }).then(r => r.data),
    exportPendingApprovals: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/compliance/pending-approvals/export', filter, undefined, format),

    // ── Admin ──
    getAuditLog: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/admin/audit-log', { params: filter }).then(r => r.data),
    exportAuditLog: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/admin/audit-log/export', filter, undefined, format),

    getUserActivity: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/admin/user-activity', { params: filter }).then(r => r.data),
    exportUserActivity: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/admin/user-activity/export', filter, undefined, format),

    // ── HR ──
    getEmployeeDirectory: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/hr/employee-directory', { params: filter }).then(r => r.data),
    exportEmployeeDirectory: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/hr/employee-directory/export', filter, undefined, format),

    getLeaveBalances: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/hr/leave-balances', { params: filter }).then(r => r.data),
    exportLeaveBalances: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/hr/leave-balances/export', filter, undefined, format),

    getAttendanceReport: (filter: ReportFilter) => api.get<ApiResponse<PagedResult<ReportDataRow>>>('/reports/hr/attendance', { params: filter }).then(r => r.data),
    exportAttendanceReport: (filter: ReportFilter, format: "excel" | "pdf" = "excel") => exportReport('/reports/hr/attendance/export', filter, undefined, format),
};

// ═══════════════════════════════════════
// HELPER FOR EXPORTS
// ═══════════════════════════════════════
async function exportReport(url: string, filter: ReportFilter, timeout?: number, format: "excel" | "pdf" = "excel") {
    const params = { ...filter, format };
    const response = await api.get(url, { params, responseType: 'blob', timeout });
    const contentDisposition = response.headers['content-disposition'];
    const contentType = response.headers['content-type'] || '';
    let filename = format === 'pdf' ? 'report.pdf' : 'report.xlsx';
    if (contentDisposition) {
        const filenameMatch = contentDisposition.match(/filename="?(.+)"?/);
        if (filenameMatch && filenameMatch.length === 2) {
            filename = filenameMatch[1];
        }
    }

    const blobType = contentType.includes('pdf') ? 'application/pdf' : 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
    const blob = new Blob([response.data], { type: blobType });
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(link.href);
}

// ═══════════════════════════════════════
// REPORT CATALOG
// ═══════════════════════════════════════

export type ChartType = 'pie' | 'donut' | 'bar' | 'horizontalBar' | 'groupedBar' | 'stackedBar' | 'line' | 'area' | 'histogram' | 'gauge';

export interface ChartColorConfig {
    key: string;
    color: string;
    label: string;
}

export interface ChartConfig {
    type: ChartType;
    title: string;
    dataKey: string;
    nameKey?: string;
    valueKey?: string;
    colorKey?: string;
    colors?: (string | ChartColorConfig)[];
    showLegend?: boolean;
    showGrid?: boolean;
    horizontal?: boolean;
    height?: number;
    formatValue?: (value: number) => string;
}

export interface SummaryCardConfig {
    title: string;
    valueKey: string;
    icon?: string;
    color?: string;
    format?: 'currency' | 'number' | 'percent' | 'date';
    description?: string;
    variant?: 'blue' | 'orange' | 'emerald' | 'purple' | 'rose' | 'amber' | 'cyan' | 'indigo' | 'teal' | 'lime' | 'success' | 'warning' | 'destructive';
}

export interface ReportConfig {
    id: string;
    name: string;
    icon: string;
    category: string;
    description: string;
    fetchData: (filter: ReportFilter) => Promise<ApiResponse<PagedResult<ReportDataRow>>>;
    fetchSummary?: (filter: ReportFilter) => Promise<Record<string, any>>;
    exportData: (filter: ReportFilter, format?: "excel" | "pdf") => Promise<void>;
    filters: string[];
    columns: { key: string; label: string; type?: 'currency' | 'number' | 'date' | 'percent' | 'boolean' }[];
    charts?: ChartConfig[];
    summaryCards?: SummaryCardConfig[];
}

export const REPORT_CATALOG: ReportConfig[] = [


    // ── Retirement ──
    {
        id: 'ret-00',
        name: 'تقارير ملفات التقاعد التفصيلية',
        category: 'التقاعد',
        icon: 'BarChart3Icon',
        description: 'تحليل وتقارير شاملة لملفات التقاعد مع إحصائيات مفصلة',
        fetchData: async (filter) => {
            const retirementFilter: RetirementFileReportFilter = {
                pageNumber: filter.pageNumber ?? 1,
                pageSize: filter.pageSize ?? 50,
                rankId: filter.rankId as number,
                branchId: filter.branchId as number,
                gender: filter.gender as number,
                reviewStatus: filter.reviewStatus as number,
                socialStatusId: filter.socialStatusId as number,
                statusId: filter.statusId as number,
            };
            const result = await reportsApi.getRetirementFileReport(retirementFilter);
            const items = (result?.items || []) as unknown as ReportDataRow[];
            return { isSuccess: true, message: '', data: { items, totalCount: result?.totalCount || 0, page: result?.page || 1, pageSize: result?.pageSize || 50, totalPages: result?.totalPages || 0 } } as ApiResponse<PagedResult<ReportDataRow>>;
        },
        fetchSummary: async (filter) => {
            const retirementFilter: RetirementFileReportFilter = {
                pageNumber: filter.pageNumber ?? 1,
                pageSize: filter.pageSize ?? 50,
                rankId: filter.rankId as number,
                branchId: filter.branchId as number,
                gender: filter.gender as number,
                reviewStatus: filter.reviewStatus as number,
                socialStatusId: filter.socialStatusId as number,
                statusId: filter.statusId as number,
            };
            const result = await reportsApi.getRetirementFileReportSummary(retirementFilter);
            return result as Record<string, any>;
        },
        exportData: async (filter, format) => {
            const retirementFilter: RetirementFileReportFilter = {
                pageNumber: filter.pageNumber ?? 1,
                pageSize: filter.pageSize ?? 50,
                rankId: filter.rankId as number,
                branchId: filter.branchId as number,
                gender: filter.gender as number,
                reviewStatus: filter.reviewStatus as number,
                socialStatusId: filter.socialStatusId as number,
                statusId: filter.statusId as number,
            };
            await reportsApi.exportRetirementFileReport(retirementFilter, format ?? 'excel');
        },
        filters: ['fromDate', 'toDate', 'search', 'rankId', 'branchId', 'gender', 'reviewStatus', 'socialStatusId', 'statusId'],
        columns: [
            { key: 'fileNumber', label: 'رقم الملف' },
            { key: 'retireeName', label: 'المتقاعد' },
            { key: 'militaryNumber', label: 'الرقم العسكري' },
            { key: 'rankName', label: 'الرتبة' },
            { key: 'branchName', label: 'الفرع' },
            { key: 'gender', label: 'الجنس' },
            { key: 'socialStatusName', label: 'الحالة الاجتماعية' },
            { key: 'retirementDate', label: 'تاريخ التقاعد', type: 'date' },
            { key: 'statusName', label: 'حالة الملفات' },
            { key: 'missingDocumentsCount', label: 'مستندات ناقصة', type: 'number' },
        ],
        charts: [
            { type: 'donut', title: 'توزيع الملفات حسب الرتبة', nameKey: 'rankName', valueKey: 'count', dataKey: 'rankName' },
            { type: 'horizontalBar', title: 'توزيع الملفات حسب الفرع', nameKey: 'branchName', valueKey: 'count', dataKey: 'branchName' },
            { type: 'pie', title: 'توزيع الملفات حسب الجنس', nameKey: 'gender', valueKey: 'count', dataKey: 'gender' },
            { type: 'area', title: 'اتجاه التأسيس الشهري', nameKey: 'month', valueKey: 'count', dataKey: 'retirementDate' },
        ],
        summaryCards: [
            { title: 'إجمالي الملفات', valueKey: 'totalCount', color: 'blue', format: 'number', variant: 'blue', description: 'جميع ملفات التقاعد', icon: 'FileText' },
            { title: 'الملفات النشطة', valueKey: 'activeCount', color: 'green', format: 'number', variant: 'emerald', description: 'نشطة مالياً', icon: 'CheckCircle' },
            { title: 'قيد المراجعة', valueKey: 'pendingReviewCount', color: 'yellow', format: 'number', variant: 'warning', description: 'بانتظار المراجعة', icon: 'Clock' },
            { title: 'مستندات ناقصة', valueKey: 'missingDocsCount', color: 'red', format: 'number', variant: 'destructive', description: 'تتطلب إكمال', icon: 'AlertTriangle' },
        ],
    },
    {
        id: 'ret-01',
        name: 'حالة الملفاتات',
        category: 'التقاعد',
        icon: 'FolderOpenIcon',
        description: 'متابعة حالة معالجة ملفات التقاعد الجديدة',
        fetchData: (filter) => reportsApi.getFileStatus(filter),
        exportData: (filter, format) => reportsApi.exportFileStatus(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'statusId'],
        columns: [
            { key: 'fileNumber', label: 'رقم الملف' },
            { key: 'retireeName', label: 'المتقاعد' },
            { key: 'rankName', label: 'الرتبة' },
            { key: 'status', label: 'الحالة' },
            { key: 'daysPending', label: 'أيام الانتظار', type: 'number' },
            { key: 'missingDocsCount', label: 'مستندات ناقصة', type: 'number' },
        ],
        charts: [
            { type: 'donut', title: 'حالة الملفاتات', nameKey: 'status', valueKey: 'count', dataKey: 'status' },
            { type: 'histogram', title: 'توزيع المستندات الناقصة', dataKey: 'missingDocsCount' },
        ],
        summaryCards: [
            { title: 'إجمالي الملفات', valueKey: 'totalCount', color: 'blue', format: 'number', variant: 'blue', description: 'جميع الملفات', icon: 'FileText' },
            { title: 'جديد', valueKey: 'newCount', color: 'yellow', format: 'number', variant: 'warning', description: 'ملفات جديدة', icon: 'Clock' },
            { title: 'قيد المعالجة', valueKey: 'processingCount', color: 'orange', format: 'number', variant: 'orange', description: 'قيد التنفيذ', icon: 'BarChart3' },
            { title: 'مكتمل', valueKey: 'completedCount', color: 'green', format: 'number', variant: 'success', description: 'تم الانتهاء', icon: 'CheckCircle' },
        ],
    },
    {
        id: 'ret-02',
        name: 'المتقاعدين النشطين',
        category: 'التقاعد',
        icon: 'UserGroupIcon',
        description: 'قائمة بالمتقاعدين النشطين مالياً',
        fetchData: (filter) => reportsApi.getActiveRetirees(filter),
        exportData: (filter, format) => reportsApi.exportActiveRetirees(filter, format),
        filters: ['fromDate', 'toDate', 'search'],
        columns: [
            { key: 'fullName', label: 'الاسم' },
            { key: 'nationalID', label: 'الهوية' },
            { key: 'rankName', label: 'الرتبة' },
            { key: 'category', label: 'الفئة' },
            { key: 'monthlyPension', label: 'المعاش الشهري', type: 'currency' },
            { key: 'governorate', label: 'المحافظة' },
        ],
        charts: [
            { type: 'donut', title: 'التوزيع حسب الرتبة', nameKey: 'rankName', valueKey: 'count', dataKey: 'rankName' },
            { type: 'horizontalBar', title: 'التوزيع حسب المحافظة', nameKey: 'governorate', valueKey: 'count', dataKey: 'governorate' },
        ],
        summaryCards: [
            { title: 'إجمالي المتقاعدين', valueKey: 'totalCount', color: 'blue', format: 'number', variant: 'blue', description: 'عدد المتقاعدين النشطين', icon: 'Users' },
            { title: 'إجمالي المعاشات', valueKey: 'totalPension', color: 'green', format: 'currency', variant: 'emerald', description: 'الإجمالي الشهري', icon: 'DollarSign' },
            { title: 'متوسط المعاش', valueKey: 'avgPension', color: 'purple', format: 'currency', variant: 'purple', description: 'لكل متقاعد', icon: 'BarChart3' },
            { title: 'المحافظات', valueKey: 'governorateCount', color: 'yellow', format: 'number', variant: 'warning', description: 'محافظات مختلفة', icon: 'MapPin' },
        ],
    },
    {
        id: 'ret-03',
        name: 'سجل الإيقافات',
        category: 'التقاعد',
        icon: 'NoSymbolIcon',
        description: 'تقرير بالإيقافات المالية وأسبابها',
        fetchData: (filter) => reportsApi.getSuspensions(filter),
        exportData: (filter, format) => reportsApi.exportSuspensions(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'statusId'],
        columns: [
            { key: 'retireeName', label: 'المتقاعد' },
            { key: 'fileNumber', label: 'رقم الملف' },
            { key: 'reason', label: 'السبب' },
            { key: 'suspendedDate', label: 'تاريخ الإيقاف', type: 'date' },
            { key: 'daysSuspended', label: 'عدد الأيام', type: 'number' },
            { key: 'isActive', label: 'نشط', type: 'boolean' },
        ],
        charts: [
            { type: 'donut', title: 'أسباب الإيقاف', nameKey: 'reason', valueKey: 'count', dataKey: 'reason' },
            { type: 'bar', title: 'مدة الإيقاف بالأيام', nameKey: 'reason', valueKey: 'avgDays', dataKey: 'reason' },
            { type: 'area', title: 'اتجاه الإيقافات', nameKey: 'month', valueKey: 'count', dataKey: 'suspendedDate' },
        ],
        summaryCards: [
            { title: 'إجمالي الإيقافات', valueKey: 'totalCount', color: 'red', format: 'number', variant: 'destructive', description: 'إجمالي حالات الإيقاف', icon: 'XCircle' },
            { title: 'نشطة', valueKey: 'activeCount', color: 'orange', format: 'number', variant: 'warning', description: 'حالات نشطة', icon: 'Clock' },
            { title: 'منتهية', valueKey: 'endedCount', color: 'green', format: 'number', variant: 'success', description: 'تم الحل', icon: 'CheckCircle' },
            { title: 'متوسط المدة', valueKey: 'avgDays', color: 'blue', format: 'number', variant: 'blue', description: 'أيام', icon: 'BarChart3' },
        ],
    },

    // ── Financial ──
    {
        id: 'fin-01',
        name: 'مسيرات المعاشات',
        category: 'المالية',
        icon: 'CreditCardIcon',
        description: 'متابعة مسيرات المعاشات الشهرية والاعتمادات',
        fetchData: (filter) => reportsApi.getSalaryBatches(filter),
        exportData: (filter, format) => reportsApi.exportSalaryBatches(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'statusId'],
        columns: [
            { key: 'period', label: 'الفترة' },
            { key: 'status', label: 'الحالة' },
            { key: 'totalAmount', label: 'إجمالي المبلغ', type: 'currency' },
            { key: 'recipientCount', label: 'عدد المستفيدين', type: 'number' },
            { key: 'createdAt', label: 'تاريخ الإنشاء', type: 'date' },
        ],
        charts: [
            { type: 'area', title: 'اتجاه المبالغ الشهرية', nameKey: 'period', valueKey: 'totalAmount', dataKey: 'period' },
            { type: 'bar', title: 'عدد المستفيدين لكل دفعة', nameKey: 'period', valueKey: 'recipientCount', dataKey: 'period' },
            { type: 'donut', title: 'حالة الدفعات', nameKey: 'status', valueKey: 'count', dataKey: 'status' },
        ],
        summaryCards: [
            { title: 'إجمالي المبالغ', valueKey: 'totalAmount', color: 'green', format: 'currency', variant: 'emerald', description: 'إجمالي المبالغ المدفوعة', icon: 'DollarSign' },
            { title: 'عدد الدفعات', valueKey: 'batchCount', color: 'blue', format: 'number', variant: 'blue', description: 'مسيرات مالية', icon: 'FileText' },
            { title: 'إجمالي المستفيدين', valueKey: 'totalRecipients', color: 'purple', format: 'number', variant: 'purple', description: 'متقاعدين', icon: 'Users' },
            { title: 'متوسط المبلغ', valueKey: 'avgAmount', color: 'yellow', format: 'currency', variant: 'warning', description: 'لكل متقاعد', icon: 'BarChart3' },
        ],
    },
    {
        id: 'fin-02',
        name: 'التسويات المالية',
        category: 'المالية',
        icon: 'CalculatorIcon',
        description: 'تقرير بالتسويات المالية والفروقات',
        fetchData: (filter) => reportsApi.getSettlements(filter),
        exportData: (filter, format) => reportsApi.exportSettlements(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'statusId'],
        columns: [
            { key: 'decisionNumber', label: 'رقم القرار' },
            { key: 'retireeName', label: 'المتقاعد' },
            { key: 'amount', label: 'المبلغ', type: 'currency' },
            { key: 'status', label: 'الحالة' },
            { key: 'createdAt', label: 'تاريخ الإنشاء', type: 'date' },
        ],
        charts: [
            { type: 'donut', title: 'التسويات حسب النوع', nameKey: 'type', valueKey: 'count', dataKey: 'type' },
            { type: 'stackedBar', title: 'الحالات', nameKey: 'status', valueKey: 'count', dataKey: 'status' },
        ],
        summaryCards: [
            { title: 'إجمالي التسويات', valueKey: 'totalCount', color: 'blue', format: 'number', variant: 'blue', description: 'تسويات مالية', icon: 'Calculator' },
            { title: 'إجمالي المبالغ', valueKey: 'totalAmount', color: 'green', format: 'currency', variant: 'emerald', description: 'قيمة التسويات', icon: 'DollarSign' },
            { title: 'معلقة', valueKey: 'pendingCount', color: 'yellow', format: 'number', variant: 'warning', description: 'بانتظار الاعتماد', icon: 'Clock' },
            { title: 'معتمدة', valueKey: 'approvedCount', color: 'green', format: 'number', variant: 'success', description: 'تم الاعتماد', icon: 'CheckCircle' },
        ],
    },
    {
        id: 'fin-03',
        name: 'الميزانية الربعية',
        category: 'المالية',
        icon: 'PresentationChartBarIcon',
        description: 'تقرير مخصصات الميزانية الربعية للمؤسسة',
        fetchData: (filter) => reportsApi.getBudgetQuarter(filter),
        exportData: (filter, format) => reportsApi.exportBudgetQuarter(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'fiscalYear', 'quarter'],
        columns: [
            { key: 'chapterName', label: 'الباب' },
            { key: 'sectionName', label: 'البند' },
            { key: 'quarter', label: 'الربع' },
            { key: 'allocated', label: 'المخصص', type: 'currency' },
            { key: 'spent', label: 'المنصرف', type: 'currency' },
            { key: 'remaining', label: 'المتبقي', type: 'currency' },
            { key: 'spentPct', label: 'نسبة الصرف', type: 'percent' },
        ],
        charts: [
            { type: 'groupedBar', title: 'المخصص vs المنصرف', nameKey: 'sectionName', valueKey: 'allocated', dataKey: 'sectionName' },
            { type: 'gauge', title: 'نسبة الصرف', dataKey: 'spentPct' },
            { type: 'pie', title: 'التوزيع حسب الباب', nameKey: 'chapterName', valueKey: 'allocated', dataKey: 'chapterName' },
        ],
        summaryCards: [
            { title: 'إجمالي المخصص', valueKey: 'totalAllocated', color: 'blue', format: 'currency', variant: 'blue', description: 'الميزانية المخططة', icon: 'DollarSign' },
            { title: 'إجمالي المنصرف', valueKey: 'totalSpent', color: 'red', format: 'currency', variant: 'destructive', description: 'المصروف فعلياً', icon: 'TrendingDown' },
            { title: 'المتبقي', valueKey: 'totalRemaining', color: 'green', format: 'currency', variant: 'success', description: 'المتبقي من الميزانية', icon: 'TrendingUp' },
            { title: 'نسبة الصرف', valueKey: 'spentPct', color: 'yellow', format: 'percent', variant: 'warning', description: 'نسبة التنفيذ', icon: 'Percent' },
        ],
    },

    // ── Documents ──
    {
        id: 'doc-01',
        name: 'المستندات الناقصة',
        category: 'الوثائق',
        icon: 'DocumentMinusIcon',
        description: 'تتبع المستندات المطلوبة وغير المكتملة',
        fetchData: (filter) => reportsApi.getMissingDocs(filter),
        exportData: (filter, format) => reportsApi.exportMissingDocs(filter, format),
        filters: ['fromDate', 'toDate', 'search'],
        columns: [
            { key: 'fileNumber', label: 'رقم الملف' },
            { key: 'retireeName', label: 'اسم المتقاعد' },
            { key: 'militaryNumber', label: 'الرقم العسكري' },
            { key: 'missingCount', label: 'عدد النواقص', type: 'number' },
            { key: 'missingDocsList', label: 'المستندات الناقصة' },
            { key: 'maxDaysMissing', label: 'أقصى أيام التأخير', type: 'number' },
            { key: 'phone', label: 'رقم الهاتف' },
            { key: 'agentPhone', label: 'رقم هاتف الوكيل' },
        ],
        charts: [
            { type: 'horizontalBar', title: 'عدد النواقص حسب نوع الوثيقة', nameKey: '_docType', valueKey: '_docTypeCount', dataKey: 'missingDocsList' },
            { type: 'pie', title: 'توزيع الملفات حسب عدد النواقص', nameKey: '_missingRange', valueKey: '_missingRangeCount', dataKey: 'missingCount' },
            { type: 'bar', title: 'توزيع الملفات حسب مدة التأخير', nameKey: '_daysRange', valueKey: '_daysRangeCount', dataKey: 'maxDaysMissing' },
        ],
        summaryCards: [
            { title: 'إجمالي الملفات', valueKey: 'totalCount', color: 'red', format: 'number', variant: 'destructive', description: 'ملفات بنواقص', icon: 'FileText' },
            { title: 'إجمالي النواقص', valueKey: 'totalMissing', color: 'orange', format: 'number', variant: 'warning', description: 'مستندات ناقصة', icon: 'AlertTriangle' },
            { title: 'متوسط النواقص', valueKey: 'avgMissing', color: 'yellow', format: 'number', variant: 'amber', description: 'لكل ملف', icon: 'BarChart3' },
            { title: 'أقصى تأخير', valueKey: 'maxDays', color: 'red', format: 'number', variant: 'destructive', description: 'أيام', icon: 'Clock' },
        ],
    },
    {
        id: 'doc-01b',
        name: 'المستندات الناقصة حسب المستفيد/الوارث',
        category: 'الوثائق',
        icon: 'UserGroupIcon',
        description: 'تتبع المستندات الناقصة لكل مستفيد أو ورثة في الملف',
        fetchData: (filter) => reportsApi.getMissingDocsByPerson(filter),
        exportData: (filter, format) => reportsApi.exportMissingDocsByPerson(filter, format),
        filters: ['fromDate', 'toDate', 'search'],
        columns: [
            { key: 'fileNumber', label: 'رقم الملف' },
            { key: 'retireeName', label: 'اسم المتقاعد' },
            { key: 'militaryNumber', label: 'الرقم العسكري' },
            { key: 'personType', label: 'نوع الشخص' },
            { key: 'personName', label: 'اسم الشخص' },
            { key: 'relationship', label: 'صلة القرابة' },
            { key: 'personPhone', label: 'رقم الهاتف' },
            { key: 'missingCount', label: 'عدد النواقص', type: 'number' },
            { key: 'missingDocsList', label: 'المستندات الناقصة' },
            { key: 'maxDaysMissing', label: 'أقصى أيام التأخير', type: 'number' },
        ],
        charts: [
            { type: 'bar', title: 'عدد النواقص حسب نوع الشخص', nameKey: 'personType', valueKey: 'missingCount', dataKey: 'personType' },
            { type: 'donut', title: 'توزيع صلة القرابة', nameKey: 'relationship', valueKey: 'missingCount', dataKey: 'relationship' },
            { type: 'bar', title: 'توزيع الملفات حسب مدة التأخير', nameKey: '_daysRange', valueKey: '_daysRangeCount', dataKey: 'maxDaysMissing' },
        ],
        summaryCards: [
            { title: 'إجمالي الأشخاص', valueKey: 'totalCount', color: 'blue', format: 'number', variant: 'blue', description: 'مستفيدين وورثة', icon: 'Users' },
            { title: 'إجمالي النواقص', valueKey: 'totalMissing', color: 'red', format: 'number', variant: 'destructive', description: 'مستندات ناقصة', icon: 'AlertTriangle' },
            { title: 'نوع المستفيد', valueKey: 'personTypeCount', color: 'purple', format: 'number', variant: 'purple', description: 'أنواع مختلفة', icon: 'UserGroup' },
            { title: 'صلة القرابة', valueKey: 'relationshipCount', color: 'yellow', format: 'number', variant: 'warning', description: 'علاقات مختلفة', icon: 'Link' },
        ],
    },
    {
        id: 'doc-02',
        name: 'المستندات المنتهية',
        category: 'الوثائق',
        icon: 'DocumentMagnifyingGlassIcon',
        description: 'تتبع المستندات التي انتهت صلاحيتها',
        fetchData: (filter) => reportsApi.getExpiredDocs(filter),
        exportData: (filter, format) => reportsApi.exportExpiredDocs(filter, format),
        filters: ['fromDate', 'toDate', 'search'],
        columns: [
            { key: 'docTypeName', label: 'نوع المستند' },
            { key: 'ownerName', label: 'صاحب المستند' },
            { key: 'ownerType', label: 'النوع' },
            { key: 'expiryDate', label: 'تاريخ الانتهاء', type: 'date' },
            { key: 'daysExpired', label: 'أيام منذ الانتهاء', type: 'number' },
            { key: 'isVerified', label: 'موثق', type: 'boolean' },
        ],
        charts: [
            { type: 'donut', title: 'المستندات حسب النوع', nameKey: 'docTypeName', valueKey: 'daysExpired', dataKey: 'docTypeName' },
            { type: 'histogram', title: 'مدة الانتهاء بالأيام', dataKey: 'daysExpired' },
            { type: 'pie', title: 'الموثقة vs غير الموثقة', nameKey: 'isVerified', valueKey: 'daysExpired', dataKey: 'isVerified' },
        ],
        summaryCards: [
            { title: 'إجمالي المستندات', valueKey: 'totalCount', color: 'orange', format: 'number', variant: 'warning', description: 'مستندات منتهية', icon: 'FileText' },
            { title: 'منتهية', valueKey: 'expiredCount', color: 'red', format: 'number', variant: 'destructive', description: 'تحتاج تجديد', icon: 'XCircle' },
            { title: 'موثقة', valueKey: 'verifiedCount', color: 'green', format: 'number', variant: 'success', description: 'تم التوثيق', icon: 'CheckCircle' },
            { title: 'متوسط الأيام', valueKey: 'avgDays', color: 'yellow', format: 'number', variant: 'amber', description: 'منذ الانتهاء', icon: 'Clock' },
        ],
    },

    // ── Dependents ──
    {
        id: 'dep-01',
        name: 'المستفيدين والورثة',
        category: 'المستحقون',
        icon: 'UsersIcon',
        description: 'تقرير شامل بالمستفيدين والأوصياء حسب الملف التقاعدي',
        fetchData: (filter) => reportsApi.getBeneficiaries(filter),
        exportData: (filter, format) => reportsApi.exportBeneficiaries(filter, format),
        filters: ['fromDate', 'toDate', 'search'],
        columns: [
            { key: 'fileNumber', label: 'رقم الملف' },
            { key: 'retireeName', label: 'اسم المتقاعد' },
            { key: 'militaryNumber', label: 'الرقم العسكري' },
            { key: 'fullName', label: 'اسم المستفيد' },
            { key: 'nationalID', label: 'الهوية' },
            { key: 'relationship', label: 'صلة القرابة' },
            { key: 'personType', label: 'نوع الشخص' },
            { key: 'guardianName', label: 'الوصي' },
            { key: 'personPhone', label: 'رقم الهاتف' },
            { key: 'monthlyAmount', label: 'المبلغ الشهري', type: 'currency' },
        ],
        charts: [
            { type: 'donut', title: 'توزيع صلة القرابة', nameKey: 'relationship', valueKey: 'count', dataKey: 'relationship' },
        ],
        summaryCards: [
            { title: 'إجمالي المستفيدين', valueKey: 'totalCount', color: 'blue', format: 'number', variant: 'blue', description: 'مستفيدين وورثة', icon: 'Users' },
            { title: 'إجمالي المبالغ', valueKey: 'totalAmount', color: 'green', format: 'currency', variant: 'emerald', description: 'المبالغ الشهرية', icon: 'DollarSign' },
            { title: 'متوسط المبلغ', valueKey: 'avgAmount', color: 'purple', format: 'currency', variant: 'purple', description: 'لكل مستفيد', icon: 'BarChart3' },
            { title: 'صلة القرابة', valueKey: 'relationshipCount', color: 'yellow', format: 'number', variant: 'warning', description: 'علاقات مختلفة', icon: 'Link' },
        ],
    },

    // ── Compliance ──
    {
        id: 'com-01',
        name: 'كشوفات الإحاله',
        category: 'الامتثال',
        icon: 'BuildingLibraryIcon',
        description: 'حالة دفعات الاعتماد المرفوعة للجهات الخارجية',
        fetchData: (filter) => reportsApi.getMinistryBatches(filter),
        exportData: (filter, format) => reportsApi.exportMinistryBatches(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'statusId'],
        columns: [
            { key: 'batchNumber', label: 'رقم الدفعة' },
            { key: 'status', label: 'الحالة' },
            { key: 'totalCount', label: 'العدد الكلي', type: 'number' },
            { key: 'processedCount', label: 'العدد المعالج', type: 'number' },
            { key: 'itemCount', label: 'عدد العناصر', type: 'number' },
            { key: 'batchDate', label: 'تاريخ الدفعة', type: 'date' },
            { key: 'submittedDate', label: 'تاريخ الإنشاء', type: 'date' },
        ],
        charts: [
            { type: 'donut', title: 'حالة الدفعات', nameKey: 'status', valueKey: 'count', dataKey: 'status' },
            { type: 'bar', title: 'العدد المعالج لكل دفعة', nameKey: 'batchNumber', valueKey: 'processedCount', dataKey: 'batchNumber' },
        ],
        summaryCards: [
            { title: 'إجمالي الدفعات', valueKey: 'totalCount', color: 'blue', format: 'number', variant: 'blue', description: 'دفعات إحالة', icon: 'FileText' },
            { title: 'المعالجة', valueKey: 'processedCount', color: 'green', format: 'number', variant: 'success', description: 'تمت المعالجة', icon: 'CheckCircle' },
            { title: 'عدد العناصر', valueKey: 'itemCount', color: 'purple', format: 'number', variant: 'purple', description: 'عناصر في الدفعات', icon: 'Users' },
            { title: 'إجمالي المبلغ', valueKey: 'totalAmount', color: 'amber', format: 'currency', variant: 'warning', description: 'المبلغ الإجمالي', icon: 'DollarSign' },
        ],
    },
    {
        id: 'com-02',
        name: 'الموافقات المعلقة',
        category: 'الامتثال',
        icon: 'ClockIcon',
        description: 'تتبع سير الموافقات المعلقة',
        fetchData: (filter) => reportsApi.getPendingApprovals(filter),
        exportData: (filter, format) => reportsApi.exportPendingApprovals(filter, format),
        filters: ['fromDate', 'toDate', 'search'],
        columns: [
            { key: 'requestType', label: 'نوع الطلب' },
            { key: 'projectName', label: 'الكيان/المشروع' },
            { key: 'requestedBy', label: 'مقدم الطلب' },
            { key: 'daysPending', label: 'أيام الانتظار', type: 'number' },
            { key: 'createdAt', label: 'تاريخ الإنشاء', type: 'date' },
        ],
        charts: [
            { type: 'horizontalBar', title: 'عدد الطلبات حسب النوع', nameKey: 'requestType', valueKey: 'count', dataKey: 'requestType' },
            { type: 'histogram', title: 'مدة الانتظار بالأيام', dataKey: 'daysPending' },
            { type: 'area', title: 'اتجاه الطلبات', nameKey: 'month', valueKey: 'count', dataKey: 'createdAt' },
        ],
        summaryCards: [
            { title: 'إجمالي الطلبات', valueKey: 'totalCount', color: 'yellow', format: 'number', variant: 'warning', description: 'طلبات معلقة', icon: 'Clock' },
            { title: 'متوسط الانتظار', valueKey: 'avgDays', color: 'orange', format: 'number', variant: 'orange', description: 'أيام', icon: 'BarChart3' },
            { title: 'أكثر من 30 يوم', valueKey: 'over30Days', color: 'red', format: 'number', variant: 'destructive', description: 'طلبات قديمة', icon: 'AlertTriangle' },
            { title: 'أنواع الطلبات', valueKey: 'requestTypeCount', color: 'blue', format: 'number', variant: 'blue', description: 'أنواع مختلفة', icon: 'Folder' },
        ],
    },

    // ── Admin ──
    {
        id: 'adm-01',
        name: 'سجل النظام (Audit Log)',
        category: 'النظام',
        icon: 'ClipboardDocumentListIcon',
        description: 'سجل حركات النظام للمراقبة والتدقيق',
        fetchData: (filter) => reportsApi.getAuditLog(filter),
        exportData: (filter, format) => reportsApi.exportAuditLog(filter, format),
        filters: ['fromDate', 'toDate', 'search'],
        columns: [
            { key: 'userName', label: 'المستخدم' },
            { key: 'action', label: 'الإجراء' },
            { key: 'tableName', label: 'الجدول' },
            { key: 'recordID', label: 'رقم السجل' },
            { key: 'timestamp', label: 'الوقت', type: 'date' },
        ],
        charts: [
            { type: 'bar', title: 'الحركات حسب الجدول', nameKey: 'tableName', valueKey: 'count', dataKey: 'tableName' },
            { type: 'pie', title: 'الحركات حسب الإجراء', nameKey: 'action', valueKey: 'count', dataKey: 'action' },
            { type: 'area', title: 'اتجاه الحركات اليومية', nameKey: 'date', valueKey: 'count', dataKey: 'date' },
        ],
        summaryCards: [
            { title: 'إجمالي الحركات', valueKey: 'totalActions', color: 'blue', format: 'number', variant: 'blue', description: 'سجلات في النظام', icon: 'BarChart3' },
            { title: 'المستخدمون النشطون', valueKey: 'activeUsers', color: 'green', format: 'number', variant: 'success', description: 'مستخدمين نشطين', icon: 'Users' },
            { title: 'إجمالي السجلات', valueKey: 'totalRecords', color: 'purple', format: 'number', variant: 'purple', description: 'سجلات', icon: 'FileText' },
            { title: 'الحركات اليوم', valueKey: 'todayActions', color: 'yellow', format: 'number', variant: 'warning', description: 'اليوم', icon: 'Clock' },
        ],
    },
    {
        id: 'adm-02',
        name: 'نشاط المستخدمين',
        category: 'النظام',
        icon: 'UserCircleIcon',
        description: 'تقرير بنشاط وصلاحيات مستخدمي النظام',
        fetchData: (filter) => reportsApi.getUserActivity(filter),
        exportData: (filter, format) => reportsApi.exportUserActivity(filter, format),
        filters: ['fromDate', 'toDate', 'search'],
        columns: [
            { key: 'userName', label: 'اسم المستخدم' },
            { key: 'fullName', label: 'الاسم' },
            { key: 'roleName', label: 'الدور' },
            { key: 'branchName', label: 'الفرع' },
            { key: 'lastLogin', label: 'آخر دخول', type: 'date' },
            { key: 'isActive', label: 'نشط', type: 'boolean' },
        ],
        charts: [
            { type: 'donut', title: 'المستخدمون حسب الدور', nameKey: 'roleName', valueKey: 'count', dataKey: 'roleName' },
            { type: 'bar', title: 'المستخدمون حسب الفرع', nameKey: 'branchName', valueKey: 'count', dataKey: 'branchName' },
            { type: 'pie', title: 'الحالة', nameKey: 'isActive', valueKey: 'count', dataKey: 'isActive' },
        ],
        summaryCards: [
            { title: 'إجمالي المستخدمين', valueKey: 'totalUsers', color: 'blue', format: 'number', variant: 'blue', description: 'مستخدمين في النظام', icon: 'Users' },
            { title: 'النشطون', valueKey: 'activeUsers', color: 'green', format: 'number', variant: 'success', description: 'مستخدمين نشطين', icon: 'CheckCircle' },
            { title: 'غير النشطين', valueKey: 'inactiveUsers', color: 'red', format: 'number', variant: 'destructive', description: 'غير نشطين', icon: 'XCircle' },
            { title: 'آخر دخول', valueKey: 'lastLoginDate', color: 'purple', format: 'date', variant: 'purple', description: 'آخر نشاط', icon: 'Clock' },
        ],
    },

    // ── HR ──
    {
        id: 'hr-01',
        name: 'دليل الموظفين الشامل',
        category: 'الموارد البشرية',
        icon: 'UsersIcon',
        description: 'تقرير ببيانات الموظفين والوضع الوظيفي الحالي',
        fetchData: (filter) => reportsApi.getEmployeeDirectory(filter),
        exportData: (filter, format) => reportsApi.exportEmployeeDirectory(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'branchId'],
        columns: [
            { key: 'employeeCode', label: 'الكود' },
            { key: 'fullName', label: 'الاسم الكامل' },
            { key: 'department', label: 'الإدارة' },
            { key: 'jobTitle', label: 'المسمى الوظيفي' },
            { key: 'joiningDate', label: 'تاريخ التعيين', type: 'date' },
            { key: 'status', label: 'الحالة' },
        ],
        charts: [
            { type: 'donut', title: 'الموظفون حسب الإدارة', nameKey: 'department', valueKey: 'count', dataKey: 'department' },
            { type: 'bar', title: 'الموظفون حسب المسمى الوظيفي', nameKey: 'jobTitle', valueKey: 'count', dataKey: 'jobTitle' },
            { type: 'pie', title: 'الحالة الوظيفية', nameKey: 'status', valueKey: 'count', dataKey: 'status' },
        ],
        summaryCards: [
            { title: 'إجمالي الموظفين', valueKey: 'totalEmployees', color: 'blue', format: 'number', variant: 'blue', description: 'الموظفين الحاليين', icon: 'Users' },
            { title: 'النشطون', valueKey: 'activeEmployees', color: 'green', format: 'number', variant: 'success', description: 'emploeed', icon: 'CheckCircle' },
            { title: 'المتربصون', valueKey: 'probationCount', color: 'yellow', format: 'number', variant: 'warning', description: 'فترة التجربة', icon: 'Clock' },
            { title: 'الإدارات', valueKey: 'departmentCount', color: 'purple', format: 'number', variant: 'purple', description: 'إدارة مختلفة', icon: 'BuildingOffice' },
        ],
    },
    {
        id: 'hr-02',
        name: 'رصيد الإجازات',
        category: 'الموارد البشرية',
        icon: 'CalendarIcon',
        description: 'تقرير بأرصدة الإجازات السنوية والمرضية للموظفين',
        fetchData: (filter) => reportsApi.getLeaveBalances(filter),
        exportData: (filter, format) => reportsApi.exportLeaveBalances(filter, format),
        filters: ['fromDate', 'toDate', 'search'],
        columns: [
            { key: 'fullName', label: 'الموظف' },
            { key: 'annualBalance', label: 'رصيد سنوي', type: 'number' },
            { key: 'sickBalance', label: 'رصيد مرضي', type: 'number' },
            { key: 'usedThisYear', label: 'المستخدم هذا العام', type: 'number' },
            { key: 'remaining', label: 'المتبقي', type: 'number' },
        ],
        charts: [
            { type: 'groupedBar', title: 'السنوي vs المرضي', nameKey: 'fullName', valueKey: 'annualBalance', dataKey: 'fullName' },
            { type: 'bar', title: 'المستخدم من الرصيد', nameKey: 'fullName', valueKey: 'usedThisYear', dataKey: 'fullName' },
            { type: 'pie', title: 'حالة الاستخدام', nameKey: 'status', valueKey: 'count', dataKey: 'status' },
        ],
        summaryCards: [
            { title: 'إجمالي الموظفين', valueKey: 'totalEmployees', color: 'blue', format: 'number', variant: 'blue', description: 'لديهم رصيد', icon: 'Users' },
            { title: 'أصحاب رصيد متبقي', valueKey: 'withBalance', color: 'green', format: 'number', variant: 'success', description: 'لم يستنفدوا', icon: 'CheckCircle' },
            { title: 'استنفد رصيده', valueKey: 'exhausted', color: 'red', format: 'number', variant: 'destructive', description: 'رصيد = 0', icon: 'XCircle' },
            { title: 'متوسط الرصيد', valueKey: 'avgBalance', color: 'purple', format: 'number', variant: 'purple', description: 'أيام', icon: 'BarChart3' },
        ],
    },
    {
        id: 'hr-04',
        name: 'تقرير الحضور والانضباط',
        category: 'الموارد البشرية',
        icon: 'ClockIcon',
        description: 'تقرير تفصيلي بمعدلات الحضور والغياب والتأخير',
        fetchData: (filter) => reportsApi.getAttendanceReport(filter),
        exportData: (filter, format) => reportsApi.exportAttendanceReport(filter, format),
        filters: ['fromDate', 'toDate', 'search'],
        columns: [
            { key: 'fullName', label: 'الموظف' },
            { key: 'presentDays', label: 'أيام الحضور', type: 'number' },
            { key: 'absentDays', label: 'أيام الغياب', type: 'number' },
            { key: 'lateMinutes', label: 'دقائق التأخير', type: 'number' },
            { key: 'attendanceRate', label: 'نسبة الانضباط', type: 'percent' },
        ],
        charts: [
            { type: 'horizontalBar', title: 'أيام الحضور حسب الموظف', nameKey: 'fullName', valueKey: 'presentDays', dataKey: 'fullName' },
            { type: 'donut', title: 'معدلات الانضباط', nameKey: 'attendanceLevel', valueKey: 'count', dataKey: 'attendanceLevel' },
            { type: 'bar', title: 'دقائق التأخير', nameKey: 'fullName', valueKey: 'lateMinutes', dataKey: 'fullName' },
        ],
        summaryCards: [
            { title: 'إجمالي الموظفين', valueKey: 'totalEmployees', color: 'blue', format: 'number', variant: 'blue', description: 'مدرجين في التقرير', icon: 'Users' },
            { title: 'متوسط الحضور', valueKey: 'avgAttendance', color: 'green', format: 'percent', variant: 'success', description: 'نسبة الحضور', icon: 'CheckCircle' },
            { title: 'الغياب المتكرر', valueKey: 'chronicAbsent', color: 'red', format: 'number', variant: 'destructive', description: 'يحتاج تدخل', icon: 'AlertTriangle' },
            { title: 'التأخيرات', valueKey: 'totalLateMinutes', color: 'yellow', format: 'number', variant: 'warning', description: 'دقائق', icon: 'Clock' },
        ],
    }, // ── Investment ──
    {
        id: 'inv-01',
        name: 'أداء المحافظ',
        category: 'الاستثمار',
        icon: 'ChartBarIcon',
        description: 'تقرير تفصيلي لربحية وأداء المحافظ الاستثمارية',
        fetchData: (filter) => reportsApi.getPortfolioPerformance(filter),
        exportData: (filter, format) => reportsApi.exportPortfolioPerformance(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'portfolioId'],
        columns: [
            { key: 'portfolioName', label: 'المحفظة' },
            { key: 'totalInvested', label: 'إجمالي الاستثمار', type: 'currency' },
            { key: 'currentValue', label: 'القيمة الحالية', type: 'currency' },
            { key: 'profitLoss', label: 'الربح/الخسارة', type: 'currency' },
            { key: 'roi', label: 'العائد ROI', type: 'percent' },
            { key: 'activeProjects', label: 'المشاريع النشطة', type: 'number' },
        ],
        charts: [
            { type: 'groupedBar', title: 'القيمة الحالية vs الاستثمار', nameKey: 'portfolioName', valueKey: 'totalInvested', dataKey: 'portfolioName' },
            { type: 'line', title: 'العائد على الاستثمار', nameKey: 'portfolioName', valueKey: 'roi', dataKey: 'portfolioName' },
            { type: 'donut', title: 'توزيع المحافظ', nameKey: 'portfolioName', valueKey: 'currentValue', dataKey: 'portfolioName' },
        ],
        summaryCards: [
            { title: 'إجمالي الاستثمار', valueKey: 'totalInvested', color: 'blue', format: 'currency', variant: 'blue', description: 'محفظات استثمارية', icon: 'DollarSign' },
            { title: 'القيمة الحالية', valueKey: 'currentValue', color: 'green', format: 'currency', variant: 'success', description: 'القيمة السوقية', icon: 'TrendingUp' },
            { title: 'الربح/الخسارة', valueKey: 'profitLoss', color: 'purple', format: 'currency', variant: 'purple', description: 'الفرق', icon: 'BarChart3' },
            { title: 'متوسط العائد', valueKey: 'avgRoi', color: 'yellow', format: 'percent', variant: 'warning', description: 'ROI', icon: 'Percent' },
        ],
    },
    {
        id: 'inv-02',
        name: 'نسب الإشغال',
        category: 'الاستثمار',
        icon: 'BuildingOfficeIcon',
        description: 'تقرير معدلات الإشغال والشواغر للعقارات',
        fetchData: (filter) => reportsApi.getOccupancy(filter),
        exportData: (filter, format) => reportsApi.exportOccupancy(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'propertyId'],
        columns: [
            { key: 'propertyName', label: 'العقار' },
            { key: 'projectName', label: 'المشروع' },
            { key: 'totalUnits', label: 'إجمالي الوحدات', type: 'number' },
            { key: 'occupiedUnits', label: 'مؤجرة', type: 'number' },
            { key: 'vacantUnits', label: 'شاغرة', type: 'number' },
            { key: 'occupancyRate', label: 'نسبة الإشغال', type: 'percent' },
        ],
        charts: [
            { type: 'gauge', title: 'نسبة الإشغال', dataKey: 'occupancyRate' },
            { type: 'stackedBar', title: 'الوحدات حسب العقار', nameKey: 'propertyName', valueKey: 'occupiedUnits', dataKey: 'propertyName' },
            { type: 'donut', title: 'ال المؤجرة vs الشاغرة', nameKey: 'status', valueKey: 'count', dataKey: 'status' },
        ],
        summaryCards: [
            { title: 'إجمالي الوحدات', valueKey: 'totalUnits', color: 'blue', format: 'number', variant: 'blue', description: 'وحدات سكنية', icon: 'BuildingOffice' },
            { title: 'المؤجرة', valueKey: 'occupiedUnits', color: 'green', format: 'number', variant: 'success', description: 'مؤجرة حالياً', icon: 'CheckCircle' },
            { title: 'الشاغرة', valueKey: 'vacantUnits', color: 'red', format: 'number', variant: 'destructive', description: 'بانتظار الإيجار', icon: 'XCircle' },
            { title: 'نسبة الإشغال', valueKey: 'occupancyRate', color: 'purple', format: 'percent', variant: 'purple', description: 'نسبة الإشغال', icon: 'Percent' },
        ],
    },
    {
        id: 'inv-03',
        name: 'تحصيل الإيجارات',
        category: 'الاستثمار',
        icon: 'BanknotesIcon',
        description: 'تقرير تحصيل الإيجارات والمبالغ المتأخرة',
        fetchData: (filter) => reportsApi.getRentCollection(filter),
        exportData: (filter, format) => reportsApi.exportRentCollection(filter, format),
        filters: ['fromDate', 'toDate', 'search'],
        columns: [
            { key: 'tenantName', label: 'المستأجر' },
            { key: 'propertyName', label: 'العقار' },
            { key: 'dueAmount', label: 'المبلغ المستحق', type: 'currency' },
            { key: 'paidAmount', label: 'المدفوع', type: 'currency' },
            { key: 'outstandingAmount', label: 'المتبقي', type: 'currency' },
            { key: 'overdueDays', label: 'أيام التأخير', type: 'number' },
            { key: 'paymentStatus', label: 'الحالة' },
        ],
        charts: [
            { type: 'groupedBar', title: 'المدفوع vs المتبقي', nameKey: 'propertyName', valueKey: 'paidAmount', dataKey: 'propertyName' },
            { type: 'donut', title: 'حالة الدفعات', nameKey: 'paymentStatus', valueKey: 'count', dataKey: 'paymentStatus' },
            { type: 'area', title: 'اتجاه التحصيل', nameKey: 'month', valueKey: 'paidAmount', dataKey: 'month' },
        ],
        summaryCards: [
            { title: 'إجمالي المستحق', valueKey: 'totalDue', color: 'blue', format: 'currency', variant: 'blue', description: 'المبلغ المستحق', icon: 'DollarSign' },
            { title: 'المدفوع', valueKey: 'totalPaid', color: 'green', format: 'currency', variant: 'success', description: 'تم التحصيل', icon: 'CheckCircle' },
            { title: 'المتبقي', valueKey: 'totalOutstanding', color: 'red', format: 'currency', variant: 'destructive', description: 'متأخر', icon: 'AlertTriangle' },
            { title: 'نسبة التحصيل', valueKey: 'collectionRate', color: 'purple', format: 'percent', variant: 'purple', description: 'نسبة النجاح', icon: 'Percent' },
        ],
    },
    {
        id: 'inv-04',
        name: 'حالة العقود',
        category: 'الاستثمار',
        icon: 'DocumentTextIcon',
        description: 'تقرير العقود النشطة والمنتهية وتواريخ التجديد',
        fetchData: (filter) => reportsApi.getContractStatus(filter),
        exportData: (filter, format) => reportsApi.exportContractStatus(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'statusId'],
        columns: [
            { key: 'tenantName', label: 'المستأجر' },
            { key: 'propertyName', label: 'العقار' },
            { key: 'status', label: 'حالة العقد' },
            { key: 'startDate', label: 'تاريخ البداية', type: 'date' },
            { key: 'endDate', label: 'تاريخ النهاية', type: 'date' },
            { key: 'annualRent', label: 'القيمة السنوية', type: 'currency' },
            { key: 'daysToExpiry', label: 'أيام للانتهاء', type: 'number' },
        ],
        charts: [
            { type: 'donut', title: 'العقود حسب الحالة', nameKey: 'status', valueKey: 'count', dataKey: 'status' },
            { type: 'bar', title: 'القيمة السنوية حسب العقار', nameKey: 'propertyName', valueKey: 'annualRent', dataKey: 'propertyName' },
            { type: 'histogram', title: 'أيام الانتهاء', dataKey: 'daysToExpiry' },
        ],
        summaryCards: [
            { title: 'إجمالي العقود', valueKey: 'totalCount', color: 'blue', format: 'number', variant: 'blue', description: 'عقود إيجار', icon: 'FileText' },
            { title: 'نشطة', valueKey: 'activeCount', color: 'green', format: 'number', variant: 'success', description: 'سارية', icon: 'CheckCircle' },
            { title: 'منتهية', valueKey: 'expiredCount', color: 'red', format: 'number', variant: 'destructive', description: 'تحتاج تجديد', icon: 'XCircle' },
            { title: 'القيمة الإجمالية', valueKey: 'totalValue', color: 'purple', format: 'currency', variant: 'purple', description: 'قيمة الإيجار', icon: 'DollarSign' },
        ],
    },
    {
        id: 'inv-05',
        name: 'المخاطر الاستثمارية',
        category: 'الاستثمار',
        icon: 'ShieldExclamationIcon',
        description: 'تقرير المخاطر المرتبطة بالمشاريع الاستثمارية',
        fetchData: (filter) => reportsApi.getRiskExposure(filter),
        exportData: (filter, format) => reportsApi.exportRiskExposure(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'projectId'],
        columns: [
            { key: 'projectName', label: 'المشروع' },
            { key: 'riskType', label: 'نوع الخطر' },
            { key: 'severity', label: 'الخطورة' },
            { key: 'status', label: 'الحالة' },
            { key: 'mitigationPlan', label: 'خطة التخفيف' },
            { key: 'identifiedDate', label: 'تاريخ التقييم', type: 'date' },
        ],
        charts: [
            { type: 'donut', title: 'المخاطر حسب النوع', nameKey: 'riskType', valueKey: 'count', dataKey: 'riskType' },
            { type: 'bar', title: 'الخطورة حسب المشروع', nameKey: 'projectName', valueKey: 'severityCount', dataKey: 'projectName' },
            { type: 'pie', title: 'حالة المخاطر', nameKey: 'status', valueKey: 'count', dataKey: 'status' },
        ],
        summaryCards: [
            { title: 'إجمالي المخاطر', valueKey: 'totalCount', color: 'red', format: 'number', variant: 'destructive', description: 'مخاطر مسجلة', icon: 'AlertTriangle' },
            { title: 'عالية الخطورة', valueKey: 'highCount', color: 'orange', format: 'number', variant: 'warning', description: 'تحتاج تدخل', icon: 'ExclamationTriangle' },
            { title: 'مخففة', valueKey: 'mitigatedCount', color: 'green', format: 'number', variant: 'success', description: 'تم التخفيف', icon: 'CheckCircle' },
            { title: 'المشاريع المتأثرة', valueKey: 'projectCount', color: 'blue', format: 'number', variant: 'blue', description: 'مشاريع', icon: 'BuildingOffice' },
        ],
    },
    {
        id: 'inv-06',
        name: 'طلبات الصيانة',
        category: 'الاستثمار',
        icon: 'WrenchScrewdriverIcon',
        description: 'تقرير طلبات الصيانة وتكاليفها',
        fetchData: (filter) => reportsApi.getMaintenanceSummary(filter),
        exportData: (filter, format) => reportsApi.exportMaintenanceSummary(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'propertyId'],
        columns: [
            { key: 'propertyName', label: 'العقار' },
            { key: 'description', label: 'الوصف' },
            { key: 'priority', label: 'الأولوية' },
            { key: 'status', label: 'الحالة' },
            { key: 'estimatedCost', label: 'التكلفة التقديرية', type: 'currency' },
            { key: 'actualCost', label: 'التكلفة الفعلية', type: 'currency' },
            { key: 'responseDays', label: 'أيام الاستجابة', type: 'number' },
        ],
        charts: [
            { type: 'bar', title: 'الطلبات حسب الأولوية', nameKey: 'priority', valueKey: 'count', dataKey: 'priority' },
            { type: 'donut', title: 'الطلبات حسب الحالة', nameKey: 'status', valueKey: 'count', dataKey: 'status' },
            { type: 'groupedBar', title: 'التكلفة التقديرية vs الفعلية', nameKey: 'propertyName', valueKey: 'estimatedCost', dataKey: 'propertyName' },
        ],
        summaryCards: [
            { title: 'إجمالي الطلبات', valueKey: 'totalCount', color: 'blue', format: 'number', variant: 'blue', description: 'طلبات صيانة', icon: 'WrenchScrewdriver' },
            { title: 'قيد التنفيذ', valueKey: 'inProgressCount', color: 'yellow', format: 'number', variant: 'warning', description: 'قيد المعالجة', icon: 'Clock' },
            { title: 'مكتملة', valueKey: 'completedCount', color: 'green', format: 'number', variant: 'success', description: 'تم الانتهاء', icon: 'CheckCircle' },
            { title: 'التكلفة الإجمالية', valueKey: 'totalCost', color: 'purple', format: 'currency', variant: 'purple', description: 'التكاليف', icon: 'DollarSign' },
        ],
    },
    {
        id: 'inv-07',
        name: 'الميزانية والمصروفات',
        category: 'الاستثمار',
        icon: 'ChartPieIcon',
        description: 'مقارنة الميزانية المخططة مع المصروفات الفعلية',
        fetchData: (filter) => reportsApi.getBudgetVsActual(filter),
        exportData: (filter, format) => reportsApi.exportBudgetVsActual(filter, format),
        filters: ['fromDate', 'toDate', 'search', 'fiscalYear'],
        columns: [
            { key: 'projectName', label: 'المشروع' },
            { key: 'fiscalYear', label: 'السنة المالية' },
            { key: 'plannedAmount', label: 'المبلغ المخطط', type: 'currency' },
            { key: 'actualSpent', label: 'المنصرف الفعلي', type: 'currency' },
            { key: 'variance', label: 'الانحراف', type: 'currency' },
            { key: 'variancePct', label: 'نسبة الانحراف', type: 'percent' },
        ],
        charts: [
            { type: 'groupedBar', title: 'المخطط vs الفعلي', nameKey: 'projectName', valueKey: 'plannedAmount', dataKey: 'projectName' },
            { type: 'gauge', title: 'نسبة التنفيذ', dataKey: 'variancePct' },
            { type: 'line', title: 'اتجاه الانحراف', nameKey: 'fiscalYear', valueKey: 'variancePct', dataKey: 'fiscalYear' },
        ],
        summaryCards: [
            { title: 'إجمالي المخطط', valueKey: 'totalPlanned', color: 'blue', format: 'currency', variant: 'blue', description: 'الميزانية', icon: 'DollarSign' },
            { title: 'إجمالي الفعلي', valueKey: 'totalActual', color: 'green', format: 'currency', variant: 'success', description: 'المنصرف', icon: 'TrendingUp' },
            { title: 'الانحراف الكلي', valueKey: 'totalVariance', color: 'red', format: 'currency', variant: 'destructive', description: 'الفرق', icon: 'AlertTriangle' },
            { title: 'نسبة التنفيذ', valueKey: 'executionRate', color: 'purple', format: 'percent', variant: 'purple', description: 'نسبة الصرف', icon: 'Percent' },
        ],
    },
];
