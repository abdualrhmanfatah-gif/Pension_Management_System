"use no memo";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { BookOpen, CreditCard, FileText, Trash2,User } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import api from "@/api/axios";
import { retirementApi } from "@/api/retirement";
import {
    BaseFormDialog,
    Button,
    DateInput,
    FileField,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    RadioGroup,
    RadioGroupItem,
    Textarea,
} from "@/components/ui";
import { LEGACY_BOOK_COLORS } from "@/constants";
import { useRecordNavigation } from "@/hooks";
import { useDocumentBlob } from "@/hooks/useDocumentBlob";
import { chartColors } from "@/lib/chartColors";
import { toDateOnlyYemen } from "@/lib/date";
import { cn } from "@/lib/utils";
import type { PaymentBookItem } from "@/types/retirement";

// ─── Validation Schema ─────────────────────────────────

const baseSchema = z.object({
    bookType: z.string().default("يدوي"),
    legacyColor: z.string().optional().or(z.literal("")),
    bookNumber: z.string().trim().optional().or(z.literal("")),
    issueDate: z.string().optional().or(z.literal("")),
    expiryDate: z.string().optional().or(z.literal("")),
    changeReason: z.string().optional().or(z.literal("")),
    recipientType: z.string().optional().or(z.literal("")),
    recipientName: z.string().optional().or(z.literal("")),
    autoCreateSalaryRecipient: z.boolean().default(true),
    salaryRecipientType: z.string().optional().or(z.literal("")),
    salaryRecipientName: z.string().optional().or(z.literal("")),
    salaryRecipientID: z.number().optional().nullable(),
    status: z.string().optional().or(z.literal("")),
    notes: z.string().trim().optional().or(z.literal("")),
    bookImage: z.instanceof(File).optional().nullable(),
    issuingBranchID: z.number().optional().nullable(),
});

const bookSchema = baseSchema.refine(data => {
    if (data.bookType === "يدوي" && !data.bookNumber) return false;
    return true;
}, { message: "رقم الدفتر مطلوب للدفاتر اليدوية", path: ["bookNumber"] });

export type BookFormValues = z.infer<typeof bookSchema>;

interface RecipientOption { id: string | number; name: string; }

interface BranchOption { id: number; name: string; }

interface ExistingImage {
    documentID: number;
    filePath: string;
}

export interface PaymentBookSubmitContext {
    paymentBookID?: number;
}

interface PaymentBookDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    /** معرّف الملف — يفعّل التنقل Alt+↑/↓ بين دفاتر الملف (اختياري) */
    fileId?: number;
    onSubmit: (values: BookFormValues, ctx: PaymentBookSubmitContext) => void | Promise<unknown>;
    isLoading?: boolean;
    mode: "create" | "edit";
    /** معرّف الدفتر عند الفتح بالتعديل (يملكه التبويب) */
    editRecordId?: number;
    initialData?: Partial<BookFormValues>;
    existingImages?: ExistingImage[];
    onDeleteImage?: (documentID: number) => void;
    lookups: {
        changeReasons: string[];
        recipientTypes: string[];
        getRecipientOptions: (type: string) => Promise<RecipientOption[]>;
    };
}

const DRAFT_KEY = "payment_book_draft";

const DEFAULT_BOOK_VALUES: BookFormValues = {
    bookType: "يدوي", legacyColor: "أخضر", bookNumber: "", issueDate: "", expiryDate: "",
    changeReason: "", status: "فعال", autoCreateSalaryRecipient: true,
    recipientType: "", recipientName: "", salaryRecipientType: "", salaryRecipientName: "",
    salaryRecipientID: null, notes: "", bookImage: null, issuingBranchID: null,
};

const COLOR_HEX: Record<string, string> = {
    "أبيض": "#ffffff",
    "أخضر": chartColors.success,
    "أزرق": chartColors.primary,
};

function BlobImage({ filePath, alt, className }: { filePath: string; alt?: string; className?: string }) {
    const { blobUrl } = useDocumentBlob(filePath);
    if (!blobUrl) return <div className={cn("bg-muted/20 animate-pulse", className)} />;
    return <img src={blobUrl} alt={alt || ""} className={className} loading="lazy" />;
}

