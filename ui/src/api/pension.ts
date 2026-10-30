/**
 * عملاء API لنظام إدارة المعاشات 
 */
import type { RecordEntitledPersonDeathRequest } from "@/lib/entitledPersonRequests";
import { entitledPersonFormData, entitledPersonMultipart, reactivateEntitledPersonForm, recordEntitledPersonDeathForm, stopEntitledPersonForm } from "@/lib/entitledPersonRequests";
import type { RecentActivity } from "@/types/dashboard";
import type {
  CreateEntitledPersonDto,
  CreateExemptionDto,
  CreateFinancialEntitlementDto,
  CreateSuspensionDto,
  DocumentUploadStats,
  EndSuspensionDto,
  // المستحقون
  EntitledPersonDetail,
  ExemptionDetail,
  // الاستحقاقات
  FinancialEntitlementDetail,
  // التسوية
  LinkSettlementItemDto,
  MinistryApprovalBatch,
  MinistryReferral,
  // أنواع مشتركة
  PagedResult,
  PaymentBookReview,
  PaymentBookStats,
  // بطاقات الصرف
  PaymentCardDetail,
  // السجلات والإحصاء
  PensionStatistics,
  PerformanceStatistics,
  PledgeDetail,
  ReactivateEntitledPersonDto,
  SalaryBatchDetail,
  SalaryPaymentItemDetail,
  ServiceHistoryData,
  // التسوية
  SettlementBatchListItem,
  SettlementDecision,
  SettlementItemDetail,
  // البحث الاجتماعي
  SocialResearchFilters,
  SocialResearchResult,
  StatusTransitionLog,
  StopEntitledPersonDto,
  // الإيقافات
  SuspensionDetail,
  SuspensionStats,
  UpdateEntitledPersonDto,
} from "@/types/pension";

import api from "./axios";
import { documentsApi } from "./documents";

// ══════════════════════════════════════════════════════════════
// 1. البحث الاجتماعي والفحص
// ══════════════════════════════════════════════════════════════

export const socialResearchApi = {
  /** البحث الاجتماعي الشامل */
  search: (params: SocialResearchFilters) =>
    api.get<{ data: PagedResult<SocialResearchResult> }>("/retirees", { params })
      .then(r => r.data.data),

  /** جلب متقاعد بالرقم الوطني */
  getByNationalId: (nationalId: string) =>
    api.get<{ data: SocialResearchResult }>(`/retirees/national-id/${nationalId}`)
      .then(r => r.data.data),

  /** جلب متقاعد بالرقم العسكري */
  getByMilitaryNumber: (militaryNumber: string) =>
    api.get<{ data: SocialResearchResult }>(`/retirees/military/${militaryNumber}`)
      .then(r => r.data.data),
};

export const paymentBookReviewApi = {
  /** مراجعة دفاتر الصرف */
  getReview: (params: {
    page?: number;
    pageSize?: number;
    search?: string;
    status?: string;
    bookType?: string;
    branchId?: number;
    expiringWithinDays?: number;
  }) =>
    api.get<{ data: PagedResult<PaymentBookReview> }>("/payment-books/review", { params })
      .then(r => r.data.data),

  /** إحصائيات دفاتر الصرف */
  getStats: () =>
    api.get<{ data: PaymentBookStats }>("/payment-books/stats")
      .then(r => r.data.data),
};

export const ministryReferralApi = {
  /** جلب كشوفات الاعتماد */
  getBatches: (params: { page?: number; pageSize?: number }) =>
    api.get<{ data: PagedResult<MinistryApprovalBatch> }>("/ministry-approvals", { params })
      .then(r => r.data.data),

  /** جلب تفاصيل الكشف */
  getBatchById: (id: number) =>
    api.get<{ data: MinistryApprovalBatch }>(`/ministry-approvals/${id}`)
      .then(r => r.data.data),

  /** جلب بنود الكشف */
  getBatchItems: (batchId: number, params: {
    page?: number;
    pageSize?: number;
    search?: string;
    isProcessed?: boolean;
  }) =>
    api.get<{ data: PagedResult<MinistryReferral> }>(`/ministry-approvals/batches/${batchId}/items`, { params })
      .then(r => r.data.data),

  /** تعليم بند كمعالج */
  processItem: (itemId: number) =>
    api.put<{ data: boolean }>(`/ministry-approvals/items/${itemId}/process`)
      .then(r => r.data.data),
};

