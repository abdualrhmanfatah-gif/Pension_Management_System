/**
 * أنواع TypeScript لنظام إدارة المعاشات 
 */

import type { PagedResult } from "@/types/api";

export type { PagedResult };

// ══════════════════════════════════════════════════════════════
// 1. البحث الاجتماعي والفحص
// ══════════════════════════════════════════════════════════════

/** فلاتر البحث الاجتماعي */
export interface SocialResearchFilters {
  search?: string;
  socialStatusId?: number;
  healthStatusId?: number;
  isAlive?: boolean;
  governorate?: string;
  city?: string;
  categoryId?: number;
  hasFiles?: boolean;
  retirementDateFrom?: string;
  retirementDateTo?: string;
  page?: number;
  pageSize?: number;
}

/** نتيجة البحث الاجتماعي */
export interface SocialResearchResult {
  retireeID: number;
  fullName: string;
  nationalID: string;
  militaryNumber: string | null;
  linkNumber: string | null;
  rankName: string | null;
  unitName: string | null;
  socialStatusName: string | null;
  healthStatusName: string | null;
  phone: string | null;
  governorate: string | null;
  city: string | null;
  isAlive: boolean;
  fileCount: number;
  fileID: number | null;
}

/** مراجعة دفتر الصرف */
export interface PaymentBookReview {
  paymentBookID: number;
  bookNumber: string;
  recipientName: string | null;
  recipientType: string | null;
  fileNumber: string | null;
  retireeName: string | null;
  retireeId: number | null;
  nationalID: string | null;
  militaryNumber: string | null;
  phone: string | null;
  bookType: string;
  legacyColor: string | null;
  issueDate: string;
  expiryDate: string | null;
  status: string;
  issuingBranchName: string | null;
  issuingBranchID: number | null;
  changeReason: string | null;
  notes: string | null;
  daysUntilExpiry: number | null;
  isExpiringSoon: boolean;
  bookImagePaths: string[];
}

/** إحصائيات دفاتر الصرف */
export interface PaymentBookStats {
  totalActive: number;
  totalExpired: number;
  totalLost: number;
  totalDamaged: number;
  expiringWithin30Days: number;
  activeCount: number;
  expiredCount: number;
  lostCount: number;
  damagedCount: number;
  totalCount: number;
}

/** محال إلى التقاعد من الوزارة */
export interface MinistryReferral {
  approvalItemID: number;
  batchId: number;
  serialNumber: string | null;
  fullName: string;
  militaryNumber: string;
  gender: number;
  rankName: string | null;
  rankId: number | null;
  unitName: string | null;
  enrollmentDate: string | null;
  retirementDate: string | null;
  salary: number | null;
  categoryName: string | null;
  categoryId: number | null;
  linkNumber: string | null;
  isProcessed: boolean;
  processedAt: string | null;
  hasRetirementFile: boolean;
  retirementFileId: number | null;
  notes: string | null;
}

/** كشف اعتماد الوزارة */
export interface MinistryApprovalBatch {
  batchID: number;
  batchNumber: string;
  batchDate: string | null;
  receivedDate: string | null;
  totalCount: number;
  processedCount: number;
  status: string;
  importedByUserName: string | null;
  notes: string | null;
}

/** إحصائيات الوثائق الناقصة */
export interface DocumentUploadStats {
  totalFilesWithMissing: number;
  totalMissingDocuments: number;
  manualMissing: number;
  autoDetectedMissing: number;
  filesWithPledges: number;
  filesWithExemptions: number;
}

// ══════════════════════════════════════════════════════════════
// 2. التسوية والربط
// ══════════════════════════════════════════════════════════════

/** كشف تسوية */
export interface SettlementBatchListItem {
  batchID: number;
  batchNumber: string;
  receivedDate: string;
  totalCount: number;
  sourceRowsCount: number;
  skippedCount: number;
  totalAmount: number | null;
  status: string;
  createdByName: string | null;
  failureReportJson: string | null;
}

