import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Calendar, FileText, Search, User, X } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState, type PointerEvent, type MouseEvent } from "react";
import { createPortal } from "react-dom";
import { type Resolver, useForm, Controller } from "react-hook-form";
import { z } from "zod";

import api from "@/api/axios";
import { retirementApi } from "@/api/retirement";
import {
    BaseFormDialog,
    Button,
    FileField,
    FormField,
    FormSection,
    Input,
    DateInput,
    nationalIdSchema,
    RadioGroup,
    RadioGroupItem,
    Text,
    Textarea,
    yemenPhoneSchema,
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { useDebounce } from "@/hooks/useDebounce";
import type { AgentDto, AgentSearchResult } from "@/types/retirement";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";

// ─── Validation Schema ─────────────────────────────────

const baseAgentSchema = z.object({
    hasAgency: z.enum(["yes", "no"]),
    fullName: z.string().trim().min(3, "اسم الوكيل مطلوب (3 أحرف على الأقل)"),
    nationalID: nationalIdSchema,
    phone: yemenPhoneSchema.optional().or(z.literal("")),
    agencyNumber: z.string().optional().or(z.literal("")),
    agencyDate: z.string().optional().or(z.literal("")),
    agencyEndDate: z.string().optional().nullable().or(z.literal("")),
    notes: z.string().trim().optional().or(z.literal("")),
    // ملفات
    personalPhoto: z.instanceof(File).optional().nullable(),
    nationalIDDocument: z.instanceof(File).optional().nullable(),
    agencyDocument: z.instanceof(File).optional().nullable(),
}).superRefine((data, ctx) => {
    if (data.hasAgency === "yes") {
        if (!data.agencyNumber || data.agencyNumber.trim().length === 0) {
            ctx.addIssue({ code: z.ZodIssueCode.custom, message: "رقم الوكالة مطلوب عند وجود وكالة", path: ["agencyNumber"] });
        }
        if (!data.agencyDate || data.agencyDate.trim().length === 0) {
            ctx.addIssue({ code: z.ZodIssueCode.custom, message: "تاريخ الوكالة مطلوب عند وجود وكالة", path: ["agencyDate"] });
        }
    }
});

type AgentFormValues = z.infer<typeof baseAgentSchema>;

interface AgentModalProps {
    open: boolean;
    onClose: () => void;
    onSaved?: () => void;
    fileId: number;
    fileNumber?: string;
    initialData?: AgentDto;
    mode: "create" | "edit";
}

const BASE_DRAFT_KEY = "agent_draft";

export function AgentDialog({
    open,
    onClose,
    onSaved,
    fileId,
    fileNumber,
    initialData,
    mode
}: AgentModalProps) {
    const userId = useAuthStore(s => s.user?.id);
    const DRAFT_KEY = useMemo(() => `${BASE_DRAFT_KEY}_${userId || "anon"}_${fileId}`, [userId, fileId]);
    const [searchTerm, setSearchTerm] = useState("");
    const debouncedSearch = useDebounce(searchTerm, 400);
    const [showSuggestions, setShowSuggestions] = useState(false);
    const [selectedAgentId, setSelectedAgentId] = useState<number | null>(null);
    const [dropdownPos, setDropdownPos] = useState<{ top: number; left: number; width: number } | null>(null);
    const inputWrapRef = useRef<HTMLDivElement>(null);

    const mutation = useApiMutation(
        async (formData: FormData) => {
            if (mode === "create") {
                return api.post("/agents", formData, { headers: { "Content-Type": "multipart/form-data" } });
            } else {
                return api.put(`/agents/${initialData?.agentID}`, formData, { headers: { "Content-Type": "multipart/form-data" } });
            }
        },
        {
            invalidateKeys: [["agents"], ["retirement-file", fileId]],
            successMessage: mode === "create" ? "تم إضافة الوكيل بنجاح" : "تم تحديث الوكيل بنجاح",
            onSuccess: () => {
                if (mode === "create") {
                    sessionStorage.removeItem(DRAFT_KEY);
                }
            }
        }
    );

    const form = useForm<AgentFormValues>({
        resolver: zodResolver(baseAgentSchema) as Resolver<z.infer<typeof baseAgentSchema>>,
        defaultValues: useMemo(() => {
            const saved = sessionStorage.getItem(DRAFT_KEY);
            if (mode === "create" && saved) {
                try {
                    return JSON.parse(saved);
                } catch { /* ignore */ }
            }
            return {
                hasAgency: "yes" as const,
                fullName: "",
                nationalID: "",
                phone: "",
                agencyNumber: "",
                agencyDate: "",
                agencyEndDate: "",
                notes: "",
                personalPhoto: null,
                nationalIDDocument: null,
                agencyDocument: null,
            };
        }, [mode])
    });

    const { control, register, reset, watch, setValue, formState: { errors, isDirty } } = form;
    const hasAgency = watch("hasAgency");

    const allValues = watch();
    useEffect(() => {
        if (mode === "create" && isDirty) {
            const { personalPhoto: _p, nationalIDDocument: _n, agencyDocument: _a, ...textFields } = allValues;
            void _p; void _n; void _a;
            sessionStorage.setItem(DRAFT_KEY, JSON.stringify(textFields));
        }
    }, [allValues, mode, isDirty]);

    // ─── Agent Search ──────────────────────────────────
    const { data: searchResults, isLoading: isSearching } = useQuery<AgentSearchResult[]>({
        queryKey: ["agents-search", debouncedSearch],
        queryFn: () => retirementApi.searchAgents(debouncedSearch, 8),
        enabled: showSuggestions && debouncedSearch.length >= 2 && mode === "create",
    });

    const updateDropdownPos = useCallback(() => {
        if (inputWrapRef.current) {
            const rect = inputWrapRef.current.getBoundingClientRect();
            setDropdownPos({ top: rect.bottom + 4, left: rect.left, width: rect.width });
        }
    }, []);

    useEffect(() => {
        if (showSuggestions) updateDropdownPos();
    }, [showSuggestions, updateDropdownPos]);

    // Close suggestions on outside click
    useEffect(() => {
        const handleClickOutside = (e: globalThis.MouseEvent) => {
            const target = e.target as Node;
            if (inputWrapRef.current && !inputWrapRef.current.contains(target)) {
                setShowSuggestions(false);
            }
        };
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    // Reset search state when dialog opens
    useEffect(() => {
        if (open) {
            setSearchTerm("");
            setShowSuggestions(false);
            setSelectedAgentId(null);
        }
    }, [open]);

    const handleSelectAgent = (agent: AgentSearchResult) => {
        setSelectedAgentId(agent.agentID);
        setValue("fullName", agent.fullName, { shouldValidate: true, shouldDirty: true });
        setValue("nationalID", agent.nationalID, { shouldValidate: true, shouldDirty: true });
        setValue("phone", agent.phone || "", { shouldValidate: true, shouldDirty: true });
        setValue("hasAgency", "yes", { shouldDirty: true });
        setValue("agencyNumber", "", { shouldDirty: true });
        setValue("agencyDate", "", { shouldDirty: true });
        setValue("agencyEndDate", "", { shouldDirty: true });
        setValue("notes", "", { shouldDirty: true });
        setSearchTerm("");
        setShowSuggestions(false);
    };

    const handleClearSelection = () => {
        setSelectedAgentId(null);
        setValue("hasAgency", "yes", { shouldDirty: true });
        setValue("fullName", "", { shouldDirty: true });
        setValue("nationalID", "", { shouldDirty: true });
        setValue("phone", "", { shouldDirty: true });
        setValue("agencyNumber", "", { shouldDirty: true });
        setValue("agencyDate", "", { shouldDirty: true });
        setValue("agencyEndDate", "", { shouldDirty: true });
        setValue("notes", "", { shouldDirty: true });
    };

    const performSubmit = async (values: AgentFormValues, keepOpen = false) => {
        const formData = new FormData();
        if (mode === "create") formData.append("fileID", fileId.toString());

        formData.append("fullName", values.fullName);
        formData.append("nationalID", values.nationalID);
        if (values.phone) formData.append("phone", values.phone);

        // Agency fields — only when agency exists
        if (values.hasAgency === "yes") {
            if (values.agencyNumber) formData.append("agencyNumber", values.agencyNumber);
            if (values.agencyDate) formData.append("agencyDate", values.agencyDate + "T00:00:00");
            if (values.agencyEndDate) formData.append("agencyEndDate", values.agencyEndDate + "T00:00:00");
        }

        if (values.notes) formData.append("notes", values.notes);

        if (values.personalPhoto) formData.append("personalPhoto", values.personalPhoto);
        if (values.nationalIDDocument) formData.append("nationalIDDocument", values.nationalIDDocument);
        if (values.hasAgency === "yes" && values.agencyDocument) formData.append("agencyDocument", values.agencyDocument);
        if (mode === "edit" && initialData?.updatedAt) formData.append("updatedAt", initialData.updatedAt);

        await mutation.mutateAsync(formData);

        onSaved?.();
        if (!keepOpen) {
            onClose();
        } else {
            reset();
            setSelectedAgentId(null);
        }
    };

    const onFormSubmit = (values: AgentFormValues) => {
        performSubmit(values, false);
    };

    const handleSaveAndNew = () => {
        form.handleSubmit((values) => {
            performSubmit(values, true);
        })();
    };

    const initialValues = useMemo(() => {
        if (!initialData) return undefined;
        const hasExistingAgency = !!(initialData.agencyNumber && initialData.agencyNumber.trim().length > 0);
        return {
            hasAgency: (hasExistingAgency ? "yes" : "no") as "yes" | "no",
            fullName: initialData.fullName,
            nationalID: initialData.nationalID,
            phone: initialData.phone || "",
            agencyNumber: initialData.agencyNumber || "",
            agencyDate: initialData.agencyDate ? initialData.agencyDate.split('T')[0] : "",
            agencyEndDate: (initialData.agencyEndDate && initialData.agencyEndDate !== "0001-01-01T00:00:00")
                ? initialData.agencyEndDate.split('T')[0]
                : "",
            personalPhoto: null,
            nationalIDDocument: null,
            agencyDocument: null,
            notes: initialData.notes || "",
        };
    }, [initialData]);

    const suggestions = searchResults?.filter(a => a.agentID !== selectedAgentId) ?? [];

    const suggestionsDropdown = showSuggestions && mode === "create" && dropdownPos && createPortal(
        <div
            className="fixed z-dialog bg-card border border-border rounded-xl overflow-hidden"
            style={{ top: dropdownPos.top, left: dropdownPos.left, width: dropdownPos.width, direction: "rtl" }}
        >
            {debouncedSearch.length < 2 ? (
                <div className="p-3 text-center text-sm text-muted-foreground">
                    <Search className="w-4 h-4 mx-auto mb-1 opacity-40" />
                    اكتب حرفين على الأقل للبحث عن وكيل موجود...
                </div>
            ) : isSearching ? (
                <div className="p-3 text-center text-sm text-muted-foreground">
                    <div className="w-4 h-4 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto mb-1" />
                    جاري البحث...
                </div>
            ) : suggestions.length === 0 ? (
                <div className="p-3 text-center text-sm text-muted-foreground">
                    لا يوجد وكيل مطابق. يمكنك إدخال بيانات جديدة.
                </div>
            ) : (
                <div className="max-h-64 overflow-y-auto">
                    <div className="p-2 text-xs text-muted-foreground font-bold border-b">
                        الوكلاء الموجودون ({suggestions.length})
                    </div>
                    {suggestions.map((agent) => (
                        <Button
                            key={agent.agentID}
                            type="button"
                            variant="ghost"
                            className="w-full justify-start p-3 h-auto border-b border-border/30 last:border-0"
                            onPointerDown={(e: PointerEvent) => e.stopPropagation()}
                            onMouseDown={(e: MouseEvent) => e.stopPropagation()}
                            onClick={() => handleSelectAgent(agent)}
                        >
                            <div className="flex items-start gap-3">
                                <div className="w-9 h-9 rounded-full bg-secondary text-secondary-foreground flex items-center justify-center font-bold text-sm shrink-0">
                                    {agent.fullName.charAt(0)}
                                </div>
                                <div className="flex-1 min-w-0 text-start">
                                    <Text variant="body" className="font-bold truncate">{agent.fullName}</Text>
                                    <div className="flex flex-wrap gap-x-3 gap-y-0.5 mt-0.5 text-xs text-muted-foreground">
                                        <span className="font-mono">{agent.nationalID}</span>
                                        {agent.phone && <span className="font-mono">{agent.phone}</span>}
                                    </div>
                                    <div className="flex flex-wrap gap-x-3 gap-y-0.5 mt-0.5 text-xs text-muted-foreground">
                                        <span>وكالة: {agent.agencyNumber}</span>
                                        {agent.fileNumber && <span>ملف: {agent.fileNumber}</span>}
                                    </div>
                                </div>
                            </div>
                        </Button>
                    ))}
                </div>
            )}
        </div>,
        document.body
    );

    return (
        <BaseFormDialog
            isOpen={open}
            onClose={onClose}
            title={mode === "create" ? "تسجيل وكيل جديد" : "تعديل بيانات الوكيل"}
            description={
                mode === "create"
                    ? `إضافة وكيل قانوني لاستلام المستحقات نيابة عن صاحب الملف ${fileNumber ? `(${fileNumber})` : ""}`
                    : `تحديث بيانات الوكيل والوكالة القانونية ${fileNumber ? `(${fileNumber})` : ""}`
            }
            size="full"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(onFormSubmit)}
            onSaveAndNew={mode === "create" ? handleSaveAndNew : undefined}
            resetOnOpen={true}
            reset={form.reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "تسجيل الوكيل" : "حفظ التعديلات"}
        >
            <div className="space-y-4">
                <FormSection title="بيانات الوكيل الأساسية" icon={User}>
                    <div className="grid grid-cols-1 md:grid-cols-6 gap-4">
                        <FormField label="الاسم الكامل للوكيل" required error={errors.fullName?.message} className="md:col-span-3">
                            <div className="relative" ref={inputWrapRef}>
                                <Input
                                    {...register("fullName")}
                                    autoFocus
                                    placeholder="الاسم الرباعي واللقب"
                                    autoComplete="off"
                                    onFocus={() => {
                                        if (mode === "create" && watch("fullName").length >= 2) {
                                            setShowSuggestions(true);
                                        }
                                    }}
                                    onChange={(e) => {
                                        register("fullName").onChange(e);
                                        setSearchTerm(e.target.value);
                                        setShowSuggestions(true);
                                        setSelectedAgentId(null);
                                    }}
                                    className={cn(selectedAgentId && "border-primary/50 bg-primary/5")}
                                />
                                {selectedAgentId && mode === "create" && (
                                    <Button
                                        type="button"
                                        variant="ghost"
                                        size="icon"
                                        className="absolute left-2 top-1/2 -translate-y-1/2 h-6 w-6"
                                        onClick={handleClearSelection}
                                        aria-label="مسح الاختيار"
                                        title="مسح الاختيار"
                                    >
                                        <X className="w-3 h-3" />
                                    </Button>
                                )}
                            </div>
                        </FormField>

                        <FormField label="الرقم الوطني" required error={errors.nationalID?.message} className="md:col-span-2">
                            <Input {...register("nationalID")} className="font-mono font-bold" placeholder="12xxxxxxxxxx" />
                        </FormField>

                        <FormField label="رقم الهاتف للتواصل" error={errors.phone?.message} className="md:col-span-1">
                            <Input {...register("phone")} placeholder="7xxxxxxx" className="font-mono font-bold" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="تفاصيل الوكالة القانونية" icon={Calendar}>
                    <RadioGroup
                        value={hasAgency}
                        onValueChange={(val) => {
                            setValue("hasAgency", val as "yes" | "no", { shouldValidate: true, shouldDirty: true });
                            if (val === "no") {
                                setValue("agencyNumber", "", { shouldDirty: true });
                                setValue("agencyDate", "", { shouldDirty: true });
                                setValue("agencyEndDate", "", { shouldDirty: true });
                            }
                        }}
                        className="flex items-center gap-6 h-10 border rounded-md px-4 bg-muted/5"
                    >
                        <label className="flex items-center gap-2 cursor-pointer">
                            <RadioGroupItem value="yes" className="accent-primary" />
                            <span className="text-sm font-medium">الوكالة موجودة</span>
                        </label>
                        <label className="flex items-center gap-2 cursor-pointer">
                            <RadioGroupItem value="no" className="accent-primary" />
                            <span className="text-sm font-medium text-muted-foreground">الوكالة غير موجودة</span>
                        </label>
                    </RadioGroup>

                    {hasAgency === "yes" && (
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                            <FormField label="رقم الوكالة الرسمية" required error={errors.agencyNumber?.message}>
                                <Input {...register("agencyNumber")} className="font-mono font-bold" placeholder="أدخل رقم الوكالة" />
                            </FormField>

                            <FormField label="تاريخ الصدور" required error={errors.agencyDate?.message}>
                                <Controller
                                    name="agencyDate"
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

                            <FormField label="تاريخ الانتهاء" error={errors.agencyEndDate?.message}>
                                <Controller
                                    name="agencyEndDate"
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

                            <FormField label="ملاحظات الوكالة" error={errors.notes?.message}>
                                <Textarea
                                    {...register("notes")}
                                    placeholder="أي تفاصيل أخرى تتعلق بصحة الوكالة..."
                                    rows={1}
                                    className="leading-relaxed min-h-[36px] max-h-[120px] resize-none overflow-y-auto"
                                    onInput={(e) => {
                                        const target = e.target as HTMLTextAreaElement;
                                        target.style.height = "auto";
                                        target.style.height = `${Math.min(target.scrollHeight, 120)}px`;
                                    }}
                                />
                            </FormField>
                        </div>
                    )}
                </FormSection>

                <FormSection title="الوثائق الثبوتية والمرفقات" icon={FileText}>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <FileField
                            name="personalPhoto"
                            control={control}
                            label="الصورة الشخصية"
                            accept="image/*"
                            size="sm"
                        />
                        <FileField
                            name="agencyDocument"
                            control={control}
                            label="وثيقة الوكالة"
                            size="sm"
                        />
                        <FileField
                            name="nationalIDDocument"
                            control={control}
                            label="الهوية الوطنية"
                            size="sm"
                        />
                    </div>
                </FormSection>
            </div>

            {suggestionsDropdown}
        </BaseFormDialog>
    );
}
