import { QueryCache, QueryClient, QueryClientProvider } from "@tanstack/react-query";
<<<<<<< HEAD
import { jwtDecode } from "jwt-decode";
import { lazy, Suspense, useCallback, useEffect, useRef, useState } from "react";
import { BrowserRouter, Navigate, Route, Routes, useNavigate } from "react-router-dom";
import { toast,Toaster } from "sonner";
=======
import { lazy, Suspense, useCallback, useEffect, useRef, useState } from "react";
import { BrowserRouter, Navigate, Route, Routes, useNavigate } from "react-router-dom";
import { jwtDecode } from "jwt-decode";
import { Toaster, toast } from "sonner";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

import { PermissionRoute } from "@/components/auth/PermissionRoute";
import { ProtectedRoute } from "@/components/auth/ProtectedRoute";
import { ErrorBoundary } from "@/components/ErrorBoundary";
import { MainLayout } from "@/components/layout/MainLayout";
import { FloatingNotification } from "@/components/notifications/FloatingNotification";
import { NotificationProvider } from "@/components/providers/NotificationProvider";
import { ThemeProvider } from "@/components/providers/ThemeProvider";
import { Loader } from "@/components/ui";
<<<<<<< HEAD
import { useGlobalCreateFileShortcut } from "@/constants/shortcuts";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useAuthStore } from "@/stores/authStore";

// ─── Lazy Pages ─────────────────────────────────────────
const LoginPage = lazy(() => import("@/pages/auth/LoginPage").then(m => ({ default: m.LoginPage })));
const DashboardPage = lazy(() => import("@/pages/DashboardPage").then(m => ({ default: m.DashboardPage })));
const HrDashboardPage = lazy(() => import("@/pages/hr/HrDashboardPage").then(m => ({ default: m.HrDashboardPage })));
const EmployeesPage = lazy(() => import("@/pages/hr/EmployeesPage").then(m => ({ default: m.EmployeesPage })));
const EmployeeDetailsPage = lazy(() => import("@/pages/hr/EmployeeDetailsPage").then(m => ({ default: m.EmployeeDetailsPage })));
const HrReferenceDataPage = lazy(() => import("@/pages/hr/HrReferenceDataPage").then(m => ({ default: m.HrReferenceDataPage })));
const HrLeavesPage = lazy(() => import("@/pages/hr/HrLeavesPage").then(m => ({ default: m.HrLeavesPage })));
const HrAttendancePage = lazy(() => import("@/pages/hr/HrAttendancePage").then(m => ({ default: m.HrAttendancePage })));
const HrDocumentsPage = lazy(() => import("@/pages/hr/HrDocumentsPage").then(m => ({ default: m.HrDocumentsPage })));
const HrServiceHistoryPage = lazy(() => import("@/pages/hr/HrServiceHistoryPage").then(m => ({ default: m.HrServiceHistoryPage })));
const NotificationsPage = lazy(() => import("@/pages/notifications/NotificationsPage").then(m => ({ default: m.NotificationsPage })));
const OutgoingRegisterPage = lazy(() => import("@/pages/correspondence/OutgoingRegisterPage").then(m => ({ default: m.OutgoingRegisterPage })));
const IncomingRegisterPage = lazy(() => import("@/pages/correspondence/IncomingRegisterPage").then(m => ({ default: m.IncomingRegisterPage })));
const ExternalPartiesPage = lazy(() => import("@/pages/correspondence/ExternalPartiesPage").then(m => ({ default: m.ExternalPartiesPage })));
const BranchesListPage = lazy(() => import("@/pages/branches/BranchesListPage").then(m => ({ default: m.BranchesListPage })));
const RanksListPage = lazy(() => import("@/pages/ranks/RanksListPage").then(m => ({ default: m.RanksListPage })));
const DocumentTypesPage = lazy(() => import("@/pages/documents/DocumentTypesPage").then(m => ({ default: m.DocumentTypesPage })));
const MissingDocsCompletionPage = lazy(() => import("@/pages/documents/MissingDocsCompletionPage").then(m => ({ default: m.MissingDocsCompletionPage })));
const BanksListPage = lazy(() => import("@/pages/banks/BanksListPage").then(m => ({ default: m.BanksListPage })));
const ForceBatchDetailsPage = lazy(() => import("@/pages/force/CodeBatchDetailsPage").then(m => ({ default: m.BatchDetailsPage })));
const RetireesPage = lazy(() => import("@/pages/retirees/RetireesPage").then(m => ({ default: m.RetireesPage })));
const RetireeDetailsPage = lazy(() => import("@/pages/retirees/RetireeDetailsPage").then(m => ({ default: m.RetireeDetailsPage })));
const RetirementFilesPage = lazy(() => import("@/pages/retirement-files/RetirementFilesPage").then(m => ({ default: m.RetirementFilesPage })));
const RetirementFileDetailsPage = lazy(() => import("@/pages/retirement-files/RetirementFileDetailsPage").then(m => ({ default: m.RetirementFileDetailsPage })));
const RetirementFileReportsPage = lazy(() => import("@/pages/retirement-files/RetirementFileReportsPage").then(m => ({ default: m.RetirementFileReportsPage })));
const FileReviewPage = lazy(() => import("@/pages/retirement-files/FileReviewPage").then(m => ({ default: m.FileReviewPage })));
const FileReviewListPage = lazy(() => import("@/pages/pension/FileReviewListPage").then(m => ({ default: m.FileReviewListPage })));