/** بند تسوية */
export interface SettlementItemDetail {
  settlementItemID: number;
  batchId: number;
  serialNumber: string | null;
  fullName: string;
  militaryNumber: string;
  currentRankId: number | null;
  currentRankName: string | null;
  processedRankId: number | null;
  processedRankName: string | null;
  amount: number | null;
  deductions: number | null;
  netAmount: number | null;
  isLinked: boolean;
  retireeId: number | null;
  retireeName: string | null;
  approvalItemId: number | null;
  decisionId: number | null;
  decisionNumber: string | null;
  decisionDate: string | null;
  decisionType: string | null;
  notes: string | null;
  createdAt: string;
}

/** قرار تسوية */
export interface SettlementDecision {
  decisionID: number;
  decisionNumber: string;
  decisionDate: string | null;
  decisionType: string | null;
  notes: string | null;
  itemCount: number;
  totalAmount: number | null;
  status: string;
  createdByName: string | null;
}

/** بيانات الخدمة السابقة */
export interface ServiceHistoryData {
  retireeId: number;
  fullName: string;
  militaryNumber: string | null;
  enrollmentDate: string | null;
  retirementDate: string | null;
  yearsOfService: number | null;
  serviceStartDate: string | null;
  serviceEndDate: string | null;
  rankName: string | null;
  baseSalary: number | null;
}

// ══════════════════════════════════════════════════════════════
// 3. المستحقون
// ══════════════════════════════════════════════════════════════

/** تفاصيل المستحق */
export interface EntitledPersonDetail {
  entitledID: number;
  fileID: number;
  fullName: string;
  nationalID: string | null;
  relationshipId: number | null;
  relationshipName: string | null;
  birthDate: string | null;
  ageYears: number | null;
  phone: string | null;
  socialStatusId: number | null;
  socialStatusName: string | null;
  healthStatusId: number | null;
  healthStatusName: string | null;
  isActive: boolean;
  isDeceased: boolean;
  deceasedDate: string | null;
  hasDisabilityException: boolean;
  stopCode: string | null;
  stopDate: string | null;
  stopReason: string | null;
  hasActiveGuardian: boolean;
  hasNationalIdDocument: boolean;
  hasPersonalPhoto: boolean;
  personalPhotoPath: string | null;
  pendingReviewStatus: number;
}

/** إيقاف المستحق */
export interface StopEntitledPersonDto {
  stopCode: string;
  stopReason: string;
  stopDate?: string;
}

/** إعادة تفعيل المستحق */
export interface ReactivateEntitledPersonDto {
  reactivateCode: string;
  reason: string;
}

/** سبب الإيقاف */
export type StopCode =
  | "AGE_LIMIT"
  | "DAUGHTER_MARRIED"
  | "WIDOW_REMARRIED"
  | "DECEASED"
  | "ADMIN_DECISION"
  | "EMPLOYED"
  | "MISSING_DATA"
  | "RETIRER_ALIVE";

/** سبب إعادة التفعيل */
export type ReactivateCode =
  | "DIVORCED"
  | "HUSBAND_DECEASED"
  | "DISABILITY_REPORT"
  | "ADMIN_DECISION";

// ══════════════════════════════════════════════════════════════
// 4. الإيقافات والتعهدات
// ══════════════════════════════════════════════════════════════

/** تفاصيل الإيقاف */
export interface SuspensionDetail {
  suspensionID: number;
  fileId: number;
  fileNumber: string | null;
  retireeName: string | null;
  suspensionDate: string;
  suspensionReason: string | null;
  suspensionSource: string | null;
  status: string;
  endDate: string | null;
  endReason: string | null;
  createdByName: string | null;
  endedByUserName: string | null;
  createdAt: string;
  linkedPledge: PledgeDetail | null;
}

/** تعهد */
export interface PledgeDetail {
  pledgeID: number;
  pledgeNumber: string;
  pledgeDate: string;
  deadline: string;
  status: string;
  reason: string | null;
  fulfilledAt: string | null;
  pledgedByType: string | null;
  pledgedByName: string | null;
  isExtended: boolean;
  extendedAt: string | null;
  extensionReason: string | null;
  originalDeadline: string | null;
  missingDocumentCount: number;
}

/** إعفاء */
export interface ExemptionDetail {
  exemptionID: number;
  fileId: number;
  directiveSource: string;
  exemptionDate: string;
  notes: string | null;
  createdAt: string;
}

