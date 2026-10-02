import { useQuery } from "@tanstack/react-query";
import { ArrowLeft, FileText, RefreshCw } from "lucide-react";
import { useState } from "react";
import { useNavigate,useParams } from "react-router-dom";

import { journalEntriesApi } from "@/api/accounting";
import { JournalEntryPostDialog, JournalEntryReverseDialog } from "@/components/domain/accounting";
import { Button, Card, type CustomColumnDef, DataTable, ErrorState, Heading, PageLayout, Skeleton, StatusBadge,StickyHeader } from "@/components/ui";
import { JOURNAL_STATUS } from "@/constants";
import { JOURNAL_ENTRY_STATUS } from "@/constants/statusConfigs";
import { formatCurrency } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";



export function JournalEntryDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const canManage = useHasPermission("Accounting.Manage", "System.Admin");
  const entryId = Number(id);

  const [showPost, setShowPost] = useState(false);
  const [showReverse, setShowReverse] = useState(false);

  const { data: entry, isLoading, error } = useQuery({
    queryKey: ["journal-entries", entryId],
    queryFn: () => journalEntriesApi.getById(entryId),
    enabled: !!entryId,
  });

  if (isLoading) {
    return (
      <div className="p-8 space-y-8" dir="rtl">
        <Skeleton className="h-12 w-64 rounded-xl" />
        <Skeleton className="h-48 rounded-3xl" />
        <Skeleton className="h-64 rounded-3xl" />
      </div>
    );
  }

  if (error || !entry) {
    return (
      <div className="flex flex-col items-center justify-center h-[60dvh] gap-4" dir="rtl">
        <ErrorState message="لم يتم العثور على القيد" />
        <Button variant="outline" onClick={() => navigate("/accounting/journal-entries")}>
          العودة إلى القيود
        </Button>
      </div>
    );
  }

  return (
    <PageLayout>
      {/* Header */}
      <StickyHeader>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <Button variant="ghost" size="icon" onClick={() => navigate("/accounting/journal-entries")}>
              <ArrowLeft className="w-5 h-5" />
            </Button>
            <div className="p-3 bg-primary/10 rounded-2xl">
              <FileText className="w-8 h-8 text-primary" />
            </div>
            <div>
              <div className="flex items-center gap-3">
                <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight font-mono ltr" dir="ltr">{entry.entryNumber}</Heading>
                <StatusBadge status={entry.status} config={JOURNAL_ENTRY_STATUS} />
              </div>
              <p className="text-muted-foreground mt-1">{entry.description}</p>
            </div>
          </div>

        <div className="flex items-center gap-2">
          {entry.status === JOURNAL_STATUS.DRAFT && canManage && (
            <Button onClick={() => setShowPost(true)} className="gap-2 bg-amber-600 hover:bg-amber-700">
              <RefreshCw className="w-4 h-4" /> ترحيل القيد
            </Button>
          )}
          {entry.status === JOURNAL_STATUS.POSTED && canManage && (
            <Button onClick={() => setShowReverse(true)} variant="destructive" className="gap-2">
              عكس القيد
            </Button>
          )}
        </div>
        </div>
      </StickyHeader>
      <Card className="border-none rounded-3xl p-6">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div>
            <span className="text-xs text-muted-foreground">التاريخ</span>
            <p className="font-medium">{new Date(entry.entryDate).toLocaleDateString("ar-SA")}</p>
          </div>
          <div>
            <span className="text-xs text-muted-foreground">الفترة</span>
            <p className="font-medium">{entry.periodName}</p>
          </div>
          <div>
            <span className="text-xs text-muted-foreground">المصدر</span>
            <p className="font-medium">{entry.sourceModule === "Manual" ? "يدوي" : entry.sourceModule}</p>
          </div>
          <div>
            <span className="text-xs text-muted-foreground">بواسطة</span>
            <p className="font-medium">{entry.createdByUserName}</p>
          </div>
          {entry.postedByUserName && (
            <>
              <div>
                <span className="text-xs text-muted-foreground">تم الترحيل بواسطة</span>
                <p className="font-medium">{entry.postedByUserName}</p>
              </div>
              <div>
                <span className="text-xs text-muted-foreground">تاريخ الترحيل</span>
                <p className="font-medium">{entry.postedAt ? new Date(entry.postedAt).toLocaleString("ar-SA") : "—"}</p>
              </div>
            </>
          )}
        </div>
      </Card>

      {/* Lines */}
      <Card className="border-none rounded-3xl overflow-hidden">
        <div className="p-4 border-b border-border/40">
          <Heading level="h3" className="font-semibold">بنود القيد</Heading>
        </div>
        <DataTable
          columns={[
            { key: "accountName", header: "الحساب", accessorFn: (row) => row.accountName },
            { key: "accountCode", header: "كود الحساب", render: (row) => <span className="font-mono text-xs ltr" dir="ltr">{row.accountCode}</span> },
            { key: "costCenterName", header: "مركز التكلفة", render: (row) => <span className="text-xs text-muted-foreground">{row.costCenterName || "—"}</span> },
            { key: "description", header: "البيان", render: (row) => <span className="text-xs text-muted-foreground">{row.description || "—"}</span> },
            { key: "debit", header: "مدين", render: (row) => <span className="text-left font-mono text-xs">{row.debit > 0 ? formatCurrency(row.debit) : "—"}</span> },
            { key: "credit", header: "دائن", render: (row) => <span className="text-left font-mono text-xs">{row.credit > 0 ? formatCurrency(row.credit) : "—"}</span> },
          ] as CustomColumnDef<typeof entry.lines[number]>[]}
          data={entry.lines}
          rowKey={(row) => row.lineID}
        />
        <div className="flex items-center justify-between px-4 py-3 bg-muted/30 font-medium border-t">
          <span>الإجمالي</span>
          <div className="flex gap-8">
            <span className="font-mono">{formatCurrency(entry.totalDebit)}</span>
            <span className="font-mono">{formatCurrency(entry.totalCredit)}</span>
          </div>
        </div>
      </Card>

      {/* Dialogs */}
      <JournalEntryPostDialog open={showPost} onOpenChange={setShowPost} entryId={entryId} entryNumber={entry.entryNumber} />
      <JournalEntryReverseDialog open={showReverse} onOpenChange={setShowReverse} entryId={entryId} entryNumber={entry.entryNumber} />
    </PageLayout>
  );
}