const DocumentProcessPage = lazy(() => import("@/pages/retirement-files/DocumentProcessPage").then(m => ({ default: m.DocumentProcessPage })));
const UploaderStatsPage = lazy(() => import("@/pages/retirement-files/UploaderStatsPage").then(m => ({ default: m.UploaderStatsPage })));
const SettlementDecisionsPage = lazy(() => import("@/pages/settlements/SettlementDecisionsPage").then(m => ({ default: m.SettlementDecisionsPage })));
const SettlementDecisionDetailsPage = lazy(() => import("@/pages/settlements/SettlementBatchDetailsPage").then(m => ({ default: m.SettlementBatchDetailsPage })));
const RetireeStatementPrint = lazy(() => import("@/pages/print/RetireeStatementPrint").then(m => ({ default: m.RetireeStatementPrint })));
const RetirementFilePrintPage = lazy(() => import("@/pages/print/RetirementFilePrintPage").then(m => ({ default: m.RetirementFilePrintPage })));
const PaymentBookPrint = lazy(() => import("@/pages/print/PaymentBookPrint").then(m => ({ default: m.PaymentBookPrint })));
const ForceItemPrintPage = lazy(() => import("@/pages/print/ForceItemPrintPage").then(m => ({ default: m.ForceItemPrintPage })));
const DiscrepancyPrintPage = lazy(() => import("@/pages/print/DiscrepancyPrintPage").then(m => ({ default: m.DiscrepancyPrintPage })));
const EmployeePrintPage = lazy(() => import("@/pages/print/EmployeePrintPage").then(m => ({ default: m.EmployeePrintPage })));
const SettlementDecisionPrint = lazy(() => import("@/pages/print/SettlementDecisionPrint").then(m => ({ default: m.SettlementDecisionPrint })));
const DocumentPledgePrint = lazy(() => import("@/pages/print/DocumentPledgePrint").then(m => ({ default: m.DocumentPledgePrint })));
const InventoryWithdrawalOrderPrint = lazy(() => import("@/pages/print/InventoryWithdrawalOrderPrint").then(m => ({ default: m.InventoryWithdrawalOrderPrint })));
const AccountingReportPrintPage = lazy(() => import("@/pages/print/AccountingReportPrintPage"));
const MissingDocumentsPrintPage = lazy(() => import("@/pages/print/MissingDocumentsPrintPage").then(m => ({ default: m.MissingDocumentsPrintPage })));
const SalaryBatchesPage = lazy(() => import("@/pages/salaries/SalaryBatchesPage").then(m => ({ default: m.SalaryBatchesPage })));
const SalaryBatchDetailsPage = lazy(() => import("@/pages/salaries/SalaryBatchDetailsPage").then(m => ({ default: m.SalaryBatchDetailsPage })));
const ComplianceDashboardPage = lazy(() => import("@/pages/compliance/ComplianceDashboardPage").then(m => ({ default: m.ComplianceDashboardPage })));
const InvestmentHub = lazy(() => import("@/pages/investment/InvestmentHub").then(m => ({ default: m.InvestmentHub })));
const PortfoliosPage = lazy(() => import("@/pages/investment/PortfoliosPage").then(m => ({ default: m.PortfoliosPage })));
const PortfolioDetailPage = lazy(() => import("@/pages/investment/PortfolioDetailPage").then(m => ({ default: m.PortfolioDetailPage })));
const ProjectsPage = lazy(() => import("@/pages/investment/ProjectsPage").then(m => ({ default: m.ProjectsPage })));
const ProjectDetailPage = lazy(() => import("@/pages/investment/ProjectDetailPage").then(m => ({ default: m.ProjectDetailPage })));
const PropertiesPage = lazy(() => import("@/pages/investment/PropertiesPage").then(m => ({ default: m.PropertiesPage })));
const PropertyDetailPage = lazy(() => import("@/pages/investment/PropertyDetailPage").then(m => ({ default: m.PropertyDetailPage })));
const LeasingPage = lazy(() => import("@/pages/investment/LeasingPage").then(m => ({ default: m.LeasingPage })));
const TenantDetailPage = lazy(() => import("@/pages/investment/TenantDetailPage").then(m => ({ default: m.TenantDetailPage })));
const MaintenancePage = lazy(() => import("@/pages/investment/MaintenancePage").then(m => ({ default: m.MaintenancePage })));
const MaintenanceDetailsPage = lazy(() => import("@/pages/investment/MaintenanceDetailsPage").then(m => ({ default: m.MaintenanceDetailsPage })));
const InvestmentGovernancePage = lazy(() => import("@/pages/investment/InvestmentGovernancePage").then(m => ({ default: m.InvestmentGovernancePage })));
const InvestmentSettingsPage = lazy(() => import("@/pages/investment/InvestmentSettingsPage").then(m => ({ default: m.InvestmentSettingsPage })));
const ChangePasswordPage = lazy(() => import("@/pages/auth/ChangePasswordPage").then(m => ({ default: m.ChangePasswordPage })));
const AccountPage = lazy(() => import("@/pages/account/AccountPage").then(m => ({ default: m.AccountPage })));
const UsersDirectoryPage = lazy(() => import("@/pages/system-admin/UsersDirectoryPage").then(m => ({ default: m.UsersDirectoryPage })));
const AuditLogsPage = lazy(() => import("@/pages/admin/AuditLogsPage").then(m => ({ default: m.AuditLogsPage })));
const LoginHistoryPage = lazy(() => import("@/pages/admin/LoginHistoryPage").then(m => ({ default: m.LoginHistoryPage })));
const DiscrepancyReportPage = lazy(() => import("@/pages/discrepancy/DiscrepancyReportPage").then(m => ({ default: m.DiscrepancyReportPage })));
const SystemSettingsPage = lazy(() => import("@/pages/admin/SystemSettingsPage").then(m => ({ default: m.SystemSettingsPage })));
const AdminLandingPage = lazy(() => import("@/pages/admin/AdminLandingPage").then(m => ({ default: m.AdminLandingPage })));
const AdminNotificationsPage = lazy(() => import("@/pages/admin/AdminNotificationsPage").then(m => ({ default: m.AdminNotificationsPage })));
const ReportsPage = lazy(() => import("@/pages/reports/ReportsPage").then(m => ({ default: m.ReportsPage })));
const ReportViewerPage = lazy(() => import("@/pages/reports/ReportViewerPage").then(m => ({ default: m.ReportViewerPage })));
const MinistrySearchPage = lazy(() => import("@/pages/pension/MinistrySearchPage").then(m => ({ default: m.MinistrySearchPage })));
const MinistryBatchesPage = lazy(() => import("@/pages/pension/MinistryBatchesPage").then(m => ({ default: m.MinistryBatchesPage })));
const KashfReportsPage = lazy(() => import("@/pages/pension/KashfReportsPage").then(m => ({ default: m.KashfReportsPage })));
const CorrespondenceDetailsPage = lazy(() => import("@/pages/correspondence/CorrespondenceDetailsPage").then(m => ({ default: m.CorrespondenceDetailsPage })));
const FileReviewDetailPage = lazy(() => import("@/pages/review/FileReviewDetailPage").then(m => ({ default: m.FileReviewDetailPage })));
const InventoryDashboardPage = lazy(() => import("@/pages/inventory/InventoryDashboardPage").then(m => ({ default: m.InventoryDashboardPage })));
const InventoryItemsPage = lazy(() => import("@/pages/inventory/InventoryItemsPage").then(m => ({ default: m.InventoryItemsPage })));
const InventoryCustodyPage = lazy(() => import("@/pages/inventory/InventoryCustodyPage").then(m => ({ default: m.InventoryCustodyPage })));
const InventoryTransactionsPage = lazy(() => import("@/pages/inventory/InventoryTransactionsPage").then(m => ({ default: m.InventoryTransactionsPage })));
const InventoryTransactionDetailsPage = lazy(() => import("@/pages/inventory/InventoryTransactionDetailsPage").then(m => ({ default: m.InventoryTransactionDetailsPage })));
const StocktakePage = lazy(() => import("@/pages/inventory/StocktakePage").then(m => ({ default: m.StocktakePage })));