export const documentReviewApi = {
  /** جلب ملفات تحتاج رفع وثائق */
  getForDocumentUpload: () =>
    api.get<{ data: PagedResult<any> }>("/retirement-files/for-document-upload")
      .then(r => r.data.data),

  /** إحصائيات رفع الوثائق */
  getStats: () =>
    api.get<{ data: DocumentUploadStats }>("/retirement-files/document-upload-stats")
      .then(r => r.data.data),

  /** النواقص التلقائية */
  getAutoDetected: documentsApi.getAutoDetectedMissing as (fileId: number) => Promise<any[]>,

  /** النواقص اليدوية */
  getManualMissing: documentsApi.getMissingByFileId as (fileId: number) => Promise<any[]>,

  /** تعليم وثيقة كمسلّمة */
  fulfillDocument: async (id: number) => {
    await documentsApi.fulfillMissing(id);
    return true;
  },
};

// ══════════════════════════════════════════════════════════════
// 2. التسوية والربط
// ══════════════════════════════════════════════════════════════

export const settlementApi = {
  /** جلب كشوفات التسوية */
  getBatches: (params: { page?: number; pageSize?: number }) =>
    api.get<{ data: PagedResult<SettlementBatchListItem> }>("/settlements/batches", { params })
      .then(r => r.data.data),

  /** جلب تفاصيل الكشف */
  getBatchById: (id: number) =>
    api.get<{ data: SettlementBatchListItem }>(`/settlements/batches/${id}`)
      .then(r => r.data.data),

  /** جلب بنود التسوية */
  getBatchItems: (batchId: number, params: {
    page?: number;
    pageSize?: number;
    search?: string;
  }) =>
    api.get<{ data: PagedResult<SettlementItemDetail> }>(`/settlements/batches/${batchId}/items`, { params })
      .then(r => r.data.data),

  /** جلب قرارات التسوية */
  getDecisions: (params: { page?: number; pageSize?: number }) =>
    api.get<{ data: PagedResult<SettlementDecision> }>("/settlements/decisions", { params })
      .then(r => r.data.data),

  /** ربط بند بمتقاعد */
  linkItemToRetiree: (dto: LinkSettlementItemDto) =>
    api.post<{ data: boolean }>("/settlements/link", dto)
      .then(r => r.data.data),

  /** اعتماد الكشف */
  approveBatch: (id: number) =>
    api.put<{ data: boolean }>(`/settlements/batches/${id}/approve`)
      .then(r => r.data.data),

  /** تصدير القرار إلى Excel */
  exportDecisionToExcel: (decisionId: number) =>
    api.get(`/settlements/decisions/${decisionId}/export`, { responseType: "blob" }),

  /** تصدير تقرير الفشل */
  exportFailureReport: (decisionId: number) =>
    api.get(`/settlements/decisions/${decisionId}/failures-report`, { responseType: "blob" }),
};

export const serviceHistoryApi = {
  /** جلب بيانات الخدمة السابقة */
  getData: (retireeId: number) =>
    api.get<{ data: ServiceHistoryData }>(`/retirees/${retireeId}/service-history`)
      .then(r => r.data.data),

  /** تحديث تواريخ الخدمة */
  updateDates: (fileId: number, data: {
    serviceStartDate?: string;
    serviceEndDate?: string;
    retirementDate?: string;
  }) =>
    api.put<{ data: boolean }>(`/retirement-files/${fileId}/service-dates`, data)
      .then(r => r.data.data),
};

// ══════════════════════════════════════════════════════════════
// 3. المستحقون
// ══════════════════════════════════════════════════════════════

