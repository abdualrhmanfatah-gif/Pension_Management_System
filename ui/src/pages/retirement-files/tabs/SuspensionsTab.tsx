import { useQuery } from "@tanstack/react-query";
<<<<<<< HEAD
import { AlertTriangle, Ban, CheckCircle2, ShieldAlert } from "lucide-react";
=======
import { AlertTriangle, CheckCircle2,ShieldAlert } from "lucide-react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import { type EndSuspensionDto,retirementApi } from "@/api/retirement";
<<<<<<< HEAD
import { EndSuspensionDialog, type EndSuspensionFormValues } from "@/components/domain/retirement/EndSuspensionDialog";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    Badge,
    Button,
    Card,
    type CustomColumnDef,
    DataTable,
    Heading,
    LoadingIndicator,
<<<<<<< HEAD
    TabHeader,
    useConfirm} from "@/components/ui";
=======
    useConfirm} from "@/components/ui";
import { EndSuspensionDialog, type EndSuspensionFormValues } from "@/components/domain/retirement/EndSuspensionDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { SUSPENSION_STATUS } from "@/constants";
import { useApiMutation } from "@/hooks";
import { cn, formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

interface Suspension {
    suspensionID: number;
    suspensionDate: string;
    endDate: string | null;
    suspensionReason: string;
    suspensionSource: string | null;
    status: string;
    linkedPledge: { pledgeID: number } | null;
}

interface SuspensionsTabProps {
    fileId: number;
}

/**
 * تبويب إيقافات صرف المعاش للملف
 */
export function SuspensionsTab({ fileId }: SuspensionsTabProps) {
    const { confirm } = useConfirm();
    const canEnd = useHasPermission("Suspension.Edit");
    const [endOpen, setEndOpen] = useState(false);
    const [endingId, setEndingId] = useState<number | null>(null);

    // جلب الإيقافات
    const { data: suspensions = [], isLoading, error, refetch } = useQuery({
        queryKey: ["suspensions", fileId],
        queryFn: () => retirementApi.getSuspensions(fileId).then(data => data as unknown as Suspension[])
    });

    const endMutation = useApiMutation(
        async (values: EndSuspensionFormValues) => {
            const dto: EndSuspensionDto = {
                endReason: values.endReason
            };
            return retirementApi.endSuspension(endingId!, dto);
        },
        {
            successMessage: "تم رفع الإيقاف بنجاح",
            onSuccess: () => {
                setEndOpen(false);
                setEndingId(null);
                refetch();
            }
        }
    );

    const activeSuspension = useMemo(() => suspensions.find(s => s.status === SUSPENSION_STATUS.ACTIVE), [suspensions]);

    const columns = useMemo<CustomColumnDef<Suspension>[]>(() => [
        {
            key: "suspensionReason",
            header: "سبب الإيقاف",
            render: (row) => <span className="font-bold text-destructive">{row.suspensionReason}</span>
        },
        { key: "suspensionSource", header: "مصدر الإيقاف", accessor: (row) => row.suspensionSource || "—" },
        {
            key: "suspensionDate",
            header: "تاريخ الإيقاف",
            render: (row) => <span className="font-mono text-xs">{formatDate(row.suspensionDate)}</span>
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => (
                <Badge variant={row.status === SUSPENSION_STATUS.ACTIVE ? "destructive" : "success"} className="font-black">
                    {row.status === SUSPENSION_STATUS.ACTIVE ? "موقوف" : "مستأنف"}
                </Badge>
            )
        },
        {
            key: "endDate",
            header: "تاريخ الاستئناف",
            render: (row) => row.endDate ? <span className="font-mono text-xs text-emerald-600">{formatDate(row.endDate)}</span> : "—"
        },
        {
            key: "linkedPledge",
            header: "التعهد",
            render: (row) => row.linkedPledge ? (
                <Badge variant="outline" className="text-[10px] sm:text-xs font-bold">تعهد #{row.linkedPledge.pledgeID}</Badge>
            ) : "—"
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row) => (row.status === SUSPENSION_STATUS.ACTIVE && canEnd) ? (
                <div className="text-center">
                    <Button
                        size="sm"
                        variant="outline"
                        className="min-h-[44px] gap-1 border-emerald-200 text-emerald-700 hover:bg-emerald-50 font-bold"
                        onClick={async () => {
                            const ok = await confirm({
                                title: "رفع الإيقاف",
                                description: "هل أنت متأكد من رغبتك في رفع هذا الإيقاف؟",
                                variant: "default"
                            });
                            if (ok) {
                                setEndingId(row.suspensionID);
                                setEndOpen(true);
                            }
                        }}
                    >
                        <CheckCircle2 className="w-3.5 h-3.5" /> رفع الإيقاف
                    </Button>
                </div>
            ) : null
        }
    ], [canEnd, confirm]);

    useEffect(() => {
        if (error) toast.error("حدث خطأ أثناء تحميل سجل الإيقافات");
    }, [error]);

    if (error) return (
        <div className="py-12 text-center text-destructive flex flex-col items-center gap-2">
            <AlertTriangle className="w-8 h-8" />
            <p className="font-bold">حدث خطأ أثناء تحميل سجل الإيقافات</p>
        </div>
    );

    if (isLoading) return <LoadingIndicator message="جاري تحميل سجل الإيقافات..." />;

    return (
        <div className="space-y-3 md:space-y-6 animate-in fade-in duration-500 text-start">
<<<<<<< HEAD
            <TabHeader
                title="سجل إيقاف المعاش"
                description="متابعة ورصد قرارات إيقاف المعاش واستئناف الصرف للملف التقاعدي"
                icon={Ban}
            />

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            {activeSuspension && (
                <Card className="bg-rose-500/5 border-rose-100 p-4 flex items-start gap-4 text-start">
                    <div className="w-10 h-10 rounded-full bg-rose-100 flex items-center justify-center text-rose-600 shrink-0">
                        <ShieldAlert className="w-5 h-5" />
                    </div>
                    <div className="space-y-1">
                        <Heading level="h4" className="font-bold text-rose-900">الملف موقوف حالياً</Heading>
<<<<<<< HEAD
                        <p className="text-sm text-rose-800">هذا الملف لديه إيقاف نشط صادر من {activeSuspension.suspensionSource} بتاريخ {formatDate(activeSuspension.suspensionDate)} بسبب {activeSuspension.suspensionReason}.</p>
=======
                        <p className="text-sm text-rose-800/80">هذا الملف لديه إيقاف نشط صادر من {activeSuspension.suspensionSource} بتاريخ {formatDate(activeSuspension.suspensionDate)} بسبب {activeSuspension.suspensionReason}.</p>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    </div>
                </Card>
            )}

            <DataTable
                columns={columns}
                data={suspensions}
                loading={isLoading}
                emptyMessage="لا توجد إيقافات مسجلة لهذا الملف"
                renderMobileCard={(item) => (
                    <div className="p-3 space-y-2">
                        <div className="flex items-center justify-between">
                            <span className="font-bold text-xs text-destructive truncate max-w-[200px]">{item.suspensionReason}</span>
                            <span className={cn("w-2 h-2 rounded-full shrink-0", item.status === SUSPENSION_STATUS.ACTIVE ? "bg-red-500" : "bg-emerald-500")} title={item.status === SUSPENSION_STATUS.ACTIVE ? "موقوف" : "مستأنف"} />
                        </div>
                        <div className="text-[10px] text-muted-foreground space-y-0.5">
                            {item.suspensionSource && <div>المصدر: {item.suspensionSource}</div>}
                            <div>التاريخ: <span className="font-mono">{formatDate(item.suspensionDate)}</span></div>
                            {item.endDate && <div>الاستئناف: <span className="font-mono text-emerald-600">{formatDate(item.endDate)}</span></div>}
                        </div>
                        {item.status === SUSPENSION_STATUS.ACTIVE && canEnd && (
                            <div className="flex justify-end">
                                <Button
                                    size="sm"
                                    variant="outline"
                                    className="min-h-[32px] h-8 gap-1 border-emerald-200 text-emerald-700 hover:bg-emerald-50 font-bold text-xs px-2"
                                    onClick={async () => {
                                        const ok = await confirm({
                                            title: "رفع الإيقاف",
                                            description: "هل أنت متأكد من رغبتك في رفع هذا الإيقاف؟",
                                            variant: "default"
                                        });
                                        if (ok) {
                                            setEndingId(item.suspensionID);
                                            setEndOpen(true);
                                        }
                                    }}
                                >
                                    <CheckCircle2 className="w-3.5 h-3.5" /> رفع الإيقاف
                                </Button>
                            </div>
                        )}
                    </div>
                )}
            />

            <EndSuspensionDialog
                open={endOpen}
                onOpenChange={setEndOpen}
                onSubmit={(v) => endMutation.mutate(v)}
                isLoading={endMutation.isPending}
            />
        </div>
    );
}
