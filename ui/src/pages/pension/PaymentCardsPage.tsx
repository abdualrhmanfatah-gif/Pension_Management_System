import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import {
    CreditCard,
    Eye,
    Pencil,
    Printer,
    AlertTriangle,
    CheckCircle2,
    Clock,
    Inbox,
    XCircle,
} from "lucide-react";

import { paymentBookReviewApi } from "@/api/pension";
import { retirementApi } from "@/api/retirement";
import { retireeApi } from "@/api/retirees";
import { CrudPageLayout } from "@/components/ui/CrudPageLayout";
import { StatCard } from "@/components/ui/StatCard";
import { Badge } from "@/components/ui/Badge";
import { EmptyState, StatusBadge } from "@/components/ui";
import { Button } from "@/components/ui/Button";
import { BaseFormDialog } from "@/components/ui/BaseFormDialog";
import { FormField } from "@/components/ui/FormField";
import { Input } from "@/components/ui/Input";
import { NativeSelect } from "@/components/ui/NativeSelect";
import { FormSection } from "@/components/ui/FormSection";
import { InfoPair } from "@/components/ui/InfoPair";
import { Separator } from "@/components/ui/Separator";
import { PaymentBookDialog } from "@/components/domain/retirement/PaymentBookDialog";
import { useApiMutation } from "@/hooks/useApiMutation";
import type { PaymentBookReview } from "@/types/pension";
import type { BookFormValues } from "@/pages/retirement-files/tabs/BookFormSchema";
import { RECIPIENT_TYPE_AR } from "@/constants";
import { PAYMENT_BOOK_STATUS } from "@/constants/statusConfigs";
import { useConfirm } from "@/components/ui";

/* ------------------------------------------------------------------ */
/*  Constants                                                          */
/* ------------------------------------------------------------------ */

const STATUS_OPTIONS = [
    { value: "all", label: "الكل" },
    { value: "فعال", label: "فعال" },
    { value: "منتهي", label: "منتهي" },
    { value: "ملغي", label: "ملغي" },
    { value: "مفقود", label: "مفقود" },
    { value: "تالف", label: "تالف" },
];

const TYPE_OPTIONS = [
    { value: "all", label: "الكل" },
    { value: "إلكتروني", label: "إلكتروني" },
    { value: "يدوي", label: "يدوي" },
];



const CHANGE_REASONS = [
    "انتهاء الصلاحية",
    "تلف",
    "فقدان",
    "تغيير بيانات",
    "أخرى",
];

const RECIPIENT_TYPES = [
    RECIPIENT_TYPE_AR.RETIREE,
    RECIPIENT_TYPE_AR.AGENT,
    RECIPIENT_TYPE_AR.GUARDIAN,
    RECIPIENT_TYPE_AR.ENTITLED,
];

/* ------------------------------------------------------------------ */
/*  Page Component                                                     */
/* ------------------------------------------------------------------ */