export const entitledPersonApi = {
  /** جلب المستحقين لملف */
  getByFileId: (fileId: number) =>
    api.get<{ data: EntitledPersonDetail[] }>(`/entitled-persons/by-file/${fileId}`)
      .then(r => r.data.data),

  /** جلب تفاصيل مستحق */
  getById: (id: number) =>
    api.get<{ data: EntitledPersonDetail }>(`/entitled-persons/${id}`)
      .then(r => r.data.data),

  /** إنشاء مستحق جديد */
  create: (dto: CreateEntitledPersonDto) => {
    const { birthDate, ...fields } = dto;
    return api.post<{ data: EntitledPersonDetail }>("/entitled-persons", entitledPersonFormData({ ...fields, dateOfBirth: birthDate }), entitledPersonMultipart)
      .then(r => r.data.data);
  },

  /** تحديث بيانات مستحق */
  update: (id: number, dto: UpdateEntitledPersonDto) => {
    const { birthDate, ...fields } = dto;
    return api.put<{ data: EntitledPersonDetail }>(`/entitled-persons/${id}`, entitledPersonFormData({ ...fields, dateOfBirth: birthDate }), entitledPersonMultipart)
      .then(r => r.data.data);
  },

  /** إيقاف الاستحقاق */
  stop: (id: number, dto: StopEntitledPersonDto) =>
    api.put<{ data: boolean }>(`/entitled-persons/${id}/stop`, stopEntitledPersonForm(dto), entitledPersonMultipart)
      .then(r => r.data.data),

  /** إعادة تفعيل الاستحقاق */
  reactivate: (id: number, dto: ReactivateEntitledPersonDto) =>
    api.put<{ data: boolean }>(`/entitled-persons/${id}/reactivate`, reactivateEntitledPersonForm(dto), entitledPersonMultipart)
      .then(r => r.data.data),

  /** تسجيل وفاة */
  recordDeath: (id: number, data: RecordEntitledPersonDeathRequest) =>
    api.post<{ data: boolean }>(`/entitled-persons/${id}/death`, recordEntitledPersonDeathForm(data), entitledPersonMultipart)
      .then(r => r.data.data),

  /** حذف مستحق (حذف ناعم) */
  delete: (id: number) =>
    api.delete<{ data: boolean }>(`/entitled-persons/${id}`)
      .then(r => r.data.data),
};

// ══════════════════════════════════════════════════════════════
// 4. الإيقافات والتعهدات
// ══════════════════════════════════════════════════════════════

export const suspensionApi = {
  /** جلب الإيقافات لملف */
  getByFileId: (fileId: number) =>
    api.get<{ data: SuspensionDetail[] }>(`/suspensions/by-file/${fileId}`)
      .then(r => r.data.data),

  /** جلب جميع الإيقافات */
  getAll: (params: {
    page?: number;
    pageSize?: number;
    status?: string;
    fromDate?: string;
    toDate?: string;
    search?: string;
  }) =>
    api.get<{ data: PagedResult<SuspensionDetail> }>("/suspensions", { params })
      .then(r => r.data.data),

  /** إنشاء إيقاف مع تعهد */
  createWithPledge: (dto: CreateSuspensionDto) =>
    api.post<{ data: SuspensionDetail }>("/suspensions", dto)
      .then(r => r.data.data),

  /** إنهاء الإيقاف */
  end: (id: number, dto: EndSuspensionDto) =>
    api.put<{ data: boolean }>(`/suspensions/${id}/end`, dto)
      .then(r => r.data.data),

  /** إحصائيات الإيقافات */
  getStats: () =>
    api.get<{ data: SuspensionStats }>("/suspensions/stats")
      .then(r => r.data.data),
};

export const pledgeApi = {
  /** جلب التعهدات لملف */
  getByFileId: (fileId: number) =>
    api.get<{ data: PledgeDetail[] }>(`/document-pledges/by-file/${fileId}`)
      .then(r => r.data.data),

  /** جلب تعهد */
  getById: (id: number) =>
    api.get<{ data: PledgeDetail }>(`/document-pledges/${id}`)
      .then(r => r.data.data),

  /** إنشاء تعهد */
  create: (dto: {
    fileId: number;
    missingDocumentIds: number[];
    deadline: string;
    pledgedByName: string;
    pledgedByType: string;
    reason?: string;
  }) =>
    api.post<{ data: PledgeDetail }>("/document-pledges", dto)
      .then(r => r.data.data),

  /** تعديل تعهد */
  update: (id: number, dto: { deadline?: string; reason?: string }) =>
    api.put<{ data: boolean }>(`/document-pledges/${id}`, dto)
      .then(r => r.data.data),

  /** تمديد التعهد */
  extend: (id: number, data: { newDeadline: string; reason: string }) =>
    api.put<{ data: boolean }>(`/document-pledges/${id}/extend`, data)
      .then(r => r.data.data),

  /** تعليم كمنجز */
  fulfill: (id: number) =>
    api.put<{ data: boolean }>(`/document-pledges/${id}/fulfill`)
      .then(r => r.data.data),

  /** حذف تعهد */
  delete: (id: number) =>
    api.delete<{ data: boolean }>(`/document-pledges/${id}`)
      .then(r => r.data.data),
};

