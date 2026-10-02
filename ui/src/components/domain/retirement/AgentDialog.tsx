import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Calendar, FileText, Search, User, Users, X } from "lucide-react";
import { type MouseEvent,type PointerEvent, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { Controller,type Resolver, useForm } from "react-hook-form";
import { z } from "zod";

import api from "@/api/axios";
import { retirementApi } from "@/api/retirement";
import {
    Badge,
    BaseFormDialog,
    Button,
    DateInput,
    FileField,
    FormField,
    FormSection,
    Input,
    nationalIdSchema,
    RadioGroup,
    RadioGroupItem,
    Text,
    Textarea,
    yemenPhoneSchema,
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { useDebounce } from "@/hooks/useDebounce";
import { focusEnterField, nextEnterIndex } from "@/lib/enterFocus";
import { cn } from "@/lib/utils";
import { useAuthStore, useHasPermission } from "@/stores/authStore";
import type { AgentDto, AgentSearchResult, EntitledPersonSummary } from "@/types/retirement";

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

const createEmptyDefaults: AgentFormValues = {
    hasAgency: "yes",
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
    const canViewEntitled = useHasPermission("EntitledPerson.View");
    const DRAFT_KEY = useMemo(() => `${BASE_DRAFT_KEY}_${userId || "anon"}_${fileId}`, [userId, fileId]);
    const [searchTerm, setSearchTerm] = useState("");
    const debouncedSearch = useDebounce(searchTerm, 400);
    const [showSuggestions, setShowSuggestions] = useState(false);
    const [selectedAgentId, setSelectedAgentId] = useState<number | null>(null);
    const [dropdownPos, setDropdownPos] = useState<{ top: number; left: number; width: number } | null>(null);
    const inputWrapRef = useRef<HTMLDivElement>(null);
    const bodyRef = useRef<HTMLDivElement>(null);

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
        defaultValues: createEmptyDefaults,
    });

    const { control, register, reset, watch, setValue, formState: { errors, isDirty } } = form;

    useEffect(() => {
        if (open && mode === "create") {
            const saved = sessionStorage.getItem(DRAFT_KEY);
            if (saved) {
                try { reset(JSON.parse(saved)); } catch { /* ignore */ }
            }
        }
    }, [open, mode, DRAFT_KEY, reset]);
    const hasAgency = watch("hasAgency");

    const allValues = watch();
    useEffect(() => {
        if (mode === "create" && isDirty) {
            const { personalPhoto: _p, nationalIDDocument: _n, agencyDocument: _a, ...textFields } = allValues;
            void _p; void _n; void _a;
            sessionStorage.setItem(DRAFT_KEY, JSON.stringify(textFields));
        }
    }, [allValues, mode, isDirty]);

    // ─── Fill From Entitled Person ───────────────
    // نفس مفتاح تبويب المستحقين: لا طلب شبكة إضافي إن كان محمّلاً
    const { data: entitledPersons = [], isLoading: loadingEntitled } = useQuery<EntitledPersonSummary[]>({
        queryKey: ["entitled-persons", fileId],
        queryFn: () => retirementApi.getEntitledPersons(fileId),
        enabled: open && mode === "create" && canViewEntitled,
    });

    const getEntitledStatusLabel = (p: EntitledPersonSummary) =>
        p.isDeceased ? "متوفى" : p.isEligible === false ? "غير مؤهل" : "مؤهل";

    const handleFillFromEntitled = (entitledId: number) => {
        const person = entitledPersons.find(p => p.entitledID === entitledId);
        if (!person) return;
        setValue("fullName", person.fullName, { shouldValidate: true, shouldDirty: true });
        setValue("nationalID", person.nationalID || "", { shouldValidate: true, shouldDirty: true });
        setValue("phone", person.phone || "", { shouldValidate: true, shouldDirty: true });
        setSelectedAgentId(null);
        setEntitledListOpen(false);
        setEntitledNavigated(false);
        setShowSuggestions(false);
    };

    const getEntitledBadgeVariant = (p: EntitledPersonSummary): "destructive" | "warning" | "success" =>
        p.isDeceased ? "destructive" : p.isEligible === false ? "warning" : "success";

    // ─── قائمة المستحقين أسفل حقل الاسم (اختيار بالكيبورد) ───
    const [entitledListOpen, setEntitledListOpen] = useState(false);
    const [entitledActiveIndex, setEntitledActiveIndex] = useState(0);
    const [entitledNavigated, setEntitledNavigated] = useState(false);
    const entitledBtnRef = useRef<HTMLButtonElement>(null);

    /**
     * Enter ينقل التركيز للحقل التالي داخل النموذج.
     * RadioGroup (الوكالة موجودة/غير موجودة) حقل منطقي واحد: Enter يخرج منه
     * إلى الحقل التالي. لا نتحقق من `defaultPrevented` لأن Radix RadioGroupItem
     * يستدعي preventDefault على Enter قبل وصول الحدث إلينا.
     * إن لم يوجد حقل تالٍ نُبقي السلوك الافتراضي (إرسال النموذج).
     */
    const handleEnterChain = useCallback(
        (e: React.KeyboardEvent) => {
            if (e.nativeEvent.isComposing) return;

            if (
                e.key !== "Enter" ||
                e.ctrlKey ||
                e.altKey ||
                e.metaKey ||
                e.shiftKey
            ) {
                return;
            }

            const target = e.target as HTMLElement;
            const body = bodyRef.current;
            if (!body) return;

            // حقل الاسم: Enter يتولاه onKeyDown الخاص بحقل الاسم
            // (اختيار من قائمة المستحقين بعد تنقل الأسهم، أو حفظ).
            if ((entitledListOpen || showSuggestions) && target.closest("[data-name-input]")) return;

            // الملاحظات: Enter سطر جديد.
            if (target.tagName === "TEXTAREA") return;

            const radioGroup = target.closest<HTMLElement>('[role="radiogroup"]');
            const keepsNativeEnter =
                !radioGroup &&
                (
                    target.tagName === "BUTTON" ||
                    target.tagName === "SELECT" ||
                    target.tagName === "A" ||
                    target.getAttribute("role") === "button"
                );
            if (keepsNativeEnter) return;

            const nextIndex = nextEnterIndex(body, target);
            if (nextIndex < 0) return;

            e.preventDefault();
            e.stopPropagation();

            focusEnterField(body, nextIndex);
        },
        [entitledListOpen, showSuggestions]
    );

    const entitledFilter = searchTerm.trim();
    const filteredEntitled = entitledFilter
        ? entitledPersons.filter(p =>
            p.fullName.includes(entitledFilter) || (p.nationalID || "").includes(entitledFilter))
        : entitledPersons;
    const safeEntitledIndex = filteredEntitled.length === 0
        ? 0
        : Math.min(entitledActiveIndex, filteredEntitled.length - 1);

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

    // Close lists on outside click
    useEffect(() => {
        const handleClickOutside = (e: globalThis.MouseEvent) => {
            const target = e.target as Node;
            if (inputWrapRef.current && !inputWrapRef.current.contains(target)
                && !(entitledBtnRef.current && entitledBtnRef.current.contains(target))) {
                setShowSuggestions(false);
                setEntitledListOpen(false);
                setEntitledNavigated(false);
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
            setEntitledNavigated(false);
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
            reset({ ...createEmptyDefaults });
            setSelectedAgentId(null);
        }
    };

    const onFormSubmit = (values: AgentFormValues) => {
        return performSubmit(values, false);
    };

    const handleSaveAndNew = () => {
        return form.handleSubmit((values) => {
            return performSubmit(values, true);
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
                                        {agent.phone && <span>{agent.phone}</span>}
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
            <div className="space-y-4" ref={bodyRef} onKeyDown={handleEnterChain}>
                <FormSection title="بيانات الوكيل الأساسية" icon={User}>
                    <div className="grid grid-cols-1 md:grid-cols-6 gap-4">
                        <FormField label="الاسم الكامل للوكيل" required error={errors.fullName?.message} className="md:col-span-3">
                            <div className="flex items-start gap-2">
                            <div className="flex-1 min-w-0" ref={inputWrapRef}>
                                <div className="relative">
                                <Input
                                    {...register("fullName")}
                                    autoFocus
                                    data-name-input
                                    placeholder="الاسم الرباعي واللقب"
                                    autoComplete="off"
                                    aria-expanded={entitledListOpen}
                                    aria-controls="entitled-listbox"
                                    onFocus={() => {
                                        if (mode === "create" && !entitledListOpen && watch("fullName").length >= 2) {
                                            setShowSuggestions(true);
                                        }
                                    }}
                                    onChange={(e) => {
                                        register("fullName").onChange(e);
                                        setSearchTerm(e.target.value);
                                        if (!entitledListOpen) setShowSuggestions(true);
                                        setSelectedAgentId(null);
                                        setEntitledNavigated(false);
                                    }}
                                    onKeyDown={(e) => {
                                        if (!entitledListOpen) return;
                                        if (e.key === "ArrowDown") {
                                            e.preventDefault();
                                            setEntitledNavigated(true);
                                            setEntitledActiveIndex((i) => filteredEntitled.length === 0 ? 0 : (i + 1) % filteredEntitled.length);
                                        } else if (e.key === "ArrowUp") {
                                            e.preventDefault();
                                            setEntitledNavigated(true);
                                            setEntitledActiveIndex((i) => filteredEntitled.length === 0 ? 0 : (i - 1 + filteredEntitled.length) % filteredEntitled.length);
                                        } else if (e.key === "Enter") {
                                            // Enter يختار فقط بعد تحريك الأسهم؛ بدون تنقل يحفظ النموذج.
                                            if (entitledNavigated) {
                                                const target = filteredEntitled[safeEntitledIndex];
                                                if (target) {
                                                    e.preventDefault();
                                                    handleFillFromEntitled(target.entitledID);
                                                    return;
                                                }
                                            }
                                            setEntitledListOpen(false);
                                        } else if (e.key === "Escape") {
                                            setEntitledListOpen(false);
                                            setEntitledNavigated(false);
                                        }
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

                                {mode === "create" && canViewEntitled && entitledListOpen && (
                                    <div
                                        id="entitled-listbox"
                                        role="listbox"
                                        aria-label="قائمة المستحقين — الأسهم للتنقل ثم Enter للاختيار، أو Enter للحفظ"
                                        className="mt-2 max-h-64 overflow-y-auto rounded-xl border bg-card shadow-lg"
                                    >
                                        {loadingEntitled ? (
                                            <div className="p-3 text-center text-sm text-muted-foreground">
                                                <div className="w-4 h-4 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto mb-1" />
                                                جاري تحميل المستحقين...
                                            </div>
                                        ) : filteredEntitled.length === 0 ? (
                                            <div className="p-3 text-center text-sm text-muted-foreground">
                                                {entitledPersons.length === 0
                                                    ? "لا يوجد مستحقون مسجلون لهذا الملف."
                                                    : "لا يوجد مستحق مطابق."}
                                            </div>
                                        ) : (
                                            filteredEntitled.map((p, i) => (
                                                <button
                                                    key={p.entitledID}
                                                    type="button"
                                                    role="option"
                                                    aria-selected={i === safeEntitledIndex}
                                                    onMouseEnter={() => setEntitledActiveIndex(i)}
                                                    onClick={() => handleFillFromEntitled(p.entitledID)}
                                                    className={cn(
                                                        "w-full text-start p-3 border-b border-border/30 last:border-0 transition-colors",
                                                        i === safeEntitledIndex ? "bg-primary/10" : "hover:bg-muted/50"
                                                    )}
                                                >
                                                    <span className="block font-bold truncate">{p.fullName}</span>
                                                    <span className="flex items-center gap-1.5 mt-0.5 text-[11px] text-muted-foreground font-medium">
                                                        <span className="truncate">{p.relationshipName || "مستحق"}</span>
                                                        {p.nationalID && <span className="font-mono" dir="ltr">{p.nationalID}</span>}
                                                        <Badge variant={getEntitledBadgeVariant(p)} className="text-[10px] h-5 shrink-0">
                                                            {getEntitledStatusLabel(p)}
                                                        </Badge>
                                                    </span>
                                                </button>
                                            ))
                                        )}
                                    </div>
                                )}
                            </div>
                            {mode === "create" && canViewEntitled && (
                                <Button
                                    ref={entitledBtnRef}
                                    type="button"
                                    variant="outline"
                                    className="shrink-0 gap-1.5 h-11 sm:h-10"
                                    title="عرض قائمة المستحقين أسفل حقل الاسم"
                                    aria-label="عرض قائمة المستحقين أسفل حقل الاسم"
                                    aria-expanded={entitledListOpen}
                                    aria-controls="entitled-listbox"
                                    onClick={() => {
                                        setShowSuggestions(false);
                                        setEntitledActiveIndex(0);
                                        setEntitledNavigated(false);
                                        setEntitledListOpen((v) => !v);
                                        requestAnimationFrame(() => {
                                            inputWrapRef.current?.querySelector("input")?.focus();
                                        });
                                    }}
                                >
                                    <Users className="w-4 h-4" />
                                    <span className="hidden sm:inline text-xs font-bold">من مستحق</span>
                                </Button>
                            )}
                            </div>
                        </FormField>

                        <FormField label="الرقم الوطني" required error={errors.nationalID?.message} className="md:col-span-2">
                            <Input {...register("nationalID")} placeholder="12xxxxxxxxxx" />
                        </FormField>

                        <FormField label="رقم الهاتف للتواصل" error={errors.phone?.message} className="md:col-span-1">
                            <Input {...register("phone")} placeholder="7xxxxxxx" />
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
                        className="flex w-fit ms-auto items-center gap-6 h-10 border rounded-md px-4 bg-muted/5"
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
                                <Input {...register("agencyNumber")} className="font-bold" placeholder="أدخل رقم الوكالة" />
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
