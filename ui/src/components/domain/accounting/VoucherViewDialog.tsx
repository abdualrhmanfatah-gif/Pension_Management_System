import { useQuery } from "@tanstack/react-query";
import {
  Building2,
  Calendar,
  CheckCircle,
  CreditCard,
  DollarSign,
  FileText,
  Hash,
  Link2,
  Printer,
  Tag,
  User,
} from "lucide-react";
import { useRef } from "react";
import { useReactToPrint } from "react-to-print";

import { vouchersApi } from "@/api/vouchers";
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
  StatusBadge,
  SummaryCard,
  SummaryRow,
} from "@/components/ui";
import { VOUCHER_STATUS } from "@/constants/statusConfigs";
import { formatCurrency, formatDate } from "@/lib/utils";
import { PrintablePaymentOrder } from "@/pages/print/PrintablePaymentOrder";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  voucherId: number;
}

export function VoucherViewDialog({ open, onOpenChange, voucherId }: Props) {
  const printRef = useRef<HTMLDivElement>(null);

  const { data: voucher, isLoading } = useQuery({
    queryKey: ["voucher", voucherId],
    queryFn: () => vouchersApi.getById(voucherId),
    enabled: open && !!voucherId,
  });

  const handlePrint = useReactToPrint({
    contentRef: printRef,
    documentTitle: voucher?.voucherNumber || "سند",
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <div className="flex items-center justify-between">
            <DialogTitle>تفاصيل السند</DialogTitle>
            <Button variant="outline" size="sm" onClick={() => handlePrint()}>
              <Printer className="w-4 h-4 me-1" /> طباعة
            </Button>
          </div>
        </DialogHeader>

        {isLoading ? (
          <div className="space-y-4 p-2">
            <Skeleton className="h-24 w-full rounded-xl" />
            <Skeleton className="h-16 w-full rounded-xl" />
            <Skeleton className="h-20 w-full rounded-xl" />
            <Skeleton className="h-16 w-full rounded-xl" />
          </div>
        ) : !voucher ? (
          <div className="p-8 text-center text-muted-foreground">السند غير موجود</div>
        ) : (
          <>
            {/* Screen view */}
            <div className="space-y-4">
              {/* معلومات أساسية */}
              <SummaryCard>
                <SummaryRow className="sm:grid-cols-2">
                  <InfoPair label="رقم السند" value={voucher.voucherNumber} icon={<Hash className="w-3.5 h-3.5" />} mono dir="ltr" />
                  <InfoPair label="التاريخ" value={formatDate(voucher.voucherDate)} icon={<Calendar className="w-3.5 h-3.5" />} />
                  <InfoPair
                    label="النوع"
                    icon={<Tag className="w-3.5 h-3.5" />}
                    value={
                      <Badge variant={voucher.voucherType === 1 ? "destructive" : "success"}>
                        {voucher.voucherTypeName}
                      </Badge>
                    }
                  />
                  <InfoPair
                    label="الحالة"
                    icon={<CheckCircle className="w-3.5 h-3.5" />}
                    value={
                      <StatusBadge status={voucher.statusName} config={VOUCHER_STATUS} />
                    }
                  />
                </SummaryRow>
              </SummaryCard>

              {/* المستفيد / الدافع */}
              <SummaryCard>
                <InfoPair
                  label={voucher.voucherType === 1 ? "المستفيد" : "الدافع"}
                  value={voucher.voucherType === 1 ? voucher.payeeName : voucher.payerName}
                  icon={<User className="w-3.5 h-3.5" />}
                />
              </SummaryCard>

              {/* المبلغ */}
              <div className="rounded-xl border bg-primary/5 border-primary p-4">
                <div className="flex items-center gap-1.5 mb-1">
                  <DollarSign className="w-3.5 h-3.5 text-primary" />
                  <span className="text-xs text-muted-foreground">المبلغ</span>
                </div>
                <p className="text-xl font-bold text-primary">{formatCurrency(voucher.amount)} ريال يمني</p>
              </div>

              {/* البيان */}
              <SummaryCard>
                <InfoPair label="البيان" value={voucher.description} icon={<FileText className="w-3.5 h-3.5" />} />
              </SummaryCard>

              {/* الحسابات */}
              <SummaryCard>
                <SummaryRow className="sm:grid-cols-2">
                  <InfoPair label="حساب المدين" value={`${voucher.debitAccountCode} - ${voucher.debitAccountName}`} icon={<Building2 className="w-3.5 h-3.5" />} mono dir="ltr" />
                  <InfoPair label="حساب الدائن" value={`${voucher.creditAccountCode} - ${voucher.creditAccountName}`} icon={<Building2 className="w-3.5 h-3.5" />} mono dir="ltr" />
                </SummaryRow>
              </SummaryCard>

              {/* معلومات إضافية */}
              {(voucher.chequeNumber || voucher.referralSource) && (
                <SummaryCard>
                  <SummaryRow className="sm:grid-cols-2">
                    {voucher.chequeNumber && (
                      <InfoPair label="رقم الشيك / المرجع" value={voucher.chequeNumber} icon={<CreditCard className="w-3.5 h-3.5" />} mono dir="ltr" />
                    )}
                    {voucher.referralSource && (
                      <InfoPair label="مصدر التوجيه" value={voucher.referralSource} icon={<Link2 className="w-3.5 h-3.5" />} />
                    )}
                  </SummaryRow>
                </SummaryCard>
              )}

              {/* القيد المرتبط */}
              {voucher.journalEntryNumber && (
                <div className="rounded-xl border bg-blue-50 border-blue-200 p-4">
                  <div className="flex items-center gap-1.5 mb-1">
                    <Link2 className="w-3.5 h-3.5 text-blue-600" />
                    <span className="text-xs text-muted-foreground">القيد المرتبط</span>
                  </div>
                  <p className="font-mono font-bold text-blue-700" dir="ltr">{voucher.journalEntryNumber}</p>
                </div>
              )}
            </div>

            {/* Print view */}
            <PrintablePaymentOrder ref={printRef} voucher={voucher} />
          </>
        )}

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>إغلاق</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
