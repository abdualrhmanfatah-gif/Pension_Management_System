import { useQuery } from "@tanstack/react-query";
import {
  Calendar,
  CheckCircle,
  FileText,
  Hash,
  Tag,
  User,
} from "lucide-react";

import { journalEntriesApi } from "@/api/accounting";
<<<<<<< HEAD
=======
import type { JournalEntryLineDto } from "@/types/accounting";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
  Badge,
  Button,
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  InfoPair,
  Skeleton,
  SummaryCard,
  SummaryRow,
} from "@/components/ui";
import { JOURNAL_STATUS } from "@/constants";
import { formatCurrency } from "@/lib/utils";
<<<<<<< HEAD
import type { JournalEntryLineDto } from "@/types/accounting";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface JournalEntryViewDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  entryId: number | null;
}

const statusBadge: Record<string, { label: string; variant: "warning" | "success" | "destructive" }> = {
  [JOURNAL_STATUS.DRAFT]: { label: "مسودة", variant: "warning" },
  [JOURNAL_STATUS.POSTED]: { label: "مرحّل", variant: "success" },
  [JOURNAL_STATUS.REVERSED]: { label: "ملغي", variant: "destructive" },
};

export function JournalEntryViewDialog({ open, onOpenChange, entryId }: JournalEntryViewDialogProps) {
  const { data: entry, isLoading } = useQuery({
    queryKey: ["journal-entries", entryId],
    queryFn: () => journalEntriesApi.getById(entryId!),
    enabled: open && !!entryId,
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>تفاصيل القيد اليومي</DialogTitle>
        </DialogHeader>

        {isLoading ? (
          <div className="space-y-4 p-2">
            <Skeleton className="h-24 w-full rounded-xl" />
            <Skeleton className="h-16 w-full rounded-xl" />
            <Skeleton className="h-32 w-full rounded-xl" />
          </div>
        ) : !entry ? (
          <div className="p-8 text-center text-muted-foreground">القيد غير موجود</div>
        ) : (
          <div className="space-y-4">
            {/* المعلومات الأساسية */}
            <SummaryCard>
              <SummaryRow className="sm:grid-cols-3">
                <InfoPair label="رقم القيد" value={entry.entryNumber} icon={<Hash className="w-3.5 h-3.5" />} mono dir="ltr" />
                <InfoPair label="التاريخ" value={new Date(entry.entryDate).toLocaleDateString("ar-SA")} icon={<Calendar className="w-3.5 h-3.5" />} />
                <InfoPair
                  label="الحالة"
                  icon={<CheckCircle className="w-3.5 h-3.5" />}
                  value={
                    <Badge variant={statusBadge[entry.status]?.variant ?? "outline"}>
                      {statusBadge[entry.status]?.label ?? entry.status}
                    </Badge>
                  }
                />
              </SummaryRow>
            </SummaryCard>

            <SummaryCard>
              <SummaryRow className="sm:grid-cols-3">
                <InfoPair label="الفترة" value={entry.periodName} icon={<Tag className="w-3.5 h-3.5" />} />
                <InfoPair label="المصدر" value={entry.sourceModule === "Manual" ? "يدوي" : entry.sourceModule} icon={<FileText className="w-3.5 h-3.5" />} />
                <InfoPair label="بواسطة" value={entry.createdByUserName} icon={<User className="w-3.5 h-3.5" />} />
              </SummaryRow>
            </SummaryCard>

            {/* البيان */}
            {entry.description && (
              <SummaryCard>
                <InfoPair label="البيان" value={entry.description} icon={<FileText className="w-3.5 h-3.5" />} />
              </SummaryCard>
            )}

            {/* بنود القيد */}
            <SummaryCard title="بنود القيد" icon={<FileText className="w-3.5 h-3.5" />}>
              <div className="border rounded-xl overflow-x-auto">
                <table className="w-full text-sm min-w-[600px]" role="table" aria-label="بنود القيد المحاسبي">
                  <thead className="bg-muted/50 sticky top-0 z-10">
                    <tr>
                      <th className="text-end p-2.5 text-xs font-medium">الحساب</th>
                      <th className="text-end p-2.5 text-xs font-medium">مركز التكلفة</th>
                      <th className="text-end p-2.5 text-xs font-medium">البيان</th>
                      <th className="text-start p-2.5 text-xs font-medium w-28">مدين</th>
                      <th className="text-start p-2.5 text-xs font-medium w-28">دائن</th>
                    </tr>
                  </thead>
                  <tbody>
                    {entry.lines.map((line: JournalEntryLineDto, idx) => (
<<<<<<< HEAD
                      <tr key={line.lineID} className={`border-t border-border ${idx % 2 === 0 ? "bg-muted/5" : ""} hover:bg-muted/20 transition-colors`}>
=======
                      <tr key={line.lineID} className={`border-t border-border/50 ${idx % 2 === 0 ? "bg-muted/5" : ""} hover:bg-muted/20 transition-colors`}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <td className="p-2.5">
                          <div className="flex items-center gap-2">
                            <span className="font-mono text-xs text-muted-foreground bg-muted px-1.5 py-0.5 rounded">{line.accountCode}</span>
                            <span className="text-sm">{line.accountName}</span>
                          </div>
                        </td>
                        <td className="p-2.5 text-xs text-muted-foreground">{line.costCenterName || "—"}</td>
                        <td className="p-2.5 text-xs text-muted-foreground max-w-[200px] truncate">{line.description || "—"}</td>
                        <td className="p-2.5 text-start font-mono text-xs">
                          {line.debit > 0 ? (
                            <span className="text-emerald-600 dark:text-emerald-400 font-medium">{formatCurrency(line.debit)}</span>
                          ) : "—"}
                        </td>
                        <td className="p-2.5 text-start font-mono text-xs">
                          {line.credit > 0 ? (
                            <span className="text-destructive font-medium">{formatCurrency(line.credit)}</span>
                          ) : "—"}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot className="bg-muted/30 font-medium border-t">
                    <tr>
                      <td colSpan={3} className="p-2.5 text-start text-sm">الإجمالي</td>
                      <td className="p-2.5 text-start font-mono text-sm text-emerald-600 dark:text-emerald-400">{formatCurrency(entry.totalDebit)}</td>
                      <td className="p-2.5 text-start font-mono text-sm text-destructive">{formatCurrency(entry.totalCredit)}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </SummaryCard>

            {/* معلومات الترحيل */}
            {entry.postedByUserName && (
              <SummaryCard>
                <InfoPair
                  label="ترحيل"
                  value={`${entry.postedByUserName} — ${entry.postedAt ? new Date(entry.postedAt).toLocaleString("ar-SA") : ""}`}
                  icon={<CheckCircle className="w-3.5 h-3.5" />}
                />
              </SummaryCard>
            )}
          </div>
        )}

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>إغلاق</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
