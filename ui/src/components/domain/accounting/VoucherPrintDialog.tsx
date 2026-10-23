import { useQuery } from "@tanstack/react-query";
import { useEffect,useRef } from "react";
import { useReactToPrint } from "react-to-print";

import { vouchersApi } from "@/api/vouchers";
import { PrintablePaymentOrder } from "@/pages/print/PrintablePaymentOrder";
import { PrintableVoucher } from "@/pages/print/PrintableVoucher";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  voucherId: number;
}

export function VoucherPrintDialog({ open, onOpenChange, voucherId }: Props) {
  const printRef = useRef<HTMLDivElement>(null);

  const { data: voucher, isLoading } = useQuery({
    queryKey: ["voucher", voucherId],
    queryFn: () => vouchersApi.getById(voucherId),
    enabled: open && !!voucherId,
  });

  const handlePrint = useReactToPrint({
    contentRef: printRef,
    documentTitle: voucher?.voucherNumber || "سند",
    onAfterPrint: () => onOpenChange(false),
    onPrintError: () => onOpenChange(false),
  });

  // طباعة تلقائية عند تحميل البيانات
  useEffect(() => {
    if (voucher && !isLoading) {
      const timer = setTimeout(() => {
        handlePrint();
      }, 500);
      return () => clearTimeout(timer);
    }
  }, [voucher, isLoading]);

  if (!open) return null;

  const isReceipt = voucher?.voucherType === 2;

  return (
    <div className="fixed inset-0 bg-white z-[9999] flex items-center justify-center flex-col gap-4">
      {isLoading ? (
        <div className="text-lg text-muted-foreground">جاري تحميل السند...</div>
      ) : voucher ? (
        isReceipt ? (
          <PrintableVoucher ref={printRef} voucher={voucher} />
        ) : (
          <PrintablePaymentOrder ref={printRef} voucher={voucher} />
        )
      ) : (
        <div className="text-lg text-muted-foreground">السند غير موجود</div>
      )}
    </div>
  );
}