export const exemptionApi = {
  /** جلب الإعفاءات لملف */
  getByFileId: (fileId: number) =>
    api.get<{ data: ExemptionDetail[] }>(`/document-exemptions/by-file/${fileId}`)
      .then(r => r.data.data),

  /** جلب إعفاء */
  getById: (id: number) =>
    api.get<{ data: ExemptionDetail }>(`/document-exemptions/${id}`)
      .then(r => r.data.data),

  /** إنشاء إعفاء */
  create: (dto: CreateExemptionDto) =>
    api.post<{ data: ExemptionDetail }>("/document-exemptions", dto)
      .then(r => r.data.data),

  /** حذف إعفاء */
  delete: (id: number) =>
    api.delete<{ data: boolean }>(`/document-exemptions/${id}`)
      .then(r => r.data.data),
};

// ══════════════════════════════════════════════════════════════
// 5. الاستحقاقات المالية
// ══════════════════════════════════════════════════════════════

export const financialEntitlementApi = {
  /** جلب الاستحقاقات لمتقاعد */
  getByRetireeId: (retireeId: number) =>
    api.get<{ data: FinancialEntitlementDetail[] }>(`/financial-entitlements/by-retiree/${retireeId}`)
      .then(r => r.data.data),

  /** جلب تفاصيل استحقاق */
  getById: (id: number) =>
    api.get<{ data: FinancialEntitlementDetail }>(`/financial-entitlements/${id}`)
      .then(r => r.data.data),

  /** إنشاء استحقاق */
  create: (dto: CreateFinancialEntitlementDto) =>
    api.post<{ data: FinancialEntitlementDetail }>("/financial-entitlements", dto)
      .then(r => r.data.data),

  /** تحديث استحقاق */
  update: (id: number, dto: Partial<CreateFinancialEntitlementDto>) =>
    api.put<{ data: boolean }>(`/financial-entitlements/${id}`, dto)
      .then(r => r.data.data),

  /** إنهاء استحقاق */
  end: (id: number, data: { endDate: string; reason?: string }) =>
    api.put<{ data: boolean }>(`/financial-entitlements/${id}/end`, data)
      .then(r => r.data.data),
};

export const salaryBatchApi = {
  /** جلب كشوفات الصرف */
  getBatches: (params: { page?: number; pageSize?: number }) =>
    api.get<{ data: PagedResult<SalaryBatchDetail> }>("/salary-batches", { params })
      .then(r => r.data.data),

  /** جلب تفاصيل الكشف */
  getBatchById: (id: number) =>
    api.get<{ data: SalaryBatchDetail }>(`/salary-batches/${id}`)
      .then(r => r.data.data),

  /** جلب بنود الدفع */
  getBatchItems: (batchId: number, params: {
    page?: number;
    pageSize?: number;
    search?: string;
  }) =>
    api.get<{ data: PagedResult<SalaryPaymentItemDetail> }>(`/salary-batches/${batchId}/items`, { params })
      .then(r => r.data.data),

  /** اعتماد الكشف */
  approveBatch: (id: number) =>
    api.put<{ data: boolean }>(`/salary-batches/${id}/approve`)
      .then(r => r.data.data),

  /** تصدير الكشف */
  exportBatch: (id: number) =>
    api.get(`/salary-batches/${id}/export`, { responseType: "blob" }),
};

// ══════════════════════════════════════════════════════════════
// 6. السجلات والإحصاء
// ══════════════════════════════════════════════════════════════

export const pensionStatsApi = {
  /** جلب الإحصائيات الشاملة */
  getStatistics: () =>
    api.get<{ data: PensionStatistics }>("/dashboard/pension-statistics")
      .then(r => r.data.data),
};

export const performanceStatsApi = {
  /** جلب إحصائيات الأداء الشهري */
  getStatistics: () =>
    api.get<{ data: PerformanceStatistics }>("/dashboard/performance-statistics")
      .then(r => r.data.data),
};

export const recentActivitiesApi = {
  /** جلب آخر عمليات النظام */
  getRecent: () =>
    api.get<{ data: RecentActivity[] }>("/dashboard/recent-activities")
      .then(r => r.data.data),
};

export const statusTransitionApi = {
  /** جلب سجل تحولات حالة ملف */
  getByFileId: (fileId: number) =>
    api.get<{ data: StatusTransitionLog[] }>(`/retirement-files/${fileId}/status-transitions`)
      .then(r => r.data.data),

  /** جلب جميع التحولات */
  getAll: (params: {
    page?: number;
    pageSize?: number;
    fileId?: number;
    newStatusId?: number;
    fromDate?: string;
    toDate?: string;
    userId?: number;
  }) =>
    api.get<{ data: PagedResult<StatusTransitionLog> }>("/status-transitions", { params })
      .then(r => r.data.data),
};