// ── الأصول الثابتة (Fixed Assets) ──
const FixedAssetsPage = lazy(() => import("@/pages/assets/FixedAssetsPage").then(m => ({ default: m.FixedAssetsPage })));
const AssetCategoriesPage = lazy(() => import("@/pages/assets/AssetCategoriesPage").then(m => ({ default: m.AssetCategoriesPage })));
const DepreciationRunPage = lazy(() => import("@/pages/assets/DepreciationRunPage").then(m => ({ default: m.DepreciationRunPage })));
const AssetDetailPage = lazy(() => import("@/pages/assets/AssetDetailPage").then(m => ({ default: m.AssetDetailPage })));

// ── الخزينة ──
const TreasuryPage = lazy(() => import("@/pages/finance/TreasuryPage").then(m => ({ default: m.TreasuryPage })));

// ── الإنجازات والمعوقات ──
const AchievementObstaclesPage = lazy(() => import("@/pages/achievements/AchievementObstaclesPage").then(m => ({ default: m.AchievementObstaclesPage })));

// ── إيقاف المعاشات ──
const SuspensionsPage = lazy(() => import("@/pages/suspensions/SuspensionsPage").then(m => ({ default: m.SuspensionsPage })));

// ── مكتب المدير العام ──
const DirectorOfficePage = lazy(() => import("@/pages/director/DirectorOfficePage").then(m => ({ default: m.DirectorOfficePage })));

// ── الأرشيف ──
const ArchivePage = lazy(() => import("@/pages/archive/ArchivePage").then(m => ({ default: m.ArchivePage })));
const DocumentReviewArchivePage = lazy(() => import("@/pages/archive/DocumentReviewArchivePage").then(m => ({ default: m.DocumentReviewArchivePage })));

