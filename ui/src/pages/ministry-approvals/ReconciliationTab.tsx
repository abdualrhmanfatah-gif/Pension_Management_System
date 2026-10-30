import { useQuery } from "@tanstack/react-query";
import { AlertCircle, CheckCircle2, UserCheck,UserX } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import { ministryApi, type ReconciliationRowDto } from "@/api/ministry";
import { Badge, Card, type CustomColumnDef, DataTable, ErrorState, Loader } from "@/components/ui";
import { GAP_STATUS } from "@/constants";

interface ReconciliationTabProps {
    batchId: number;
}

export function ReconciliationTab({ batchId }: ReconciliationTabProps) {
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);

    const { data: matrixRes, isLoading, error } = useQuery({
        queryKey: ["force", "reconciliation", batchId, page, pageSize],
        queryFn: () => ministryApi.getReconciliationMatrix(batchId, page, pageSize),
        staleTime: 30000
    });

    const data = matrixRes?.data?.items || [];
    const totalCount = matrixRes?.data?.totalCount || 0;

    useEffect(() => {
        if (error) toast.error("تعذر تحميل بيانات التسوية", { id: "reconciliation" });
    }, [error]);

    if (isLoading) {
        return (
            <div className="flex flex-col items-center justify-center h-[60dvh] gap-4" dir="rtl">
                <Loader variant="page" text="جاري تحميل بيانات التسوية..." />
            </div>
        );
    }

    if (error) {
        return <ErrorState message="تعذر تحميل بيانات التسوية" />;
    }

    const columns: CustomColumnDef<ReconciliationRowDto>[] = [
        {
            key: "militaryId",
            header: "الرقم العسكري",
            accessorKey: "militaryId",
            className: "font-mono text-sm"
        },
        {
            key: "ministryName",
            header: "الاسم في الكشف",
            accessorKey: "ministryName"
        },
        {
            key: "dbName",
            header: "الاسم في النظام",
            render: (row) => row.dbName || <span className="text-muted-foreground italic text-xs">غير مسجل</span>
        },
        {
            key: "statusGap",
            header: "حالة المطابقة",
            render: (row) => {
                const isMatched = row.statusGap === GAP_STATUS.MATCHED;
                const isMissing = row.statusGap === GAP_STATUS.NOT_IN_SYSTEM;
                
                return (
                    <Badge 
                        variant={isMatched ? "success" : isMissing ? "danger" : "warning"}
                        className="gap-1"
                    >
                        {isMatched ? <CheckCircle2 className="w-3 h-3" /> : <AlertCircle className="w-3 h-3" />}
                        {row.statusGap}
                    </Badge>
                );
            }
        },
        {
            key: "action",
            header: "الإجراء المطلوب",
            render: (row) => (
                <div className="flex items-center gap-2">
                    {row.isActionRequired ? (
<<<<<<< HEAD
                        <span className="text-xs text-danger flex items-center gap-1 font-medium">
                            <UserX className="w-3 h-3" /> يتطلب معالجة
                        </span>
                    ) : (
                        <span className="text-xs text-success flex items-center gap-1 font-medium">
=======
                        <span className="text-xs text-rose-600 flex items-center gap-1 font-medium">
                            <UserX className="w-3 h-3" /> يتطلب معالجة
                        </span>
                    ) : (
                        <span className="text-xs text-emerald-600 flex items-center gap-1 font-medium">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            <UserCheck className="w-3 h-3" /> مكتمل
                        </span>
                    )}
                </div>
            )
        }
    ];

    return (
        <div className="space-y-4">
<<<<<<< HEAD
            <div className="bg-warning/10  border border-warning  p-4 rounded-xl flex gap-3 items-start">
                <AlertCircle className="w-5 h-5 text-warning shrink-0 mt-0.5" />
                <div className="space-y-1">
                    <h4 className="text-sm font-semibold text-warning ">تحليل المطابقة الذكي</h4>
                    <p className="text-xs text-warning  leading-relaxed">
=======
            <div className="bg-amber-50 dark:bg-amber-950/20 border border-amber-200 dark:border-amber-900/30 p-4 rounded-xl flex gap-3 items-start">
                <AlertCircle className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
                <div className="space-y-1">
                    <h4 className="text-sm font-semibold text-amber-900 dark:text-amber-400">تحليل المطابقة الذكي</h4>
                    <p className="text-xs text-amber-800/80 dark:text-amber-500/80 leading-relaxed">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        يقوم النظام بمقارنة بيانات هذه الإحالة مع قاعدة بيانات المتقاعدين الحالية. 
                        الإحالات التي تحمل حالة "غير موجود في النظام" تتطلب إنشاء ملفات تقاعد جديدة أو ربطها يدوياً.
                    </p>
                </div>
            </div>

            <Card className="">
                    <DataTable
                        columns={columns}
                        data={data}
                        loading={isLoading}
                        emptyMessage="لا توجد بيانات للمطابقة في هذه الإحالة."
                        pagination={{
                        page: page,
                        pageSize: pageSize,
                        total: totalCount,
                        onPageChange: setPage
                    }}
                />
            </Card>
        </div>
    );
}
