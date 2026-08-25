import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { Eye, PlusCircle, Trash2, Send, Ban, Receipt, CreditCard, Upload, Pencil, Printer, Calendar } from "lucide-react";

import { vouchersApi, type VoucherDto, type VoucherFilter } from "@/api/vouchers";
import { Badge, Button, Card, DateInput, DataTable, type CustomColumnDef, Heading, Input, PageLayout, Skeleton, Select, SelectContent, SelectItem, SelectTrigger, SelectValue, StickyHeader, StatusBadge } from "@/components/ui";
import { useHasPermission } from "@/stores/authStore";
import { formatCurrency, formatDate } from "@/lib/utils";
import { VoucherFormDialog } from "@/components/domain/accounting/VoucherFormDialog";
import { VoucherViewDialog } from "@/components/domain/accounting/VoucherViewDialog";
import { VoucherPrintDialog } from "@/components/domain/accounting/VoucherPrintDialog";
import { VoucherImportDialog } from "@/components/domain/accounting/VoucherImportDialog";
import { VoucherSetAccountDialog } from "@/components/domain/accounting/VoucherSetAccountDialog";
import { VOUCHER_STATUS } from "@/constants/statusConfigs";

const ALL = "__all__";



export function VoucherListPage() {
  const canManage = useHasPermission("Accounting.Manage", "System.Admin");
  const queryClient = useQueryClient();

  const [showCreate, setShowCreate] = useState(false);
  const [showImport, setShowImport] = useState(false);
  const [viewId, setViewId] = useState<number | null>(null);
  const [printId, setPrintId] = useState<number | null>(null);
  const [setAccountId, setSetAccountId] = useState<number | null>(null);
  const [filters, setFilters] = useState<VoucherFilter>({
    pageNumber: 1,
    pageSize: 50,
  });

  const { data, isLoading } = useQuery({
    queryKey: ["vouchers", filters],
    queryFn: () => vouchersApi.list(filters),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => vouchersApi.delete(id),
    onSuccess: (res) => {
      toast.success(res.message || "تم الحذف");
      queryClient.invalidateQueries({ queryKey: ["vouchers"] });
    },
    onError: (err: any) => toast.error(err?.response?.data?.message || "خطأ"),
  });

  const postMutation = useMutation({
    mutationFn: (id: number) => vouchersApi.post(id),
    onSuccess: (res) => {
      toast.success(res.message || "تم الترحيل");
      queryClient.invalidateQueries({ queryKey: ["vouchers"] });
    },
    onError: (err: any) => toast.error(err?.response?.data?.message || "خطأ"),
  });

  const cancelMutation = useMutation({
    mutationFn: (id: number) => vouchersApi.cancel(id),
    onSuccess: (res) => {
      toast.success(res.message || "تم الإلغاء");
      queryClient.invalidateQueries({ queryKey: ["vouchers"] });
    },
    onError: (err: any) => toast.error(err?.response?.data?.message || "خطأ"),
  });

  const vouchers = data?.items || [];

  return (
    <PageLayout>
      <StickyHeader>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-primary/10 rounded-2xl">
              <Receipt className="w-8 h-8 text-primary" />
            </div>
            <div>
              <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">سندات الصرف والقبض</Heading>
              <p className="text-muted-foreground mt-1">إدارة سندات الصرف والقبض المحاسبية</p>
            </div>
          </div>
          {canManage && (
            <div className="flex gap-2">
              <Button onClick={() => setShowCreate(true)}>
                <PlusCircle className="w-4 h-4 me-2" /> سند جديد
              </Button>
              <Button variant="outline" onClick={() => setShowImport(true)}>
                <Upload className="w-4 h-4 me-2" /> استيراد
              </Button>
            </div>
          )}
        </div>
      </StickyHeader>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3 mb-4">
        <Select value={filters.voucherType != null ? String(filters.voucherType) : ALL} onValueChange={(v) => setFilters((f) => ({ ...f, voucherType: v === ALL ? undefined : Number(v) as any, pageNumber: 1 }))}>
          <SelectTrigger className="w-40"><SelectValue placeholder="النوع" /></SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL}>الكل</SelectItem>
            <SelectItem value="1">سند صرف</SelectItem>
            <SelectItem value="2">سند قبض</SelectItem>
          </SelectContent>
        </Select>
        <Select value={filters.status != null ? String(filters.status) : ALL} onValueChange={(v) => setFilters((f) => ({ ...f, status: v === ALL ? undefined : Number(v) as any, pageNumber: 1 }))}>
          <SelectTrigger className="w-36"><SelectValue placeholder="الحالة" /></SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL}>الكل</SelectItem>
            <SelectItem value="1">مسودة</SelectItem>
            <SelectItem value="2">مرحّل</SelectItem>
            <SelectItem value="3">ملغي</SelectItem>
          </SelectContent>
        </Select>

        {/* فلتر التاريخ */}
        <div className="flex items-center gap-2">
          <Calendar className="w-4 h-4 text-muted-foreground" />
          <DateInput
            value={filters.fromDate || ""}
            onChange={(v) => setFilters((f) => ({ ...f, fromDate: v || undefined, pageNumber: 1 }))}
            placeholder="من"
            className="w-40"
          />
          <span className="text-xs text-muted-foreground">إلى</span>
          <DateInput
            value={filters.toDate || ""}
            onChange={(v) => setFilters((f) => ({ ...f, toDate: v || undefined, pageNumber: 1 }))}
            placeholder="إلى"
            className="w-40"
          />
        </div>

        <Input
          className="w-56"
          placeholder="بحث بالرقم أو البيان..."
          value={filters.search || ""}
          onChange={(e) => setFilters((f) => ({ ...f, search: e.target.value || undefined, pageNumber: 1 }))}
        />
      </div>

      {/* Table */}
      <Card className="overflow-hidden">
        {isLoading ? (
          <div className="p-6 space-y-3">{Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-12" />)}</div>
        ) : vouchers.length === 0 ? (
          <div className="p-8 text-center text-muted-foreground">لا توجد سندات</div>
        ) : (
          <DataTable
            columns={[
              { key: "voucherNumber", header: "رقم السند", render: (row) => <span className="font-mono text-xs font-bold" dir="ltr">{row.voucherNumber}</span> },
              { key: "voucherDate", header: "التاريخ", render: (row) => <span className="text-xs">{formatDate(row.voucherDate)}</span> },
              { key: "voucherType", header: "النوع", render: (row) => <Badge variant={row.voucherType === 1 ? "destructive" : "success"} className="gap-1">{row.voucherType === 1 ? <CreditCard className="w-3 h-3" /> : <Receipt className="w-3 h-3" />}{row.voucherTypeName}</Badge> },
              { key: "payee", header: "المستفيد / الدافع", render: (row) => <span className="text-xs">{row.voucherType === 1 ? row.payeeName : row.payerName}</span> },
              { key: "amount", header: "المبلغ", render: (row) => <span className="font-mono text-xs font-bold">{formatCurrency(row.amount)}</span> },
              { key: "description", header: "البيان", render: (row) => <span className="text-xs max-w-[200px] truncate block">{row.description}</span> },
              { key: "journalEntryNumber", header: "القيد", render: (row) => <span className="font-mono text-xs" dir="ltr">{row.journalEntryNumber || "—"}</span> },
              { key: "status", header: "الحالة", render: (row) => <StatusBadge status={row.statusName} config={VOUCHER_STATUS} /> },
              { key: "actions", header: "الإجراءات", render: (row) => (
                <div className="flex items-center gap-1">
                  <Button variant="ghost" size="icon" onClick={() => setViewId(row.voucherID)}><Eye className="w-4 h-4" /></Button>
                  <Button variant="ghost" size="icon" className="text-blue-600" onClick={() => setPrintId(row.voucherID)} title="طباعة"><Printer className="w-4 h-4" /></Button>
                  {canManage && row.status === 1 && (
                    <>
                      {(!row.debitAccountID || row.debitAccountID === 0) && (
                        <Button variant="ghost" size="icon" className="text-orange-600" onClick={() => setSetAccountId(row.voucherID)} title="تحديد حساب المصروف"><Pencil className="w-4 h-4" /></Button>
                      )}
                      <Button variant="ghost" size="icon" className="text-green-600" onClick={() => postMutation.mutate(row.voucherID)}><Send className="w-4 h-4" /></Button>
                      <Button variant="ghost" size="icon" className="text-destructive" onClick={() => deleteMutation.mutate(row.voucherID)}><Trash2 className="w-4 h-4" /></Button>
                    </>
                  )}
                  {canManage && row.status === 2 && (
                    <Button variant="ghost" size="icon" className="text-orange-600" onClick={() => cancelMutation.mutate(row.voucherID)}><Ban className="w-4 h-4" /></Button>
                  )}
                </div>
              )},
            ] as CustomColumnDef<VoucherDto>[]}
            data={vouchers}
            rowKey={(row) => row.voucherID}
          />
        )}
      </Card>

      <VoucherFormDialog open={showCreate} onOpenChange={setShowCreate} />
      <VoucherImportDialog open={showImport} onOpenChange={setShowImport} onImported={() => queryClient.invalidateQueries({ queryKey: ["vouchers"] })} />
      {setAccountId && (
        <VoucherSetAccountDialog
          open={!!setAccountId}
          onOpenChange={(o) => { if (!o) setSetAccountId(null); }}
          voucherId={setAccountId}
          currentAccountId={vouchers.find((v) => v.voucherID === setAccountId)?.debitAccountID}
        />
      )}
      {viewId && <VoucherViewDialog open={!!viewId} onOpenChange={(o) => { if (!o) setViewId(null); }} voucherId={viewId} />}
      {printId && <VoucherPrintDialog open={!!printId} onOpenChange={(o) => { if (!o) setPrintId(null); }} voucherId={printId} />}
    </PageLayout>
  );
}