// ── إدارة المعاشات  (الجديد) ──
const SocialResearchPage = lazy(() => import("@/pages/pension/SocialResearchPage").then(m => ({ default: m.SocialResearchPage })));
const MinistryReferralsPage = lazy(() => import("@/pages/pension/MinistryReferralsPage").then(m => ({ default: m.MinistryReferralsPage })));
const DocumentReviewPage = lazy(() => import("@/pages/pension/DocumentReviewPage").then(m => ({ default: m.DocumentReviewPage })));
const EntitledPersonsPage = lazy(() => import("@/pages/pension/EntitledPersonsPage").then(m => ({ default: m.EntitledPersonsPage })));
const SalaryReconciliationPage = lazy(() => import("@/pages/pension/SalaryReconciliationPage").then(m => ({ default: m.SalaryReconciliationPage })));
const PensionSuspensionsPage = lazy(() => import("@/pages/pension/PensionSuspensionsPage").then(m => ({ default: m.PensionSuspensionsPage })));
const MonthlySummariesPage = lazy(() => import("@/pages/pension/MonthlySummariesPage").then(m => ({ default: m.MonthlySummariesPage })));
const PensionStatisticsPage = lazy(() => import("@/pages/pension/PensionStatisticsPage").then(m => ({ default: m.PensionStatisticsPage })));
const PaymentCardsPage = lazy(() => import("@/pages/pension/PaymentCardsPage").then(m => ({ default: m.PaymentCardsPage })));
const AnnualBookReviewPage = lazy(() => import("@/pages/pension/AnnualBookReviewPage").then(m => ({ default: m.AnnualBookReviewPage })));
const AgentsPage = lazy(() => import("@/pages/pension/AgentsPage").then(m => ({ default: m.AgentsPage })));
const GuardiansPage = lazy(() => import("@/pages/pension/GuardiansPage").then(m => ({ default: m.GuardiansPage })));
const SalaryRecipientsPage = lazy(() => import("@/pages/pension/SalaryRecipientsPage").then(m => ({ default: m.SalaryRecipientsPage })));
const FinancialEntitlementsPage = lazy(() => import("@/pages/pension/FinancialEntitlementsPage").then(m => ({ default: m.FinancialEntitlementsPage })));

// ── المحاسبة (Accounting) ──
const AccountingDashboardPage = lazy(() => import("@/pages/accounting/AccountingDashboardPage").then(m => ({ default: m.AccountingDashboardPage })));
const ChartOfAccountsPage = lazy(() => import("@/pages/accounting/ChartOfAccountsPage").then(m => ({ default: m.ChartOfAccountsPage })));
const JournalEntriesPage = lazy(() => import("@/pages/accounting/JournalEntriesPage").then(m => ({ default: m.JournalEntriesPage })));
const VoucherListPage = lazy(() => import("@/pages/accounting/VoucherListPage").then(m => ({ default: m.VoucherListPage })));
const JournalEntryDetailsPage = lazy(() => import("@/pages/accounting/JournalEntryDetailsPage").then(m => ({ default: m.JournalEntryDetailsPage })));
const CostCentersPage = lazy(() => import("@/pages/accounting/CostCentersPage").then(m => ({ default: m.CostCentersPage })));
const FiscalPeriodsPage = lazy(() => import("@/pages/accounting/FiscalPeriodsPage").then(m => ({ default: m.FiscalPeriodsPage })));
const QuartersPage = lazy(() => import("@/pages/accounting/QuartersPage").then(m => ({ default: m.QuartersPage })));
const QuarterDetailsPage = lazy(() => import("@/pages/accounting/QuarterDetailsPage").then(m => ({ default: m.QuarterDetailsPage })));
const AccountingReportsPage = lazy(() => import("@/pages/accounting/ReportsPage").then(m => ({ default: m.ReportsPage })));
const InitialOpeningBalancesPage = lazy(() => import("@/pages/accounting/InitialOpeningBalancesPage").then(m => ({ default: m.InitialOpeningBalancesPage })));
const BudgetPage = lazy(() => import("@/pages/accounting/BudgetPage").then(m => ({ default: m.BudgetPage })));
const CashFlowPage = lazy(() => import("@/pages/accounting/CashFlowPage").then(m => ({ default: m.CashFlowPage })));

// ─── QueryClient with global error handling ─────────────

function createQueryClient() {
  return new QueryClient({
    queryCache: new QueryCache({
      onError: (error, query) => {
        console.error(`[Query Error] ${query.queryKey}:`, error);
      },
    }),
    defaultOptions: {
      queries: {
        retry: 1,
        staleTime: 30_000,
        gcTime: 60_000, // 1 minute — reduced from 3min to free memory faster on mobile
        refetchOnWindowFocus: false,
      },
    },
  });
}

// ─── App ────────────────────────────────────────────────

