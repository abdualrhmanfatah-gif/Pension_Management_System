import { useQuery } from "@tanstack/react-query";
<<<<<<< HEAD
import { Eye, FileText, PlusCircle, RefreshCw } from "lucide-react";
import { useState } from "react";

import { fiscalYearsApi,journalEntriesApi } from "@/api/accounting";
import { JournalEntryFormDialog, JournalEntryPostDialog, JournalEntryReverseDialog, JournalEntryViewDialog } from "@/components/domain/accounting";
import { Button, Card, Heading, Input, PageLayout, Select, SelectContent, SelectItem, SelectTrigger, SelectValue, StatusBadge, StickyHeader, Text } from "@/components/ui";
import type { CustomColumnDef, PaginationConfig } from "@/components/ui/DataTable";
import { DataTable } from "@/components/ui/DataTable";
import { JOURNAL_STATUS } from "@/constants";
import { JOURNAL_ENTRY_STATUS } from "@/constants/statusConfigs";
import { formatCurrency } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { JournalEntryDto, JournalEntryFilterDto } from "@/types/accounting";
=======
import { useState } from "react";
import { Eye, FileText, PlusCircle, RefreshCw } from "lucide-react";

import { journalEntriesApi, fiscalYearsApi } from "@/api/accounting";
import { useHasPermission } from "@/stores/authStore";
import type { JournalEntryDto, JournalEntryFilterDto } from "@/types/accounting";
import { Button, Card, Heading, Input, PageLayout, Select, SelectContent, SelectItem, SelectTrigger, SelectValue, StickyHeader, StatusBadge, Text } from "@/components/ui";
import { DataTable } from "@/components/ui/DataTable";
import type { CustomColumnDef, PaginationConfig } from "@/components/ui/DataTable";
import { formatCurrency } from "@/lib/utils";
import { JOURNAL_STATUS } from "@/constants";
import { JOURNAL_ENTRY_STATUS } from "@/constants/statusConfigs";
import { JournalEntryFormDialog, JournalEntryPostDialog, JournalEntryReverseDialog, JournalEntryViewDialog } from "@/components/domain/accounting";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

const ALL = "__all__";

const statusFilterOptions = [
  { value: ALL, label: "الكل" },
  { value: JOURNAL_STATUS.DRAFT, label: "مسودة" },
  { value: JOURNAL_STATUS.POSTED, label: "مرحّل" },
  { value: JOURNAL_STATUS.REVERSED, label: "ملغي" },
];

const sourceFilterOptions = [
  { value: ALL, label: "الكل" },
  { value: "Manual", label: "يدوي" },
  { value: "Salary", label: "معاشات" },
  { value: "Settlement", label: "تسويات" },
];



