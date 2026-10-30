import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
<<<<<<< HEAD
import {
  BarChart3,
  Database,
  Minus,
  Plus,
  RefreshCw,
  TrendingDown,
  TrendingUp,
} from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import { fiscalYearsApi, periodsApi } from "@/api/accounting";
import { budgetApi } from "@/api/budget";
import { BudgetFormDialog } from "@/components/domain/accounting/BudgetFormDialog";
import { BudgetReport } from "@/components/domain/accounting/BudgetReport";
=======
import { useEffect, useState } from "react";
import {
  BarChart3,
  Database,
  Plus,
  RefreshCw,
  TrendingUp,
  TrendingDown,
  Minus,
} from "lucide-react";

import { budgetApi } from "@/api/budget";
import { fiscalYearsApi, periodsApi } from "@/api/accounting";
import { useHasPermission } from "@/stores/authStore";
import { toast } from "sonner";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
  Button,
  Card,
  ConfirmDialog,
  Heading,
  PageLayout,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
<<<<<<< HEAD
  StickyHeader,
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
<<<<<<< HEAD
=======
  StickyHeader,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  Text,
  useConfirm,
} from "@/components/ui";
import { formatCurrency } from "@/lib/utils";
<<<<<<< HEAD
import { useHasPermission } from "@/stores/authStore";
import type { FiscalPeriodDto,FiscalYearSummaryDto } from "@/types/accounting";
import type { BudgetReportDto, MonthlyBudgetReportDto,QuarterlyBudgetReportDto } from "@/types/budget";
=======
import { BudgetFormDialog } from "@/components/domain/accounting/BudgetFormDialog";
import { BudgetReport } from "@/components/domain/accounting/BudgetReport";
import type { BudgetReportDto, QuarterlyBudgetReportDto, MonthlyBudgetReportDto } from "@/types/budget";
import type { FiscalYearSummaryDto, FiscalPeriodDto } from "@/types/accounting";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export function BudgetPage() {
  const canManage = useHasPermission("Accounting.Manage", "System.Admin");
  const queryClient = useQueryClient();
  const [fiscalYearID, setFiscalYearID] = useState<number | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editItem, setEditItem] = useState<any>(null);
  const [showSeedConfirm, setShowSeedConfirm] = useState(false);
  const [activeTab, setActiveTab] = useState("annual");
  const [selectedMonth, setSelectedMonth] = useState<number | null>(null);
  const { confirm, ConfirmDialogElement } = useConfirm();

  // ═══ حذف بند الموازنة ═══
  const deleteMutation = useMutation({
    mutationFn: (id: number) => budgetApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["budget-report"] });
      toast.success("تم الحذف بنجاح");
    },
    onError: (err: Error) => {
      toast.error(err.message ?? "حدث خطأ أثناء الحذف");
    },
  });

  // ═══ إدخال الموازنة ═══
  const seedMutation = useMutation({
    mutationFn: (yearId: number) => budgetApi.seedBudget(yearId),
    onSuccess: (res) => {
      queryClient.invalidateQueries({ queryKey: ["budget-report"] });
      toast.success(res?.message ?? "تم الإدخال بنجاح");
    },
    onError: (err: Error) => {
      toast.error(err.message ?? "حدث خطأ");
    },
  });

  // جلب السنوات المالية
  const { data: fiscalYears } = useQuery<FiscalYearSummaryDto[]>({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  // جلب الفترات الشهرية
  const { data: periods } = useQuery<FiscalPeriodDto[]>({
    queryKey: ["periods", fiscalYearID],
    queryFn: () => periodsApi.getAll(fiscalYearID!),
    enabled: fiscalYearID != null,
  });

  // جلب تقرير الموازنة
  const {
    data: report,
    isLoading,
    refetch,
  } = useQuery<BudgetReportDto | QuarterlyBudgetReportDto | MonthlyBudgetReportDto>({
    queryKey: ["budget-report", fiscalYearID, activeTab],
    queryFn: async () => {
      if (activeTab === "annual") {
        return await budgetApi.getAnnualReport(fiscalYearID!);
      }
      if (activeTab === "monthly") {
        return await budgetApi.getMonthlyReport(fiscalYearID!);
      }
      return await budgetApi.getQuarterlyReport(fiscalYearID!);
    },
    enabled: fiscalYearID != null,
  });

  // Auto-select current year
  useEffect(() => {
    if (fiscalYears && fiscalYearID === null) {
      const currentYear = new Date().getFullYear().toString();
      const match = fiscalYears.find((y) => y.yearName === currentYear);
      if (match) {
        setFiscalYearID(match.fiscalYearID);
      } else if (fiscalYears.length > 0) {
        setFiscalYearID(fiscalYears[0].fiscalYearID);
      }
    }
  }, [fiscalYears]);

  // حساب الإحصائيات
  const annualReport = activeTab === "annual" ? (report as BudgetReportDto) : null;
  const stats = {
    totalPlanned: annualReport?.totalPlanned ?? 0,
    totalActual: annualReport?.totalActual ?? 0,
    variance: annualReport?.totalVariance ?? 0,
    variancePercentage: annualReport?.totalVariancePercentage ?? 0,
  };

  return (
    <PageLayout>
      {/* Header */}
      <StickyHeader>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-primary/10 rounded-2xl">
              <BarChart3 className="w-8 h-8 text-primary" />
            </div>
            <div>
              <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">نظام الموازنة</Heading>
              <p className="text-muted-foreground mt-1">التخطيط والمتابعة المالية</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <Select
              value={fiscalYearID != null ? String(fiscalYearID) : ""}
              onValueChange={(v) => setFiscalYearID(Number(v))}
            >
              <SelectTrigger className="w-40">
                <SelectValue placeholder="اختر السنة المالية" />
              </SelectTrigger>
              <SelectContent>
                {fiscalYears?.map((y) => (
                  <SelectItem key={y.fiscalYearID} value={String(y.fiscalYearID)}>
                    {y.yearName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Button variant="outline" onClick={() => refetch()} className="gap-2">
              <RefreshCw className="w-4 h-4" /> تحديث
            </Button>

            {canManage && (
              <>
                <Button
                  variant="outline"
                  onClick={() => setShowSeedConfirm(true)}
                  disabled={!fiscalYearID || seedMutation.isPending}
                  className="gap-2"
                >
                  <Database className="w-4 h-4" /> إدخال الموازنة
                </Button>
                <Button onClick={() => setShowForm(true)} className="gap-2">
                  <Plus className="w-4 h-4" /> إضافة بند
                </Button>
              </>
            )}
          </div>
        </div>
      </StickyHeader>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="p-4">
          <Text variant="description">المخطط</Text>
          <Text variant="body" className="text-lg md:text-xl lg:text-2xl font-bold">{formatCurrency(stats.totalPlanned)}</Text>
        </Card>
        <Card className="p-4">
          <Text variant="description">الفعلي</Text>
          <Text variant="body" className="text-lg md:text-xl lg:text-2xl font-bold">{formatCurrency(stats.totalActual)}</Text>
        </Card>
        <Card className="p-4">
          <Text variant="description">الفرق</Text>
          <Text
            variant="body"
            className={`text-lg md:text-xl lg:text-2xl font-bold ${
              stats.variance >= 0 ? "text-red-600" : "text-green-600"
            }`}
          >
            {formatCurrency(Math.abs(stats.variance))}
          </Text>
        </Card>
        <Card className="p-4">
          <Text variant="description">نسبة الانحراف</Text>
          <div className="flex items-center gap-2">
            {stats.variancePercentage > 0 ? (
              <TrendingUp className="w-5 h-5 text-red-600" />
            ) : stats.variancePercentage < 0 ? (
              <TrendingDown className="w-5 h-5 text-green-600" />
            ) : (
              <Minus className="w-5 h-5 text-muted-foreground" />
            )}
            <Text variant="body" className="text-lg md:text-xl lg:text-2xl font-bold">
              {Math.abs(stats.variancePercentage)}%
            </Text>
          </div>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="annual">تقرير سنوي</TabsTrigger>
          <TabsTrigger value="quarterly">تقرير حسب الفصول</TabsTrigger>
          <TabsTrigger value="monthly">تقرير شهري</TabsTrigger>
        </TabsList>

        <TabsContent value="annual">
          <BudgetReport
            data={report}
            isLoading={isLoading}
            onRefresh={refetch}
            onEdit={(item) => {
              setEditItem(item);
              setShowForm(true);
            }}
            onDelete={async (item) => {
              const ok = await confirm({
                title: `هل أنت متأكد من حذف بند "${item.accountName}"؟`,
                variant: "danger",
              });
              if (ok) {
                deleteMutation.mutate(item.budgetItemID);
              }
            }}
          />
        </TabsContent>

        <TabsContent value="quarterly">
          <BudgetReport
            data={report}
            isLoading={isLoading}
            onRefresh={refetch}
            showQuarters
            onEdit={(item) => {
              setEditItem(item);
              setShowForm(true);
            }}
            onDelete={async (item) => {
              const ok = await confirm({
                title: `هل أنت متأكد من حذف بند "${item.accountName}"؟`,
                variant: "danger",
              });
              if (ok) {
                deleteMutation.mutate(item.budgetItemID);
              }
            }}
          />
        </TabsContent>

        <TabsContent value="monthly">
          {/* فلتر الشهر */}
          <div className="mb-4">
            <Select
              value={selectedMonth != null ? String(selectedMonth) : "all"}
              onValueChange={(v) => setSelectedMonth(v === "all" ? null : Number(v))}
            >
              <SelectTrigger className="w-60">
                <SelectValue placeholder="اختر الشهر" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">جميع الشهور</SelectItem>
                {periods?.map((p) => (
                  <SelectItem key={p.periodID} value={String(p.periodID)}>
                    {p.periodName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <BudgetReport
            data={report}
            isLoading={isLoading}
            onRefresh={refetch}
            showMonthly
            selectedMonth={selectedMonth}
            onEdit={(item) => {
              setEditItem(item);
              setShowForm(true);
            }}
            onDelete={async (item) => {
              const ok = await confirm({
                title: `هل أنت متأكد من حذف بند "${item.accountName}"؟`,
                variant: "danger",
              });
              if (ok) {
                deleteMutation.mutate(item.budgetItemID);
              }
            }}
          />
        </TabsContent>
      </Tabs>

      {/* Form Dialog */}
      <BudgetFormDialog
        open={showForm}
        onOpenChange={(open) => {
          setShowForm(open);
          if (!open) setEditItem(null);
        }}
        fiscalYearID={fiscalYearID}
        editItem={editItem}
        onSuccess={() => {
          setShowForm(false);
          setEditItem(null);
          refetch();
        }}
      />

      {/* Seed Budget Confirm */}
      <ConfirmDialog
        open={showSeedConfirm}
        onOpenChange={setShowSeedConfirm}
        title="إدخال موازنة مسبقة"
        description="سيتم إدخال بنود الموازنة (سنوية + ربعية + شهرية) للسنة المالية المحددة. الحسابات الموجودة مسبقاً لن تتكرر."
        confirmLabel="إدخال"
        variant="warning"
        isLoading={seedMutation.isPending}
        onConfirm={() => {
          if (fiscalYearID) seedMutation.mutate(fiscalYearID);
        }}
      />
      {ConfirmDialogElement}
    </PageLayout>
  );
}
