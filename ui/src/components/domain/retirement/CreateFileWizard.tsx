/**
<<<<<<< HEAD

 * CreateFileWizard — 4-step wizard for creating or editing a retirement file.

 */

import { useQuery } from "@tanstack/react-query";

import { AlertTriangle, ArrowLeft, ArrowRight, Briefcase, FileUp, IdCard, MapPin, Save, Search, UserCheck } from "lucide-react";

import { useCallback, useEffect, useLayoutEffect, useRef, useState } from "react";

import { createPortal } from "react-dom";

import { useNavigate } from "react-router-dom";

import api from "@/api/axios";

import { ranksApi } from "@/api/ranks";

import { lookupApi } from "@/api/pension";

import {

    Button,

    FileUploader,

    FormField,

    FormSection,

    Input,

    NativeSelect,

    RadioGroup,

    RadioGroupItem,

    Stepper,

    Text,

    Textarea,

    useConfirm
} from "@/components/ui";

import { DateInput } from "@/components/ui/Input";

import { YEMEN_GOVERNORATES } from "@/constants/retirement";

import { useApiMutation } from "@/hooks";

import { normalizeApiError } from "@/lib/errorUtils";

import {

    DRAFT_SESSION_KEY,

    buildFormData,

    findActiveFile,

    isRetireeStepBlocked,

    isStepValid,

    mapRetireeToForm,

    resolveInitialForm,

    shouldConfirmExplicitCancel,

    shouldOpenExistingFileOnEnter,

    toDraft,

    validateStep,

    type ExistingFile,

    type FormState,

    type RetireeFullData,

} from "@/lib/retirementFileForm";

import { focusEnterField, getEnterFocusables } from "@/lib/enterFocus";
import { normalizeArabicDigits } from "@/lib/utils";

// ─── Types ─────────────────────────────────────────────

interface RetireeSearch {

    retireeID: number;

    fullName: string;

    nationalID: string;

    militaryNumber: string | null;

}

interface LookupItem {

    id: number;

    name: string;

}

interface Props {

    onSuccess: () => void;

    onCancel: () => void;

    onCreated?: (fileId: number) => void;

    initialRetireeId?: number;

    initialRetireeName?: string;

    mode?: "create" | "edit";

    fileId?: number;

    /** يُبلَّغ صعودًا ليتحكم الغلاف في مطالبة التأكيد عند X / Escape / النقر خارجًا. */

    onDirtyChange?: (isDirty: boolean) => void;

}

const steps = [

    { label: "المتقاعد", icon: <UserCheck className="w-4 h-4" />, description: "البحث والاختيار" },

    { label: "البيانات", icon: <IdCard className="w-4 h-4" />, description: "المعلومات الشخصية" },

    { label: "العنوان", icon: <MapPin className="w-4 h-4" />, description: "الاتصال والعنوان" },

    { label: "الوثائق", icon: <FileUp className="w-4 h-4" />, description: "الملفات والحفظ" },

];

const PREFILL_FAILED = "فشل في تحميل بيانات المتقاعد. لا يمكن الحفظ قبل نجاح التحميل — يرجى المحاولة مرة أخرى.";