export function PaymentCardsPage() {
    const navigate = useNavigate();
    const [search, setSearch] = useState("");
    const [statusFilter, setStatusFilter] = useState("all");
    const [typeFilter, setTypeFilter] = useState("all");
    const [expiringFilter, setExpiringFilter] = useState(false);

    const [detailItem, setDetailItem] = useState<PaymentBookReview | null>(null);

    const [showIssueDialog, setShowIssueDialog] = useState(false);
    const [issueFileId, setIssueFileId] = useState<number | null>(null);

    const [editItem, setEditItem] = useState<PaymentBookReview | null>(null);

    const [retireeSearchOpen, setRetireeSearchOpen] = useState(false);
    const [retireeSearch, setRetireeSearch] = useState("");
    const [selectedRetiree, setSelectedRetiree] = useState<{
        retireeId: number;
        fileId: number;
        name: string;
        nationalId: string;
    } | null>(null);
    const { confirm, ConfirmDialogElement } = useConfirm();

    /* ---- Lookups ---- */
    const getRecipientOptions = useMemo(
        () => async (_type: string) => [] as { id: number; name: string }[],
        []
    );

    const lookups = useMemo(
        () => ({
            changeReasons: CHANGE_REASONS,
            recipientTypes: RECIPIENT_TYPES,
            getRecipientOptions,
        }),
        [getRecipientOptions]
    );

    /* ---- Stats ---- */
    const { data: stats } = useQuery({
        queryKey: ["payment-book-stats"],
        queryFn: () => paymentBookReviewApi.getStats(),
    });

    /* ---- Main data ---- */
    const { data, isLoading } = useQuery({
        queryKey: ["payment-cards", { search, statusFilter, typeFilter, expiringFilter }],
        queryFn: () =>
            paymentBookReviewApi.getReview({
                page: 1,
                pageSize: 100,
                search: search || undefined,
                status: statusFilter === "all" ? undefined : statusFilter,
                bookType: typeFilter === "all" ? undefined : typeFilter,
                expiringWithinDays: expiringFilter ? 30 : undefined,
            }),
    });

    /* ---- Retiree search ---- */
    const { data: retireeResults, isLoading: isSearching } = useQuery({
        queryKey: ["retiree-search", retireeSearch],
        queryFn: () =>
            retireeApi.getAll({
                search: retireeSearch,
                page: 1,
                pageSize: 10,
            }),
        enabled: retireeSearch.length >= 2,
    });

    const rows = useMemo(() => data?.items ?? [], [data]);

    /* ---- Delete mutation ---- */
    const deleteMutation = useApiMutation(
        async (item: PaymentBookReview) => {
            await retirementApi.deletePaymentBook(item.paymentBookID);
        },
        {
            successMessage: "تم حذف الدفتر بنجاح",
            invalidateKeys: [["payment-cards"], ["payment-book-stats"]],
        }
    );

    /* ---- Save (issue) mutation ---- */
    const issueMutation = useApiMutation(
        async (values: BookFormValues) => {
            const formData = new FormData();
            formData.append("FileID", issueFileId!.toString());
            formData.append(
                "IssueDate",
                values.issueDate
                    ? new Date(values.issueDate).toISOString()
                    : new Date().toISOString()
            );
            formData.append("BookType", values.bookType ?? "");
            if (values.bookType === "يدوي") {
                if (values.bookNumber) formData.append("BookNumber", values.bookNumber);
                if (values.legacyColor) formData.append("LegacyColor", values.legacyColor);
            } else {
                if (values.expiryDate)
                    formData.append("ExpiryDate", new Date(values.expiryDate).toISOString());
                if (values.changeReason) formData.append("ChangeReason", values.changeReason);
            }
            if (values.recipientName) formData.append("RecipientName", values.recipientName);
            if (values.recipientType) formData.append("RecipientType", values.recipientType);
            if (values.notes) formData.append("Notes", values.notes);
            if (values.issuingBranchID)
                formData.append("IssuingBranchID", values.issuingBranchID.toString());
            if (values.bookImage instanceof File) {
                formData.append("BookImage", values.bookImage);
            }
            return retirementApi.createPaymentBook(formData);
        },
        {
            successMessage: "تم إصدار البطاقة بنجاح",
            invalidateKeys: [["payment-cards"], ["payment-book-stats"]],
            onSuccess: () => {
                setShowIssueDialog(false);
                setIssueFileId(null);
                setSelectedRetiree(null);
            },
        }
    );

    /* ---- Edit mutation ---- */
    const editMutation = useApiMutation(
        async (values: BookFormValues) => {
            const updateDto = {
                bookNumber: values.bookNumber,
                issueDate: values.issueDate
                    ? new Date(values.issueDate).toISOString()
                    : undefined,
                expiryDate: values.expiryDate
                    ? new Date(values.expiryDate).toISOString()
                    : undefined,
                status: values.status,
                legacyColor: values.legacyColor,
                changeReason: values.changeReason,
                recipientName: values.recipientName,
                recipientType: values.recipientType,
                notes: values.notes,
                issuingBranchID: values.issuingBranchID || null,
            };
            return retirementApi.updatePaymentBook(editItem!.paymentBookID, updateDto);
        },
        {
            successMessage: "تم تعديل البطاقة بنجاح",
            invalidateKeys: [["payment-cards"], ["payment-book-stats"]],
            onSuccess: () => setEditItem(null),
        }
    );

    /* ---- Columns ---- */
    const columns = useMemo(
        () => [
            {
                key: "bookNumber",
                header: "رقم الدفتر",
                render: (row: PaymentBookReview) => (
                    <span className="font-mono font-bold text-xs text-primary">
                        {row.bookNumber}
                    </span>
                ),
            },
            {
                key: "retireeName",
                header: "المتقاعد",
                render: (row: PaymentBookReview) => (
                    <div>
                        <p className="font-medium text-sm">{row.retireeName || "—"}</p>
                        <div className="flex items-center gap-2 text-xs text-muted-foreground flex-wrap">
                            {row.fileNumber && <span>ملف: {row.fileNumber}</span>}
                            {row.militaryNumber && <span>عسكري: {row.militaryNumber}</span>}
                        </div>
                    </div>
                ),
            },
            {
                key: "bookType",
                header: "النوع",
                render: (row: PaymentBookReview) => (
                    <Badge variant="outline">{row.bookType || "—"}</Badge>
                ),
            },
            {
                key: "issuingBranchName",
                header: "الفرع",
                render: (row: PaymentBookReview) => (
                    <span className="text-sm">{row.issuingBranchName || "—"}</span>
                ),
            },
            {
                key: "issueDate",
                header: "تاريخ الإصدار",
                render: (row: PaymentBookReview) => (
                    <span className="text-sm">
                        {row.issueDate ? new Date(row.issueDate).toLocaleDateString("ar-SA") : "—"}
                    </span>
                ),
            },
            {
                key: "expiryDate",
                header: "تاريخ الانتهاء",
                render: (row: PaymentBookReview) => {
                    if (!row.expiryDate) return <span className="text-sm">—</span>;
                    const date = new Date(row.expiryDate);
                    const now = new Date();
                    const daysLeft = Math.ceil(
                        (date.getTime() - now.getTime()) / (1000 * 60 * 60 * 24)
                    );
                    const isExpiringSoon = daysLeft >= 0 && daysLeft <= 30;
                    const isExpired = daysLeft < 0;
                    return (
                        <div className="flex items-center gap-1">
                            {isExpiringSoon && (
                                <AlertTriangle className="h-4 w-4 text-amber-500" />
                            )}
                            {isExpired && <XCircle className="h-4 w-4 text-red-500" />}
                            <span
                                className={`text-sm ${
                                    isExpiringSoon
                                        ? "text-amber-600 font-medium"
                                        : isExpired
                                          ? "text-red-600"
                                          : ""
                                }`}
                            >
                                {new Date(row.expiryDate).toLocaleDateString("ar-SA")}
                            </span>
                        </div>
                    );
                },
            },
            {
                key: "status",
                header: "الحالة",
                render: (row: PaymentBookReview) => (
                    <StatusBadge status={row.status ?? ""} config={PAYMENT_BOOK_STATUS} />
                ),
            },
            {
                key: "actions",
                header: "",
                render: (row: PaymentBookReview) => (
                    <div className="flex items-center gap-1">
                        <Button
                            size="sm"
                            variant="ghost-action"
                            onClick={(e) => {
                                e.stopPropagation();
                                setDetailItem(row);
                            }}
                        >
                            <Eye className="h-4 w-4" />
                        </Button>
                        <Button
                            size="sm"
                            variant="ghost-action"
                            onClick={(e) => {
                                e.stopPropagation();
                                setEditItem(row);
                            }}
                        >
                            <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                            size="sm"
                            variant="ghost-action"
                            onClick={(e) => {
                                e.stopPropagation();
                                navigate(`/payment-books/${row.paymentBookID}/print`);
                            }}
                        >
                            <Printer className="h-4 w-4" />
                        </Button>
                        <Button
                            size="sm"
                            variant="destructive"
                            onClick={async (e) => {
                                e.stopPropagation();
                                const ok = await confirm({
                                    title: "هل أنت متأكد من حذف هذا الدفتر؟",
                                    variant: "danger",
                                });
                                if (ok) {
                                    deleteMutation.mutate(row);
                                }
                            }}
                        >
                            <XCircle className="h-4 w-4" />
                        </Button>
                    </div>
                ),
            },
        ],
        []
    );

    /* ---- Render ---- */
    return (
        <CrudPageLayout
            title="بطاقات الصرف"
            description="إدارة دفاتر الصرف وبطاقات الصرف للمتقاعدين"
            columns={columns}
            data={rows}
            loading={isLoading}
            rowKey={(row) => row.paymentBookID}
            onRowClick={setDetailItem}
            searchable
            searchInHeader
            searchPlaceholder="بحث بالاسم / الرقم العسكري / رقم الملف / رقم الدفتر..."
            searchValue={search}
            onSearchChange={setSearch}
            createLabel="إصدار بطاقة جديدة"
            onCreateClick={() => setRetireeSearchOpen(true)}
            headerActions={
                <div className="flex gap-2">
                    <NativeSelect
                        value={statusFilter}
                        onChange={setStatusFilter}
                        options={STATUS_OPTIONS}
                        placeholder="الحالة"
                    />
                    <NativeSelect
                        value={typeFilter}
                        onChange={setTypeFilter}
                        options={TYPE_OPTIONS}
                        placeholder="النوع"
                    />
                    <Button
                        variant={expiringFilter ? "warning" : "outline"}
                        size="sm"
                        onClick={() => setExpiringFilter(!expiringFilter)}
                    >
                        <AlertTriangle className="h-4 w-4 ml-1" />
                        تنتهي خلال 30 يوم
                    </Button>
                </div>
            }
        >
            {stats && (
                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-2 sm:gap-4 mb-6">
                    <StatCard
                        title="الدفاتر الفعّالة"
                        value={stats.activeCount}
                        icon={<CheckCircle2 />}
                        variant="success"
                    />
                    <StatCard
                        title="المنتهية"
                        value={stats.expiredCount}
                        icon={<Clock />}
                        variant="warning"
                    />
                    <StatCard
                        title="المفقودة"
                        value={stats.lostCount}
                        icon={<XCircle />}
                        variant="destructive"
                    />
                    <StatCard
                        title="التالفة"
                        value={stats.damagedCount}
                        icon={<AlertTriangle />}
                        variant="destructive"
                    />
                    <StatCard
                        title="المجموع"
                        value={stats.totalCount}
                        icon={<CreditCard />}
                        variant="info"
                    />
                </div>
            )}

            {/* Retiree Search Dialog */}
            <BaseFormDialog
                open={retireeSearchOpen}
                onOpenChange={setRetireeSearchOpen}
                title="بحث عن المتتقاعد"
                description="أدخل الرقم الوطني أو اسم المتتقاعد للبحث"
                onSubmit={async () => {
                    if (!selectedRetiree) {
                        toast.error("يرجى اختيار متتقاعد");
                        return;
                    }
                    setIssueFileId(selectedRetiree.fileId);
                    setRetireeSearchOpen(false);
                    setShowIssueDialog(true);
                }}
                submitLabel="إصدار بطاقة"
                size="lg"
            >
                <FormField label="البحث">
                    <Input
                        value={retireeSearch}
                        onChange={(e) => setRetireeSearch(e.target.value)}
                        placeholder="أدخل الرقم الوطني أو الاسم..."
                    />
                </FormField>
                {isSearching && (
                    <p className="text-sm text-muted-foreground mt-2">جاري البحث...</p>
                )}
                {retireeResults && retireeResults.items.length > 0 && (
                    <div className="mt-3 space-y-2 max-h-60 overflow-auto">
                        {retireeResults.items.map((r) => (
                            <div
                                key={r.retireeID}
                                className={`p-3 border rounded-lg cursor-pointer transition-colors ${
                                    selectedRetiree?.retireeId === r.retireeID
                                        ? "border-blue-500 bg-blue-50"
                                        : "border-border hover:border-blue-300"
                                }`}
                                onClick={() => {
                                    setSelectedRetiree({
                                        retireeId: r.retireeID,
                                        fileId: r.fileID ?? 0,
                                        name: r.fullName,
                                        nationalId: r.nationalID,
                                    });
                                }}
                            >
                                <div className="font-medium">{r.fullName}</div>
                                <div className="text-sm text-muted-foreground">رقم وطني: {r.nationalID}</div>
                                <div className="text-sm text-muted-foreground">رقم عسكري: {r.militaryNumber || "—"}</div>
                            </div>
                        ))}
                    </div>
                )}
                {retireeResults &&
                    retireeResults.items.length === 0 &&
                    retireeSearch.length >= 2 && (
                        <EmptyState icon={Inbox} title="لا توجد نتائج" />
                    )}
            </BaseFormDialog>

            {/* Issue New Card Dialog */}
            {showIssueDialog && issueFileId && (
                <PaymentBookDialog
                    open={showIssueDialog}
                    onOpenChange={setShowIssueDialog}
                    mode="create"
                    lookups={lookups}
                    onSubmit={async (values: BookFormValues) => {
                        await issueMutation.mutateAsync(values);
                    }}
                />
            )}

            {/* Edit Dialog */}
            {editItem && (
                <PaymentBookDialog
                    open={!!editItem}
                    onOpenChange={(open) => {
                        if (!open) setEditItem(null);
                    }}
                    mode="edit"
                    initialData={{
                        bookNumber: editItem.bookNumber ?? "",
                        bookType: editItem.bookType ?? "إلكتروني",
                        issueDate: editItem.issueDate ?? "",
                        expiryDate: editItem.expiryDate ?? "",
                        status: editItem.status ?? "فعال",
                        legacyColor: editItem.legacyColor ?? "",
                        changeReason: editItem.changeReason ?? "",
                        recipientName: editItem.recipientName ?? "",
                        recipientType: editItem.recipientType ?? "",
                        notes: editItem.notes ?? "",
                        issuingBranchID: Number(editItem.issuingBranchID) || 0,
                    }}
                    lookups={lookups}
                    onSubmit={async (values: BookFormValues) => {
                        await editMutation.mutateAsync(values);
                    }}
                />
            )}

            {/* Detail Dialog */}
            <BaseFormDialog
                open={!!detailItem}
                onOpenChange={(open) => {
                    if (!open) setDetailItem(null);
                }}
                title="تفاصيل دفتر الصرف"
                submitLabel="إغلاق"
                onSubmit={async () => {
                    setDetailItem(null);
                }}
                size="lg"
            >
                {detailItem && (
                    <>
                        <FormSection title="بيانات المتتقاعد">
                            <InfoPair label="اسم المتتقاعد" value={detailItem.retireeName} />
                            <InfoPair label="رقم الملف" value={detailItem.fileNumber} />
                        </FormSection>
                        <Separator className="my-4" />
                        <FormSection title="بيانات الدفتر">
                            <InfoPair label="رقم الدفتر" value={detailItem.bookNumber} />
                            <InfoPair label="النوع" value={detailItem.bookType} />
                            <InfoPair
                                label="الفرع"
                                value={detailItem.issuingBranchName}
                            />
                            <InfoPair
                                label="الحالة"
                                value={
                                    <StatusBadge status={detailItem.status ?? ""} config={PAYMENT_BOOK_STATUS} />
                                }
                            />
                        </FormSection>
                        <Separator className="my-4" />
                        <FormSection title="التواريخ">
                            <InfoPair
                                label="تاريخ الإصدار"
                                value={detailItem.issueDate ? new Date(detailItem.issueDate).toLocaleDateString("ar-SA") : "—"}
                            />
                            <InfoPair
                                label="تاريخ الانتهاء"
                                value={detailItem.expiryDate ? new Date(detailItem.expiryDate).toLocaleDateString("ar-SA") : "—"}
                            />
                        </FormSection>
                        {detailItem.recipientName && (
                            <>
                                <Separator className="my-4" />
                                <FormSection title="المستفيد">
                                    <InfoPair
                                        label="اسم المستفيد"
                                        value={detailItem.recipientName}
                                    />
                                    <InfoPair
                                        label="نوع المستفيد"
                                        value={detailItem.recipientType}
                                    />
                                </FormSection>
                            </>
                        )}
                        {detailItem.bookImagePaths && detailItem.bookImagePaths.length > 0 && (
                            <>
                                <Separator className="my-4" />
                                <FormSection title="صورة الدفتر">
                                    <div className="flex flex-wrap gap-2">
                                        {detailItem.bookImagePaths.map((img, i) => (
                                            <img
                                                key={i}
                                                src={img}
                                                alt={`صورة الدفتر ${i + 1}`}
                                                className="max-w-full max-h-64 rounded border"
                                            />
                                        ))}
                                    </div>
                                </FormSection>
                            </>
                        )}
                    </>
                )}
            </BaseFormDialog>
            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
