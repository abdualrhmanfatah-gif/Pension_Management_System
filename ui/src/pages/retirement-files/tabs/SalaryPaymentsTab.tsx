import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, CreditCard } from "lucide-react";
import { useEffect, useMemo } from "react";
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
import {
    Badge,
    type CustomColumnDef,
    DataTable,
    LoadingIndicator,
    StatusBadge,
    TabHeader} from "@/components/ui";
import { RECIPIENT_TYPE,RECIPIENT_TYPE_AR, SALARY_STATUS } from "@/constants";
import { SALARY_PAYMENT_STATUS } from "@/constants/statusConfigs";
import { formatDate } from "@/lib/utils";

interface SalaryPaymentItem {
    paymentID: number;
    amount: number;
    statusName: string;
    transactionNumber: string | null;
    paymentBookNumber: string | null;
    recipientType: string;
    sendDate: string | null;
    salaryStatus: string;
}

interface SalaryPaymentsTabProps {
    fileId: number;
}

/**
 * تبويب سجل عمليات صرف المعاشات للملف
 */
export function SalaryPaymentsTab({ fileId }: SalaryPaymentsTabProps) {
    // جلب سجل عمليات الصرف
    const { data: items = [], isLoading, error } = useQuery({
        queryKey: ["salary-payments", fileId],
        queryFn: () => retirementApi.getSalaryPayments(fileId).then(data => data as unknown as SalaryPaymentItem[])
    });



    const columns = useMemo<CustomColumnDef<SalaryPaymentItem>[]>(() => [
        {
            key: "amount",
            header: "المبلغ",
            render: (row) => (
                <span className="font-mono font-bold text-primary">
                    {row.amount.toLocaleString("ar-YE")} ر.ي
                </span>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => (
                <StatusBadge status={row.statusName} config={SALARY_PAYMENT_STATUS} />
            )
        },
        {
            key: "transactionNumber",
            header: "رقم الحوالة",
            render: (row) => <span className="font-mono text-xs">{row.transactionNumber || "—"}</span>
        },
        {
            key: "recipient",
            header: "المستلم",
            render: (row) => (
                <div className="text-xs text-muted-foreground text-start">
                    {row.recipientType === RECIPIENT_TYPE.RETIREE ? RECIPIENT_TYPE_AR.RETIREE :
                        row.recipientType === RECIPIENT_TYPE.AGENT ? RECIPIENT_TYPE_AR.AGENT :
                            row.recipientType === RECIPIENT_TYPE.GUARDIAN ? RECIPIENT_TYPE_AR.GUARDIAN : RECIPIENT_TYPE_AR.ENTITLED}
                </div>
            )
        },
        { key: "paymentBookNumber", header: "الدفتر", accessor: (row) => row.paymentBookNumber || "—" },
        {
            key: "sendDate",
            header: "تاريخ الإرسال",
            render: (row) => <span className="text-[10px] sm:text-xs font-mono">{row.sendDate ? formatDate(row.sendDate) : "—"}</span>
        },
        {
            key: "salaryStatus",
            header: "حالة القيد",
            render: (row) => (
                <Badge variant={row.salaryStatus === SALARY_STATUS.SUSPENDED ? "danger" : "success"} className="text-[10px] sm:text-xs h-5 px-1.5">
                    {row.salaryStatus || "—"}
                </Badge>
            )
        }
    ], []);

    useEffect(() => {
        if (error) toast.error("حدث خطأ أثناء تحميل سجل الصرف");
    }, [error]);

    if (error) return (
        <div className="py-12 text-center text-destructive flex flex-col items-center gap-2">
            <AlertTriangle className="w-8 h-8" />
            <p className="font-bold">حدث خطأ أثناء تحميل سجل الصرف</p>
        </div>
    );

    if (isLoading) return <LoadingIndicator message="جاري تحميل سجل الصرف..." />;

    return (
        <div className="space-y-2 md:space-y-4 animate-in fade-in duration-500 text-start">
            <TabHeader
                title="سجل عمليات المعاشات"
                description="عرض وتتبع عمليات صرف المعاشات والحوالات المالية الصادرة للملف التقاعدي"
                icon={CreditCard}
            />

            <DataTable
                columns={columns}
                data={items}
                loading={isLoading}
                emptyMessage="لا توجد عمليات صرف مسجلة لهذا الملف"
                renderMobileCard={(item) => (
                    <div className="p-3 space-y-2">
                        <div className="flex items-center justify-between">
                            <span className="font-mono font-bold text-primary text-base">
                                {item.amount.toLocaleString("ar-YE")} <span className="text-[10px] text-muted-foreground">ر.ي</span>
                            </span>
                            <StatusBadge status={item.statusName} config={SALARY_PAYMENT_STATUS} className="text-[9px] h-3.5" />
                        </div>
                        <div className="text-[10px] text-muted-foreground space-y-0.5">
                            <div>
                                المستلم: {item.recipientType === RECIPIENT_TYPE.RETIREE ? RECIPIENT_TYPE_AR.RETIREE :
                                    item.recipientType === RECIPIENT_TYPE.AGENT ? RECIPIENT_TYPE_AR.AGENT :
                                        item.recipientType === RECIPIENT_TYPE.GUARDIAN ? RECIPIENT_TYPE_AR.GUARDIAN : RECIPIENT_TYPE_AR.ENTITLED}
                            </div>
                            {item.transactionNumber && <div>رقم الحوالة: <span className="font-mono">{item.transactionNumber}</span></div>}
                            {item.paymentBookNumber && <div>الدفتر: {item.paymentBookNumber}</div>}
                            {item.sendDate && <div>الإرسال: <span className="font-mono">{formatDate(item.sendDate)}</span></div>}
                        </div>
                    </div>
                )}
            />
        </div>
    );
}
