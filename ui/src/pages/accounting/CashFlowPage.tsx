import { useQuery } from "@tanstack/react-query";
import { ArrowDown, ArrowUp,DollarSign, RefreshCw, TrendingDown, TrendingUp } from "lucide-react";
import { useEffect, useState } from "react";

import { fiscalYearsApi,reportsApi } from "@/api/accounting";
import {
  Button,
  Card,
  Heading,
  PageLayout,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Skeleton,
  StickyHeader,
  Text,
} from "@/components/ui";
import { formatCurrency } from "@/lib/utils";
import type { CashFlowStatementDto,FiscalYearSummaryDto } from "@/types/accounting";

export function CashFlowPage() {
  const [fiscalYearID, setFiscalYearID] = useState<number | null>(null);

  const { data: fiscalYears } = useQuery<FiscalYearSummaryDto[]>({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  const { data: report, isLoading, refetch } = useQuery<CashFlowStatementDto>({
    queryKey: ["cash-flow", fiscalYearID],
    queryFn: () => reportsApi.cashFlow(fiscalYearID!),
    enabled: fiscalYearID != null,
  });

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

  if (isLoading) {
    return (
      <div className="p-8 space-y-8" dir="rtl">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {[1, 2, 3].map((i) => <Skeleton key={i} className="h-32 rounded-3xl" />)}
        </div>
        <Skeleton className="h-[400px] rounded-3xl" />
      </div>
    );
  }

  return (
    <PageLayout>
      <StickyHeader>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-primary/10 rounded-2xl">
              <DollarSign className="w-8 h-8 text-primary" />
            </div>
            <div>
              <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">قائمة التدفقات النقدية</Heading>
              <p className="text-muted-foreground mt-1">تحليل التدفقات النقدية للسنة المالية</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <Select
              value={fiscalYearID != null ? String(fiscalYearID) : undefined}
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
          </div>
        </div>
      </StickyHeader>

      {report ? (
        <>
          {/* الإجماليات */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Card className="p-4">
              <div className="flex items-center justify-between">
                <div>
                  <Text variant="description">التدفقات التشغيلية</Text>
                  <Text variant="body" className={`text-lg md:text-xl lg:text-2xl font-bold ${report.netOperatingCashFlow >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                    {formatCurrency(report.netOperatingCashFlow)}
                  </Text>
                </div>
                {report.netOperatingCashFlow >= 0 ? (
                  <TrendingUp className="w-6 h-6 text-green-600" />
                ) : (
                  <TrendingDown className="w-6 h-6 text-red-600" />
                )}
              </div>
            </Card>
            <Card className="p-4">
              <div className="flex items-center justify-between">
                <div>
                  <Text variant="description">التدفقات الاستثمارية</Text>
                  <Text variant="body" className={`text-lg md:text-xl lg:text-2xl font-bold ${report.netInvestingCashFlow >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                    {formatCurrency(report.netInvestingCashFlow)}
                  </Text>
                </div>
                {report.netInvestingCashFlow >= 0 ? (
                  <TrendingUp className="w-6 h-6 text-green-600" />
                ) : (
                  <TrendingDown className="w-6 h-6 text-red-600" />
                )}
              </div>
            </Card>
            <Card className="p-4">
              <div className="flex items-center justify-between">
                <div>
                  <Text variant="description">التدفقات التمويلية</Text>
                  <Text variant="body" className={`text-lg md:text-xl lg:text-2xl font-bold ${report.netFinancingCashFlow >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                    {formatCurrency(report.netFinancingCashFlow)}
                  </Text>
                </div>
                {report.netFinancingCashFlow >= 0 ? (
                  <TrendingUp className="w-6 h-6 text-green-600" />
                ) : (
                  <TrendingDown className="w-6 h-6 text-red-600" />
                )}
              </div>
            </Card>
          </div>

          {/* صافي التدفق النقدي */}
          <Card className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <Text variant="description">صافي التدفق النقدي</Text>
                <Text variant="body" className={`text-xl md:text-2xl lg:text-3xl font-bold ${report.netCashFlow >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                  {formatCurrency(report.netCashFlow)}
                </Text>
              </div>
              <div className="text-left">
                <Text variant="description">الرصيد النهائي</Text>
                <Text variant="body" className="text-base md:text-lg lg:text-xl font-bold">{formatCurrency(report.endingCashBalance)}</Text>
              </div>
            </div>
          </Card>

          {/* التفاصيل */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
            {/* التدفقات التشغيلية */}
            <Card className="border-none rounded-3xl overflow-hidden">
              <div className="p-4 bg-muted/30">
                <Heading level="h3" className="font-bold text-lg">التدفقات التشغيلية</Heading>
                <div className="flex gap-4 text-sm mt-2">
                  <span className="text-green-600">وارد: {formatCurrency(report.operatingCashInflow)}</span>
                  <span className="text-red-600">صادر: {formatCurrency(report.operatingCashOutflow)}</span>
                </div>
              </div>
              <div className="p-4 max-h-64 overflow-y-auto">
                {report.operatingItems.length === 0 ? (
                  <Text variant="description" className="text-center">لا توجد حركات</Text>
                ) : (
                  <div className="space-y-2">
                    {report.operatingItems.map((item, i) => (
                      <div key={i} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
                        <div className="flex items-center gap-2">
                          {item.type === "Inflow" ? (
                            <ArrowDown className="w-4 h-4 text-green-600" />
                          ) : (
                            <ArrowUp className="w-4 h-4 text-red-600" />
                          )}
                          <span className="text-sm">{item.accountName}</span>
                        </div>
                        <span className={`text-sm font-medium ${item.type === "Inflow" ? "text-green-600" : "text-red-600"}`}>
                          {formatCurrency(item.amount)}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </Card>

            {/* التدفقات الاستثمارية */}
            <Card className="border-none rounded-3xl overflow-hidden">
              <div className="p-4 bg-muted/30">
                <Heading level="h3" className="font-bold text-lg">التدفقات الاستثمارية</Heading>
                <div className="flex gap-4 text-sm mt-2">
                  <span className="text-green-600">وارد: {formatCurrency(report.investingCashInflow)}</span>
                  <span className="text-red-600">صادر: {formatCurrency(report.investingCashOutflow)}</span>
                </div>
              </div>
              <div className="p-4 max-h-64 overflow-y-auto">
                {report.investingItems.length === 0 ? (
                  <Text variant="description" className="text-center">لا توجد حركات</Text>
                ) : (
                  <div className="space-y-2">
                    {report.investingItems.map((item, i) => (
                      <div key={i} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
                        <div className="flex items-center gap-2">
                          {item.type === "Inflow" ? (
                            <ArrowDown className="w-4 h-4 text-green-600" />
                          ) : (
                            <ArrowUp className="w-4 h-4 text-red-600" />
                          )}
                          <span className="text-sm">{item.accountName}</span>
                        </div>
                        <span className={`text-sm font-medium ${item.type === "Inflow" ? "text-green-600" : "text-red-600"}`}>
                          {formatCurrency(item.amount)}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </Card>

            {/* التدفقات التمويلية */}
            <Card className="border-none rounded-3xl overflow-hidden">
              <div className="p-4 bg-muted/30">
                <Heading level="h3" className="font-bold text-lg">التدفقات التمويلية</Heading>
                <div className="flex gap-4 text-sm mt-2">
                  <span className="text-green-600">وارد: {formatCurrency(report.financingCashInflow)}</span>
                  <span className="text-red-600">صادر: {formatCurrency(report.financingCashOutflow)}</span>
                </div>
              </div>
              <div className="p-4 max-h-64 overflow-y-auto">
                {report.financingItems.length === 0 ? (
                  <Text variant="description" className="text-center">لا توجد حركات</Text>
                ) : (
                  <div className="space-y-2">
                    {report.financingItems.map((item, i) => (
                      <div key={i} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
                        <div className="flex items-center gap-2">
                          {item.type === "Inflow" ? (
                            <ArrowDown className="w-4 h-4 text-green-600" />
                          ) : (
                            <ArrowUp className="w-4 h-4 text-red-600" />
                          )}
                          <span className="text-sm">{item.accountName}</span>
                        </div>
                        <span className={`text-sm font-medium ${item.type === "Inflow" ? "text-green-600" : "text-red-600"}`}>
                          {formatCurrency(item.amount)}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </Card>
          </div>
        </>
      ) : (
        <Card                     className="p-6 sm:p-12 text-center text-muted-foreground">
          لا توجد بيانات متاحة
        </Card>
      )}
    </PageLayout>
  );
}
