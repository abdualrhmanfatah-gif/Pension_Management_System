import { useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import {
  ArrowUpRight,
  Banknote,
  BookOpen,
  Calculator,
  DollarSign,
  FileText,
  Landmark,
  Receipt,
  TrendingDown,
  TrendingUp,
} from "lucide-react";

import { accountingApi } from "@/api/accounting";
import { Button, Card, ErrorState, Heading, PageLayout, Skeleton, StatCard, StickyHeader, Text } from "@/components/ui";
import { formatCurrency } from "@/lib/utils";
import { InitialOpeningBalanceDialog } from "@/components/domain/accounting/InitialOpeningBalanceDialog";
import type { AccountingDashboardDto } from "@/types/accounting";

export function AccountingDashboardPage() {
  const navigate = useNavigate();
  const [showInitialOpening, setShowInitialOpening] = useState(false);

  const { data: dashboard, isLoading, error } = useQuery<AccountingDashboardDto>({
    queryKey: ["accounting", "dashboard"],
    queryFn: () => accountingApi.getDashboard(new Date().getFullYear()),
  });

  useEffect(() => {
    if (error) toast.error("تعذر تحميل لوحة المحاسبة", { id: "accounting-dashboard" });
  }, [error]);

  if (isLoading) {
    return (
      <div className="p-8 space-y-8" dir="rtl">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 sm:gap-6">
          {[1, 2, 3, 4].map((i) => <Skeleton key={i} className="h-32 rounded-3xl" />)}
        </div>
        <Skeleton className="h-[400px] rounded-3xl" />
      </div>
    );
  }

  if (error) {
    return <ErrorState message="تعذر تحميل بيانات المحاسبة" />;
  }

  const d = dashboard;

  return (
    <PageLayout>
      {/* Header */}
      <StickyHeader>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-primary/10 rounded-2xl">
              <Calculator className="w-8 h-8 text-primary" />
            </div>
            <div>
              <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">مركز الالإدارة المالية</Heading>
              <p className="text-muted-foreground mt-1">إدارة الحسابات والتقارير المالية</p>
            </div>
          </div>

          <div className="flex items-center gap-3 flex-wrap">
            <Button onClick={() => setShowInitialOpening(true)} variant="outline" className="gap-2">
              <Banknote className="w-4 h-4" />
              أرصدة افتتاحية
            </Button>
            <Button onClick={() => navigate("/accounting/journal-entries")} variant="outline" className="gap-2">
              <FileText className="w-4 h-4" />
              القيود اليومية
            </Button>
            <Button onClick={() => navigate("/accounting/chart-of-accounts")} variant="outline" className="gap-2">
              <BookOpen className="w-4 h-4" />
              دليل الحسابات
            </Button>
            <Button onClick={() => navigate("/accounting/reports")} variant="outline" className="gap-2">
              <Receipt className="w-4 h-4" />
              التقارير المالية
            </Button>
          </div>
        </div>
      </StickyHeader>

      {/* KPIs */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <StatCard
          title="إجمالي الأصول"
          value={formatCurrency(d?.totalAssets || 0)}
          icon={<Landmark className="w-6 h-6" />}
          variant="blue"
        />
        <StatCard
          title="إجمالي الخصوم"
          value={formatCurrency(d?.totalLiabilities || 0)}
          icon={<DollarSign className="w-6 h-6" />}
          variant="amber"
        />
        <StatCard
          title="حقوق الملكية"
          value={formatCurrency(d?.totalEquity || 0)}
          icon={<Banknote className="w-6 h-6" />}
          variant="emerald"
        />
        <StatCard
          title="صافي الدخل"
          value={formatCurrency(d?.netIncome || 0)}
          icon={(d?.netIncome ?? 0) >= 0 ? <TrendingUp className="w-6 h-6" /> : <TrendingDown className="w-6 h-6" />}
          trend={d ? { value: Math.abs(d.netIncome / (d.totalRevenue || 1) * 100), isPositive: d.netIncome >= 0 } : undefined}
          variant={(d?.netIncome ?? 0) >= 0 ? "emerald" : "destructive"}
        />
      </div>

      {/* Secondary KPIs */}
      <div className="grid grid-cols-2 md:grid-cols-3 gap-3 sm:gap-6">
        <StatCard
          title="إجمالي الإيرادات"
          value={formatCurrency(d?.totalRevenue || 0)}
          icon={<TrendingUp className="w-6 h-6" />}
          variant="emerald"
        />
        <StatCard
          title="إجمالي المصروفات"
          value={formatCurrency(d?.totalExpenses || 0)}
          icon={<TrendingDown className="w-6 h-6" />}
          variant="destructive"
        />
        <StatCard
          title="عدد القيود"
          value={String(d?.totalJournalEntries ?? 0)}
          icon={<FileText className="w-6 h-6" />}
          description={`${d?.draftEntries ?? 0} مسودة · ${d?.postedEntries ?? 0} مرحّل · ${d?.reversedEntries ?? 0} ملغي`}
          variant="purple"
        />
      </div>

      {/* Monthly Summary + Recent Entries */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Monthly Summary */}
        <Card className="border-none rounded-3xl p-6">
          <Heading level="h3" className="font-semibold mb-4">الملخص الشهري</Heading>
          {d?.monthlySummary && d.monthlySummary.length > 0 ? (
            <div className="space-y-3">
              {d.monthlySummary.map((m) => (
                <div key={m.month} className="flex items-center justify-between py-2 border-b border-border/40 last:border-0">
                  <span className="text-sm font-medium w-24">{m.month}</span>
                  <div className="flex gap-4 text-xs">
                    <span className="text-emerald-600">{formatCurrency(m.revenue)}</span>
                    <span className="text-red-600">{formatCurrency(m.expenses)}</span>
                    <span className={m.netIncome >= 0 ? "text-emerald-700 font-medium" : "text-red-700 font-medium"}>
                      {formatCurrency(m.netIncome)}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <Text variant="description">لا توجد بيانات شهرية</Text>
          )}
        </Card>

        {/* Recent Entries */}
        <Card className="border-none rounded-3xl p-6">
          <div className="flex items-center justify-between mb-4">
            <Heading level="h3" className="font-semibold">آخر القيود</Heading>
            <Button variant="ghost" size="sm" onClick={() => navigate("/accounting/journal-entries")} className="gap-1">
              عرض الكل <ArrowUpRight className="w-3.5 h-3.5" />
            </Button>
          </div>
          {d?.recentEntries && d.recentEntries.length > 0 ? (
            <div className="space-y-2">
              {d.recentEntries.map((entry) => (
                <div
                  key={entry.journalEntryID}
                  className="flex items-center justify-between p-2 rounded-xl hover:bg-accent/50 cursor-pointer transition-colors"
                  onClick={() => navigate(`/accounting/journal-entries/${entry.journalEntryID}`)}
                >
                  <div>
                    <Text variant="label">{entry.description}</Text>
                    <Text variant="caption">
                      <span className="font-mono ltr" dir="ltr">{entry.entryNumber}</span> · {new Date(entry.entryDate).toLocaleDateString("ar-SA")}
                    </Text>
                  </div>
                  <div className="text-left">
                    <Text variant="body" className="font-mono">{formatCurrency(entry.totalDebit)}</Text>
                    <Text variant="caption">{entry.journalEntryTypeName || entry.status}</Text>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <Text variant="description">لا توجد قيود حديثة</Text>
          )}
        </Card>
      </div>

      {/* Periods info */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card className="border-none rounded-3xl p-6">
          <Heading level="h3" className="font-semibold mb-2">الفترات المحاسبية</Heading>
          <div className="flex items-center gap-4 text-sm">
            <span>مفتوحة: <strong>{d?.openPeriods ?? 0}</strong></span>
            <span>مغلقة: <strong>{d?.closedPeriods ?? 0}</strong></span>
          </div>
        </Card>
        <Card className="border-none rounded-3xl p-6">
          <Heading level="h3" className="font-semibold mb-2">مراكز التكلفة</Heading>
          {d?.costCenterBreakdown && d.costCenterBreakdown.length > 0 ? (
            <div className="space-y-1">
              {d.costCenterBreakdown.slice(0, 5).map((cc, i) => (
                <div key={i} className="flex justify-between text-sm">
                  <span>{cc.costCenterName}</span>
                  <span className="font-mono text-xs">{formatCurrency(cc.totalDebit)}</span>
                </div>
              ))}
            </div>
          ) : (
            <Text variant="description">لا توجد بيانات</Text>
          )}
        </Card>
      </div>

      <InitialOpeningBalanceDialog open={showInitialOpening} onOpenChange={setShowInitialOpening} />
    </PageLayout>
  );
}