export function App() {
  const [queryClient] = useState(() => createQueryClient());
  const hydrate = useAuthStore((s) => s.hydrate);
  const refreshPermissions = useAuthStore((s) => s.refreshPermissions);
  const isHydrated = useAuthStore((s) => s.isHydrated);
  const tokenRef = useRef<string | null>(null);

  // Keep tokenRef in sync
  useEffect(() => {
    const unsub = useAuthStore.subscribe((state) => { tokenRef.current = state.token; });
    tokenRef.current = useAuthStore.getState().token;
    return unsub;
  }, []);

  // Hydrate on mount
  useEffect(() => {
    hydrate();
  }, [hydrate]);

  // Safe refresh — skip if token expired > 2 min (401 interceptor handles full recovery)
  const safeRefresh = useCallback(() => {
    const token = tokenRef.current;
    if (!token) return;
    try {
      const { exp } = jwtDecode<{ exp: number }>(token);
      const timeLeft = exp * 1000 - Date.now();
      if (timeLeft < -120_000) return; // expired > 2 min — let 401 interceptor handle
    } catch {
      return;
    }
    refreshPermissions(true);
  }, [refreshPermissions]);

  useEffect(() => {
    // Permission refresh on a 5-minute interval only when page is visible.
    const interval = setInterval(() => {
      if (document.visibilityState === "visible") {
        safeRefresh();
      }
    }, 5 * 60 * 1000);
    return () => {
      clearInterval(interval);
    };
  }, [safeRefresh]);

  // ─── Activity-based token refresh ──────────────────────
  // يراقب نشاط المستخدم (نقرات، كتابة، تمرير) ويجدد التوكن تلقائياً
  // طالما المستخدم يعمل → الجلسة لا تنتهي أبداً
  const refreshTokenNow = useAuthStore((s) => s.refreshTokenNow);

  useEffect(() => {
    let activityTimeout: ReturnType<typeof setTimeout> | null = null;

    const handleActivity = () => {
      // Throttle: فقط كل 60 ثانية نفحص إذا نحتاج تجديد
      if (activityTimeout) return;
      activityTimeout = setTimeout(() => {
        activityTimeout = null;
      }, 60_000);

      // refreshTokenNow داخلياً يفحص إذا التوكن قريب من الانتهاء
      refreshTokenNow();
    };

    const events = ["click", "keydown", "scroll", "mousemove", "touchstart"] as const;
    events.forEach((e) => window.addEventListener(e, handleActivity, { passive: true }));

    return () => {
      events.forEach((e) => window.removeEventListener(e, handleActivity));
      if (activityTimeout) clearTimeout(activityTimeout);
    };
  }, [refreshTokenNow]);

  // عند عودة المستخدم للتبويب → تجديد التوكن فوراً + تحديث الصلاحيات
  useEffect(() => {
    const handleVisibility = () => {
      if (document.visibilityState === "visible") {
        refreshTokenNow(); // ✅ تجديد التوكن فوراً عند العودة
        safeRefresh();      // تحديث الصلاحيات
      }
    };
    document.addEventListener("visibilitychange", handleVisibility);
    return () => document.removeEventListener("visibilitychange", handleVisibility);
  }, [safeRefresh, refreshTokenNow]);

  // Mobile: detect bfcache restore (page restored from frozen state)
  useEffect(() => {
    const handlePageShow = (e: PageTransitionEvent) => {
      if (e.persisted) {
        // Page restored from bfcache — rehydrate to validate token
        hydrate();
      }
    };
    window.addEventListener("pageshow", handlePageShow);
    return () => window.removeEventListener("pageshow", handlePageShow);
  }, [hydrate]);

  // console.log removed — was running on every render, causing unnecessary overhead

  // ✅ CRITICAL: Show loader until hydration completes to prevent route guards from redirecting too early
  if (!isHydrated) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <Loader variant="fullscreen" />
      </div>
    );
  }

  return (
    <QueryClientProvider client={queryClient}>
      <Toaster position="top-left" richColors />
      <NotificationProvider>
        <ThemeProvider>
          <BrowserRouter>
            {/* ✅ FIXED: All listeners moved INSIDE BrowserRouter */}
            <AuthEventListener />
            <QueryErrorToast />
            <ForbiddenListener />
<<<<<<< HEAD
            <GlobalShortcuts />
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            <FloatingNotification />
            <ErrorBoundary>
              <Suspense fallback={<Loader variant="fullscreen" />}>
                <Routes>
                  {/* Public */}
                  <Route path="/login" element={<LoginPage />} />

                  {/* Protected — Authentication required */}
                  <Route element={<ProtectedRoute />}>
                    <Route path="/change-password" element={<ChangePasswordPage />} />
                    <Route element={<ErrorBoundary><MainLayout /></ErrorBoundary>}>
                      {/* ── عام: متاح لجميع المصادقين ──────────── */}
                      <Route path="/dashboard" element={<DashboardPage />} />
                      <Route path="/account" element={<AccountPage />} />
                      <Route path="/notifications" element={<NotificationsPage />} />

                      {/* ── الشؤون الإدارية (HR) ───────────────────── */}
                      <Route element={<PermissionRoute anyPermission={["HR.Dashboard.View", "HR.Employee.View", "HR.Reference.View", "HR.ServiceHistory.View", "System.Admin"]} />}>
                        <Route path="/hr" element={<HrDashboardPage />} />
                        <Route path="/hr/employees" element={<EmployeesPage />} />
                        <Route path="/hr/employees/print/:employeeId" element={<EmployeePrintPage />} />
                        <Route path="/hr/employees/:id" element={<EmployeeDetailsPage />} />
                        <Route path="/hr/reference" element={<HrReferenceDataPage />} />
                        <Route path="/hr/leaves" element={<HrLeavesPage />} />
                        <Route path="/hr/attendance" element={<HrAttendancePage />} />
                        <Route path="/hr/documents" element={<HrDocumentsPage />} />
                        <Route path="/hr/service-history" element={<HrServiceHistoryPage />} />
                      </Route>

                      {/* ── سجل الصادر والوارد ── */}
                      <Route element={<PermissionRoute anyPermission={["Correspondence.View", "System.Admin"]} />}>
                        <Route path="/correspondence/outgoing" element={<OutgoingRegisterPage />} />
                        <Route path="/correspondence/incoming" element={<IncomingRegisterPage />} />
                        <Route path="/correspondence/external-parties" element={<ExternalPartiesPage />} />
                        <Route path="/correspondence/:id" element={<CorrespondenceDetailsPage />} />
                      </Route>

                      {/* ── ملفات التقاعد ── */}
                      <Route element={<PermissionRoute anyPermission={["Retiree.View", "File.View.Sensitive", "File.Create", "File.Edit.BasicData", "File.Edit.FinancialData", "File.ChangeStatus.Approve", "File.Documents.Upload", "System.Admin"]} />}>
                        <Route path="/retirees" element={<RetireesPage />} />
                        <Route path="/retirees/:id" element={<RetireeDetailsPage />} />
                        <Route path="/files" element={<RetirementFilesPage />} />
                        <Route path="/files/uploader-stats" element={<UploaderStatsPage />} />
                        <Route path="/files/:id" element={<RetirementFileDetailsPage />} />
                        <Route path="/files/:id/review" element={<FileReviewPage />} />
                        <Route path="/files/:id/documents/:docId/process" element={<DocumentProcessPage />} />
                        <Route path="/files/:id/print" element={<RetirementFilePrintPage />} />
                        <Route path="/files/:id/missing-docs-print" element={<MissingDocumentsPrintPage />} />
                        <Route path="/payment-books/:id/print" element={<PaymentBookPrint />} />
                        <Route path="/pledges/:id/print" element={<DocumentPledgePrint />} />
                      </Route>

                      {/* ── الإنجازات والمعوقات ── */}
                      <Route element={<PermissionRoute anyPermission={["AchievementObstacle.View", "System.Admin"]} />}>
                        <Route path="/achievements" element={<AchievementObstaclesPage />} />
                      </Route>

                      {/* ── إيقاف المعاشات ── */}
                      <Route element={<PermissionRoute anyPermission={["Suspension.View", "System.Admin"]} />}>
                        <Route path="/suspensions" element={<SuspensionsPage />} />
                      </Route>

                      {/* ── إدارة المعاشات  (الجديد) ── */}
                      <Route element={<PermissionRoute anyPermission={["Retiree.View", "File.View.Sensitive", "System.Admin"]} />}>
                        {/* البحث الاجتماعي والفحص */}
                        <Route path="/pension/social-research" element={<SocialResearchPage />} />
                        <Route path="/pension/document-review" element={<DocumentReviewPage />} />
                        <Route path="/pension/file-review" element={<FileReviewListPage />} />

                        <Route path="/pension/entitled-persons" element={<EntitledPersonsPage />} />


                        {/* الاستحقاقات */}
                        <Route path="/pension/suspensions" element={<PensionSuspensionsPage />} />
                        <Route path="/pension/monthly-summaries" element={<MonthlySummariesPage />} />

                        {/* السجلات والإحصاء */}

                        <Route path="/pension/statistics" element={<PensionStatisticsPage />} />
                        <Route path="/pension/payment-cards" element={<PaymentCardsPage />} />
                        <Route path="/pension/annual-book-review" element={<AnnualBookReviewPage />} />
                        <Route path="/pension/agents" element={<AgentsPage />} />
                        <Route path="/pension/guardians" element={<GuardiansPage />} />
                        <Route path="/pension/salary-recipients" element={<SalaryRecipientsPage />} />
                        <Route path="/pension/financial-entitlements" element={<FinancialEntitlementsPage />} />

                        {/* الأرشيف */}
                        <Route path="/archive" element={<ArchivePage />} />
                        <Route path="/archive/document-review" element={<DocumentReviewArchivePage />} />
                      </Route>

                      <Route element={<PermissionRoute anyPermission={["Ministry.View", "System.Admin"]} />}>
                        <Route path="/pension/ministry-referrals" element={<MinistryReferralsPage />} />
                        <Route path="/pension/salary-reconciliation" element={<SalaryReconciliationPage />} />

                      </Route>

                      {/* ── طباعة المباينة ── */}
                      <Route element={<PermissionRoute anyPermission={["Retiree.View", "Ministry.View", "Ministry.Manage", "System.Admin"]} />}>
                        <Route path="/retirees/:id/statement" element={<RetireeStatementPrint />} />
                      </Route>

                      {/* ── مراجعة التعديلات ── */}
                      <Route element={<PermissionRoute anyPermission={["File.Review.View", "File.Review.Approve", "File.ChangeStatus.Approve", "System.Admin"]} />}>
                        <Route path="/review/:fileId" element={<FileReviewDetailPage />} />
                      </Route>

                      {/* ── استكمال النواقص ── */}
                      <Route element={<PermissionRoute anyPermission={["File.Documents.Upload", "System.Admin"]} />}>
                        <Route path="/documents" element={<MissingDocsCompletionPage />} />
                      </Route>

                      <Route element={<PermissionRoute anyPermission={["SalaryBatch.View", "SalaryBatch.Manage", "Settlement.View", "Settlement.Manage", "File.Edit.FinancialData", "System.Admin"]} />}>
                        <Route path="/salaries" element={<SalaryBatchesPage />} />
                        <Route path="/salaries/:id" element={<SalaryBatchDetailsPage />} />
                        <Route path="/settlements" element={<SettlementDecisionsPage />} />
                        <Route path="/settlements/:id" element={<SettlementDecisionDetailsPage />} />
                        <Route path="/settlements/:id/print" element={<SettlementDecisionPrint />} />
                      </Route>


                      {/* ── المحاسبة (Accounting) ── */}
                      <Route element={<PermissionRoute anyPermission={["Accounting.View", "Accounting.Reports", "Accounting.Manage", "System.Admin"]} />}>
                        <Route path="/accounting" element={<AccountingDashboardPage />} />
                        <Route path="/accounting/chart-of-accounts" element={<ChartOfAccountsPage />} />
                        <Route path="/accounting/journal-entries" element={<JournalEntriesPage />} />
                        <Route path="/accounting/journal-entries/:id" element={<JournalEntryDetailsPage />} />
                        <Route path="/accounting/vouchers" element={<VoucherListPage />} />
                        <Route path="/accounting/cost-centers" element={<CostCentersPage />} />
                        <Route path="/accounting/periods" element={<FiscalPeriodsPage />} />
                        <Route path="/accounting/quarters" element={<QuartersPage />} />
                        <Route path="/accounting/quarters/:id" element={<QuarterDetailsPage />} />
                        <Route path="/accounting/reports" element={<AccountingReportsPage />} />
                        <Route path="/reports/:reportType/print" element={<AccountingReportPrintPage />} />
                        <Route path="/accounting/initial-opening-balances" element={<InitialOpeningBalancesPage />} />
                        <Route path="/accounting/budget" element={<BudgetPage />} />
                        <Route path="/accounting/cash-flow" element={<CashFlowPage />} />
                        <Route path="/finance/treasury" element={<TreasuryPage />} />
                      </Route>

                      {/* ── إدارة الاستثمار ── */}
                      <Route element={<PermissionRoute anyPermission={["Investment.View", "System.Admin"]} />}>
                        <Route path="/investment" element={<InvestmentHub />} />
                        <Route path="/investment/portfolios" element={<PortfoliosPage />} />
                        <Route path="/investment/portfolios/:id" element={<PortfolioDetailPage />} />
                        <Route path="/investment/projects" element={<ProjectsPage />} />
                        <Route path="/investment/projects/:id" element={<ProjectDetailPage />} />
                        <Route path="/investment/properties" element={<PropertiesPage />} />
                        <Route path="/investment/properties/:id" element={<PropertyDetailPage />} />
                        <Route path="/investment/leasing" element={<LeasingPage />} />
                        <Route path="/investment/leasing/tenants/:id" element={<TenantDetailPage />} />
                        <Route path="/investment/maintenance" element={<MaintenancePage />} />
                        <Route path="/investment/maintenance/:id" element={<MaintenanceDetailsPage />} />
                        <Route path="/investment/governance" element={<InvestmentGovernancePage />} />
                        <Route path="/investment/settings" element={<InvestmentSettingsPage />} />
                      </Route>

<<<<<<< HEAD
                      {/* ── مركز التقارير ── */}
                      <Route element={<PermissionRoute anyPermission={["System.Admin", "Report.Pension.View", "Report.Financial.View", "Report.Entitlements.View", "Report.Documents.View", "Report.Admin.View"]} />}>
                        <Route path="/reports/retirement-file-reports" element={<RetirementFileReportsPage />} />
                        <Route path="/reports" element={<ReportsPage />} />
                        <Route path="/reports/kashf" element={<KashfReportsPage />} />
                        <Route path="/reports/:reportId" element={<ReportViewerPage />} />
                      </Route>

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                      {/* ── إدارة النظام ── */}
                      <Route element={<PermissionRoute permission="System.Admin" />}>
                        <Route path="/users" element={<Navigate to="/system-admin/users" replace />} />
                        <Route path="/system-admin/users" element={<UsersDirectoryPage />} />
                        <Route path="/admin" element={<AdminLandingPage />} />
<<<<<<< HEAD
=======
                        <Route path="/reports/retirement-file-reports" element={<RetirementFileReportsPage />} />
                        <Route path="/reports" element={<ReportsPage />} />
                        <Route path="/reports/kashf" element={<KashfReportsPage />} />
                        <Route path="/reports/:reportId" element={<ReportViewerPage />} />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <Route path="/branches" element={<BranchesListPage />} />
                        <Route path="/banks" element={<BanksListPage />} />
                        <Route path="/ranks" element={<RanksListPage />} />
                        <Route path="/document-types" element={<DocumentTypesPage />} />
                        <Route path="/admin/settings" element={<SystemSettingsPage />} />
                        <Route path="/admin/notifications" element={<AdminNotificationsPage />} />
                        <Route path="/director-office" element={<DirectorOfficePage />} />
                      </Route>

                      {/* ── كشوفات الإحالة إلى التقاعد ── */}
                      <Route element={<PermissionRoute anyPermission={["Ministry.View", "System.Admin"]} />}>
                        <Route path="/pension/ministry-search" element={<MinistrySearchPage />} />
                        <Route path="/pension/ministry-batches" element={<MinistryBatchesPage />} />
                        <Route path="/force/batches/:id" element={<ForceBatchDetailsPage />} />
                        <Route path="/force/items/:id/print" element={<ForceItemPrintPage />} />
                        <Route path="/force/batches" element={<Navigate to="/pension/ministry-batches" replace />} />
                        <Route path="/approvals" element={<Navigate to="/pension/ministry-batches" replace />} />
                        <Route path="/ministry-approvals" element={<Navigate to="/pension/ministry-batches" replace />} />
                      </Route>

                      {/* ── التدقيق والأمان ── */}
                      <Route element={<PermissionRoute anyPermission={["System.Admin", "Audit.ViewAll"]} />}>
                        <Route path="/admin/audit-logs" element={<AuditLogsPage />} />
                        <Route path="/admin/login-history" element={<LoginHistoryPage />} />
                        <Route path="/admin/discrepancy-report" element={<DiscrepancyReportPage />} />
                        <Route path="/admin/discrepancy-report/print" element={<DiscrepancyPrintPage />} />
                      </Route>

                      {/* ── المخزون ── */}
                      <Route element={<PermissionRoute anyPermission={["Inventory.Item.View", "Inventory.Custody.View", "Inventory.Transaction.View", "System.Admin"]} />}>
                        <Route path="/inventory" element={<InventoryDashboardPage />} />
                        <Route path="/inventory/items" element={<InventoryItemsPage />} />
                        <Route path="/inventory/custody" element={<InventoryCustodyPage />} />
                        <Route path="/inventory/transactions" element={<InventoryTransactionsPage />} />
                        <Route path="/inventory/transactions/:id" element={<InventoryTransactionDetailsPage />} />
                        <Route path="/inventory/withdrawal-orders/:orderNumber/print" element={<InventoryWithdrawalOrderPrint />} />
                        <Route path="/inventory/asset-stocktake" element={<StocktakePage />} />
                        <Route path="/inventory/simple-stocktake" element={<StocktakePage />} />
                        <Route path="/inventory/stocktake" element={<StocktakePage />} />
                      </Route>

                      {/* ── الأصول الثابتة ── */}
                      <Route element={<PermissionRoute anyPermission={["Inventory.Item.View", "System.Admin"]} />}>
                        <Route path="/assets/fixed" element={<FixedAssetsPage />} />
                        <Route path="/assets/categories" element={<AssetCategoriesPage />} />
                        <Route path="/assets/fixed/:id" element={<AssetDetailPage />} />
                        <Route path="/assets/depreciation-run" element={<DepreciationRunPage />} />
                      </Route>

                      {/* ── الامتثال ── */}
                      <Route element={<PermissionRoute anyPermission={["System.Admin", "Audit.ViewAll"]} />}>
                        <Route path="/compliance" element={<ComplianceDashboardPage />} />
                      </Route>
                    </Route>
                  </Route>

                  {/* Root — redirect based on auth state */}
                  <Route path="/" element={<RootRedirect />} />

                  {/* Fallback */}
                  <Route path="*" element={<FallbackRedirect />} />
                </Routes>
              </Suspense>
            </ErrorBoundary>
          </BrowserRouter>
        </ThemeProvider>
      </NotificationProvider>
    </QueryClientProvider>
  );
}

