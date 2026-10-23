import { useMutation, useQuery } from "@tanstack/react-query";
import { Calendar, CheckCircle, ChevronDown, ChevronRight, Clock, Eye,RefreshCw } from "lucide-react";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { fiscalYearsApi, quartersApi } from "@/api/accounting";
import {
  Badge,
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
} from "@/components/ui";
import { useHasPermission } from "@/stores/authStore";
import type { FiscalYearSummaryDto, QuarterDto } from "@/types/accounting";


export function QuartersPage() {
  const navigate = useNavigate();
  const canManage = useHasPermission("Accounting.Manage", "System.Admin");
  const [fiscalYearID, setFiscalYearID] = useState<number | null>(null);
  const [expandedQuarter, setExpandedQuarter] = useState<number | null>(null);

  const { data: fiscalYears } = useQuery<FiscalYearSummaryDto[]>({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  const {
    data: quarters,
    isLoading,
    refetch,
  } = useQuery<QuarterDto[]>({
    queryKey: ["quarters", fiscalYearID],
    queryFn: () => quartersApi.getAll(fiscalYearID!),
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

  const closeMutation = useMutation({
    mutationFn: (quarterID: number) => quartersApi.close({ quarterID }),
    onSuccess: () => {
      toast.success("تم إغلاق الفصل بنجاح");
      refetch();
    },
    onError: (err: any) => {
      toast.error(err?.response?.data?.message || "فشل في إغلاق الفصل");
    },
  });

  const selectedYear = fiscalYears?.find((y) => y.fiscalYearID === fiscalYearID);

  const toggleExpand = (quarterId: number) => {
    setExpandedQuarter(expandedQuarter === quarterId ? null : quarterId);
  };

  const allClosed = quarters?.every((q) => q.isClosed) ?? false;

  return (
    <PageLayout>
      <StickyHeader>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-primary/10 rounded-2xl">
              <Calendar className="w-8 h-8 text-primary" />
            </div>
            <div>
              <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">الفصول المالية</Heading>
              <p className="text-muted-foreground mt-1">إدارة الأرباع السنوية</p>
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

      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} className="h-48 rounded-3xl" />
          ))}
        </div>
      ) : !quarters || quarters.length === 0 ? (
        <Card className="border-none rounded-3xl p-4 sm:p-8 lg:p-12 text-center text-muted-foreground">
          لا توجد فصول مالية لسنة {selectedYear?.yearName ?? "..."}
        </Card>
      ) : (
        <>
          {/* Summary cards */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {quarters.map((q) => (
              <Card
                key={q.quarterID}
                className={`border-none rounded-3xl p-5 cursor-pointer transition-all ${
                  expandedQuarter === q.quarterID ? "ring-2 ring-primary" : ""
                }`}
              >
                <div className="flex items-center justify-between mb-3" onClick={() => toggleExpand(q.quarterID)}>
                   <Heading level="h3" className="text-lg font-bold">{q.quarterName}</Heading>
                  {q.isClosed ? (
                    <CheckCircle className="w-5 h-5 text-green-600" />
                  ) : (
                    <Clock className="w-5 h-5 text-amber-500" />
                  )}
                </div>
                <div className="space-y-2 text-sm text-muted-foreground" onClick={() => toggleExpand(q.quarterID)}>
                  <div className="flex justify-between">
                    <span>الفترات</span>
                    <span className="font-medium text-foreground">{q.periods.length}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>المدة</span>
                    <span className="text-xs">
                      {new Date(q.startDate).toLocaleDateString("ar-SA")} — {new Date(q.endDate).toLocaleDateString("ar-SA")}
                    </span>
                  </div>
                  <Badge variant={q.isClosed ? "destructive" : "success"} className="w-full justify-center">
                    {q.isClosed ? "مغلق" : "مفتوح"}
                  </Badge>
                </div>
                <Button
                  size="sm"
                  variant="outline"
                  className="w-full mt-3 gap-1"
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/accounting/quarters/${q.quarterID}`);
                  }}
                >
                  <Eye className="w-4 h-4" /> تفاصيل
                </Button>
              </Card>
            ))}
          </div>

          {/* Expanded quarter details */}
          {expandedQuarter && quarters && (
            <Card className="border-none rounded-3xl overflow-hidden">
              {quarters
                .filter((q) => q.quarterID === expandedQuarter)
                .map((q) => (
                  <div key={q.quarterID}>
                    <div className="flex items-center justify-between p-4 bg-muted/30">
                      <div className="flex items-center gap-2">
                        {expandedQuarter ? (
                          <ChevronDown className="w-5 h-5" />
                        ) : (
                          <ChevronRight className="w-5 h-5" />
                        )}
                         <Heading level="h3" className="font-bold text-lg">{q.quarterName} — تفاصيل الفترات</Heading>
                      </div>
                      {!q.isClosed && canManage && (
                        <Button
                          size="sm"
                          variant="destructive"
                          onClick={() => closeMutation.mutate(q.quarterID)}
                          disabled={closeMutation.isPending}
                        >
                          إغلاق الفصل
                        </Button>
                      )}
                    </div>
                    <div className="overflow-x-auto">
                    <table className="w-full text-sm" role="table" aria-label="فترة الفصل">
                      <thead className="bg-muted/50 sticky top-0 z-10">
                        <tr>
                          <th className="text-end p-3">الفترة</th>
                          <th className="text-end p-3">الرقم</th>
                          <th className="text-end p-3">تاريخ البداية</th>
                          <th className="text-end p-3">تاريخ النهاية</th>
                          <th className="text-end p-3">الحالة</th>
                        </tr>
                      </thead>
                      <tbody>
                        {q.periods.map((period) => (
                          <tr key={period.periodID} className="border-t border-border hover:bg-muted/20">
                            <td className="p-3 font-medium">{period.periodName}</td>
                            <td className="p-3">{period.periodNumber}</td>
                            <td className="p-3 text-xs">
                              {new Date(period.startDate).toLocaleDateString("ar-SA")}
                            </td>
                            <td className="p-3 text-xs">
                              {new Date(period.endDate).toLocaleDateString("ar-SA")}
                            </td>
                            <td className="p-3">
                              <Badge variant={period.isClosed ? "destructive" : "success"}>
                                {period.isClosed ? "مغلقة" : "مفتوحة"}
                              </Badge>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                    </div>
                  </div>
                ))}
            </Card>
          )}

          {/* Year-end close notice */}
          {allClosed && (
            <div className="bg-emerald-500/5 border border-emerald-500 text-emerald-800 px-4 py-3 rounded-xl text-sm">
              جميع فصول السنة مغلقة — يمكن إقفال السنة المالية
            </div>
          )}
        </>
      )}
    </PageLayout>
  );
}