export function CreateFileWizard({ onSuccess, onCancel, onCreated, initialRetireeId, initialRetireeName, mode = "create", fileId, onDirtyChange }: Props) {

    const { confirm, ConfirmDialogElement } = useConfirm();

    const navigate = useNavigate();

    const firstStep = mode === "edit" ? 1 : 0;

    const [step, setStep] = useState(firstStep);

    const computeInitialForm = useCallback(() => {

        return resolveInitialForm({

            mode,

            initialRetireeId,

            rawDraft: typeof sessionStorage === "undefined" ? null : sessionStorage.getItem(DRAFT_SESSION_KEY),

        });

    }, [mode, initialRetireeId]);

    const [initialFormResult] = useState(computeInitialForm);

    const [form, setForm] = useState<FormState>(initialFormResult.form);

    const [isDirty, setIsDirty] = useState(false);

    const [draftRestored] = useState(initialFormResult.draftRestored);

    const [prefillError, setPrefillError] = useState<string | null>(null);

    /** كل تعديل من المستخدم يمر من هنا: يوسم النموذج كقابل للتلافي ويبلّغ الغلاف. */

    const patchForm = useCallback(

        (updater: (previous: FormState) => FormState) => {

            setForm((prev) => {

                const nextState = updater(prev);

                setIsDirty((wasDirty) => {

                    if (!wasDirty) onDirtyChange?.(true);

                    return true;

                });

                return nextState;

            });

        },

        [onDirtyChange],

    );

    const selectRequestIdRef = useRef(0);

    const isInitialized = useRef(false);

    // حفظ المسودة بعد أول تعديل فقط (لا تُنشأ مسودة بمجرد فتح النافذة)

    useEffect(() => {

        if (mode === "edit" || !isDirty) return;

        sessionStorage.setItem(DRAFT_SESSION_KEY, JSON.stringify(toDraft(form)));

    }, [form, mode, isDirty]);

    // إعادة الضبط الدقيقة عند تغير ملف/متقاعد/وضع

    useEffect(() => {

        const init = computeInitialForm();

        setForm(init.form);

        setIsDirty(false);

        onDirtyChange?.(false);

        setPrefillError(null);

        setStep(firstStep);

        setExistingFile(null);

        setExistingCheckFailed(false);

        isInitialized.current = false;

    }, [fileId, initialRetireeId, mode, firstStep, computeInitialForm, onDirtyChange]);

    const [retireeSearch, setRetireeSearch] = useState("");

    const [retireeResults, setRetireeResults] = useState<RetireeSearch[]>([]);

    const [searchingRetirees, setSearchingRetirees] = useState(false);

    const [errors, setErrors] = useState<Record<string, string>>({});

    const searchContainerRef = useRef<HTMLDivElement>(null);

    const [dropdownPos, setDropdownPos] = useState<{ top: number; left: number; width: number } | null>(null);

    const [highlightedIndex, setHighlightedIndex] = useState(-1);

    const dropdownRef = useRef<HTMLDivElement>(null);

    const [existingFile, setExistingFile] = useState<ExistingFile | null>(null);

    const [existingCheckFailed, setExistingCheckFailed] = useState(false);

    const [checkingExistingFile, setCheckingExistingFile] = useState(false);

    // ─── Queries ───────────────────────────────────────

    const { data: socialStatuses = [] } = useQuery({

        queryKey: ["lookups", "social-statuses"],

        queryFn: async () => {

            const res = await api.get<{ data: LookupItem[] }>("/lookups/social-statuses");

            return res.data.data || [];

        },

        staleTime: 5 * 60 * 1000,

    });

    const { data: healthStatuses = [] } = useQuery({

        queryKey: ["lookups", "health-statuses"],

        queryFn: async () => {

            const res = await api.get<{ data: LookupItem[] }>("/lookups/health-statuses");

            return res.data.data || [];

        },

        staleTime: 5 * 60 * 1000,

    });

    const { data: ranks = [] } = useQuery({

        queryKey: ["lookups", "ranks"],

        queryFn: () => ranksApi.getAll(),

        enabled: mode === "edit",

        staleTime: 5 * 60 * 1000,

    });

    const { data: branches = [] } = useQuery({

        queryKey: ["lookups", "branches"],

        queryFn: () => lookupApi.getBranches(),

        enabled: mode === "edit",

        staleTime: 5 * 60 * 1000,

    });

    const { data: prefillData, isPending: prefillPending, isError: prefillFailed, refetch: refetchPrefill } = useQuery({

        queryKey: ["retiree-full-data", initialRetireeId, fileId],

        queryFn: async ({ signal }) => {

            const fullDataRes = await api.get<{ data: RetireeFullData }>(`/retirees/${initialRetireeId}/full-data`, { signal });

            return fullDataRes.data.data;

        },

        enabled: !!initialRetireeId,

        retry: false,

        staleTime: 0,

    });

    // استعلام مدمج وموحد لبيانات الملف وملاحظاته في وضع التعديل

    const { data: editFileData, isPending: editFilePending, isError: editFileFailed, refetch: refetchEditFile } = useQuery({

        queryKey: ["retirement-file-edit-data", fileId],

        queryFn: async ({ signal }) => {

            const res = await api.get<{ data: { notes?: string | null; rankID?: number | null; branchID?: number | null } }>(`/retirement-files/${fileId}`, { signal });

            return res.data.data;

        },

        enabled: mode === "edit" && !!fileId,

        retry: false,

        staleTime: 0,

    });

    // مزامنة تهيئة البيانات بدون جعل النموذج Dirty

    useEffect(() => {

        if (isInitialized.current) return;

        if (isDirty) return;

        if (mode === "edit") {

            if (!prefillData || !editFileData) return;

            setForm((f) => ({

                ...f,

                ...mapRetireeToForm(prefillData, editFileData.notes ?? f.notes, initialRetireeName),

                rankID: editFileData.rankID != null ? String(editFileData.rankID) : f.rankID,

                branchID: editFileData.branchID != null ? String(editFileData.branchID) : f.branchID,

            }));

            setPrefillError(null);

            isInitialized.current = true;

        } else if (mode === "create" && initialRetireeId) {

            if (!prefillData) return;

            setForm((f) => ({

                ...f,

                ...mapRetireeToForm(prefillData, f.notes, initialRetireeName),

            }));

            setPrefillError(null);

            isInitialized.current = true;

        }

    }, [prefillData, editFileData, initialRetireeName, initialRetireeId, mode, isDirty]);

    useEffect(() => {

        if (mode === "create" && initialRetireeId && prefillFailed) {

            setPrefillError(PREFILL_FAILED);

        } else if (mode === "edit" && (prefillFailed || editFileFailed)) {

            setPrefillError(PREFILL_FAILED);

        }

    }, [prefillFailed, editFileFailed, initialRetireeId, mode]);

    const retryPrefill = useCallback(() => {

        void refetchPrefill();

        if (mode === "edit") {

            void refetchEditFile();

        }

    }, [refetchPrefill, refetchEditFile, mode]);

    /** حارس التحقق من وجود ملف موجود للمتقاعد. */

    const checkExistingFile = useCallback(async (retireeId: number): Promise<{ active: ExistingFile | null; failed: boolean }> => {

        try {

            const res = await api.get<{ data: ExistingFile[] }>(`/retirement-files/by-retiree/${retireeId}`);

            return { active: findActiveFile(res.data.data ?? []) ?? null, failed: false };

        } catch {

            return { active: null, failed: true };

        }

    }, []);

    // فحص وجود ملف سابق للمتقاعد في وضع الإنشاء عند تمرير initialRetireeId

    useEffect(() => {

        if (mode !== "create" || !initialRetireeId) return;

        let isSubscribed = true;

        setCheckingExistingFile(true);

        checkExistingFile(initialRetireeId).then((fileCheck) => {

            if (!isSubscribed) return;

            setExistingFile(fileCheck.failed ? null : fileCheck.active);

            setExistingCheckFailed(fileCheck.failed);

            setCheckingExistingFile(false);

        });

        return () => {

            isSubscribed = false;

        };

    }, [mode, initialRetireeId, checkExistingFile]);

    const createMutation = useApiMutation(

        (data: FormData) => {

            if (mode === "edit") {

                if (!fileId) throw new Error("رقم الملف مطلوب لتعديل البيانات");

                return api.put(`/retirement-files/${fileId}`, data, { headers: { "Content-Type": "multipart/form-data" } });

            }

            return api.post("/retirement-files", data, { headers: { "Content-Type": "multipart/form-data" } });

        },

        {

            successMessage: mode === "edit" ? "تم تحديث الملف بنجاح" : "تم إنشاء الملف بنجاح",

            onSuccess: (response: { data?: { data?: { fileID?: number; FileID?: number; id?: number }; fileID?: number; FileID?: number; id?: number }; fileID?: number; FileID?: number; id?: number }) => {

                sessionStorage.removeItem(DRAFT_SESSION_KEY);

                onDirtyChange?.(false);

                const payload = response?.data ?? response;

                const newFileId = payload?.data?.fileID ?? payload?.data?.FileID ?? payload?.data?.id;

                if (mode === "create" && newFileId != null) onCreated?.(newFileId);

                onSuccess();

            },

        }

    );

    useEffect(() => {

        if (retireeSearch.length < 2) {

            setRetireeResults([]);

            setSearchingRetirees(false);

            return;

        }

        const controller = new AbortController();

        const timer = setTimeout(async () => {

            setSearchingRetirees(true);

            try {
                const normalizedSearch = normalizeArabicDigits(retireeSearch.trim());
                const res = await api.get<{ data: { items: RetireeSearch[] } }>("/retirees", {
                    params: { search: normalizedSearch, page: 1, pageSize: 10 },
                    signal: controller.signal,
                });

                if (controller.signal.aborted) return;

                setRetireeResults(res.data.data.items || []);

            } catch (err) {

                if (controller.signal.aborted) return;

                setRetireeResults([]);

                setErrors((prev) => ({

                    ...prev,

                    retireeSearch: normalizeApiError(err).message || "تعذّر البحث، حاول مرة أخرى",

                }));

            } finally {

                if (!controller.signal.aborted) setSearchingRetirees(false);

            }

        }, 400);

        return () => {

            clearTimeout(timer);

            controller.abort();

        };

=======
 * CreateFileWizard — 4-step wizard for creating or editing a retirement file.
 */

import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, ArrowLeft, ArrowRight, FileUp,IdCard, MapPin, Save, Search, UserCheck } from "lucide-react";
import { useCallback, useEffect, useLayoutEffect, useRef,useState } from "react";
import { createPortal } from "react-dom";

import api from "@/api/axios";
import {
    Button,
    FileUploader,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    RadioGroup,
    RadioGroupItem,
    Stepper,
    Text,
    Textarea,
    useConfirm} from "@/components/ui";
import { DateInput } from "@/components/ui/Input";
import { useApiMutation } from "@/hooks";
import { normalizeArabicDigits } from "@/lib/utils";

// ... (Types) ...

// ─── Mapping Helper ────────────────────────────────────

function mapRetireeToForm(fullData: RetireeFullData, currentNotes: string, initialName?: string): Partial<FormState> {
    return {
        retireeID: fullData.retireeID,
        retireeDisplayName: initialName 
            ? `${initialName} — ${fullData.nationalID}` 
            : `${fullData.fullName} — ${fullData.nationalID}`,
        nationalID: fullData.nationalID || "",
        gender: fullData.gender?.toString() || "1",
        birthDate: fullData.birthDate ? new Date(fullData.birthDate).toISOString().split("T")[0] : "",
        socialStatusID: fullData.socialStatusID?.toString() || "2",
        healthStatusID: fullData.healthStatusID?.toString() || "1",
        isAlive: fullData.isAlive ?? true,
        deathDate: fullData.deathDate ? new Date(fullData.deathDate).toISOString().split("T")[0] : "",
        governorate: fullData.governorate || "",
        city: fullData.city || "",
        address: fullData.address || "",
        phone: fullData.phone || "",
        notes: currentNotes,
    };
}


// ─── Types ─────────────────────────────────────────────

interface RetireeSearch {
    retireeID: number;
    fullName: string;
    nationalID: string;
    militaryNumber: string | null;
}

interface ExistingFile {
    fileID: number;
    fileNumber: string;
    statusID: number | null;
    fileStatusName: string | null;
}

const CLOSED_STATUS_ID = 4;

interface LookupItem {
    id: number;
    name: string;
}

interface RetireeFullData {
    retireeID: number;
    fullName: string;
    nationalID: string;
    militaryNumber: string | null;
    gender: number;
    birthDate: string | null;
    phone: string | null;
    address: string | null;
    city: string | null;
    governorate: string | null;
    socialStatusID: number | null;
    healthStatusID: number | null;
    isAlive: boolean;
    deathDate: string | null;
    personalPhotoPath: string | null;
}

interface Props {
    onSuccess: () => void;
    onCancel: () => void;
    onCreated?: (fileId: number) => void;
    initialRetireeId?: number;
    initialRetireeName?: string;
    mode?: "create" | "edit";
    fileId?: number;
}

// ─── Form State ────────────────────────────────────────

interface FormState {
    retireeID: number;
    retireeDisplayName: string;
    nationalID: string;
    gender: string;
    birthDate: string;
    socialStatusID: string;
    healthStatusID: string;
    isAlive: boolean;
    deathDate: string;
    governorate: string;
    city: string;
    address: string;
    phone: string;
    personalPhoto: File | null;
    idDocument: File | null;
    primaryDocument: File | null;
    deathCertificate: File | null;
    notes: string;
}

// ─── Constants ──────────────────────────────────────────
const DRAFT_SESSION_KEY = "retirement_file_draft";

const YEMEN_GOVERNORATES = [
    "أمانة العاصمة", "عدن", "تعز", "الحديدة", "إب", "ذمار", "حجة",
    "صعدة", "عمران", "المحويت", "ريمة", "صنعاء", "البيضاء", "لحج",
    "أبين", "الضالع", "شبوة", "حضرموت", "المهرة", "سقطرى", "مأرب", "الجوف"
];

const initialForm: FormState = {
    retireeID: 0,
    retireeDisplayName: "",
    nationalID: "",
    gender: "1", // ذكر
    birthDate: "",
    socialStatusID: "2", // متزوج كقيمة افتراضية
    healthStatusID: "1", // سليم كقيمة افتراضية
    isAlive: true,
    deathDate: "",
    governorate: "",
    city: "",
    address: "",
    phone: "",
    personalPhoto: null,
    idDocument: null,
    primaryDocument: null,
    deathCertificate: null,
    notes: "",
};

const steps = [
    { label: "المتقاعد", icon: <UserCheck className="w-4 h-4" />, description: "البحث والاختيار" },
    { label: "البيانات", icon: <IdCard className="w-4 h-4" />, description: "المعلومات الشخصية" },
    { label: "العنوان", icon: <MapPin className="w-4 h-4" />, description: "الاتصال والعنوان" },
    { label: "الوثائق", icon: <FileUp className="w-4 h-4" />, description: "الملفات والحفظ" },
];

export function CreateFileWizard({ onSuccess, onCancel, onCreated, initialRetireeId, initialRetireeName, mode = "create", fileId }: Props) {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const [step, setStep] = useState(mode === "edit" ? 1 : 0);
    const [form, setForm] = useState<FormState>(() => {
        const saved = sessionStorage.getItem(DRAFT_SESSION_KEY);
        if (mode === "create" && saved) {
            try {
                const draft = JSON.parse(saved);
                
                // --- حل مشكلة تعارض المسودة مع المتقاعد المختار ---
                // إذا كانت المسودة تخص متقاعداً مختلفاً عن الذي اخترناه الآن من الصفحة
                // نقوم بتصفير بيانات الهوية في المسودة لإجبار النظام على جلب بيانات المتقاعد الجديد
                if (initialRetireeId && draft.retireeID !== initialRetireeId) {
                    draft.retireeID = 0;
                    draft.retireeDisplayName = "";
                }

                const merged = { ...initialForm, ...draft };
                if (!draft.socialStatusID) merged.socialStatusID = initialForm.socialStatusID;
                if (!draft.healthStatusID) merged.healthStatusID = initialForm.healthStatusID;
                if (!draft.gender) merged.gender = initialForm.gender;
                return merged;
            } catch {
                return initialForm;
            }
        }
        return initialForm;
    });


    // Save draft to sessionStorage on changes
    useEffect(() => {
        if (mode === "edit") return; // Don't save draft in edit mode
        const { personalPhoto: _p, idDocument: _i, primaryDocument: _pr, deathCertificate: _d, ...textFields } = form;
        void _p; void _i; void _pr; void _d;
        sessionStorage.setItem(DRAFT_SESSION_KEY, JSON.stringify(textFields));
    }, [form, mode]);

    const [retireeSearch, setRetireeSearch] = useState("");
    const [retireeResults, setRetireeResults] = useState<RetireeSearch[]>([]);
    const [searchingRetirees, setSearchingRetirees] = useState(false);
    const [errors, setErrors] = useState<Record<string, string>>({});
    const searchContainerRef = useRef<HTMLDivElement>(null);
    const [dropdownPos, setDropdownPos] = useState<{ top: number; left: number; width: number } | null>(null);
    const [highlightedIndex, setHighlightedIndex] = useState(-1);
    const dropdownRef = useRef<HTMLDivElement>(null);
    const [existingFile, setExistingFile] = useState<ExistingFile | null>(null);
    const [checkingExistingFile, setCheckingExistingFile] = useState(false);

    // ─── Queries ───────────────────────────────────────
    const { data: socialStatuses = [] } = useQuery({
        queryKey: ["lookups", "social-statuses"],
        queryFn: async () => {
            const res = await api.get<{ data: LookupItem[] }>("/lookups/social-statuses");
            return res.data.data || [];
        },
        staleTime: 5 * 60 * 1000, // 5 minutes
    });

    const { data: healthStatuses = [] } = useQuery({
        queryKey: ["lookups", "health-statuses"],
        queryFn: async () => {
            const res = await api.get<{ data: LookupItem[] }>("/lookups/health-statuses");
            return res.data.data || [];
        },
        staleTime: 5 * 60 * 1000, // 5 minutes
    });

    // Fetch initial retiree data if provided
    const { data: prefillData } = useQuery({
        queryKey: ["retiree-full-data", initialRetireeId, fileId],
        queryFn: async () => {
            const fullDataRes = await api.get<{ data: RetireeFullData }>(`/retirees/${initialRetireeId}/full-data`);
            const fullData = fullDataRes.data.data;
            
            let notes = "";
            if (mode === "edit" && fileId) {
                try {
                    const fileRes = await api.get<{ data: { notes: string } }>(`/retirement-files/${fileId}`);
                    notes = fileRes.data.data.notes || "";
                } catch { /* ignore */ }
            }
            return { fullData, notes };
        },
        enabled: !!initialRetireeId && (form.retireeID === 0 || mode === "edit"),
        staleTime: 5 * 60 * 1000, // 5 minutes
    });

    const isInitialized = useRef(false);
    useEffect(() => {
        if (prefillData && !isInitialized.current) {
            setForm((f) => ({
                ...f,
                ...mapRetireeToForm(prefillData.fullData, prefillData.notes || f.notes, initialRetireeName)
            }));
            isInitialized.current = true;
        }
    }, [prefillData, initialRetireeName]);


    const createMutation = useApiMutation(
        (data: FormData) => {
            if (mode === "edit" && fileId) {
                return api.put(`/retirement-files/${fileId}`, data, { headers: { "Content-Type": "multipart/form-data" } });
            }
            return api.post("/retirement-files", data, { headers: { "Content-Type": "multipart/form-data" } });
        },
        {
            successMessage: mode === "edit" ? "تم تحديث الملف بنجاح" : "تم إنشاء الملف بنجاح",
            onSuccess: (response: any) => {
                sessionStorage.removeItem(DRAFT_SESSION_KEY);
                // AxiosResponse: response.data = API wrapper, response.data.data = payload
                const payload = response?.data ?? response;
                const newFileId = payload?.data?.FileID ?? payload?.data?.fileID ?? payload?.data?.id;
                if (mode === "create" && newFileId) {
                    onCreated?.(newFileId);
                }
                onSuccess();
            },
        }
    );

    useEffect(() => {
        if (retireeSearch.length < 2) {
            setRetireeResults([]);
            return;
        }
        const timer = setTimeout(async () => {
            setSearchingRetirees(true);
            try {
                const res = await api.get<{ data: { items: RetireeSearch[] } }>("/retirees", {
                    params: { search: retireeSearch, page: 1, pageSize: 10 },
                });
                setRetireeResults(res.data.data.items || []);
            } catch {
                setRetireeResults([]);
            } finally {
                setSearchingRetirees(false);
            }
        }, 400);
        return () => clearTimeout(timer);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }, [retireeSearch]);

    const showDropdown = (retireeResults.length > 0 || searchingRetirees);

    useEffect(() => {
<<<<<<< HEAD

        setHighlightedIndex(-1);

    }, [retireeResults]);

    /** إغلاق القائمة عند النقر خارج حقل البحث أو القائمة نفسها. */

    useEffect(() => {

        if (!showDropdown) return;

        const onPointerDown = (event: MouseEvent) => {

            const target = event.target as Node;

            if (searchContainerRef.current?.contains(target)) return;

            if (dropdownRef.current?.contains(target)) return;

            setRetireeResults([]);

            setHighlightedIndex(-1);

        };

        document.addEventListener("mousedown", onPointerDown);

        return () => document.removeEventListener("mousedown", onPointerDown);

    }, [showDropdown]);

    useLayoutEffect(() => {

        if (!showDropdown || !searchContainerRef.current) {

            setDropdownPos(null);

            return;

        }

        const updatePos = () => {

            if (!searchContainerRef.current) return;

            const rect = searchContainerRef.current.getBoundingClientRect();

            setDropdownPos({ top: rect.bottom + 4, left: rect.left, width: rect.width });

        };

        updatePos();

        window.addEventListener("resize", updatePos);

        window.addEventListener("scroll", updatePos, true);

        return () => {

            window.removeEventListener("resize", updatePos);

            window.removeEventListener("scroll", updatePos, true);

        };

    }, [showDropdown]);

    const selectRetiree = useCallback(

        async (r: RetireeSearch) => {

            const currentReqId = ++selectRequestIdRef.current;

            patchForm((f) => ({

                ...f,

                retireeID: r.retireeID,

                retireeDisplayName: `${r.fullName} — ${r.nationalID}`,

            }));

            setRetireeSearch("");

            setRetireeResults([]);

            setErrors({});

            setExistingFile(null);

            setExistingCheckFailed(false);

            setCheckingExistingFile(true);

            try {

                const [fullDataRes, fileCheck] = await Promise.all([

                    api.get<{ data: RetireeFullData }>(`/retirees/${r.retireeID}/full-data`),

                    checkExistingFile(r.retireeID),

                ]);

                if (selectRequestIdRef.current !== currentReqId) return;

                setExistingFile(fileCheck.failed ? null : fileCheck.active);

                setExistingCheckFailed(fileCheck.failed);

                patchForm((f) => ({ ...f, ...mapRetireeToForm(fullDataRes.data.data, f.notes) }));

            } catch (err) {

                if (selectRequestIdRef.current !== currentReqId) return;

                setErrors((prev) => ({

                    ...prev,

                    retireeSearch: normalizeApiError(err).message || "فشل في تحميل بيانات المتقاعد. يرجى المحاولة مرة أخرى.",

                }));

                patchForm((f) => ({ ...f, retireeID: 0, retireeDisplayName: "" }));

            } finally {

                if (selectRequestIdRef.current === currentReqId) {

                    setCheckingExistingFile(false);

                }

            }

        },

        [checkExistingFile, patchForm],

    );

    const handleSearchKeyDown = useCallback(
        (e: React.KeyboardEvent) => {
            // Enter أثناء تركيب النص (IME) يؤكد الحرف — لا نعترضه ولا نمنعه.
            if (e.nativeEvent.isComposing) return;

            // Enter داخل البحث مخصص لاختيار المتقاعد فقط ولا يجب أن يصل لمعالج النموذج العام.
            if (e.key === "Enter") {
                if (retireeResults.length === 0) {
                    // لا نتائج: نمرّر الحدث للمعالج العام ليتقدّم بالخطوة أو يفسّر الخطوة.
                    return;
                }

                e.preventDefault();
                e.stopPropagation();

                const index = highlightedIndex >= 0 ? highlightedIndex : 0;
                const selected = retireeResults[index];

                if (selected) {
                    void selectRetiree(selected);
                }
                return;
            }

            if (e.key === "Escape") {
                if (!showDropdown) return;

                e.preventDefault();
                e.stopPropagation();
                setRetireeResults([]);
                setHighlightedIndex(-1);
                return;
            }

            if (!showDropdown) return;

            if (e.key === "ArrowDown") {
                e.preventDefault();
                e.stopPropagation();
                setHighlightedIndex((prev) => (prev < retireeResults.length - 1 ? prev + 1 : 0));
                return;
            }

            if (e.key === "ArrowUp") {
                e.preventDefault();
                e.stopPropagation();
                setHighlightedIndex((prev) => (prev > 0 ? prev - 1 : retireeResults.length - 1));
            }
        },
        [showDropdown, retireeResults, highlightedIndex, selectRetiree],
    );


    useEffect(() => {

        if (highlightedIndex < 0 || !dropdownRef.current) return;

        const items = dropdownRef.current.querySelectorAll('[data-dropdown-item]');

        items[highlightedIndex]?.scrollIntoView({ block: "nearest" });

    }, [highlightedIndex]);

    const prefillLoading =

        (!!initialRetireeId && prefillPending) ||

        (mode === "edit" && !!fileId && (editFilePending || prefillPending));

    const prefillBlocked = prefillError !== null || prefillLoading || (mode === "edit" && editFileFailed);

    const effectivePrefillError =

        prefillError ??

        (prefillLoading ? "جاري تحميل بيانات المتقاعد…" : null) ??

        (editFileFailed ? "تعذّر تحميل ملاحظات وبيانات الملف. أعد المحاولة قبل الحفظ." : null);

    const runValidation = useCallback(

        (s: number): boolean => {

            const stepErrors = validateStep(form, s, {

                existingFile,

                existingCheckFailed,

                prefillError: effectivePrefillError,

                lifeStatusReadOnly: mode === "edit",

            });

            setErrors((prev) => ({ ...prev, ...stepErrors, prefill: stepErrors.prefill ?? "" }));

            return isStepValid(stepErrors);

        },

        [form, existingFile, existingCheckFailed, effectivePrefillError, mode],

    );

    const containerRef = useRef<HTMLDivElement>(null);

    const rootRef = useRef<HTMLDivElement>(null);

    /**
     * ينقل التركيز للحقل رقم index بعد تهدئة التحديث.
     * يُعاد الاستعلام من DOM عند التنفيذ لا من العقدة المخزّنة،
     * لأن re-render بين المفتاح والتنفيذ يفصل العقدة.
     */
    const focusFieldAt = useCallback((index: number) => {
        const container = containerRef.current;
        if (!container) return;

        focusEnterField(container, index);
    }, []);

    const next = useCallback(() => {

        if (runValidation(step)) setStep((s) => Math.min(s + 1, steps.length - 1));

    }, [step, runValidation]);

    const prev = useCallback(() => setStep((s) => Math.max(s - 1, firstStep)), [firstStep]);

    const handleCancelRequest = async () => {

        if (shouldConfirmExplicitCancel(mode, form, step, firstStep)) {

            const confirmed = await confirm({

                title: mode === "edit" ? "التراجع عن التعديلات" : "التراجع عن إنشاء الملف",

                description: "هل أنت متأكد؟ سيتم حذف البيانات المدخلة والمسودة المحفوظة.",

                confirmLabel: "نعم، إلغاء",

                cancelLabel: "تراجع",

                variant: "danger",

            });

            if (!confirmed) return;

        }

        // المسودة تُحذف مع الإلغاء الصريح — للإغلاق العرضي تبقى للاستئناف

        sessionStorage.removeItem(DRAFT_SESSION_KEY);

        onDirtyChange?.(false);

        onCancel();

    };

    const handleSubmit = useCallback(() => {

        if (!runValidation(step)) return;

        if (prefillBlocked) return;

        createMutation.mutate(buildFormData(form, mode === "create"));

    }, [step, runValidation, prefillBlocked, createMutation, form, mode]);

    /** Enter على متقاعد لديه ملف موجود: افتح تفاصيل الملف بدل إظهار خطأ منع المتابعة. */
    const openExistingFile = useCallback(() => {

        if (!existingFile) return;

        // تدفق الإنشاء متروك: تُحذف المسودة لئلا تُستأنف مع تنبيه «ملف موجود» في المرة القادمة.
        sessionStorage.removeItem(DRAFT_SESSION_KEY);

        onDirtyChange?.(false);

        navigate(`/files/${existingFile.fileID}`);

    }, [existingFile, navigate, onDirtyChange]);

    /**
     * Enter = الانتقال للحقل التالي داخل المعالج.
     *
     * المستمع على document (bubble) وليس على div الجذر: بعد اختيار المتقاعد
     * لا يتبقّى في الخطوة أي حقل، فيقع التركيز على body ولا يصل الحدث إلى
     * div الجذر إطلاقًا.
     */
    const handleEnterChain = useCallback(
        (e: KeyboardEvent) => {
            // Enter أثناء تركيب النص (IME) يؤكد الحرف العربي؛ لا نشغّل السلسلة وقتها.
            if (e.isComposing) return;

            if (
                e.key !== "Enter" ||
                e.ctrlKey ||
                e.altKey ||
                e.metaKey ||
                e.shiftKey
            ) {
                return;
            }

            const root = rootRef.current;
            if (!root) return;

            const target = e.target as HTMLElement | null;

            // الهدف إما داخل المعالج أو body (لا يوجد تركيز)، وكلاهما معنا.
            // أي هدف آخر — نافذة تأكيد، رافع ملفات، حقل خارج المعالج — يُترك.
            if (target && target !== document.body && !root.contains(target)) return;

            // الحقول التي تدير Enter بنفسها تُعلن ذلك بسمة صريحة.
            // لا نستخدم `e.defaultPrevented` كمؤشر: Radix RadioGroupItem
            // يستدعي preventDefault على Enter (لمنع إرسال النموذج) قبل وصول
            // الحدث إلينا، فكان الحارس يوقف انتقال التركيز داخل RadioGroup.
            if (target?.closest("[data-enter-handled]")) return;

            // Enter داخل الملاحظات يبقى سطرًا جديدًا طبيعيًا.
            if (target?.tagName === "TEXTAREA") return;

            // عناصر نتائج البحث تدير Enter بنفسها.
            if (target?.closest("[data-dropdown-item]")) return;

            const radioGroup = target?.closest<HTMLElement>('[role="radiogroup"]') ?? null;

            // الأزرار والروابط تحتفظ بسلوك Enter الافتراضي.
            // SELECT مستثنى: نتولى انتقاله هنا عبر المنطق الموحد، إلا إذا كانت فارغة
            // (يستدعي NativeSelect.onKeyDown الفتح natively حتى بعد إضافة preventDefault هنا).
            // RadioGroup مستثنى حتى نعامل المجموعة كحقل منطقي واحد.
            const keepsNativeEnter =
                !radioGroup &&
                !!target &&
                (
                    target.tagName === "BUTTON" ||
                    target.tagName === "A" ||
                    target.getAttribute("role") === "button"
                );

            // SELECT بقيمة محددة: نديره عبر السلسلة (الأولوية للمنطق أدناه).
            // SELECT بلا قيمة: نتركه يفتح منتقي الاختيار natively عبر NativeSelect.onKeyDown.
            const isEmptySelect =
                target?.tagName === "SELECT" &&
                !(target as HTMLSelectElement).value;

            if (keepsNativeEnter || isEmptySelect) return;

            e.preventDefault();

            // متقاعد لديه ملف موجود بالفعل: Enter ينقل إلى تفاصيل الملف، لا إلى الخطوة التالية.
            if (shouldOpenExistingFileOnEnter({ mode, step, existingFile })) {
                openExistingFile();
                return;
            }

            // حقول يُقصد بها Enter = الانتقال للخطوة التالية لا الحقل التالي
            // (مثل تاريخ الوفاة: بقية بيانات المتوفى اختيارية، فالتالي هو التبويب التالي).
            if (target?.closest("[data-enter-advance]")) {
                next();
                return;
            }

            if (!containerRef.current) return;

            const focusables = getEnterFocusables(containerRef.current);

            let nextIndex = -1;

            // مجموعة حي/متوفى تعامل كحقل واحد؛ Enter يخرج منها إلى الحقل التالي.
            if (radioGroup) {
                const radioIndexes = focusables
                    .map((element, index) => (radioGroup.contains(element) ? index : -1))
                    .filter((index) => index >= 0);

                if (radioIndexes.length > 0) {
                    nextIndex = Math.max(...radioIndexes) + 1;
                }
            } else if (target) {
                // بعض المكونات قد ترسل الحدث من عنصر داخلي، لذلك نتحقق من contains أيضًا.
                const currentIndex = focusables.findIndex(
                    (element) => element === target || element.contains(target)
                );

                if (currentIndex >= 0) {
                    nextIndex = currentIndex + 1;
                }
            }

            // يوجد حقل تالٍ في نفس الخطوة.
            if (nextIndex >= 0 && nextIndex < focusables.length) {
                // نعيد الاستعلام بالـ index داخل المؤقت بدل الاحتفاظ بعقدة DOM:
                // بين المفتاح والمؤقت قد يتغيّر النموذج (حقل شرطي أو رسائل خطأ)،
                // فتُفصل العقدة المخزّنة عن DOM ويضيع التركيز.
                focusFieldAt(nextIndex);
                return;
            }

            // لا حقل تالٍ داخل الخطوة.
            // في الخطوة الأخيرة: نُركّز زر الحفظ ليعلم المستخدم أن Enter التالي سيحفظ.
            // في أي خطوة أخرى: ننتقل مباشرةً بدون ضغطة Enter إضافية.
            if (step === steps.length - 1) {
                const finalAction = rootRef.current?.querySelector<HTMLElement>(
                    "[data-enter-chain-target]"
                );
                if (finalAction) {
                    finalAction.focus();
                    return;
                }
                handleSubmit();
                return;
            }

            next();
        },
        [step, next, handleSubmit, focusFieldAt, mode, existingFile, openExistingFile],
    );

    // تسجيل سلسلة Enter على document ليعمل Enter حتى بلا حقل مركّز داخل المعالج
    useEffect(() => {
        document.addEventListener("keydown", handleEnterChain);
        return () => document.removeEventListener("keydown", handleEnterChain);
    }, [handleEnterChain]);


    /**
     * عند دخول خطوة جديدة: التركيز على أول عنصر فيها.
     *
     * الإلغاء الوحيد: أن يكون المستخدم هو من ركّز عنصرًا داخل جسم الخطوة خلال
     * نافذة الانتظار (فلا نقفز ونقطع كتابته). الاعتماد على `focusin` لا على
     * `document.activeElement`، لأن زر «التالي» يبقى داخل المعالج بعد النقر
     * ووجوده كتركيز حالي كان يمنع التركيز التلقائي تمامًا.
     */
    useEffect(() => {
        let userPickedField = false;

        const onFocusIn = (e: FocusEvent) => {
            if (containerRef.current?.contains(e.target as Node)) userPickedField = true;
        };

        document.addEventListener("focusin", onFocusIn, true);

        const timer = window.setTimeout(() => {
            const container = containerRef.current;
            const root = rootRef.current;
            if (!container || !root) return;
            if (userPickedField) return;

            // في خطوة البيانات نبدأ من الخيار المحدد في RadioGroup إن وجد.
            const checkedRadio = container.querySelector<HTMLElement>(
                '[role="radio"][aria-checked="true"]:not([disabled])'
            );

            const target = checkedRadio ?? getEnterFocusables(container)[0] ?? root;

            target.focus();
            target.scrollIntoView({ block: "nearest" });
        }, 150);

        return () => {
            window.clearTimeout(timer);
            document.removeEventListener("focusin", onFocusIn, true);
        };
    }, [step]);

    return (

        <div ref={rootRef} className="flex flex-col h-full overflow-x-auto bg-background" tabIndex={-1} data-wizard-root>

            <div className="shrink-0 px-3 py-3 sm:px-6 sm:py-4 bg-card rounded-t-2xl">

                <Stepper steps={steps} activeStep={step} onStepClick={(i) => i < step && i >= firstStep && setStep(i)} />

            </div>

            <div

                ref={containerRef}

                className="flex-1 overflow-y-auto flex flex-col"

                style={{ padding: 'var(--dlg-body-py) var(--dlg-body-px)', gap: 'var(--dlg-body-gap)' }}

            >

                {prefillError && (

                    <div className="rounded-lg border border-destructive bg-destructive/5 px-4 py-3 flex items-start justify-between gap-3">

                        <div className="flex items-start gap-2 text-sm text-destructive">

                            <AlertTriangle className="w-5 h-5 shrink-0 mt-0.5" />

                            <span>{prefillError}</span>

                        </div>

                        <Button type="button" variant="outline" size="sm" onClick={retryPrefill}>إعادة المحاولة</Button>

                    </div>

                )}

                {draftRestored && !prefillError && (

                    <div className="rounded-lg border border-amber-300 bg-amber-50 px-4 py-2 text-sm text-amber-800">

                        تم استرجاع مسودة غير مكتملة. راجع البيانات قبل الحفظ، أو اضغط «إلغاء» لحذفها.

                    </div>

                )}

                {step === 0 && mode === "create" && (

                    <FormSection>

                        <div className="space-y-4">

                            {!form.retireeID ? (

                                <div className="max-w-2xl mx-auto w-full">

                                        <FormField label="ابحث عن المتقاعد بالاسم أو الرقم الوطني" error={errors.retireeID}>

                                            <div ref={searchContainerRef} className="relative">

                                                <Search className="absolute end-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground z-10" />

                                                <Input

                                                    value={retireeSearch}

                                                    onChange={(e) => setRetireeSearch(e.target.value)}

                                                    onKeyDown={handleSearchKeyDown}

                                                    className="pe-10"

                                                    placeholder="أدخل اسم المتقاعد أو الرقم الوطني..."

                                                    autoFocus

                                                />

                                            </div>

                                        </FormField>

                                        {showDropdown && dropdownPos && createPortal(

                                            <div

                                                ref={dropdownRef}

                                                className="fixed z-dialog rounded-lg border bg-card max-h-64 overflow-y-auto shadow-lg"

                                                style={{ top: dropdownPos.top, left: dropdownPos.left, width: dropdownPos.width }}

                                            >

                                                {searchingRetirees && <div className="px-4 py-3 text-sm text-muted-foreground">جاري البحث...</div>}

                                                {retireeResults.map((r, i) => (

                                                    <Button

                                                        key={r.retireeID}

                                                        type="button"

                                                        variant="ghost"

                                                        data-dropdown-item

                                                        className={`w-full justify-start px-4 py-3 h-auto border-b border-border last:border-0 gap-3 ${i === highlightedIndex ? "bg-primary/10" : ""}`}

                                                        onClick={() => selectRetiree(r)}

                                                        onMouseEnter={() => setHighlightedIndex(i)}

                                                    >

                                                        <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center text-primary font-bold text-xs shrink-0">{r.fullName.charAt(0)}</div>

                                                        <div className="min-w-0 flex-1 text-start">

                                                            <div className="font-bold text-sm truncate">{r.fullName}</div>

                                                            <div className="text-[10px] sm:text-xs text-muted-foreground flex gap-3">

                                                                <span>الرقم الوطني: {r.nationalID}</span>

                                                                {r.militaryNumber && <span>الرقم العسكري: {r.militaryNumber}</span>}

                                                            </div>

                                                        </div>

                                                    </Button>

                                                ))}

                                            </div>,

                                            document.body

                                        )}

                                        {errors.retireeSearch && (

                                            <div className="text-sm text-destructive font-medium bg-destructive/5 border border-destructive rounded-lg px-4 py-2.5 flex items-center gap-2 animate-in fade-in slide-in-from-top-1">

                                                <span className="text-destructive">⚠️</span> {errors.retireeSearch}

                                            </div>

                                        )}

                                </div>

                            ) : (

                                <>

                                    <div className="flex items-center justify-between rounded-lg border border-emerald-200 px-4 py-3 bg-emerald-50/50 flex-wrap gap-2">

                                        <div className="flex items-center gap-3">

                                            <div className="w-8 h-8 rounded bg-emerald-100 flex items-center justify-center text-emerald-600"><UserCheck className="w-4 h-4" /></div>

                                            <Text variant="body" className="font-bold text-emerald-700">{form.retireeDisplayName}</Text>

                                        </div>

                                        <Button type="button" variant="ghost" size="sm" className="h-11 text-xs" onClick={() => { patchForm((f) => ({ ...f, retireeID: 0, retireeDisplayName: "" })); setExistingFile(null); setExistingCheckFailed(false); }}>تغيير</Button>

                                    </div>

                                    {checkingExistingFile && (

                                        <Text variant="caption">جاري التحقق من الملفات السابقة...</Text>

                                    )}

                                    {existingFile && (

                                        <div className="rounded-lg border border-amber-300 bg-amber-50 px-4 py-3 flex items-start gap-3 animate-in fade-in slide-in-from-top-1">

                                            <AlertTriangle className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />

                                            <div className="text-sm text-amber-800">

                                                <Text variant="body" className="font-bold">هذا المتقاعد لديه ملف موجود بالفعل</Text>

                                                <Text variant="body" className="mt-0.5">

                                                    رقم الملف: <span className="font-mono font-semibold">{existingFile.fileNumber}</span>

                                                    {existingFile.fileStatusName && <> — الحالة: {existingFile.fileStatusName}</>}

                                                </Text>

                                                <Text variant="caption" className="mt-1 text-amber-700">لا يمكن فتح ملف جديد لمتقاعد لديه ملف غير مغلق.</Text>

                                            </div>

                                        </div>

                                    )}

                                    {existingCheckFailed && (

                                        <div className="rounded-lg border border-destructive bg-destructive/5 px-4 py-3 flex items-start gap-3 animate-in fade-in slide-in-from-top-1">

                                            <AlertTriangle className="w-5 h-5 text-destructive shrink-0 mt-0.5" />

                                            <div className="text-sm text-destructive">

                                                <Text variant="body" className="font-bold">تعذّر التحقق من ملفات المتقاعد</Text>

                                                <Text variant="caption" className="mt-1 block">أعد اختيار المتقاعد للمحاولة مرة أخرى قبل المتابعة.</Text>

                                            </div>

                                        </div>

                                    )}

                                </>

                            )}

                        </div>

                    </FormSection>

                )}

                {step === 1 && (

                    <div className="animate-in fade-in duration-200">

                        <FormSection title="المعلومات الشخصية" icon={IdCard}>

                            {/* Is Alive Radio Group */}

                            <div className="flex flex-col gap-2 mb-4">

                                <label className="label-field">حالة المتقاعد</label>

                                <RadioGroup
                                    disabled={mode === "edit"}

                                    value={form.isAlive ? "alive" : "dead"}

                                    onValueChange={(val) => {

                                        if (mode === "edit") return;

                                        if (val === "alive") {

                                            patchForm((f) => ({ ...f, isAlive: true, deathDate: "", healthStatusID: f.healthStatusID === "4" ? "1" : f.healthStatusID }));

                                        } else {

                                            patchForm((f) => ({ ...f, isAlive: false, healthStatusID: "4" }));

                                        }

                                    }}

                                    onKeyDown={(e) => {

                                        if (mode === "edit") return;

                                        if (e.key === "ArrowLeft" || e.key === "ArrowRight" || e.key === "ArrowUp" || e.key === "ArrowDown") {

                                            e.preventDefault();

                                            patchForm((f) => {

                                                const nextAlive = !f.isAlive;

                                                return {

                                                    ...f,

                                                    isAlive: nextAlive,

                                                    deathDate: nextAlive ? "" : f.deathDate,

                                                    healthStatusID: nextAlive ? (f.healthStatusID === "4" ? "1" : f.healthStatusID) : "4",

                                                };

                                            });

                                        }

                                    }}

                                    className="flex items-center h-11 border rounded-md bg-muted/5 max-w-sm overflow-hidden focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2"

                                >

                                    <label className={`flex items-center justify-center gap-2 cursor-pointer flex-1 h-full px-4 transition-colors ${form.isAlive ? "bg-primary/10 font-bold text-primary" : "hover:bg-muted/50"}`}>

                                        <RadioGroupItem value="alive" className="accent-primary" id="status-alive" />

                                        <span className="text-sm font-medium">على قيد الحياة</span>

                                    </label>

                                    <div className="w-px h-full bg-border"></div>

                                    <label className={`flex items-center justify-center gap-2 cursor-pointer flex-1 h-full px-4 transition-colors ${!form.isAlive ? "bg-destructive/10 font-bold text-destructive" : "hover:bg-muted/50"}`}>

                                        <RadioGroupItem value="dead" className="accent-destructive" id="status-dead" />

                                        <span className="text-sm font-medium text-destructive">متوفي</span>

                                    </label>

                                </RadioGroup>

                                {mode === "edit" && <p className="text-sm text-muted-foreground">لتغيير حالة الحياة استخدم إجراء تسجيل الوفاة أو تصحيح الحالة من صفحة الملف.</p>}

                            </div>

                            {!form.isAlive && mode === "create" && (

                                <div className="pb-4 border-b border-border mb-4 animate-in fade-in slide-in-from-top-1">

                                    <div className="max-w-sm">

                                        <FormField label="تاريخ الوفاة" required error={errors.deathDate}>

                                            <DateInput data-enter-advance value={form.deathDate} onChange={(val) => patchForm((f) => ({ ...f, deathDate: val || "" }))} />

                                        </FormField>


                                    </div>

                                </div>

                            )}

                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 items-start">

                                <FormField label="الرقم الوطني" required={form.isAlive} error={errors.nationalID}>

                                    <Input value={form.nationalID} onChange={(e) => patchForm((f) => ({ ...f, nationalID: normalizeArabicDigits(e.target.value).replace(/\D/g, "") }))} placeholder={!form.isAlive ? "اختياري للمتوفين" : "000000000000"} maxLength={12} />

                                </FormField>

                                <FormField label="الجنس">

                                    <NativeSelect

                                        options={[

                                            { value: "1", label: "ذكر" },

                                            { value: "2", label: "أنثى" },

                                        ]}

                                        value={form.gender}

                                        onChange={(val) => patchForm((f) => ({ ...f, gender: val }))}

                                        placeholder="اختر الجنس..."

                                    />

                                </FormField>

                                <FormField label="تاريخ الميلاد" required={form.isAlive} error={errors.birthDate}>

                                    <DateInput value={form.birthDate} onChange={(val) => patchForm((f) => ({ ...f, birthDate: val || "" }))} />

                                </FormField>

                                <FormField label="الحالة الاجتماعية">

                                    <NativeSelect

                                        options={socialStatuses.map((s) => ({ value: s.id.toString(), label: s.name }))}

                                        value={form.socialStatusID}

                                        onChange={(val) => patchForm((f) => ({ ...f, socialStatusID: val }))}

                                        placeholder="اختر..."

                                    />

                                </FormField>

                                <FormField label="الحالة الصحية">

                                    <NativeSelect

                                        options={healthStatuses
                                            .filter((s) => mode !== "edit" || !form.isAlive || s.id <= 3)
                                            .map((s) => ({ value: s.id.toString(), label: s.name }))}

                                        value={form.healthStatusID}

                                        disabled={mode === "edit" && !form.isAlive}

                                        onChange={(val) => patchForm((f) => ({ ...f, healthStatusID: val }))}

                                        placeholder="اختر..."

                                    />

                                </FormField>

                            </div>

                        </FormSection>

                        {mode === "edit" && (

                            <FormSection title="بيانات الرتبة والفرع" icon={Briefcase} className="mt-5">

                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start">

                                    <FormField label="الرتبة">

                                        <NativeSelect

                                            options={ranks.map((r) => ({ value: r.rankID.toString(), label: r.rankName }))}

                                            value={form.rankID}

                                            onChange={(val) => patchForm((f) => ({ ...f, rankID: val }))}

                                            placeholder="اختر الرتبة..."

                                        />

                                    </FormField>

                                    <FormField label="الفرع">

                                        <NativeSelect

                                            options={branches.map((b) => ({ value: b.id.toString(), label: b.name }))}

                                            value={form.branchID}

                                            onChange={(val) => patchForm((f) => ({ ...f, branchID: val }))}

                                            placeholder="اختر الفرع..."

                                        />

                                    </FormField>

                                </div>

                            </FormSection>

                        )}

                    </div>

                )}

                {step === 2 && (

                    <div className="animate-in fade-in duration-200">

                        <FormSection title="معلومات السكن والاتصال" icon={MapPin}>

                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 items-start">

                                <FormField label="المحافظة">

                                    <NativeSelect

                                        options={YEMEN_GOVERNORATES.map(g => ({ value: g, label: g }))}

                                        value={form.governorate}

                                        onChange={(val) => patchForm((f) => ({ ...f, governorate: val }))}

                                        placeholder="اختر المحافظة..."

                                    />

                                </FormField>

                                <FormField label="المدينة"><Input value={form.city} onChange={(e) => patchForm((f) => ({ ...f, city: e.target.value }))} placeholder="المدينة" /></FormField>

                                <FormField label="رقم الهاتف" error={errors.phone}><Input value={form.phone} onChange={(e) => patchForm((f) => ({ ...f, phone: e.target.value }))} placeholder="7xxxxxxx" /></FormField>

                                <div className="md:col-span-2 lg:col-span-3"><FormField label="العنوان التفصيلي"><Input value={form.address} onChange={(e) => patchForm((f) => ({ ...f, address: e.target.value }))} placeholder="العنوان بالكامل..." /></FormField></div>

                            </div>

                        </FormSection>

                    </div>

                )}

                {step === 3 && (

                    <div className="animate-in fade-in duration-200 dlg-body-gap flex flex-col">

                        <FormSection title="الوثائق والمستندات" icon={FileUp}>

                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 dlg-gap">

                                <FormField label="الصورة الشخصية"><FileUploader accept="image/*" value={form.personalPhoto} onChange={(file) => patchForm((f) => ({ ...f, personalPhoto: file }))} enableScanner={true} size="sm" /></FormField>

                                <FormField label="أوليات الملف"><FileUploader accept="image/*,application/pdf" value={form.primaryDocument} onChange={(file) => patchForm((f) => ({ ...f, primaryDocument: file }))} enableScanner={true} size="sm" /></FormField>

                                {form.isAlive ? (

                                    <div className="md:col-span-2 grid grid-cols-1 md:grid-cols-2 gap-4">

                                        <FormField label="الهوية الوطنية"><FileUploader accept="image/*,application/pdf" value={form.idDocument} onChange={(file) => patchForm((f) => ({ ...f, idDocument: file }))} enableScanner={true} size="sm" /></FormField>

                                    </div>

                                ) : <FormField label="شهادة الوفاة"><FileUploader accept="image/*,application/pdf" value={form.deathCertificate} onChange={(file) => patchForm((f) => ({ ...f, deathCertificate: file }))} enableScanner={true} size="sm" /></FormField>}

                            </div>

                        </FormSection>

                        <FormField label="ملاحظات"><Textarea rows={2} data-enter-skip value={form.notes} onChange={(e) => patchForm((f) => ({ ...f, notes: e.target.value }))} placeholder="أضف ملاحظاتك هنا..." /></FormField>

                    </div>

                )}

            </div>

            <div className="shrink-0 bg-background z-30" style={{ padding: 'var(--dlg-footer-py) var(--dlg-body-px)' }}>

                <div className="flex items-center justify-between gap-3 w-full">

                    <Button type="button" variant="outline" onClick={handleCancelRequest} disabled={createMutation.isPending}>إلغاء</Button>

                    <div className="flex items-center gap-2">

                        {step > (mode === "edit" ? 1 : 0) && <Button type="button" variant="outline" onClick={prev} disabled={createMutation.isPending}><ArrowRight className="w-4 h-4 ms-2" />السابق</Button>}

                        {step < steps.length - 1 ? (

                            <Button type="button" onClick={next} disabled={(step === 0 && (!form.retireeID || isRetireeStepBlocked({ prefillError: effectivePrefillError, existingFile, existingCheckFailed }) || checkingExistingFile)) || createMutation.isPending}>التالي<ArrowLeft className="w-4 h-4 me-2" /></Button>

                        ) : <Button type="button" data-enter-chain-target onClick={handleSubmit} loading={createMutation.isPending}><Save className="w-4 h-4 ms-2" />{mode === "edit" ? "حفظ التعديلات" : "حفظ الملف"}</Button>}

                    </div>

                </div>

            </div>

            {ConfirmDialogElement}

        </div>

    );

}


=======
        setHighlightedIndex(-1);
    }, [retireeResults]);

    useLayoutEffect(() => {
        if (!showDropdown || !searchContainerRef.current) {
            setDropdownPos(null);
            return;
        }
        const updatePos = () => {
            if (!searchContainerRef.current) return;
            const rect = searchContainerRef.current.getBoundingClientRect();
            setDropdownPos({ top: rect.bottom + 4, left: rect.left, width: rect.width });
        };
        updatePos();
        window.addEventListener("resize", updatePos);
        window.addEventListener("scroll", updatePos, true);
        return () => {
            window.removeEventListener("resize", updatePos);
            window.removeEventListener("scroll", updatePos, true);
        };
    }, [showDropdown]);

    const selectRetiree = useCallback(async (r: RetireeSearch) => {
        setForm((f) => ({
            ...f,
            retireeID: r.retireeID,
            retireeDisplayName: `${r.fullName} — ${r.nationalID}`,
        }));
        setRetireeSearch("");
        setRetireeResults([]);
        setErrors({});
        setExistingFile(null);
        setCheckingExistingFile(true);
        try {
            const [fullDataRes] = await Promise.all([
                api.get<{ data: RetireeFullData }>(`/retirees/${r.retireeID}/full-data`),
                (async () => {
                    try {
                        const filesRes = await api.get<{ data: ExistingFile[] }>(`/retirement-files/by-retiree/${r.retireeID}`);
                        const files = filesRes.data.data || [];
                        const active = files.find((file) => file.statusID !== CLOSED_STATUS_ID);
                        setExistingFile(active || null);
                    } catch { /* ignore file-check failure */ }
                })(),
            ]);
            const fullData = fullDataRes.data.data;
            setForm((f) => ({
                ...f,
                ...mapRetireeToForm(fullData, f.notes)
            }));
        } catch (err) {
            const message = "فشل في تحميل بيانات المتقاعد. يرجى المحاولة مرة أخرى.";
            setErrors((prev) => ({ ...prev, retireeSearch: message }));
            setForm((f) => ({ ...f, retireeID: 0, retireeDisplayName: "" }));
        } finally {
            setCheckingExistingFile(false);
        }
    }, []);

    const handleSearchKeyDown = useCallback((e: React.KeyboardEvent) => {
        if (!showDropdown) return;
        if (e.key === "ArrowDown") {
            e.preventDefault();
            setHighlightedIndex((prev) => {
                const next = prev < retireeResults.length - 1 ? prev + 1 : 0;
                return next;
            });
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            setHighlightedIndex((prev) => {
                const next = prev > 0 ? prev - 1 : retireeResults.length - 1;
                return next;
            });
        } else if (e.key === "Enter" && highlightedIndex >= 0) {
            e.preventDefault();
            selectRetiree(retireeResults[highlightedIndex]);
        } else if (e.key === "Escape") {
            setRetireeResults([]);
            setHighlightedIndex(-1);
        }
    }, [showDropdown, retireeResults, highlightedIndex, selectRetiree]);

    useEffect(() => {
        if (highlightedIndex < 0 || !dropdownRef.current) return;
        const items = dropdownRef.current.querySelectorAll('[data-dropdown-item]');
        items[highlightedIndex]?.scrollIntoView({ block: "nearest" });
    }, [highlightedIndex]);

    const validateStep = useCallback((s: number): boolean => {
        const errs: Record<string, string> = {};
        if (s === 0 && !form.retireeID) errs.retireeID = "يجب اختيار المتقاعد أولاً";
        if (s === 0 && existingFile) errs.retireeID = "لا يمكن المتابعة، لهذا المتقاعد ملف نشط بالفعل";
        if (s === 1) {
            // Birth Date validation
            if (form.isAlive && !form.birthDate) {
                errs.birthDate = "تاريخ الميلاد مطلوب";
            } else if (form.birthDate) {
                const bDate = new Date(form.birthDate);
                if (isNaN(bDate.getTime())) {
                    errs.birthDate = "صيغة التاريخ غير صحيحة";
                } else if (bDate > new Date()) {
                    errs.birthDate = "تاريخ الميلاد لا يمكن أن يكون في المستقبل";
                }
            }

            if (form.isAlive) {
                if (!form.nationalID?.trim()) {
                    errs.nationalID = "الرقم الوطني مطلوب لغير المتوفين";
                } else if (!/^\d{12}$/.test(form.nationalID.trim())) {
                    errs.nationalID = "الرقم الوطني يجب أن يكون 12 رقماً ويحتوي على أرقام فقط";
                }
            } else {
                if (form.nationalID?.trim() && !/^\d{12}$/.test(form.nationalID.trim())) {
                    errs.nationalID = "الرقم الوطني يجب أن يكون 12 رقماً ويحتوي على أرقام فقط";
                }
            }
            
            if (!form.isAlive) {
                if (!form.deathDate) {
                    errs.deathDate = "تاريخ الوفاة مطلوب";
                } else {
                    const dDate = new Date(form.deathDate);
                    if (isNaN(dDate.getTime())) {
                        errs.deathDate = "صيغة التاريخ غير صحيحة";
                    } else if (dDate > new Date()) {
                        errs.deathDate = "تاريخ الوفاة لا يمكن أن يكون في المستقبل";
                    } else if (form.birthDate && dDate < new Date(form.birthDate)) {
                        errs.deathDate = "تاريخ الوفاة لا يمكن أن يكون قبل تاريخ الميلاد";
                    }
                }
            }
        }
        if (s === 3) {
            // No mandatory file validation
        }
        setErrors(errs);
        return Object.keys(errs).length === 0;
    }, [form, mode, existingFile]);

    const next = () => { if (validateStep(step)) setStep((s) => Math.min(s + 1, steps.length - 1)); };
    const prev = () => setStep((s) => Math.max(s - 1, mode === "edit" ? 1 : 0));

    const handleCancelRequest = async () => {
        if (step > (mode === "edit" ? 1 : 0) || (form.retireeID > 0 && mode === "create")) {
            const confirmed = await confirm({
                title: mode === "edit" ? "التراجع عن التعديلات" : "التراجع عن إنشاء الملف",
                description: "هل أنت متأكد؟ سيتم فقدان البيانات المدخلة.",
                confirmLabel: "نعم، إلغاء",
                cancelLabel: "تراجع",
                variant: "danger"
            });
            if (confirmed) onCancel();
        } else onCancel();
    };

    const handleSubmit = async () => {
        if (!validateStep(step)) return;
        const formData = new FormData();
        formData.append("retireeID", form.retireeID.toString());
        formData.append("notes", form.notes || "");
        formData.append("nationalID", form.nationalID.trim());
        formData.append("gender", form.gender);
        if (form.birthDate) formData.append("birthDate", new Date(form.birthDate).toISOString());
        formData.append("socialStatusID", form.socialStatusID || "");
        formData.append("healthStatusID", form.healthStatusID || "");
        formData.append("isAlive", form.isAlive.toString());
        if (!form.isAlive && form.deathDate) formData.append("deathDate", new Date(form.deathDate).toISOString());
        formData.append("governorate", form.governorate || "");
        formData.append("city", form.city || "");
        formData.append("address", form.address || "");
        formData.append("phone", form.phone || "");
        if (form.personalPhoto) formData.append("personalPhoto", form.personalPhoto);
        if (form.isAlive && form.idDocument) formData.append("idDocument", form.idDocument);
        if (!form.isAlive && form.deathCertificate) formData.append("deathCertificate", form.deathCertificate);
        if (form.primaryDocument) formData.append("primaryDocument", form.primaryDocument);

        createMutation.mutate(formData);
    };

    return (
        <div className="flex flex-col h-full overflow-x-auto bg-background">
            <div className="shrink-0 py-3 px-4 border-b bg-muted/5">
                <Stepper steps={steps} activeStep={step} onStepClick={(i) => (mode === "create" ? i < step : (i < step && i > 0)) && setStep(i)} />
            </div>

            <div
                className="flex-1 overflow-y-auto flex flex-col"
                style={{ padding: 'var(--dlg-body-py) var(--dlg-body-px)', gap: 'var(--dlg-body-gap)' }}
            >
                {step === 0 && mode === "create" && (
                 
                        <FormSection title="البحث واختيار المتقاعد" icon={UserCheck}>
                            <div className="space-y-4">
                                {!form.retireeID ? (
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 dlg-gap items-end">
                                        <div className="md:col-span-3 relative" ref={searchContainerRef}>
                                            <FormField label="ابحث عن المتقاعد بالاسم أو الرقم الوطني" error={errors.retireeID}>
                                                <div className="relative">
                                                    <Search className="absolute end-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground z-10" />
                                                    <Input
                                                        value={retireeSearch}
                                                        onChange={(e) => setRetireeSearch(e.target.value)}
                                                        onKeyDown={handleSearchKeyDown}
                                                        className="pe-10"
                                                        placeholder="أدخل اسم المتقاعد أو الرقم الوطني..."
                                                        autoFocus
                                                    />
                                                </div>
                                            </FormField>
                                            {showDropdown && dropdownPos && createPortal(
                                                <div
                                                    ref={dropdownRef}
                                                    className="fixed z-dialog rounded-lg border bg-card max-h-64 overflow-y-auto shadow-lg"
                                                    style={{ top: dropdownPos.top, left: dropdownPos.left, width: dropdownPos.width }}
                                                >
                                                    {searchingRetirees && <div className="px-4 py-3 text-sm text-muted-foreground">جاري البحث...</div>}
                                                    {retireeResults.map((r, i) => (
                                                        <Button
                                                            key={r.retireeID}
                                                            type="button"
                                                            variant="ghost"
                                                            data-dropdown-item
                                                            className={`w-full justify-start px-4 py-3 h-auto border-b border-border/40 last:border-0 gap-3 ${i === highlightedIndex ? "bg-primary/10" : ""}`}
                                                            onClick={() => selectRetiree(r)}
                                                            onMouseEnter={() => setHighlightedIndex(i)}
                                                        >
                                                            <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center text-primary font-bold text-xs shrink-0">{r.fullName.charAt(0)}</div>
                                                            <div className="min-w-0 flex-1 text-start">
                                                                <div className="font-bold text-sm truncate">{r.fullName}</div>
                                                                <div className="text-[10px] sm:text-xs text-muted-foreground flex gap-3">
                                                                    <span>الرقم الوطني: {r.nationalID}</span>
                                                                    {r.militaryNumber && <span>الرقم العسكري: {r.militaryNumber}</span>}
                                                                </div>
                                                            </div>
                                                        </Button>
                                                    ))}
                                                </div>,
                                                document.body
                                            )}
                                            {errors.retireeSearch && (
                                                <div className="text-sm text-destructive font-medium bg-destructive/5 border border-destructive/20 rounded-lg px-4 py-2.5 flex items-center gap-2 animate-in fade-in slide-in-from-top-1">
                                                    <span className="text-destructive/70">⚠️</span> {errors.retireeSearch}
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                ) : (
                                    <>
                                        <div className="flex items-center justify-between rounded-lg border border-emerald-200 px-4 py-3 bg-emerald-50/50 flex-wrap gap-2">
                                            <div className="flex items-center gap-3">
                                                <div className="w-8 h-8 rounded bg-emerald-100 flex items-center justify-center text-emerald-600"><UserCheck className="w-4 h-4" /></div>
                                                <Text variant="body" className="font-bold text-emerald-700">{form.retireeDisplayName}</Text>
                                            </div>
                                            <Button type="button" variant="ghost" size="sm" className="h-11 text-xs" onClick={() => { setForm((f) => ({ ...f, retireeID: 0, retireeDisplayName: "" })); setExistingFile(null); }}>تغيير</Button>
                                        </div>
                                        {checkingExistingFile && (
                                            <Text variant="caption">جاري التحقق من الملفات السابقة...</Text>
                                        )}
                                        {existingFile && (
                                            <div className="rounded-lg border border-amber-300 bg-amber-50 px-4 py-3 flex items-start gap-3 animate-in fade-in slide-in-from-top-1">
                                                <AlertTriangle className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
                                                <div className="text-sm text-amber-800">
                                                    <Text variant="body" className="font-bold">هذا المتقاعد لديه ملف نشط بالفعل</Text>
                                                    <Text variant="body" className="mt-0.5">
                                                        رقم الملف: <span className="font-mono font-semibold">{existingFile.fileNumber}</span>
                                                        {existingFile.fileStatusName && <> — الحالة: {existingFile.fileStatusName}</>}
                                                    </Text>
                                                    <Text variant="caption" className="mt-1 text-amber-700">لا يمكن فتح ملف جديد لمتقاعد لديه ملف غير مغلق.</Text>
                                                </div>
                                            </div>
                                        )}
                                    </>
                                )}
                            </div>
                        </FormSection>
            
                )}

                {step === 1 && (
                    <div className="animate-in fade-in duration-200">
                        <FormSection title="المعلومات الشخصية" icon={IdCard}>
                            {/* Is Alive Radio Group */}
                            <div className="flex flex-col gap-2 mb-4">
                                <label className="text-sm font-semibold text-foreground">حالة المتقاعد</label>
                                <RadioGroup
                                    value={form.isAlive ? "alive" : "dead"}
                                    onValueChange={(val) => {
                                        if (val === "alive") {
                                            setForm((f) => ({ ...f, isAlive: true, deathDate: "", healthStatusID: f.healthStatusID === "4" ? "1" : f.healthStatusID }));
                                        } else {
                                            setForm((f) => ({ ...f, isAlive: false, healthStatusID: "4" }));
                                        }
                                    }}
                                    className="flex items-center h-11 border rounded-md bg-muted/5 max-w-sm overflow-hidden"
                                >
                                    <label className="flex items-center justify-center gap-2 cursor-pointer flex-1 h-full px-4 has-[:checked]:bg-primary/5 transition-colors">
                                        <RadioGroupItem value="alive" className="accent-primary" />
                                        <span className="text-sm font-medium">على قيد الحياة</span>
                                    </label>
                                    <div className="w-px h-full bg-border"></div>
                                    <label className="flex items-center justify-center gap-2 cursor-pointer flex-1 h-full px-4 has-[:checked]:bg-destructive/5 transition-colors">
                                        <RadioGroupItem value="dead" className="accent-destructive" />
                                        <span className="text-sm font-medium text-destructive">متوفي</span>
                                    </label>
                                </RadioGroup>
                            </div>

                            {!form.isAlive && (
                                <div className="pb-4 border-b border-border/50 mb-4 animate-in fade-in slide-in-from-top-1">
                                    <div className="max-w-sm">
                                        <FormField label="تاريخ الوفاة" required error={errors.deathDate}>
                                            <DateInput value={form.deathDate} onChange={(val) => setForm((f) => ({ ...f, deathDate: val || "" }))} />
                                        </FormField>
                                    </div>
                                </div>
                            )}

                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 items-start">
                                <FormField label="الرقم الوطني" required={form.isAlive} error={errors.nationalID}>
                                    <Input value={form.nationalID} onChange={(e) => setForm((f) => ({ ...f, nationalID: normalizeArabicDigits(e.target.value).replace(/\D/g, "") }))} placeholder={!form.isAlive ? "اختياري للمتوفين" : "000000000000"} maxLength={12} />
                                </FormField>
                                <FormField label="الجنس">
                                    <NativeSelect
                                        options={[
                                            { value: "1", label: "ذكر" },
                                            { value: "2", label: "أنثى" },
                                        ]}
                                        value={form.gender}
                                        onChange={(val) => setForm((f) => ({ ...f, gender: val }))}
                                        placeholder="اختر الجنس..."
                                    />
                                </FormField>
                                <FormField label="تاريخ الميلاد" required={form.isAlive} error={errors.birthDate}>
                                   <DateInput value={form.birthDate} onChange={(val) => setForm((f) => ({ ...f, birthDate: val || "" }))} />
                                </FormField>

                                <FormField label="الحالة الاجتماعية">
                                    <NativeSelect
                                        options={socialStatuses.map((s) => ({ value: s.id.toString(), label: s.name }))}
                                        value={form.socialStatusID}
                                        onChange={(val) => setForm((f) => ({ ...f, socialStatusID: val }))}
                                        placeholder="اختر..."
                                    />
                                </FormField>
                                <FormField label="الحالة الصحية">
                                    <NativeSelect
                                        options={healthStatuses.map((s) => ({ value: s.id.toString(), label: s.name }))}
                                        value={form.healthStatusID}
                                        onChange={(val) => setForm((f) => ({ ...f, healthStatusID: val }))}
                                        placeholder="اختر..."
                                    />
                                </FormField>
                            </div>
                        </FormSection>
                    </div>
                )}

                {step === 2 && (
                    <div className="animate-in fade-in duration-200">
                        <FormSection title="معلومات السكن والاتصال" icon={MapPin}>
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 items-start">
                                <FormField label="المحافظة">
                                    <NativeSelect
                                        options={YEMEN_GOVERNORATES.map(g => ({ value: g, label: g }))}
                                        value={form.governorate}
                                        onChange={(val) => setForm((f) => ({ ...f, governorate: val }))}
                                        placeholder="اختر المحافظة..."
                                    />
                                </FormField>
                                <FormField label="المدينة"><Input value={form.city} onChange={(e) => setForm((f) => ({ ...f, city: e.target.value }))} placeholder="المدينة" /></FormField>
                                <FormField label="رقم الهاتف" error={errors.phone}><Input value={form.phone} onChange={(e) => setForm((f) => ({ ...f, phone: normalizeArabicDigits(e.target.value).replace(/\D/g, "") }))} placeholder="7xxxxxxx" maxLength={9} /></FormField>
                                <div className="md:col-span-2 lg:col-span-3"><FormField label="العنوان التفصيلي"><Input value={form.address} onChange={(e) => setForm((f) => ({ ...f, address: e.target.value }))} placeholder="العنوان بالكامل..." /></FormField></div>
                            </div>
                        </FormSection>
                    </div>
                )}

                {step === 3 && (
                    <div className="animate-in fade-in duration-200 dlg-body-gap flex flex-col">
                        <FormSection title="الوثائق والمستندات" icon={FileUp}>
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 dlg-gap">
                                <FormField label="الصورة الشخصية" error={errors.personalPhoto}><FileUploader accept="image/*" value={form.personalPhoto} onChange={(file) => setForm((f) => ({ ...f, personalPhoto: file }))} enableScanner={true} size="sm" /></FormField>
                                <FormField label="أوليات الملف"><FileUploader accept="image/*,application/pdf" value={form.primaryDocument} onChange={(file) => setForm((f) => ({ ...f, primaryDocument: file }))} enableScanner={true} size="sm" /></FormField>
                                {form.isAlive ? (
                                    <div className="md:col-span-2 grid grid-cols-1 md:grid-cols-2 gap-4">
                                        <FormField label="الهوية الوطنية"><FileUploader accept="image/*,application/pdf" value={form.idDocument} onChange={(file) => setForm((f) => ({ ...f, idDocument: file }))} enableScanner={true} size="sm" /></FormField>
                                    </div>
                                ) : <FormField label="شهادة الوفاة" error={errors.deathCertificate}><FileUploader accept="image/*,application/pdf" value={form.deathCertificate} onChange={(file) => setForm((f) => ({ ...f, deathCertificate: file }))} enableScanner={true} size="sm" /></FormField>}
                            </div>
                        </FormSection>
                        <FormField label="ملاحظات"><Textarea rows={2} value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} placeholder="أضف ملاحظاتك هنا..." /></FormField>
                    </div>
                )}
            </div>

            <div className="shrink-0 border-t bg-background z-30" style={{ padding: 'var(--dlg-footer-py) var(--dlg-body-px)' }}>
                <div className="flex items-center justify-between gap-3 w-full">
                    <Button type="button" variant="outline" onClick={handleCancelRequest} disabled={createMutation.isPending}>إلغاء</Button>
                    <div className="flex items-center gap-2">
                        {step > (mode === "edit" ? 1 : 0) && <Button type="button" variant="outline" onClick={prev} disabled={createMutation.isPending}><ArrowRight className="w-4 h-4 ms-2" />السابق</Button>}
                        {step < steps.length - 1 ? (
                            <Button type="button" onClick={next} disabled={(step === 0 && (!!existingFile || checkingExistingFile)) || createMutation.isPending}>التالي<ArrowLeft className="w-4 h-4 me-2" /></Button>
                        ) : <Button type="button" onClick={handleSubmit} loading={createMutation.isPending}><Save className="w-4 h-4 ms-2" />{mode === "edit" ? "حفظ التعديلات" : "حفظ الملف"}</Button>}
                    </div>
                </div>
            </div>
            {ConfirmDialogElement}
        </div>
    );
}

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
