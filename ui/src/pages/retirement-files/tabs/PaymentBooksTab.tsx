import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, Ban, BookOpen, Check, Edit2, FileText, Plus, Printer } from "lucide-react";
import { useEffect, useMemo,useState } from "react";
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
import { type BookFormValues,PaymentBookDialog } from "@/components/domain/retirement/PaymentBookDialog";
import { Badge, Button, type CustomColumnDef,DataTable, StatusBadge, TabHeader, useConfirm } from "@/components/ui";
import { PENDING_REVIEW_STATUS, RECIPIENT_TYPE_AR } from "@/constants";
import { PAYMENT_BOOK_STATUS } from "@/constants/statusConfigs";
import { useApiMutation, useCreateShortcut, useCrudState } from "@/hooks";
import { useAgentsQuery, useEntitledPersonsQuery, useFileDetailsQuery, useGuardiansQuery } from "@/hooks/queries";
import { cn, formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { AgentSummary, EntitledPersonSummary,GuardianSummary } from "@/types/retirement";

// ─── Types ──────────────────────────────────────────────

export interface PaymentBook {
    paymentBookID: number;
    fileID: number;
    bookNumber: string;
    issueDate: string;
    expiryDate: string;
    status: string;
    bookType: string;
    legacyColor?: string | null;
    changeReason?: string | null;
    recipientName?: string | null;
    recipientType?: string | null;
    notes?: string | null;
    issuingBranchID?: number | null;
    issuingBranchName?: string | null;
    createdAt: string;
    updatedAt?: string | null;
    pendingReviewStatus?: number;
}

interface RecipientOption { id: string | number; name: string; }

interface PaymentBooksTabProps {
    fileId: number;
}

const CHANGE_REASONS = [
    "إصدار أول",
    "تجديد سنوي",
    "تغيير وكيل",
    "تعيين وكيل",
    "تعديل وصي",
    "تعديل مستحقين",
    "بدل فاقد",
    "بدل تالف",
];
const RECIPIENT_TYPES = [RECIPIENT_TYPE_AR.RETIREE, RECIPIENT_TYPE_AR.AGENT, RECIPIENT_TYPE_AR.GUARDIAN, RECIPIENT_TYPE_AR.ENTITLED];

/**
 * تبويب سجل دفاتر صرف المعاش لملف التقاعد
 */
export function PaymentBooksTab({ fileId }: PaymentBooksTabProps) {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const crud = useCrudState<PaymentBook>();


    // الصلاحيات
    const canCreate = useHasPermission("PaymentBook.Add");
    const canEdit = useHasPermission("PaymentBook.Edit");
    const canDelete = useHasPermission("PaymentBook.Delete", "PaymentBook.Manage");

    const [modalMode, setModalMode] = useState<"create" | "edit">("create");

    const { file } = useFileDetailsQuery(fileId);
    const { data: agents = [] } = useAgentsQuery(fileId);
    const { data: guardians = [] } = useGuardiansQuery(fileId);
    const { data: entitleds = [] } = useEntitledPersonsQuery(fileId);

    // جلب البيانات
    const { data: books = [], isLoading, error } = useQuery({
        queryKey: ["payment-books", fileId],
        queryFn: () => retirementApi.getPaymentBooks(fileId).then(data => data as unknown as PaymentBook[])
    });

    const acknowledgeMutation = useApiMutation(
        (params: { entityType: string; entityId: number }) =>
            retirementApi.acknowledgeEntityRejection(params.entityType, params.entityId),
        {
            successMessage: "تم إعادة المراجعة وإعادة الكيان لحالته الطبيعية",
            invalidateKeys: [["payment-books", fileId], ["retirement-file"]]
        }
    );

    // طفرات البيانات
    const saveMutation = useApiMutation(
        async ({ values, paymentBookID }: { values: BookFormValues; paymentBookID?: number }) => {
            if (paymentBookID) {
                const updateDto = {
                    bookNumber: values.bookNumber,
                    issueDate: values.issueDate ? values.issueDate : undefined,
                    expiryDate: values.expiryDate ? values.expiryDate : undefined,
                    status: values.status,
                    legacyColor: values.legacyColor,
                    changeReason: values.changeReason,
                    recipientName: values.recipientName,
                    recipientType: values.recipientType,
                    notes: values.notes,
                    issuingBranchID: values.issuingBranchID || null,
                };
                return retirementApi.updatePaymentBook(paymentBookID, updateDto);
            } else {
                const formData = new FormData();
                formData.append("FileID", fileId.toString());
                formData.append("IssueDate", values.issueDate ? values.issueDate : new Date().toISOString().split('T')[0]);
                formData.append("BookType", values.bookType);
                if (values.bookType === "يدوي") {
                    if (values.bookNumber) formData.append("BookNumber", values.bookNumber);
                    if (values.legacyColor) formData.append("LegacyColor", values.legacyColor);
                } else {
                    if (values.expiryDate) formData.append("ExpiryDate", values.expiryDate);
                    if (values.changeReason) formData.append("ChangeReason", values.changeReason);
                }

                if (values.recipientName) formData.append("RecipientName", values.recipientName);
                if (values.recipientType) formData.append("RecipientType", values.recipientType);
                if (values.notes) formData.append("Notes", values.notes);
                if (values.issuingBranchID) formData.append("IssuingBranchID", values.issuingBranchID.toString());
                
                // Auto Salary Recipient fields
                if (values.autoCreateSalaryRecipient !== undefined) {
                    formData.append("AutoCreateSalaryRecipient", values.autoCreateSalaryRecipient.toString());
                }
                if (values.autoCreateSalaryRecipient && values.salaryRecipientID && values.salaryRecipientType) {
                    if (values.salaryRecipientType === RECIPIENT_TYPE_AR.RETIREE) formData.append("RetireeID", values.salaryRecipientID.toString());
                    else if (values.salaryRecipientType === RECIPIENT_TYPE_AR.AGENT) formData.append("AgentID", values.salaryRecipientID.toString());
                    else if (values.salaryRecipientType === RECIPIENT_TYPE_AR.ENTITLED) formData.append("EntitledID", values.salaryRecipientID.toString());
                    else if (values.salaryRecipientType === RECIPIENT_TYPE_AR.GUARDIAN) formData.append("GuardianID", values.salaryRecipientID.toString());
                }

                if (values.bookImage instanceof File) {
                    formData.append("BookImage", values.bookImage);
                }
                return retirementApi.createPaymentBook(formData);
            }
        },
        {
            successMessage: "تم حفظ دفتر الصرف بنجاح",
            invalidateKeys: [["payment-books", fileId], ["retirement-file"], ["salary-recipients", fileId]],
            onSuccess: () => {
                sessionStorage.removeItem("payment_book_draft");
                crud.closeForm();
            }
        }
    );

    const deleteMutation = useApiMutation(
        (id: number) => retirementApi.deletePaymentBook(id),
        {
            successMessage: "تم إلغاء تفعيل الدفتر بنجاح",
            invalidateKeys: [["payment-books", fileId], ["retirement-file"]]
        }
    );

    useEffect(() => {
        if (error) toast.error("حدث خطأ أثناء تحميل دفاتر الصرف");
    }, [error]);

    const handleDelete = async (book: PaymentBook) => {
        const ok = await confirm({
            title: "إلغاء تفعيل الدفتر",
            description: `هل أنت متأكد من رغبتك في إلغاء تفعيل الدفتر رقم "${book.bookNumber}"؟`,
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(book.paymentBookID);
    };

    const getRecipientOptions = async (recipientType: string): Promise<RecipientOption[]> => {
        if (!recipientType) return [];
        if (recipientType === RECIPIENT_TYPE_AR.RETIREE) {
            const name = file?.retireeFullName;
            const id = file?.retireeID; 
            return name && id ? [{ id: id, name: name }] : [];
        } else if (recipientType === RECIPIENT_TYPE_AR.AGENT) {
            return (agents as AgentSummary[])
                .filter(a => a.isActive && !a.isDeleted)
                .map(item => ({ id: item.agentID, name: item.fullName }));
        } else if (recipientType === RECIPIENT_TYPE_AR.GUARDIAN) {
            return (guardians as GuardianSummary[])
                .filter(g => g.isActive && !g.isDeleted)
                .map(item => ({ id: item.guardianID, name: item.fullName }));
        } else if (recipientType === RECIPIENT_TYPE_AR.ENTITLED) {
            return (entitleds as EntitledPersonSummary[])
                .filter(e => e.isEligible && !e.isDeceased && !e.isDeleted)
                .map(item => ({ id: item.entitledID, name: item.fullName }));
        }
        return [];
    };



    const getBookTypeStyle = (book: PaymentBook) => {
        if (book.bookType === "إلكتروني") {
            return { bg: "bg-green-500/5 dark:bg-green-950/20", icon: BookOpen, iconBg: "bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400", label: "إلكتروني" };
        }
        switch (book.legacyColor) {
            case "أبيض": return { bg: "bg-muted dark:bg-muted/20", icon: FileText, iconBg: "bg-muted dark:bg-muted text-muted-foreground", label: "يدوي - أبيض" };
            case "أخضر": return { bg: "bg-green-500/5 dark:bg-green-950/20", icon: FileText, iconBg: "bg-green-200 dark:bg-green-900/30 text-green-700 dark:text-green-400", label: "يدوي - أخضر" };
            case "أزرق": return { bg: "bg-blue-500/5 dark:bg-blue-950/20", icon: FileText, iconBg: "bg-blue-200 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400", label: "يدوي - أزرق" };
            default: return { bg: "bg-muted dark:bg-muted/20", icon: FileText, iconBg: "bg-muted dark:bg-muted text-muted-foreground", label: "يدوي" };
        }
    };

    const columns = useMemo<CustomColumnDef<PaymentBook>[]>(() => [
        {
            key: "bookNumber",
            header: "رقم الدفتر",
            render: (row) => {
                const style = getBookTypeStyle(row);
                const Icon = style.icon;
                return (
                    <div className={`flex items-center gap-2 p-1.5 rounded-lg ${style.bg}`}>
                        <div className={`w-8 h-8 rounded-lg flex items-center justify-center ${style.iconBg}`}>
                            <Icon className="w-4 h-4" />
                        </div>
                        <div className="flex flex-col">
                            <span className="font-mono font-bold tracking-wider flex items-center gap-1.5">
                                {row.bookNumber}
                                {row.pendingReviewStatus === PENDING_REVIEW_STATUS.PENDING_REVIEW && (
                                    <Badge variant="warning" className="text-[10px] sm:text-xs h-4 leading-none px-1">قيد المراجعة</Badge>
                                )}
                                {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                    <Badge variant="destructive" className="text-[10px] sm:text-xs h-4 leading-none px-1">مرفوض</Badge>
                                )}
                            </span>
                            <span className="text-[10px] sm:text-xs text-muted-foreground">{style.label}</span>
                        </div>
                    </div>
                );
            }
        },
        {
            key: "issueDate",
            header: "تاريخ الإصدار",
            render: (row) => <span className="font-mono text-xs">{formatDate(row.issueDate)}</span>
        },
        {
            key: "expiryDate",
            header: "تاريخ الانتهاء",
            render: (row) => row.bookType === "يدوي"
                ? <span className="text-xs text-muted-foreground">—</span>
                : <span className="font-mono text-xs">{formatDate(row.expiryDate)}</span>
        },
        {
            key: "recipient",
            header: "المستلم",
            render: (row) => (
                <div className="flex flex-col gap-0.5 text-start">
                    <span className="font-bold text-sm">{row.recipientName || "—"}</span>
                    <Badge variant="outline" className="w-fit text-[10px] sm:text-xs h-4 leading-none px-1.5">{row.recipientType || "—"}</Badge>
                </div>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => <StatusBadge status={row.status} config={PAYMENT_BOOK_STATUS} />
        },
        {
            key: "issuingBranch",
            header: "جهة الإصدار",
            render: (row) => <span className="text-xs">{row.issuingBranchName || "—"}</span>
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row) => (
                <div className="flex items-center justify-center gap-1">
                    {row.bookType !== "يدوي" && (
                        <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px]" onClick={() => window.open(`/payment-books/${row.paymentBookID}/print`, "_blank")}>
                            <Printer className="w-4 h-4 text-primary" />
                        </Button>
                    )}
                    {canEdit && (
                        <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px]" onClick={() => { setModalMode("edit"); crud.openEdit(row); }}>
                            <Edit2 className="w-4 h-4" />
                        </Button>
                    )}
                    {canDelete && (
                        <Button 
                            variant="ghost" 
                            size="icon" 
                            className="min-h-[44px] min-w-[44px] text-destructive" 
                            disabled={row.status === "ملغي"}
                            onClick={() => handleDelete(row)}
                        >
                            <Ban className="w-4 h-4" />
                        </Button>
                    )}
                    {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                        <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px] text-amber-600" onClick={() => acknowledgeMutation.mutate({ entityType: "PaymentBook", entityId: row.paymentBookID })} title="إعادة المراجعة">
                            <Check className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            )
        }
    ], [crud, file, agents, guardians, entitleds, handleDelete, canEdit, canDelete, acknowledgeMutation]);

    const handleOpenCreate = () => { setModalMode("create"); crud.openCreate(); };

    useCreateShortcut(handleOpenCreate, canCreate);

    if (error) return (
        <div className="py-12 text-center text-destructive flex flex-col items-center gap-2">
            <AlertTriangle className="w-8 h-8" />
            <p className="font-bold">حدث خطأ أثناء تحميل دفاتر الصرف</p>
        </div>
    );

    return (
        <div className="space-y-2 md:space-y-4 animate-in fade-in duration-500 text-start">
            <TabHeader
                title="سجل دفاتر الصرف"
                description="قائمة بدفاتر صرف المستحقات الصادرة لهذا الملف"
                icon={BookOpen}
                action={{
                    label: "إضافة دفتر جديد",
                    icon: Plus,
                    onClick: () => { setModalMode("create"); crud.openCreate(); },
                    show: canCreate
                }}
            />

            <DataTable
                columns={columns}
                data={books}
                loading={isLoading}
                emptyMessage="لا يوجد دفاتر صرف مسجلة لهذا الملف"
                renderMobileCard={(item) => {
                    const style = getBookTypeStyle(item);
                    const Icon = style.icon;
                    return (
                        <div className="p-3 space-y-2">
                            <div className="flex items-center gap-2">
                                <div className={`w-7 h-7 rounded-lg flex items-center justify-center ${style.iconBg}`}>
                                    <Icon className="w-3.5 h-3.5" />
                                </div>
                                <div className="flex-1 min-w-0">
                                    <div className="font-mono font-bold text-xs tracking-wider flex items-center gap-1">
                                        <span className="truncate">{item.bookNumber}</span>
                                        {item.pendingReviewStatus === PENDING_REVIEW_STATUS.PENDING_REVIEW && (
                                            <Badge variant="warning" className="text-[8px] h-3.5 shrink-0">قيد المراجعة</Badge>
                                        )}
                                        {item.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                            <Badge variant="destructive" className="text-[8px] h-3.5 shrink-0">مرفوض</Badge>
                                        )}
                                    </div>
                                    <span className="text-[10px] text-muted-foreground">{style.label}</span>
                                </div>
                                <span className={cn("w-2 h-2 rounded-full shrink-0", item.status === "ملغي" ? "bg-muted-foreground" : "bg-emerald-500")} title={item.status} />
                            </div>
                            <div className="text-[10px] text-muted-foreground space-y-0.5">
                                {item.recipientName && <div>المستلم: <span className="font-bold">{item.recipientName}</span></div>}
                                {item.issuingBranchName && <div>جهة الإصدار: <span className="font-bold">{item.issuingBranchName}</span></div>}
                                <div>الإصدار: <span className="font-mono">{formatDate(item.issueDate)}</span></div>
                                {item.bookType !== "يدوي" && <div>الانتهاء: <span className="font-mono">{formatDate(item.expiryDate)}</span></div>}
                            </div>
                            <div className="flex justify-end gap-0">
                                {item.bookType !== "يدوي" && (
                                    <Button variant="ghost" size="icon" className="min-h-[32px] min-w-[32px] h-8 w-8" onClick={() => window.open(`/payment-books/${item.paymentBookID}/print`, "_blank")}>
                                        <Printer className="w-3.5 h-3.5 text-primary" />
                                    </Button>
                                )}
                                {canEdit && (
                                    <Button variant="ghost" size="icon" className="min-h-[32px] min-w-[32px] h-8 w-8" onClick={() => { setModalMode("edit"); crud.openEdit(item); }}>
                                        <Edit2 className="w-3.5 h-3.5" />
                                    </Button>
                                )}
                                {canDelete && (
                                    <Button variant="ghost" size="icon" className="min-h-[32px] min-w-[32px] h-8 w-8 text-destructive" disabled={item.status === "ملغي"} onClick={() => handleDelete(item)}>
                                        <Ban className="w-3.5 h-3.5" />
                                    </Button>
                                )}
                                {item.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                    <Button variant="ghost" size="icon" className="min-h-[32px] min-w-[32px] h-8 w-8 text-amber-600" onClick={() => acknowledgeMutation.mutate({ entityType: "PaymentBook", entityId: item.paymentBookID })}>
                                        <Check className="w-3.5 h-3.5" />
                                    </Button>
                                )}
                            </div>
                        </div>
                    );
                }}
            />

            <PaymentBookDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                fileId={fileId}
                mode={modalMode}
                editRecordId={crud.selectedItem?.paymentBookID}
                onSubmit={(v, ctx) => saveMutation.mutateAsync({ values: v, paymentBookID: ctx.paymentBookID })}
                isLoading={saveMutation.isPending}
                initialData={crud.selectedItem ? {
                    bookNumber: crud.selectedItem.bookNumber,
                    issueDate: crud.selectedItem.issueDate,
                    expiryDate: crud.selectedItem.expiryDate,
                    bookType: crud.selectedItem.bookType,
                    legacyColor: crud.selectedItem.legacyColor || undefined,
                    changeReason: crud.selectedItem.changeReason || undefined,
                    recipientType: crud.selectedItem.recipientType || undefined,
                    recipientName: crud.selectedItem.recipientName || undefined,
                    status: crud.selectedItem.status,
                    notes: crud.selectedItem.notes || undefined,
                    issuingBranchID: crud.selectedItem.issuingBranchID || undefined,
                } : undefined}
                lookups={{
                    changeReasons: CHANGE_REASONS,
                    recipientTypes: RECIPIENT_TYPES,
                    getRecipientOptions: getRecipientOptions
                }}
            />

            {ConfirmDialogElement}
        </div>
    );
}