export function PaymentBookDialog({
    open,
    onOpenChange,
    fileId,
    onSubmit,
    isLoading,
    mode,
    editRecordId,
    initialData,
    existingImages = [],
    onDeleteImage,
    lookups,
}: PaymentBookDialogProps) {
    const form = useForm<BookFormValues>({
        resolver: zodResolver(bookSchema) as import("react-hook-form").Resolver<BookFormValues>,
        defaultValues: DEFAULT_BOOK_VALUES,
    });

    const { control, register, setValue, watch, reset, formState: { errors, isDirty } } = form;

    // تحويل صف القائمة إلى قيم النموذج (لا يوجد endpoint لتفاصيل دفتر واحد)
    const toFormValues = useCallback((row: PaymentBookItem): BookFormValues => ({
        ...DEFAULT_BOOK_VALUES,
        bookType: row.bookType || DEFAULT_BOOK_VALUES.bookType,
        legacyColor: row.legacyColor || "",
        bookNumber: row.bookNumber || "",
        issueDate: row.issueDate ? row.issueDate.split("T")[0] : "",
        expiryDate: row.expiryDate ? row.expiryDate.split("T")[0] : "",
        changeReason: row.changeReason || "",
        recipientType: row.recipientType || "",
        recipientName: row.recipientName || "",
        status: row.status || "",
        notes: row.notes || "",
        issuingBranchID: row.issuingBranchID || null,
    }), []);

    // ─── Alt+↑/↓: التنقل بين دفاتر صرف الملف (المنطق المشترك في useRecordNavigation) ───
    const savingRef = useRef(false);
    const { activeRecord, detailReady, effectiveMode } = useRecordNavigation<
        PaymentBookItem,
        PaymentBookItem,
        BookFormValues
    >({
        open: open && fileId != null,
        mode,
        editRecordId: mode === "edit" ? editRecordId : undefined,
        listKey: ["payment-books", fileId],
        fetchList: async () => (await retirementApi.getPaymentBooks(fileId as number)) as unknown as PaymentBookItem[],
        getId: b => b.paymentBookID,
        emptyMessage: "لا يوجد دفاتر صرف مسجلة لهذا الملف",
        emptyDefaults: DEFAULT_BOOK_VALUES,
        toFormValues: record => toFormValues(record as PaymentBookItem),
        form,
        isDirty,
        isBusyRef: savingRef,
        focusField: "bookNumber",
    });

    // معرّف الدفتر المستهدف للحفظ: السجل المتنقَّل إليه، وإلا دفتر فتح التعديل
    const effectiveRecordId = activeRecord?.paymentBookID ?? (mode === "edit" ? editRecordId : undefined);
    // رقم الدفتر المرجعي لفحص التكرار في وضع التعديل
    const baselineBookNumber = activeRecord?.bookNumber ?? initialData?.bookNumber;

    // مزامنة حارس التنقل مع حالة الحفظ القادمة من التبويب
    useEffect(() => {
        savingRef.current = !!isLoading;
    }, [isLoading]);

    useEffect(() => {
        if (open && mode === "create") {
            const saved = sessionStorage.getItem(DRAFT_KEY);
            if (saved) {
                try { reset({ ...DEFAULT_BOOK_VALUES, ...JSON.parse(saved), bookImage: null }); } catch { /* ignore */ }
            }
        }
    }, [open, mode, reset]);

    const allValues = watch();
    useEffect(() => {
        if (open && effectiveMode === "create" && isDirty) {
            const { bookImage: _b, ...textFields } = allValues;
            void _b;
            sessionStorage.setItem(DRAFT_KEY, JSON.stringify(textFields));
        }
    }, [allValues, open, effectiveMode, isDirty]);

    const bookType = useWatch({ control, name: "bookType" });
    const issueDate = useWatch({ control, name: "issueDate" });
    const recipientType = useWatch({ control, name: "recipientType" });
    const salaryRecipientType = useWatch({ control, name: "salaryRecipientType" });
    const autoCreateSalaryRecipient = useWatch({ control, name: "autoCreateSalaryRecipient" });

    const isLegacy = bookType === "يدوي";

    // ─── Duplicate book number check ───
    const [bookNumberError, setBookNumberError] = useState<string | null>(null);
    const [checkingBookNumber, setCheckingBookNumber] = useState(false);

    const checkBookNumberDuplicate = useCallback(async (value: string) => {
        if (!value || !isLegacy) {
            setBookNumberError(null);
            return;
        }
        setCheckingBookNumber(true);
        try {
            const exists = await retirementApi.checkBookNumber(value);
            setBookNumberError(exists ? "رقم الدفتر مسجل مسبقاً" : null);
        } catch {
            setBookNumberError(null);
        } finally {
            setCheckingBookNumber(false);
        }
    }, [isLegacy]);

    useEffect(() => {
        if (!isLegacy && issueDate) {
            const date = new Date(issueDate);
            if (!isNaN(date.getTime())) {
                date.setFullYear(date.getFullYear() + 1);
                setValue("expiryDate", toDateOnlyYemen(date));
            }
        }
    }, [issueDate, setValue, isLegacy]);

    const { data: recipientOptions = [], isLoading: loadingRecipients } = useQuery({
        queryKey: ["payment-book-recipients", recipientType],
        queryFn: () => lookups.getRecipientOptions(recipientType!),
        enabled: open && !!recipientType,
    });

    const { data: salaryRecipientOptions = [], isLoading: loadingSalaryRecipients } = useQuery({
        queryKey: ["salary-recipients-options", salaryRecipientType],
        queryFn: () => lookups.getRecipientOptions(salaryRecipientType!),
        enabled: open && !!salaryRecipientType && autoCreateSalaryRecipient,
    });

    const { data: branches = [], isLoading: loadingBranches } = useQuery({
        queryKey: ["branches-lookup"],
        queryFn: async () => {
            const res = await api.get<{ data: BranchOption[] } | BranchOption[]>("/lookups/branches");
            const data = res.data;
            return Array.isArray(data) ? data : (data?.data || []);
        },
        enabled: open,
    });

    // ─── Submit with duplicate check ───
    const handleSubmitWithCheck = useMemo(() => {
        return form.handleSubmit(
            async (data) => {
                try {
                    // Only check for duplicates if it's a legacy book AND 
                    // (it's create mode OR the book number actually changed during edit)
                    if (isLegacy && data.bookNumber && (effectiveMode === "create" || data.bookNumber !== baselineBookNumber)) {
                        try {
                            const exists = await retirementApi.checkBookNumber(data.bookNumber);
                            if (exists) {
                                setBookNumberError("رقم الدفتر مسجل مسبقاً");
                                return;
                            }
                        } catch (apiError) {
                            // If the endpoint is missing (404) on an older backend or network fails, 
                            // we log it but proceed to avoid blocking the user.
                            console.warn("فشل التحقق من رقم الدفتر، سيتم المتابعة:", apiError);
                        }
                    }
                    await onSubmit(data, {
                        paymentBookID: effectiveMode === "edit" ? effectiveRecordId : undefined,
                    });
                } catch (error) {
                    console.error("خطأ أثناء حفظ الدفتر:", error);
                }
            },
            (validationErrors) => {
                console.warn("أخطاء التحقق:", validationErrors);
            }
        );
    }, [form, onSubmit, isLegacy, effectiveMode, baselineBookNumber, effectiveRecordId]);

    const initialValues = useMemo(() => {
        if (effectiveMode === "create") return DEFAULT_BOOK_VALUES;
        if (!initialData) return undefined;
        return {
            ...DEFAULT_BOOK_VALUES,
            ...initialData,
            issueDate: initialData.issueDate ? initialData.issueDate.split('T')[0] : "",
            expiryDate: initialData.expiryDate ? initialData.expiryDate.split('T')[0] : "",
            issuingBranchID: initialData.issuingBranchID || null,
        } as BookFormValues;
    }, [initialData, effectiveMode]);

    return (
        <BaseFormDialog<BookFormValues>
            open={open}
            onOpenChange={onOpenChange}
            title={effectiveMode === "create" ? (isLegacy ? "إضافة دفتر صرف قديم" : "إصدار دفتر صرف جديد") : "تعديل بيانات دفتر الصرف"}
            description={effectiveMode === "create"
                ? (isLegacy ? "إضافة دفتر يدوي قديم (أبيض/أخضر/أزرق) مع بيانات المستلم." : "سيتم إلغاء تفعيل أي دفتر حالي للملف تلقائياً.")
                : `تعديل بيانات الدفتر رقم: ${baselineBookNumber ?? ""}`}
            size="full"
            isLoading={isLoading || !detailReady}
            isDirty={isDirty}
            onSubmit={handleSubmitWithCheck}
            resetOnOpen={true}
            reset={form.reset}
            initialValues={initialValues || undefined}
            showGlobalError
            formErrors={errors}
            submitLabel={effectiveMode === "create" ? (isLegacy ? "إضافة الدفتر القديم" : "إصدار الدفتر") : "حفظ التعديلات"}
        >
            <div className="space-y-4">

                {/* ── Section 1: نوع الدفتر + البيانات + التواريخ ── */}
                <FormSection title="بيانات الدفتر" icon={BookOpen}>
                    <div className="space-y-4">

                        {/* Radio Cards: نوع الدفتر */}
                        <FormField label="نوع الدفتر" required error={errors.bookType?.message}>
                            <RadioGroup
                                dir="rtl"
                                value={isLegacy ? "يدوي" : "إلكتروني"}
                                onValueChange={(val) => {
                                    if (val === "إلكتروني") {
                                        setValue("bookType", "إلكتروني", { shouldDirty: true, shouldValidate: true });
                                        setValue("legacyColor", "");
                                        setValue("autoCreateSalaryRecipient", true);
                                    } else {
                                        setValue("bookType", "يدوي", { shouldDirty: true, shouldValidate: true });
                                        setValue("changeReason", "");
                                        setValue("expiryDate", "");
                                        setValue("issueDate", "");
                                        setValue("autoCreateSalaryRecipient", false);
                                    }
                                }}
                                className="grid grid-cols-2 gap-3"
                            >
                                <label className={cn(
                                    "flex items-center gap-3 p-3 rounded-lg border-2 cursor-pointer transition-all",
                                    !isLegacy
                                        ? "border-primary bg-primary/5"
                                        : "border-border hover:border-border"
                                )}>
                                    <RadioGroupItem value="إلكتروني" className="sr-only" />
                                    <CreditCard className={cn("w-5 h-5 shrink-0", !isLegacy ? "text-primary" : "text-muted-foreground")} />
                                    <div className="min-w-0">
                                        <p className="text-sm font-medium">إلكتروني</p>
                                        <p className="text-xs text-muted-foreground truncate">بطاقة صرف</p>
                                    </div>
                                </label>

                                <label className={cn(
                                    "flex items-center gap-3 p-3 rounded-lg border-2 cursor-pointer transition-all",
                                    isLegacy
                                        ? "border-primary bg-primary/5"
                                        : "border-border hover:border-border"
                                )}>
                                    <RadioGroupItem value="يدوي" className="sr-only" />
                                    <BookOpen className={cn("w-5 h-5 shrink-0", isLegacy ? "text-primary" : "text-muted-foreground")} />
                                    <div className="min-w-0">
                                        <p className="text-sm font-medium">يدوي</p>
                                        <p className="text-xs text-muted-foreground truncate">دفتر قديم</p>
                                    </div>
                                </label>
                            </RadioGroup>
                        </FormField>

                      
                        <div className="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-4">

                            {/* Color Swatches: لون الدفتر (يظهر فقط إذا يدوي) */}
                            {isLegacy && (
                                <FormField label="لون الدفتر" required error={errors.legacyColor?.message}>
                                    <div className="flex gap-2">
                                        {LEGACY_BOOK_COLORS.map((color) => (
                                            <Button
                                                key={color}
                                                type="button"
                                                variant={form.watch("legacyColor") === color ? "default" : "outline"}
                                                size="sm"
                                                className="h-auto gap-1.5 px-2 py-1.5"
                                                onClick={() => setValue("legacyColor", color, { shouldDirty: true, shouldValidate: true })}
                                            >
                                                <div
                                                    className="w-5 h-5 rounded border border-black"
                                                    style={{ backgroundColor: COLOR_HEX[color] || "#ccc" }}
                                                />
                                                <span className="text-xs text-muted-foreground">{color}</span>
                                            </Button>
                                        ))}
                                    </div>
                                </FormField>
                            )}

                            {/* رقم الدفتر */}
                            {isLegacy ? (
                                <FormField
                                    label="رقم الدفتر"
                                    required
                                    error={errors.bookNumber?.message || bookNumberError || undefined}
                                >
                                    <div className="relative">
                                        <Input
                                            {...register("bookNumber")}
                                            placeholder="أدخل الرقم المطبوع على الدفتر"
                                            autoFocus
                                            className={cn(
                                                "font-bold",
                                                bookNumberError && "border-destructive focus:ring-destructive/20"
                                            )}
                                            onBlur={(e) => {
                                                register("bookNumber").onBlur(e);
                                                checkBookNumberDuplicate(e.target.value);
                                            }}
                                        />
                                        {checkingBookNumber && (
                                            <div className="absolute left-2 top-1/2 -translate-y-1/2">
                                                <div className="w-4 h-4 border-2 border-primary border-t-primary rounded-full animate-spin" />
                                            </div>
                                        )}
                                    </div>
                                </FormField>
                            ) : (
                                <FormField label="رقم الدفتر">
                                    <Input value="سيتم التوليد تلقائياً" disabled className="text-muted-foreground" />
                                </FormField>
                            )}

                            {/* تاريخ الإصدار */}
                            <FormField label="تاريخ الإصدار" error={errors.issueDate?.message}>
                                <Controller
                                    name="issueDate"
                                    control={control}
                                    render={({ field }) => (
                                        <DateInput
                                            id={field.name}
                                            value={field.value || undefined}
                                            onChange={field.onChange}
                                            className="font-mono"
                                        />
                                    )}
                                />
                            </FormField>

                            {/* جهة الإصدار */}
                            <FormField label="جهة الإصدار" error={errors.issuingBranchID?.message}>
                                <Controller<BookFormValues, "issuingBranchID">
                                    name="issuingBranchID"
                                    control={control}
                                    render={({ field }) => (
                                        <NativeSelect
                                            options={branches.map(b => ({ value: b.id.toString(), label: b.name }))}
                                            value={field.value?.toString() || ""}
                                            onChange={(val) => field.onChange(val ? parseInt(val) : null)}
                                            disabled={loadingBranches}
                                            placeholder={loadingBranches ? "جاري التحميل..." : "اختر الفرع..."}
                                        />
                                    )}
                                />
                            </FormField>

                            {/* تاريخ الانتهاء (يظهر فقط إلكتروني) */}
                            {!isLegacy && (
                                <FormField label="تاريخ الانتهاء" required error={errors.expiryDate?.message}>
                                    <Controller
                                        name="expiryDate"
                                        control={control}
                                        render={({ field }) => (
                                            <DateInput
                                                id={field.name}
                                                value={field.value || undefined}
                                                onChange={field.onChange}
                                                className="font-mono"
                                            />
                                        )}
                                    />
                                </FormField>
                            )}
                        </div>

                        {/* سبب الإصدار (يظهر فقط إلكتروني) */}
                        {!isLegacy && (
                            <FormField label="سبب الإصدار" required error={errors.changeReason?.message}>
                                <Controller<BookFormValues, "changeReason">
                                    name="changeReason"
                                    control={control}
                                    render={({ field }) => (
                                        <NativeSelect
                                            options={lookups.changeReasons.map(r => ({ value: r, label: r }))}
                                            value={field.value ?? ""}
                                            onChange={field.onChange}
                                        />
                                    )}
                                />
                            </FormField>
                        )}
                    </div>
                </FormSection>

                {/* ── Section 2: المستلمين ── */}
                <FormSection title="المستلمين" icon={User}>
                    <div className="space-y-4">

                        {/* مستلم الدفتر */}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <FormField label="نوع مستلم الدفتر" error={errors.recipientType?.message}>
                                <Controller<BookFormValues, "recipientType">
                                    name="recipientType"
                                    control={control}
                                    render={({ field }) => (
                                        <NativeSelect
                                            options={lookups.recipientTypes.map(r => ({ value: r, label: r }))}
                                            value={field.value || ""}
                                            onChange={(val) => {
                                                field.onChange(val);
                                                setValue("recipientName", "");
                                            }}
                                        />
                                    )}
                                />
                            </FormField>
                            <FormField label="اسم مستلم الدفتر" error={errors.recipientName?.message}>
                                <Controller<BookFormValues, "recipientName">
                                    name="recipientName"
                                    control={control}
                                    render={({ field }) => (
                                        <NativeSelect
                                            options={recipientOptions.map(o => ({ value: o.name, label: o.name }))}
                                            value={field.value || ""}
                                            onChange={field.onChange}
                                            disabled={!recipientType || loadingRecipients}
                                            placeholder={loadingRecipients ? "جاري التحميل..." : "اختر الاسم..."}
                                        />
                                    )}
                                />
                            </FormField>
                        </div>

                        {/* مستلم المعاش (يظهر فقط في الإنشاء الإلكتروني) */}
                        {!isLegacy && effectiveMode === "create" && (
                            <>
                                <label className="flex items-center gap-2 text-sm font-medium cursor-pointer">
                                    <input
                                        type="checkbox"
                                        {...register("autoCreateSalaryRecipient")}
                                        className="rounded border-border text-primary focus:ring-primary h-4 w-4"
                                    />
                                    إنشاء مستلم معاش جديد وإلغاء المستلم السابق
                                </label>
                                {autoCreateSalaryRecipient && (
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                        <FormField label="نوع مستلم المعاش" error={errors.salaryRecipientType?.message}>
                                            <Controller<BookFormValues, "salaryRecipientType">
                                                name="salaryRecipientType"
                                                control={control}
                                                render={({ field }) => (
                                                    <NativeSelect
                                                        options={lookups.recipientTypes.map(r => ({ value: r, label: r }))}
                                                        value={field.value || ""}
                                                        onChange={(val) => {
                                                            field.onChange(val);
                                                            setValue("salaryRecipientName", "");
                                                            setValue("salaryRecipientID", null);
                                                        }}
                                                    />
                                                )}
                                            />
                                        </FormField>
                                        <FormField label="اسم مستلم المعاش" error={errors.salaryRecipientName?.message}>
                                            <Controller<BookFormValues, "salaryRecipientName">
                                                name="salaryRecipientName"
                                                control={control}
                                                render={({ field }) => (
                                                    <NativeSelect
                                                        options={salaryRecipientOptions.map(o => ({ value: o.name, label: o.name }))}
                                                        value={field.value || ""}
                                                        onChange={(val) => {
                                                            field.onChange(val);
                                                            const opt = salaryRecipientOptions.find(o => o.name === val);
                                                            if (opt) {
                                                                setValue("salaryRecipientID", typeof opt.id === 'string' ? parseInt(opt.id) : opt.id);
                                                            }
                                                        }}
                                                        disabled={!salaryRecipientType || loadingSalaryRecipients}
                                                        placeholder={loadingSalaryRecipients ? "جاري التحميل..." : "اختر مستلم المعاش..."}
                                                    />
                                                )}
                                            />
                                        </FormField>
                                    </div>
                                )}
                            </>
                        )}
                    </div>
                </FormSection>

                {/* ── Section 3: ملاحظات ومرفقات ── */}
                <FormSection title="ملاحظات ومرفقات" icon={FileText}>
                    <div className="space-y-4">
                        <FormField label="صور الدفتر" error={errors.bookImage?.message as string}>
                            <div className="space-y-3">
                                {existingImages.length > 0 && (
                                    <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
                                        {existingImages.map((img) => (
                                            <div key={img.documentID} className="relative group rounded-lg border overflow-hidden">
                                                <BlobImage
                                                    filePath={img.filePath}
                                                    alt="صورة الدفتر"
                                                    className="w-full h-32 object-contain bg-muted/20"
                                                />
                                                {onDeleteImage && (
                                                    <Button
                                                        type="button"
                                                        variant="destructive"
                                                        size="icon"
                                                        className="absolute top-1 left-1 h-6 w-6 opacity-0 group-hover:opacity-100 transition-opacity"
                                                        onClick={() => onDeleteImage(img.documentID)}
                                                        title="حذف الصورة"
                                                    >
                                                        <Trash2 className="w-3 h-3" />
                                                    </Button>
                                                )}
                                            </div>
                                        ))}
                                    </div>
                                )}
                                <FileField<BookFormValues> name="bookImage" control={control} label="إضافة صورة جديدة" size="sm" />
                            </div>
                        </FormField>
                        <FormField label="ملاحظات إضافية" error={errors.notes?.message}>
                            <Textarea
                                {...register("notes")}
                                placeholder="أي ملاحظات حول هذا الإصدار..."
                                rows={1}
                                className="min-h-[36px] max-h-[120px] resize-none overflow-y-auto"
                                onInput={(e) => {
                                    const target = e.target as HTMLTextAreaElement;
                                    target.style.height = "auto";
                                    target.style.height = `${Math.min(target.scrollHeight, 120)}px`;
                                }}
                            />
                        </FormField>
                    </div>
                </FormSection>

            </div>
        </BaseFormDialog>
    );
}