/** إحصائيات الإيقافات */
export interface SuspensionStats {
  activeSuspensions: number;
  pendingPledges: number;
  activeExemptions: number;
  totalSuspendedAmount: number;
}

// ══════════════════════════════════════════════════════════════
// 5. الاستحقاقات المالية
// ══════════════════════════════════════════════════════════════

/** استحقاق مالي */
export interface FinancialEntitlementDetail {
  entitlementID: number;
  retireeId: number;
  retireeName: string | null;
  rankName: string | null;
  amount: number;
  effectiveDate: string;
  endDate: string | null;
  isActive: boolean;
  isExpired: boolean;
  notes: string | null;
  createdAt: string;
}

/** كشف رواتب */
export interface SalaryBatchDetail {
  batchID: number;
  batchNumber: string;
  batchMonth: string;
  receivedDate: string;
  bankId: number | null;
  bankName: string | null;
  totalCount: number;
  totalAmount: number;
  status: string;
  importedByUserName: string | null;
  approvedByUserName: string | null;
  approvedAt: string | null;
  notes: string | null;
}

/** بند دفع */
export interface SalaryPaymentItemDetail {
  paymentItemID: number;
  batchId: number;
  fileId: number | null;
  fileNumber: string | null;
  retireeId: number | null;
  retireeName: string | null;
  transactionNumber: string | null;
  financialNumber: string | null;
  amount: number;
  sendDate: string | null;
  receiveDate: string | null;
  statusName: string | null;
  failureReason: string | null;
  senderBranch: string | null;
  receiverBranch: string | null;
  paymentBookNumber: string | null;
  recipientType: string | null;
}


// ══════════════════════════════════════════════════════════════
// 6. السجلات والإحصاء
// ══════════════════════════════════════════════════════════════

/** إحصائيات المعاشات */
export interface PensionStatistics {
  totalRetirees: number;
  activeFiles: number;
  suspendedFiles: number;
  closedFiles: number;
  newFiles: number;
  underReviewFiles: number;
  activeEntitled: number;
  activeAgents: number;
  activeGuardians: number;
  totalMonthlyAmount: number;
  rankDistribution: RankDistributionItem[];
  fileStatusDistribution: FileStatusDistributionItem[];
  governorateDistribution: GovernorateDistributionItem[];
  categoryDistribution: CategoryDistributionItem[];
  branchDistribution: BranchDistributionItem[];
  settlementByGovernorate: SettlementByGovernorateItem[];
}

/** إحصائيات الأداء الشهري */
export interface PerformanceStatistics {
  totalRetirees: number;
  activeFiles: number;
  underReviewFiles: number;
  newFiles: number;
  monthlyData: MonthlyPerformanceItem[];
}

/** عنصر أداء شهري */
export interface MonthlyPerformanceItem {
  month: string;
  retirees: number;
  files: number;
  newFiles: number;
}

/** توزيع حسب الرتبة */
export interface RankDistributionItem {
  rankName: string;
  count: number;
  percentage: number;
}

/** توزيع حسب حالة الملفات */
export interface FileStatusDistributionItem {
  statusName: string;
  count: number;
  percentage: number;
  color: string;
}

/** توزيع حسب المحافظة */
export interface GovernorateDistributionItem {
  governorate: string;
  count: number;
  percentage: number;
}

/** توزيع حسب الفئة */
export interface CategoryDistributionItem {
  categoryName: string;
  count: number;
  percentage: number;
}

/** توزيع حسب الفرع */
export interface BranchDistributionItem {
  branchName: string;
  count: number;
  percentage: number;
}

/** إحصائيات المحالين حسب المحافظة */
export interface SettlementByGovernorateItem {
  governorate: string;
  count: number;
  totalAmount: number;
  percentage: number;
}

/** سجل تحول الحالة */
export interface StatusTransitionLog {
  transitionID: number;
  fileId: number;
  fileNumber: string | null;
  retireeName: string | null;
  oldStatusName: string | null;
  newStatusName: string | null;
  reason: string | null;
  changedByUserName: string | null;
  changedAt: string;
  changeType: string;
}

// ══════════════════════════════════════════════════════════════
// 7. بطاقات صرف المعاشات
// ══════════════════════════════════════════════════════════════

