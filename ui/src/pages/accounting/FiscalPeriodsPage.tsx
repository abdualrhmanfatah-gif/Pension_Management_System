import { useQuery } from "@tanstack/react-query";
import { Calendar, RefreshCw } from "lucide-react";
import { useEffect, useState } from "react";

import { fiscalYearsApi, periodsApi } from "@/api/accounting";
import { FiscalYearCloseDialog, FiscalYearFormDialog } from "@/components/domain/accounting";
import { Badge, Button, Card, type CustomColumnDef, DataTable, Heading, PageLayout, Select, SelectContent, SelectItem, SelectTrigger, SelectValue, Skeleton, StickyHeader } from "@/components/ui";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { FiscalPeriodDto, FiscalYearSummaryDto } from "@/types/accounting";

export function FiscalPeriodsPage() {
  const canManage = useHasPermission("Accounting.Manage", "System.Admin");
  const [showYearClose, setShowYearClose] = useState(false);
  const [showCreateYear, setShowCreateYear] = useState(false);
  const [fiscalYearID, setFiscalYearID] = useState<number | null>(null);

  const { data: fiscalYears } = useQuery<FiscalYearSummaryDto[]>({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  const { data: periods, isLoading, error, refetch } = useQuery<FiscalPeriodDto[]>({
    queryKey: ["periods", fiscalYearID],
    queryFn: () => periodsApi.getAll(fiscalYearID!),
    enabled: fiscalYearID != null,
  });

  const { data: activePeriod } = useQuery<FiscalPeriodDto>({
    queryKey: ["periods", "active", fiscalYearID],
    queryFn: () => periodsApi.getActive(fiscalYearID!),
    enabled: fiscalYearID != null,
  });

  // Auto-select current year when data loads
  useEffect(() => {
    if (fiscalYears && fiscalYearID === null) {
      const currentYear = new Date().getFullYear().toString();
      const match = fiscalYears.find(y => y.yearName === currentYear);
      if (match) {
        setFiscalYearID(match.fiscalYearID);
      } else if (fiscalYears.length > 0) {
        setFiscalYearID(fiscalYears[0].fiscalYearID);
      }
    }
  }, [fiscalYears]);

  const selectedYear = fiscalYears?.find(y => y.fiscalYearID === fiscalYearID);

  const columns: CustomColumnDef<FiscalPeriodDto>[] = [
    { key: "periodName", header: "الفترة", accessorFn: (row) => row.periodName },
    { key: "quarter", header: "الفصل", render: (row) => <Badge variant="outline" className="text-xs">Q{Math.ceil(row.periodNumber / 3)}</Badge> },
    { key: "periodNumber", header: "الرقم", accessorFn: (row) => row.periodNumber },
    { key: "startDate", header: "تاريخ البداية", render: (row) => <span className="text-xs">{formatDate(row.startDate)}</span> },
    { key: "endDate", header: "تاريخ النهاية", render: (row) => <span className="text-xs">{formatDate(row.endDate)}</span> },
    { key: "isClosed", header: "الحالة", render: (row) => <Badge variant={row.isClosed ? "destructive" : "success"}>{row.isClosed ? "مغلقة" : "مفتوحة"}</Badge> },
    { key: "closedByUserName", header: "غلق بواسطة", render: (row) => <span className="text-xs text-muted-foreground">{row.closedByUserName || "—"}</span> },
  ];

  return (
    <PageLayout>
      <StickyHeader>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-primary/10 rounded-2xl">
              <Calendar className="w-8 h-8 text-primary" />
            </div>
            <div>
              <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">الفترات المحاسبية</Heading>
              <p className="text-muted-foreground mt-1">إدارة الفترات المالية</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <Select value={fiscalYearID != null ? String(fiscalYearID) : undefined} onValueChange={(v) => setFiscalYearID(Number(v))}>
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
                <Button variant="outline" onClick={() => setShowCreateYear(true)} className="gap-2">
                  إضافة سنة مالية
                </Button>
                <Button onClick={() => setShowYearClose(true)} className="gap-2">
                  إقفال السنة
                </Button>
              </>
            )}
          </div>
        </div>
      </StickyHeader>

      {/* Active period notice */}
      {activePeriod && (
        <div className="bg-emerald-500/5 border border-emerald-500 text-emerald-800 px-4 py-3 rounded-xl text-sm">
          الفترة النشطة حالياً: <strong>{activePeriod.periodName}</strong> —
          من {new Date(activePeriod.startDate).toLocaleDateString("ar-SA")}
          إلى {new Date(activePeriod.endDate).toLocaleDateString("ar-SA")}
        </div>
      )}

      {isLoading ? (
        <Skeleton className="h-64 rounded-3xl" />
      ) : error ? (
        <div className="text-destructive text-center p-8">فشل في تحميل الفترات</div>
      ) : !periods || periods.length === 0 ? (
        <Card className="border-none rounded-3xl p-4 sm:p-8 lg:p-12 text-center text-muted-foreground">
          لا توجد فترات محاسبية لسنة {selectedYear?.yearName ?? "..."}
        </Card>
      ) : (
        <Card className="border-none rounded-3xl overflow-hidden">
          <DataTable
            columns={columns}
            data={periods}
            rowKey={(row) => row.periodID}
          />
        </Card>
      )}

      <FiscalYearCloseDialog open={showYearClose} onOpenChange={setShowYearClose} />
      <FiscalYearFormDialog open={showCreateYear} onOpenChange={setShowCreateYear} />
    </PageLayout>
  );
}
