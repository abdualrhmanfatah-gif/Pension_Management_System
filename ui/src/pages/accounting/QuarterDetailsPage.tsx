import { useQuery } from "@tanstack/react-query";
import {
  Download,
  Printer,
  RefreshCw,
  TrendingDown,
  TrendingUp,
  Wallet,
} from "lucide-react";
import { useEffect, useState } from "react";

import { fiscalYearsApi, quartersApi, reportsApi } from "@/api/accounting";
import type { VoucherDto } from "@/api/vouchers";
import { vouchersApi } from "@/api/vouchers";
import { TrialBalanceReport } from "@/components/domain/accounting";
import {
  altRowFill,
  buildReportFooter,
  buildReportHeader,
  buildTableHeader,
  buildTotalsRow,
  createWorkbook,
  exportWorkbook,
  type HeaderField,
  styleDataCell,
} from "@/components/domain/print/excel.utils";
import { StickyHeader, Text } from "@/components/ui";
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
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { cn, formatCurrency } from "@/lib/utils";
import type { FiscalYearSummaryDto, QuarterDetailsDto,QuarterDto, TrialBalanceItemDto } from "@/types/accounting";

export function QuarterDetailsPage() {
  const [fiscalYearID, setFiscalYearID] = useState<number | "">("");
  const [quarterId, setQuarterId] = useState<number | "">("");
  const [activeTab, setActiveTab] = useState("periods");

  const { data: fiscalYears } = useQuery<FiscalYearSummaryDto[]>({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  const { data: quarters } = useQuery<QuarterDto[]>({
    queryKey: ["quarters", fiscalYearID],
    queryFn: () => quartersApi.getAll(fiscalYearID as number),
    enabled: fiscalYearID !== "",
  });

  const { data: details, isLoading, refetch } = useQuery<QuarterDetailsDto>({
    queryKey: ["quarter-details", quarterId],
    queryFn: () => quartersApi.getDetails(quarterId as number),
    enabled: quarterId !== "",
  });

  const selectedQuarter = quarters?.find((q) => q.quarterID === quarterId);

  const { data: trialBalance } = useQuery<TrialBalanceItemDto[]>({
    queryKey: ["reports", "trial-balance", selectedQuarter?.startDate, selectedQuarter?.endDate],
    queryFn: () =>
      reportsApi.trialBalance({
        fromDate: selectedQuarter!.startDate,
        toDate: selectedQuarter!.endDate,
      }),
    enabled: !!selectedQuarter,
  });

  const { data: vouchersResult } = useQuery<{ items: VoucherDto[] }>({
    queryKey: ["vouchers-quarter", selectedQuarter?.startDate, selectedQuarter?.endDate],
    queryFn: () =>
      vouchersApi.list({
        fromDate: selectedQuarter!.startDate,
        toDate: selectedQuarter!.endDate,
        voucherType: 1,
        pageSize: 500,
      }),
    enabled: !!selectedQuarter,
  });

  const quarterVouchers = vouchersResult?.items ?? [];

  useEffect(() => {
    if (fiscalYears && fiscalYearID === "") {
      const currentYear = new Date().getFullYear().toString();
      const match = fiscalYears.find((y) => y.yearName === currentYear);
      if (match) setFiscalYearID(match.fiscalYearID);
      else if (fiscalYears.length > 0) setFiscalYearID(fiscalYears[0].fiscalYearID);
    }
  }, [fiscalYears]);

  useEffect(() => {
    if (quarters && quarters.length > 0 && quarterId === "") {
      setQuarterId(quarters[0].quarterID);
    }
  }, [quarters]);

  /* ═══ التصدير بتصميم احترافي ═══ */
  const quarterName = selectedQuarter?.quarterName ?? "";
  const yearNum = selectedQuarter?.startDate ? new Date(selectedQuarter.startDate).getFullYear() : "";
  const trialTotalDebit = trialBalance?.reduce((s, i) => s + i.totalDebit, 0) ?? 0;
  const trialTotalCredit = trialBalance?.reduce((s, i) => s + i.totalCredit, 0) ?? 0;
  void trialTotalCredit;

  const metaQuarter: HeaderField[] = [
    { label: "الفصل", value: quarterName },
    { label: "السنة", value: String(yearNum) },
  ];

  const exportPeriods = async () => {
    if (!details) return;
    const { ws } = createWorkbook("تفصيل الفترة");
    ws.columns = [
      { header: "", key: "period", width: 25 },
      { header: "", key: "opening", width: 20 },
      { header: "", key: "withdrawn", width: 20 },
      { header: "", key: "spent", width: 20 },
      { header: "", key: "closing", width: 20 },
    ];
    const startRow = await buildReportHeader(ws, "تفصيل حسب الفترة", 5, metaQuarter);
    buildTableHeader(ws, startRow, ["الفترة", "الرصيد الافتتاحي", "سحب من البنك", "الصرف الفعلي", "الرصيد الختامي"]);

    details.periods.forEach((p, i) => {
      const r = ws.getRow(startRow + 1 + i);
      r.getCell(1).value = p.periodName;
      r.getCell(2).value = p.openingBalance;
      r.getCell(3).value = p.withdrawn;
      r.getCell(4).value = p.spent;
      r.getCell(5).value = p.closingBalance;
      styleDataCell(r.getCell(1), { bold: true, align: "start" });
      styleDataCell(r.getCell(2), { numFmt: "#,##0" });
      styleDataCell(r.getCell(3), { color: "FF1D4ED8", numFmt: "#,##0" });
      styleDataCell(r.getCell(4), { color: "FFDC2626", numFmt: "#,##0" });
      styleDataCell(r.getCell(5), { bold: true, numFmt: "#,##0" });
      for (let c = 1; c <= 5; c++) altRowFill(r.getCell(c), i);
    });

    const totalRowNum = startRow + 1 + details.periods.length;
    buildTotalsRow(ws, totalRowNum, [
      { value: "الإجمالي" },
      { value: "—" },
      { value: details.totalWithdrawn, numFmt: "#,##0", color: "FF1D4ED8" },
      { value: details.totalSpent, numFmt: "#,##0", color: "FFDC2626" },
      { value: details.remainingBalance, numFmt: "#,##0", bold: true },
    ]);
    ws.getRow(totalRowNum).height = 22;

    buildReportFooter(ws, 5);
    await exportWorkbook(ws.workbook, `فترة_${selectedQuarter?.quarterName ?? "فصل"}`);
  };

  const exportExpenses = async () => {
    if (!details) return;
    const { ws } = createWorkbook("تفصيل المصروفات");
    ws.columns = [
      { header: "", key: "code", width: 15 },
      { header: "", key: "name", width: 35 },
      { header: "", key: "amount", width: 20 },
      { header: "", key: "count", width: 12 },
    ];
    const startRow = await buildReportHeader(ws, "تفصيل المصروفات حسب الحساب", 4, metaQuarter);
    buildTableHeader(ws, startRow, ["كود الحساب", "اسم الحساب", "المبلغ", "عدد القيود"]);

    details.expenseBreakdown.forEach((e, i) => {
      const r = ws.getRow(startRow + 1 + i);
      r.getCell(1).value = e.accountCode;
      r.getCell(2).value = e.accountName;
      r.getCell(3).value = e.amount;
      r.getCell(4).value = e.entryCount;
      styleDataCell(r.getCell(1), { bold: true });
      styleDataCell(r.getCell(2), { bold: true, align: "start" });
      styleDataCell(r.getCell(3), { color: "FFDC2626", numFmt: "#,##0" });
      styleDataCell(r.getCell(4), { bold: true });
      for (let c = 1; c <= 4; c++) altRowFill(r.getCell(c), i);
    });

    buildReportFooter(ws, 4);
    await exportWorkbook(ws.workbook, `مصروفات_${selectedQuarter?.quarterName ?? "فصل"}`);
  };

  const exportTrialBalanceData = async (trialData: TrialBalanceItemDto[]) => {
    const { ws } = createWorkbook("ميزان المراجعه");
    const COL_COUNT = 9;
    ws.columns = [
      { header: "", key: "code", width: 15 },
      { header: "", key: "name", width: 28 },
      { header: "", key: "openDebit", width: 18 },
      { header: "", key: "openCredit", width: 18 },
      { header: "", key: "movDebit", width: 18 },
      { header: "", key: "movCredit", width: 18 },
      { header: "", key: "closeDebit", width: 18 },
      { header: "", key: "closeCredit", width: 18 },
      { header: "", key: "balance", width: 18 },
    ];
    const startRow = await buildReportHeader(ws, "ميزان المراجعه للفصل", COL_COUNT, metaQuarter);
    buildTableHeader(ws, startRow, [
      "الكود", "الحساب",
      "افتتاحي مدين", "افتتاحي دائن",
      "حركه مدين", "حركه دائن",
      "ختامي مدين", "ختامي دائن",
      "الرصيد",
    ]);

    trialData.forEach((item, i) => {
      const r = ws.getRow(startRow + 1 + i);
      r.getCell(1).value = item.accountCode;
      r.getCell(2).value = item.accountName;
      r.getCell(3).value = item.openingDebit;
      r.getCell(4).value = item.openingCredit;
      r.getCell(5).value = item.movementDebit;
      r.getCell(6).value = item.movementCredit;
      r.getCell(7).value = item.closingDebit;
      r.getCell(8).value = item.closingCredit;
      r.getCell(9).value = item.balance;
      styleDataCell(r.getCell(1), { bold: true });
      styleDataCell(r.getCell(2), { bold: true, align: "start" });
      [3, 4, 5, 6, 7, 8].forEach(c => styleDataCell(r.getCell(c), { numFmt: "#,##0" }));
      styleDataCell(r.getCell(9), { numFmt: "#,##0", bold: true });
      for (let c = 1; c <= 9; c++) altRowFill(r.getCell(c), i);
    });

    const totalRowNum = startRow + 1 + trialData.length;
    const totOpenDebit = trialData.reduce((s, i) => s + i.openingDebit, 0);
    const totOpenCredit = trialData.reduce((s, i) => s + i.openingCredit, 0);
    const totMovDebit = trialData.reduce((s, i) => s + i.movementDebit, 0);
    const totMovCredit = trialData.reduce((s, i) => s + i.movementCredit, 0);
    const totCloseDebit = trialData.reduce((s, i) => s + i.closingDebit, 0);
    const totCloseCredit = trialData.reduce((s, i) => s + i.closingCredit, 0);
    buildTotalsRow(ws, totalRowNum, [
      { value: "الإجمالي" },
      undefined,
      { value: totOpenDebit, numFmt: "#,##0" },
      { value: totOpenCredit, numFmt: "#,##0" },
      { value: totMovDebit, numFmt: "#,##0" },
      { value: totMovCredit, numFmt: "#,##0" },
      { value: totCloseDebit, numFmt: "#,##0" },
      { value: totCloseCredit, numFmt: "#,##0" },
      { value: trialTotalDebit, numFmt: "#,##0" },
    ]);
    ws.getRow(totalRowNum).height = 22;

    buildReportFooter(ws, COL_COUNT);
    await exportWorkbook(ws.workbook, `ميزان_المراجعه_${selectedQuarter?.quarterName ?? "فصل"}`);
  };

  return (
    <>
      <PageLayout>
        {/* ═══ الشاشة ═══ */}
        <div className="screen-only">
          <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
            <StickyHeader>
              <div className="flex items-center gap-3">
                <div className="p-3 bg-primary/10 rounded-2xl"><Wallet className="w-8 h-8 text-primary" /></div>
                <div>
                   <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">تفاصيل الفصل</Heading>
                   <Text variant="description" className="mt-1">الميزانية التشغيلية حسب الفصل</Text>
                </div>
              </div>
            </StickyHeader>
            <div className="flex items-center gap-3">
              <Select value={fiscalYearID !== "" ? String(fiscalYearID) : ""} onValueChange={(v) => { setFiscalYearID(Number(v)); setQuarterId(""); }}>
                <SelectTrigger className="w-40"><SelectValue placeholder="السنة المالية" /></SelectTrigger>
                <SelectContent>{fiscalYears?.map((y) => (<SelectItem key={y.fiscalYearID} value={String(y.fiscalYearID)}>{y.yearName}</SelectItem>))}</SelectContent>
              </Select>
              <Select value={quarterId !== "" ? String(quarterId) : ""} onValueChange={(v) => setQuarterId(Number(v))}>
                <SelectTrigger className="w-40"><SelectValue placeholder="الفصل" /></SelectTrigger>
                <SelectContent>{quarters?.map((q) => (<SelectItem key={q.quarterID} value={String(q.quarterID)}>{q.quarterName}</SelectItem>))}</SelectContent>
              </Select>
              <Button variant="outline" onClick={() => refetch()} className="gap-2"><RefreshCw className="w-4 h-4" /></Button>
              <Button variant="outline" onClick={() => window.print()} className="gap-2"><Printer className="w-4 h-4" />طباعة</Button>
            </div>
          </div>

          {isLoading ? (
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">{[1, 2, 3].map((i) => (<Skeleton key={i} className="h-32 rounded-3xl" />))}</div>
          ) : !details ? (
            <Card className="border-none rounded-3xl p-4 sm:p-8 lg:p-12 text-center text-muted-foreground">اختر الفصل لعرض التفاصيل</Card>
          ) : (
            <div className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <Card className="border-none rounded-3xl p-6">
                  <div className="flex items-center gap-3 mb-3"><div className="p-2 bg-info/10 rounded-xl"><TrendingUp className="w-5 h-5 text-info" /></div><span className="text-sm text-muted-foreground">إجمالي السحب من البنك</span></div>
                  <Text variant="body" className="text-lg md:text-xl lg:text-2xl font-bold text-info">{formatCurrency(details.totalWithdrawn)}</Text>
                </Card>
                <Card className="border-none rounded-3xl p-6">
                  <div className="flex items-center gap-3 mb-3"><div className="p-2 bg-danger/10 rounded-xl"><TrendingDown className="w-5 h-5 text-danger" /></div><span className="text-sm text-muted-foreground">إجمالي الصرف الفعلي</span></div>
                  <Text variant="body" className="text-lg md:text-xl lg:text-2xl font-bold text-danger">{formatCurrency(details.totalSpent)}</Text>
                </Card>
                <Card className="border-none rounded-3xl p-6">
                  <div className="flex items-center gap-3 mb-3"><div className="p-2 bg-success/10 rounded-xl"><Wallet className="w-5 h-5 text-success" /></div><span className="text-sm text-muted-foreground">المتبقي</span></div>
                  <Text variant="body" className={cn("text-lg md:text-xl lg:text-2xl font-bold", details.remainingBalance >= 0 ? "text-success" : "text-danger")}>{formatCurrency(details.remainingBalance)}</Text>
                </Card>
              </div>

              <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full" dir="rtl">
                <TabsList className="grid w-full grid-cols-2 sm:grid-cols-4 h-14 bg-muted/50 rounded-2xl p-1">
                  <TabsTrigger value="periods" className="rounded-xl text-base font-bold data-[state=active]:bg-primary data-[state=active]:text-primary-foreground">
                    تفصيل الفترة
                  </TabsTrigger>
                  <TabsTrigger value="expenses" className="rounded-xl text-base font-bold data-[state=active]:bg-primary data-[state=active]:text-primary-foreground">
                    المصروفات حسب الحساب
                  </TabsTrigger>
                  <TabsTrigger value="vouchers" className="rounded-xl text-base font-bold data-[state=active]:bg-primary data-[state=active]:text-primary-foreground">
                    أوامر الصرف {quarterVouchers.length > 0 && `(${quarterVouchers.length})`}
                  </TabsTrigger>
                  <TabsTrigger value="trial-balance" className="rounded-xl text-base font-bold data-[state=active]:bg-primary data-[state=active]:text-primary-foreground">
                    ميزان المراجعه
                  </TabsTrigger>
                </TabsList>

                {/* ── تفصيل الفترة ── */}
                <TabsContent value="periods" className="mt-4">
                  <Card className="border-none rounded-3xl overflow-hidden">
                    <div className="p-4 bg-muted/30 font-bold flex items-center justify-between">تفصيل حسب الفترة<Button variant="outline" size="sm" onClick={() => exportPeriods()} className="gap-2"><Download className="w-4 h-4" />تصدير Excel</Button></div>
                        <div className="overflow-x-auto">
                        <table className="w-full text-base text-foreground" role="table" aria-label="أوامر الصرف">
                      <thead className="bg-muted/50 sticky top-0 z-10"><tr><th className="text-end p-4 text-sm md:text-base lg:text-xl">الفترة</th><th className="text-end p-4 text-sm md:text-base lg:text-xl">الافتتاحي</th><th className="text-end p-4 text-sm md:text-base lg:text-xl">سحب من البنك</th><th className="text-end p-4 text-sm md:text-base lg:text-xl">الصرف الفعلي</th><th className="text-end p-4 text-sm md:text-base lg:text-xl">الختامي</th></tr></thead>
                      <tbody>
                        {details.periods.map((p) => (<tr key={p.periodID} className="border-t border-border/50 hover:bg-muted/20"><td className="p-4 font-medium">{p.periodName}</td><td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg">{formatCurrency(p.openingBalance)}</td><td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg text-info">{p.withdrawn > 0 ? `+${formatCurrency(p.withdrawn)}` : "—"}</td><td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg text-destructive">{p.spent > 0 ? `-${formatCurrency(p.spent)}` : "—"}</td><td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg">{formatCurrency(p.closingBalance)}</td></tr>))}
                        <tr className="bg-muted/30 font-bold"><td className="p-4">الإجمالي</td><td className="p-4">—</td><td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg text-info">+{formatCurrency(details.totalWithdrawn)}</td><td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg text-destructive">-{formatCurrency(details.totalSpent)}</td><td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg">{formatCurrency(details.remainingBalance)}</td></tr>
                      </tbody>
                    </table>
                    </div>
                  </Card>
                </TabsContent>

                {/* ── المصروفات ── */}
                <TabsContent value="expenses" className="mt-4">
                  <Card className="border-none rounded-3xl overflow-hidden">
                    <div className="p-4 bg-muted/30 font-bold flex items-center justify-between">تفصيل المصروفات حسب الحساب<Button variant="outline" size="sm" onClick={() => exportExpenses()} className="gap-2"><Download className="w-4 h-4" />تصدير Excel</Button></div>
                    {details.expenseBreakdown.length > 0 ? (
                      <div className="overflow-x-auto">
                      <table className="w-full text-base text-foreground" role="table" aria-label="تفصيل المصروفات">
                        <thead className="bg-muted/50 sticky top-0 z-10"><tr><th className="text-end p-4 text-sm md:text-base lg:text-xl">كود الحساب</th><th className="text-end p-4 text-sm md:text-base lg:text-xl">اسم الحساب</th><th className="text-end p-4 text-sm md:text-base lg:text-xl">المبلغ</th><th className="text-end p-4 text-sm md:text-base lg:text-xl">عدد القيود</th></tr></thead>
                        <tbody>{details.expenseBreakdown.map((e) => (<tr key={e.accountID} className="border-t border-border/50 hover:bg-muted/20"><td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg">{e.accountCode}</td><td className="p-4">{e.accountName}</td><td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg text-destructive">{formatCurrency(e.amount)}</td><td className="p-4 text-center font-bold text-sm md:text-base lg:text-lg">{e.entryCount}</td></tr>))}</tbody>
                      </table>
                      </div>
                    ) : (
                      <div className="p-8 text-center text-muted-foreground">لا توجد مصروفات في هذا الفصل</div>
                    )}
                  </Card>
                </TabsContent>

                {/* ── أوامر الصرف ── */}
                <TabsContent value="vouchers" className="mt-4">
                  <Card className="border-none rounded-3xl overflow-hidden">
                    <div className="p-4 bg-muted/30 font-bold flex items-center justify-between">
                      أوامر الصرف ({quarterVouchers.length} سند)
                    </div>
                    {quarterVouchers.length > 0 ? (
                      <div className="overflow-x-auto">
                    <table className="w-full text-base text-foreground" role="table" aria-label="تفصيل الفترة">
                          <thead className="bg-muted/50 sticky top-0 z-10">
                            <tr>
                              <th className="text-end p-4 text-sm md:text-base lg:text-xl">رقم السند</th>
                              <th className="text-end p-4 text-sm md:text-base lg:text-xl">التاريخ</th>
                              <th className="text-end p-4 text-sm md:text-base lg:text-xl">المبلغ</th>
                              <th className="text-end p-4 text-sm md:text-base lg:text-xl">جهة الاستلام</th>
                              <th className="text-end p-4 text-sm md:text-base lg:text-xl">البيان</th>
                              <th className="text-end p-4 text-sm md:text-base lg:text-xl">الحساب المدين</th>
                              <th className="text-end p-4 text-sm md:text-base lg:text-xl">الحالة</th>
                            </tr>
                          </thead>
                          <tbody>
                            {quarterVouchers.map((v) => (
                              <tr key={v.voucherID} className="border-t border-border/50 hover:bg-muted/20">
                                <td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg">{v.voucherNumber}</td>
                                <td className="p-4">{new Date(v.voucherDate).toLocaleDateString("ar-SA")}</td>
                                <td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg text-destructive">{formatCurrency(v.amount)}</td>
                                <td className="p-4">{v.payeeName || "—"}</td>
                                <td className="p-4 max-w-[200px] truncate">{v.description || "—"}</td>
                                <td className="p-4 font-mono">{v.debitAccountCode} - {v.debitAccountName}</td>
                                <td className="p-4">
                                  <Badge variant={v.status === 2 ? "success" : v.status === 3 ? "destructive" : "warning"}>
                                    {v.statusName}
                                  </Badge>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                          <tfoot className="bg-muted/30 font-bold">
                            <tr>
                              <td className="p-4">الإجمالي</td>
                              <td className="p-4">{quarterVouchers.length} سند</td>
                              <td className="p-4 font-mono font-bold text-sm md:text-base lg:text-lg text-destructive">
                                {formatCurrency(quarterVouchers.reduce((sum, v) => sum + v.amount, 0))}
                              </td>
                              <td className="p-4" colSpan={4}></td>
                            </tr>
                          </tfoot>
                        </table>
                      </div>
                    ) : (
                      <div className="p-8 text-center text-muted-foreground">لا توجد أوامر صرف في هذا الفصل</div>
                    )}
                  </Card>
                </TabsContent>

                {/* ── ميزان المراجعه ── */}
                <TabsContent value="trial-balance" className="mt-4">
                  <Card className="border-none rounded-3xl overflow-hidden">
                    <div className="p-4 bg-muted/30 font-bold">ميزان المراجعه للفصل</div>
                    <div className="p-4">
                      {selectedQuarter && <TrialBalanceReport fromDate={selectedQuarter.startDate} toDate={selectedQuarter.endDate} onExport={(d) => exportTrialBalanceData(d)} />}
                    </div>
                  </Card>
                </TabsContent>
              </Tabs>
            </div>
          )}
        </div>

        {/* ═══════════════════════════════════════════
            الطباعة: تصميم رسمي باستخدام PrintLayout
           ═══════════════════════════════════════════ */}
        {details && (
          <div className="print-only">
            <PrintLayout
              reportName={`التقرير المالي الفصلي - ${quarterName} - ${yearNum}`}
              reportNumber={`QR-${yearNum}-Q${quarterId}`}
              config={{
                showWatermark: true,
                watermarkOpacity: 0.06,
                pageSize: 'A3',
                orientation: 'landscape',
              }}
            >
              <div className="space-y-6 text-black" dir="rtl">
                {/* 1. ملخص */}
                <section>
                  <div className="print-section-heading"><div className="print-section-number">1</div><span className="print-section-title">ملخص الفصل المالي</span></div>
                  <table className="print-table">
                    <thead><tr><th>البيان</th><th>المبلغ (ريال يمني)</th></tr></thead>
                    <tbody>
                      <tr><td className="text-start font-bold">إجمالي السحب من البنك</td><td className="mono fb text-info">{formatCurrency(details.totalWithdrawn)}</td></tr>
                      <tr><td className="text-start font-bold">إجمالي الصرف الفعلي</td><td className="mono fb text-danger">{formatCurrency(details.totalSpent)}</td></tr>
                    </tbody>
                    <tfoot><tr><td className="text-start font-black text-xs">الرصيد المتبقي المرحل</td><td className={cn("mono fb text-xs", details.remainingBalance >= 0 ? "text-success" : "text-danger")}>{formatCurrency(details.remainingBalance)}</td></tr></tfoot>
                  </table>
                </section>

                {/* 2. الفترة */}
                <section>
                  <div className="print-section-heading"><div className="print-section-number">2</div><span className="print-section-title">تفصيل حسب الفترة</span></div>
                  <table className="print-table">
                    <thead><tr><th>الفترة</th><th>الافتتاحي</th><th>سحب من البنك</th><th>الصرف الفعلي</th><th>الختامي</th></tr></thead>
                    <tbody>
                      {details.periods.map((p) => (
                        <tr key={p.periodID}>
                          <td className="font-bold">{p.periodName}</td>
                          <td className="mono fb">{formatCurrency(p.openingBalance)}</td>
                          <td className="mono fb text-info">{p.withdrawn > 0 ? `+${formatCurrency(p.withdrawn)}` : "—"}</td>
                          <td className="mono fb text-danger">{p.spent > 0 ? `-${formatCurrency(p.spent)}` : "—"}</td>
                          <td className="mono fb">{formatCurrency(p.closingBalance)}</td>
                        </tr>
                      ))}
                    </tbody>
                    <tfoot><tr><td className="font-black text-xs">الإجمالي</td><td>—</td><td className="mono fb text-info">+{formatCurrency(details.totalWithdrawn)}</td><td className="mono fb text-danger">-{formatCurrency(details.totalSpent)}</td><td className="mono fb">{formatCurrency(details.remainingBalance)}</td></tr></tfoot>
                  </table>
                </section>

                {/* 3. المصروفات */}
                {details.expenseBreakdown.length > 0 && (
                  <section>
                    <div className="print-section-heading"><div className="print-section-number">3</div><span className="print-section-title">تفصيل المصروفات حسب الحساب</span></div>
                    <table className="print-table">
                      <thead><tr><th>كود الحساب</th><th>اسم الحساب</th><th>المبلغ</th><th>عدد القيود</th></tr></thead>
                      <tbody>{details.expenseBreakdown.map((e) => (<tr key={e.accountID}><td className="mono fb">{e.accountCode}</td><td className="font-bold">{e.accountName}</td><td className="mono fb text-danger">{formatCurrency(e.amount)}</td><td className="fb">{e.entryCount}</td></tr>))}</tbody>
                    </table>
                  </section>
                )}

                {/* 4. أوامر الصرف */}
                {quarterVouchers.length > 0 && (
                  <section>
                    <div className="print-section-heading"><div className="print-section-number">4</div><span className="print-section-title">أوامر الصرف ({quarterVouchers.length} سند)</span></div>
                    <table className="print-table">
                      <thead><tr><th>رقم السند</th><th>التاريخ</th><th>المبلغ</th><th>جهة الاستلام</th><th>البيان</th><th>الحساب المدين</th></tr></thead>
                      <tbody>
                        {quarterVouchers.map((v) => (
                          <tr key={v.voucherID}>
                            <td className="mono fb">{v.voucherNumber}</td>
                            <td>{new Date(v.voucherDate).toLocaleDateString("ar-YE")}</td>
                            <td className="mono fb text-danger">{formatCurrency(v.amount)}</td>
                            <td className="font-bold">{v.payeeName || "—"}</td>
                            <td>{v.description || "—"}</td>
                            <td className="mono">{v.debitAccountCode}</td>
                          </tr>
                        ))}
                      </tbody>
                      <tfoot><tr><td className="font-black">الإجمالي ({quarterVouchers.length} سند)</td><td>—</td><td className="mono fb text-danger text-xs">{formatCurrency(quarterVouchers.reduce((sum, v) => sum + v.amount, 0))}</td><td colSpan={3}>—</td></tr></tfoot>
                    </table>
                  </section>
                )}

                {/* 5. ميزان المراجعه */}
                {trialBalance && trialBalance.length > 0 && (() => {
                  const printableData = trialBalance.filter(item =>
                    item.openingDebit !== 0 || item.openingCredit !== 0 ||
                    item.movementDebit !== 0 || item.movementCredit !== 0 ||
                    item.closingDebit !== 0 || item.closingCredit !== 0 ||
                    item.balance !== 0
                  );
                  return printableData.length > 0 && (
                    <section>
                      <div className="print-section-heading"><div className="print-section-number">5</div><span className="print-section-title">ميزان المراجعه للفصل</span></div>
                      <table className="print-table">
                        <thead><tr><th>الكود</th><th>الحساب</th><th>الطبيعة</th><th>افتتاحي م</th><th>افتتاحي د</th><th>حركه م</th><th>حركه د</th><th>ختامي م</th><th>ختامي د</th><th>الرصيد</th></tr></thead>
                        <tbody>
                          {printableData.map((item) => (
                            <tr key={item.accountID}>
                              <td className="mono fb" dir="ltr">{item.accountCode}</td>
                              <td className="font-bold">{item.accountName}</td>
                              <td><span className={cn("px-1.5 rounded-full text-[9px] font-extrabold", item.accountNature === "Debit" ? "bg-info/10 text-info" : "bg-success/10 text-success")}>{item.accountNature === "Debit" ? "مدين" : "دائن"}</span></td>
                              <td className="mono fb">{formatCurrency(item.openingDebit)}</td>
                              <td className="mono fb">{formatCurrency(item.openingCredit)}</td>
                              <td className="mono fb">{formatCurrency(item.movementDebit)}</td>
                              <td className="mono fb">{formatCurrency(item.movementCredit)}</td>
                              <td className="mono fb">{formatCurrency(item.closingDebit)}</td>
                              <td className="mono fb">{formatCurrency(item.closingCredit)}</td>
                              <td className="mono fb">{formatCurrency(item.balance)}</td>
                            </tr>
                          ))}
                        </tbody>
                        <tfoot><tr><td colSpan={3} className="text-start font-black text-xs">الإجمالي</td><td className="mono fb text-xs">{formatCurrency(printableData.reduce((s, i) => s + i.openingDebit, 0))}</td><td className="mono fb text-xs">{formatCurrency(printableData.reduce((s, i) => s + i.openingCredit, 0))}</td><td className="mono fb text-xs">{formatCurrency(printableData.reduce((s, i) => s + i.movementDebit, 0))}</td><td className="mono fb text-xs">{formatCurrency(printableData.reduce((s, i) => s + i.movementCredit, 0))}</td><td className="mono fb text-xs">{formatCurrency(printableData.reduce((s, i) => s + i.closingDebit, 0))}</td><td className="mono fb text-xs">{formatCurrency(printableData.reduce((s, i) => s + i.closingCredit, 0))}</td><td className="mono fb text-xs">—</td></tr></tfoot>
                      </table>
                    </section>
                  );
                })()}

                {/* التوقيعات */}
                <div className="print-sigs">
                  <div className="print-sig-box"><p className="print-sig-label">إعداد: محاسب الصندوق</p></div><p className="print-sig-sign">التوقيع: ........................</p>
                  <div className="print-sig-box"><p className="print-sig-label">مراجعة: رئيس قسم الحسابات</p></div><p className="print-sig-sign">التوقيع: ........................</p>
                  <div className="print-sig-box"><p className="print-sig-label">اعتماد: المدير المالي</p></div><p className="print-sig-sign">التوقيع: ........................</p>
                </div>
              </div>
            </PrintLayout>
          </div>
        )}
      </PageLayout>
    </>
  );
}