/** تفاصيل بطاقة الصرف */
export interface PaymentCardDetail {
  paymentBookID: number;
  bookNumber: string;
  recipientName: string | null;
  recipientType: string | null;
  retireeName: string | null;
  retireeId: number | null;
  fileNumber: string | null;
  fileId: number | null;
  bookType: string;
  legacyColor: string | null;
  issueDate: string;
  expiryDate: string | null;
  status: string;
  issuingBranchName: string | null;
  issuingBranchId: number | null;
  changeReason: string | null;
  createdAt: string;
}

// ══════════════════════════════════════════════════════════════
// 9. أنواع إنشاء/تحديث
// ══════════════════════════════════════════════════════════════

/** إنشاء مستحق */
export interface CreateEntitledPersonDto {
  fileId: number;
  fullName: string;
  nationalID: string;
  relationshipId: number;
  birthDate?: string;
  phone?: string;
  socialStatusId?: number;
  healthStatusId?: number;
  hasDisabilityException?: boolean;
}

/** تحديث مستحق */
export interface UpdateEntitledPersonDto {
  fullName?: string;
  nationalID?: string;
  relationshipId?: number;
  birthDate?: string;
  phone?: string;
  socialStatusId?: number;
  healthStatusId?: number;
  hasDisabilityException?: boolean;
}

/** إنشاء إيقاف */
export interface CreateSuspensionDto {
  fileId: number;
  suspensionDate: string;
  suspensionReason: string;
  suspensionSource: string;
  missingDocumentTypeIds?: number[];
}

/** إنهاء إيقاف */
export interface EndSuspensionDto {
  endDate: string;
  endReason: string;
}

/** إنشاء إعفاء */
export interface CreateExemptionDto {
  fileId: number;
  directiveSource: string;
  exemptionDate: string;
  missingDocumentTypeIds?: number[];
  notes?: string;
}

/** إنشاء استحقاق مالي */
export interface CreateFinancialEntitlementDto {
  retireeId: number;
  amount: number;
  effectiveDate: string;
  endDate?: string;
  notes?: string;
}

/** ربط بند تسوية بمتقاعد */
export interface LinkSettlementItemDto {
  settlementItemId: number;
  retireeId: number;
}

/** استيراد تسوية */
export interface ImportSettlementExcelDto {
  decisionNumber: string;
  decisionDate?: string;
  decisionType?: string;
}

// ══════════════════════════════════════════════════════════════
// 10. أنواع الأعمدة والجدول
// ══════════════════════════════════════════════════════════════

/** حالة الدفتر */
export const PaymentBookStatus = {
  Active: "Active",
  Expired: "Expired",
  Lost: "Lost",
  Damaged: "Damaged",
  Cancelled: "Cancelled",
} as const;

export type PaymentBookStatusType = typeof PaymentBookStatus[keyof typeof PaymentBookStatus];

/** لون الحالة */
export const StatusColors: Record<string, string> = {
  Active: "success",
  Expired: "warning",
  Lost: "danger",
  Damaged: "danger",
  Cancelled: "default",
  Suspended: "warning",
  Closed: "default",
  New: "info",
  UnderReview: "purple",
  Rejected: "danger",
  Pending: "warning",
  Pledged: "info",
  Exempted: "purple",
  Fulfilled: "success",
};

/** أكواد الإيقاف بالعربية */
export const StopCodeLabels: Record<StopCode, string> = {
  AGE_LIMIT: "بلوغ السن القانونية",
  DAUGHTER_MARRIED: "زواج البنت",
  WIDOW_REMARRIED: "زواج الأرملة",
  DECEASED: "وفاة",
  ADMIN_DECISION: "قرار إداري",
  EMPLOYED: "توظيف",
  MISSING_DATA: "بيانات ناقصة",
  RETIRER_ALIVE: "المتقاعد على قيد الحياة",
};

/** أكواد إعادة التفعيل بالعربية */
export const ReactivateCodeLabels: Record<ReactivateCode, string> = {
  DIVORCED: "طلاق",
  HUSBAND_DECEASED: "وفاة الزوج",
  DISABILITY_REPORT: "تقرير طبي",
  ADMIN_DECISION: "قرار إداري",
};