export function JournalEntriesPage() {
  const canManage = useHasPermission("Accounting.Manage", "System.Admin");

  const [showCreate, setShowCreate] = useState(false);
  const [viewEntryId, setViewEntryId] = useState<number | null>(null);
  const [postEntryId, setPostEntryId] = useState<number | null>(null);
  const [postEntryNumber, setPostEntryNumber] = useState("");
  const [reverseEntryId, setReverseEntryId] = useState<number | null>(null);
  const [reverseEntryNumber, setReverseEntryNumber] = useState("");
  const [page, setPage] = useState(1);
  const [selectedYear, setSelectedYear] = useState<string>("");

  const { data: rawYears } = useQuery({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });
  const fiscalYears: { fiscalYearID: number; yearName: string; startDate: string; endDate: string }[] = Array.isArray(rawYears) ? rawYears : [];

  const [filters, setFilters] = useState<JournalEntryFilterDto>({
    fromDate: new Date(new Date().getFullYear(), 0, 1).toISOString().split("T")[0],
    toDate: new Date().toISOString().split("T")[0],
    status: "",
    sourceModule: "",
    page: 1,
    pageSize: 20,
  });

  const { data, isLoading, refetch } = useQuery({
    queryKey: ["journal-entries", filters, page],
    queryFn: () => journalEntriesApi.getAll({ ...filters, page }),
  });

  const handleFilterChange = (key: string, value: string) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
    setPage(1);
  };

  const handleYearChange = (fyId: string) => {
    setSelectedYear(fyId);
    const fy = fiscalYears.find((y) => y.fiscalYearID === Number(fyId));
    if (fy) {
      setFilters((prev) => ({
        ...prev,
        fromDate: fy.startDate.split("T")[0],
        toDate: fy.endDate.split("T")[0],
      }));
    }
    setPage(1);
  };

  // API returns ApiResponse<PagedResult<JournalEntryDto>>
  // → { isSuccess, data: { items: [...], totalCount, ... } }
  const entries: JournalEntryDto[] =
    (data as { data?: { items?: JournalEntryDto[] } })?.data?.items ?? [];
  const totalCount =
    (data as { data?: { totalCount?: number } })?.data?.totalCount ?? 0;

  const columns: CustomColumnDef<JournalEntryDto>[] = [
    {
      key: "entryNumber",
      header: "رقم القيد",
      className: "font-mono text-xs ltr",
      render: (row) => <span dir="ltr">{row.entryNumber}</span>,
    },
    {
      key: "entryDate",
      header: "التاريخ",
      render: (row) => <span className="text-xs">{new Date(row.entryDate).toLocaleDateString("ar-SA")}</span>,
    },
    {
      key: "description",
      header: "البيان",
      render: (row) => <span className="text-xs max-w-[200px] truncate block">{row.description}</span>,
    },
    {
      key: "sourceModule",
      header: "المصدر",
      render: (row) => <span className="text-xs">{row.sourceModule === "Manual" ? "يدوي" : row.sourceModule}</span>,
    },
    {
      key: "status",
      header: "الحالة",
      render: (row) => <StatusBadge status={row.status} config={JOURNAL_ENTRY_STATUS} />,
    },
    {
      key: "totalDebit",
      header: "المدين",
      className: "text-left",
      render: (row) => <span className="font-mono text-sm font-semibold">{formatCurrency(row.totalDebit)}</span>,
    },
    {
      key: "totalCredit",
      header: "الدائن",
      className: "text-left",
      render: (row) => <span className="font-mono text-sm font-semibold">{formatCurrency(row.totalCredit)}</span>,
    },
    {
      key: "actions",
      header: "إجراءات",
      className: "text-center !min-w-[180px] whitespace-nowrap",
      render: (row) => (
        <div className="flex items-center justify-center gap-3">
          <Button
            variant="ghost"
            size="icon"
            title="عرض"
            onClick={(e) => { e.stopPropagation(); setViewEntryId(row.journalEntryID); }}
          >
            <Eye className="w-7 h-7" />
          </Button>
          {row.status === JOURNAL_STATUS.DRAFT && canManage && (
            <Button
              variant="ghost"
              size="icon"
              className="text-amber-600 hover:bg-amber-100"
              title="ترحيل"
              onClick={(e) => { e.stopPropagation(); setPostEntryId(row.journalEntryID); setPostEntryNumber(row.entryNumber); }}
            >
              <FileText className="w-7 h-7" />
            </Button>
          )}
          {row.status === JOURNAL_STATUS.POSTED && canManage && (
            <Button
              variant="ghost"
              size="icon"
              className="text-destructive hover:bg-destructive/10"
              title="عكس"
              onClick={(e) => { e.stopPropagation(); setReverseEntryId(row.journalEntryID); setReverseEntryNumber(row.entryNumber); }}
            >
              <RefreshCw className="w-7 h-7" />
            </Button>
          )}
        </div>
      ),
    },
  ];

  const pagination: PaginationConfig = {
    page,
    pageSize: filters.pageSize ?? 20,
    total: totalCount,
    onPageChange: setPage,
    pageSizeOptions: [10, 20, 50],
    onPageSizeChange: (size) => { setFilters((prev) => ({ ...prev, pageSize: size })); setPage(1); },
  };

  return (
    <PageLayout>
      {/* Header */}
      <StickyHeader>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-primary/10 rounded-2xl">
              <FileText className="w-8 h-8 text-primary" />
            </div>
            <div>
              <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">القيود اليومية</Heading>
              <p className="text-muted-foreground mt-1">إدارة وتسجيل القيود المحاسبية</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <Button variant="outline" onClick={() => refetch()} className="gap-2">
              <RefreshCw className="w-4 h-4" /> تحديث
            </Button>
            {canManage && (
              <Button onClick={() => setShowCreate(true)} className="gap-2">
                <PlusCircle className="w-4 h-4" /> قيد جديد
              </Button>
            )}
          </div>
        </div>
      </StickyHeader>

      {/* Filters */}
      <Card className="border-none rounded-2xl p-4">
        <div className="grid grid-cols-2 md:grid-cols-6 gap-3">
          <div className="space-y-1">
            <Text variant="caption" className="font-medium">السنة</Text>
            <Select value={selectedYear} onValueChange={handleYearChange}>
              <SelectTrigger className="h-9 text-xs">
                <SelectValue placeholder="السنة المالية" />
              </SelectTrigger>
              <SelectContent>
                {fiscalYears.map((fy) => (
                  <SelectItem key={fy.fiscalYearID} value={fy.fiscalYearID.toString()}>{fy.yearName}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-1">
            <Text variant="caption" className="font-medium">من تاريخ</Text>
            <Input
              type="date"
              value={filters.fromDate || ""}
              onChange={(e) => handleFilterChange("fromDate", e.target.value)}
              className="h-9 text-xs"
            />
          </div>
          <div className="space-y-1">
            <Text variant="caption" className="font-medium">إلى تاريخ</Text>
            <Input
              type="date"
              value={filters.toDate || ""}
              onChange={(e) => handleFilterChange("toDate", e.target.value)}
              className="h-9 text-xs"
            />
          </div>
          <div className="space-y-1">
            <Text variant="caption" className="font-medium">الحالة</Text>
            <Select value={filters.status || ALL} onValueChange={(v) => handleFilterChange("status", v === ALL ? "" : v)}>
              <SelectTrigger className="h-9 text-xs">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {statusFilterOptions.map((o) => (
                  <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-1">
            <Text variant="caption" className="font-medium">المصدر</Text>
            <Select value={filters.sourceModule || ALL} onValueChange={(v) => handleFilterChange("sourceModule", v === ALL ? "" : v)}>
              <SelectTrigger className="h-9 text-xs">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {sourceFilterOptions.map((o) => (
                  <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-1 flex items-end">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => {
                setSelectedYear("");
                setFilters({
                  fromDate: "",
                  toDate: "",
                  status: "",
                  sourceModule: "",
                  page: 1,
                  pageSize: 20,
                });
              }}
              className="h-9 text-xs"
            >
              تصفير الكل
            </Button>
          </div>
        </div>
      </Card>

      {/* Table */}
      <DataTable
        columns={columns}
        data={entries}
        loading={isLoading}
        pagination={pagination}
        emptyMessage="لا توجد قيود في الفترة المحددة"
        rowKey={(row) => row.journalEntryID}
      />

      {/* Dialogs */}
      <JournalEntryFormDialog open={showCreate} onOpenChange={setShowCreate} />
      <JournalEntryViewDialog open={!!viewEntryId} onOpenChange={() => setViewEntryId(null)} entryId={viewEntryId} />
      {postEntryId && (
        <JournalEntryPostDialog
          open={!!postEntryId}
          onOpenChange={() => setPostEntryId(null)}
          entryId={postEntryId}
          entryNumber={postEntryNumber}
        />
      )}
      {reverseEntryId && (
        <JournalEntryReverseDialog
          open={!!reverseEntryId}
          onOpenChange={() => setReverseEntryId(null)}
          entryId={reverseEntryId}
          entryNumber={reverseEntryNumber}
        />
      )}
    </PageLayout>
  );
}