// ══════════════════════════════════════════════════════════════
// 7. بطاقات صرف المعاشات
// ══════════════════════════════════════════════════════════════

export const paymentCardApi = {
  /** جلب بطاقات الصرف */
  getAll: (params: {
    page?: number;
    pageSize?: number;
    status?: string;
    bookType?: string;
    search?: string;
  }) =>
    api.get<{ data: PagedResult<PaymentCardDetail> }>("/payment-cards", { params })
      .then(r => r.data.data),

  /** جلب تفاصيل البطاقة */
  getById: (id: number) =>
    api.get<{ data: PaymentCardDetail }>(`/payment-cards/${id}`)
      .then(r => r.data.data),

  /** إصدار بطاقة جديدة */
  create: (dto: {
    fileId: number;
    bookType: string;
    recipientName?: string;
    recipientType?: string;
    issuingBranchId?: number;
  }) =>
    api.post<{ data: PaymentCardDetail }>("/payment-cards", dto)
      .then(r => r.data.data),

  /** تحديث البطاقة */
  update: (id: number, dto: Partial<PaymentCardDetail>) =>
    api.put<{ data: boolean }>(`/payment-cards/${id}`, dto)
      .then(r => r.data.data),

  /** إلغاء البطاقة */
  cancel: (id: number, reason: string) =>
    api.put<{ data: boolean }>(`/payment-cards/${id}/cancel`, { reason })
      .then(r => r.data.data),
};

// ══════════════════════════════════════════════════════════════
// 9. التوثيق
// ══════════════════════════════════════════════════════════════

export const archiveApi = {
  /** جلب الأرشيف */
  getAll: (params: {
    page?: number;
    pageSize?: number;
    search?: string;
    closedDateFrom?: string;
    closedDateTo?: string;
  }) =>
    api.get<{ data: PagedResult<any> }>("/archive", { params })
      .then(r => r.data.data),

  /** بحث في الأرشيف */
  search: (params: {
    search?: string;
    closedDateFrom?: string;
    closedDateTo?: string;
  }) =>
    api.get<{ data: PagedResult<any> }>("/archive/search", { params })
      .then(r => r.data.data),

  /** أرشفة ملف */
  archiveFile: (fileId: number) =>
    api.post<{ data: boolean }>(`/archive/files/${fileId}`)
      .then(r => r.data.data),

  /** استعادة من الأرشيف */
  unarchiveFile: (fileId: number) =>
    api.delete<{ data: boolean }>(`/archive/files/${fileId}`)
      .then(r => r.data.data),
};

// ══════════════════════════════════════════════════════════════
// 10. بيانات مرجعية
// ══════════════════════════════════════════════════════════════

export const lookupApi = {
  /** الحالات الاجتماعية */
  getSocialStatuses: () =>
    api.get<{ data: { id: number; name: string }[] }>("/lookups/social-statuses")
      .then(r => r.data.data),

  /** الحالات الصحية */
  getHealthStatuses: () =>
    api.get<{ data: { id: number; name: string }[] }>("/lookups/health-statuses")
      .then(r => r.data.data),

  /** أنواع الوثائق */
  getDocumentTypes: () =>
    api.get<{ data: { id: number; name: string }[] }>("/lookups/document-types")
      .then(r => r.data.data),

  /** المحافظات */
  getGovernorates: () =>
    api.get<{ data: { id: number; name: string }[] }>("/lookups/governorates")
      .then(r => r.data.data),

  /** الفروع */
  getBranches: () =>
    api.get<{ data: { id: number; name: string }[] }>("/lookups/branches")
      .then(r => r.data.data),

  /** الفئات */
  getCategories: () =>
    api.get<{ data: { id: number; name: string }[] }>("/lookups/retiree-categories")
      .then(r => r.data.data),

  /** صلات القرابة */
  getRelationships: () =>
    api.get<{ data: { id: number; name: string }[] }>("/lookups/relationships")
      .then(r => r.data.data),

  /** البنوك */
  getBanks: () =>
    api.get<{ data: { id: number; name: string }[] }>("/lookups/banks")
      .then(r => r.data.data),
};