// ─── Root Redirect ──────────────────────────────────────

function RootRedirect() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  return isAuthenticated ? <Navigate to="/dashboard" replace /> : <Navigate to="/login" replace />;
}

// ─── Fallback Redirect ─────────────────────────────────

function FallbackRedirect() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  return isAuthenticated ? <Navigate to="/dashboard" replace /> : <Navigate to="/login" replace />;
}

// ─── Auth Event Listener ────────────────────────────────

function AuthEventListener() {
  const navigate = useNavigate();

  useEffect(() => {
    const handleLogout = () => {
      toast.info("تم تسجيل الخروج");
      navigate("/login", { replace: true });
    };
    const handleSessionExpired = () => {
      const currentPath = window.location.pathname;
      if (currentPath && currentPath !== "/login") {
        sessionStorage.setItem("postLoginRedirect", currentPath);
      }
      toast.error("انتهت الجلسة", { description: "يرجى تسجيل الدخول مرة أخرى" });
      navigate("/login", { replace: true, state: { expired: true, from: { pathname: currentPath } } });
    };

    window.addEventListener("auth:logout", handleLogout);
    window.addEventListener("auth:session-expired", handleSessionExpired);

    return () => {
      window.removeEventListener("auth:logout", handleLogout);
      window.removeEventListener("auth:session-expired", handleSessionExpired);
    };
  }, [navigate]);

  return null;
}

// ─── Query Error Toast ──────────────────────────────────

function QueryErrorToast() {
  useEffect(() => {
    const handler = (e: Event) => {
      const detail = (e as CustomEvent).detail;
      if (detail?.queryKey && detail?.error) {
        toast.error("خطأ في تحميل البيانات", {
          description: `${detail.queryKey}: ${detail.error?.message || "خطأ غير معروف"}`,
        });
      }
    };
    window.addEventListener("query:error", handler);
    return () => window.removeEventListener("query:error", handler);
  }, []);

  return null;
}

// ─── 403 Listener ───────────────────────────────────────

function ForbiddenListener() {
  useEffect(() => {
    const handler = (e: Event) => {
      const detail = (e as CustomEvent).detail;
      toast.error(detail?.message || "ليس لديك صلاحية لتنفيذ هذا الإجراء");
    };
    window.addEventListener("api:forbidden", handler);
    return () => window.removeEventListener("api:forbidden", handler);
  }, []);

  return null;
}
<<<<<<< HEAD

// ─── Global Keyboard Shortcuts ──────────────────────────

function GlobalShortcuts() {
  useGlobalCreateFileShortcut();
  return null;
}
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
