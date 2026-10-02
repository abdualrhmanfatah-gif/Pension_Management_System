/**
 * منطق نقي (بلا React) لنموذج إنشاء/تعديل ملف التقاعد.
 * الاختبار: ui/tests/retirementFileForm.test.ts (node:test — لا DOM harness في المشروع).
 */

import { normalizeDateInputValue, todayYemen } from "./date.ts";

export const DRAFT_SESSION_KEY = "retirement_file_draft";

export interface FormState {
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
    /** حقول الملف (PUT فقط) — لا تُرسل فارغة. */
    rankID: string;
    branchID: string;
    personalPhoto: File | null;
    idDocument: File | null;
    primaryDocument: File | null;
    deathCertificate: File | null;
    notes: string;
}

export type DraftFields = Omit<
    FormState,
    "personalPhoto" | "idDocument" | "primaryDocument" | "deathCertificate"
>;

export const initialFormState: FormState = {
    retireeID: 0,
    retireeDisplayName: "",
    nationalID: "",
    gender: "1",
    birthDate: "",
    socialStatusID: "2",
    healthStatusID: "1",
    isAlive: true,
    deathDate: "",
    governorate: "",
    city: "",
    address: "",
    phone: "",
    rankID: "",
    branchID: "",
    personalPhoto: null,
    idDocument: null,
    primaryDocument: null,
    deathCertificate: null,
    notes: "",
};

export interface RetireeFullData {
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

export interface ExistingFile {
    fileID: number;
    fileNumber: string;
    statusID: number | null;
    fileStatusName: string | null;
}

// ─── المسودة ────────────────────────────────────────────

/** يزيل حقول الملفات قبل التخزين: File لا يُسلسل نصيًا ويصبح `{}` عند الاستعادة. */
export function toDraft(form: FormState): DraftFields {
    return {
        retireeID: form.retireeID,
        retireeDisplayName: form.retireeDisplayName,
        nationalID: form.nationalID,
        gender: form.gender,
        birthDate: form.birthDate,
        socialStatusID: form.socialStatusID,
        healthStatusID: form.healthStatusID,
        isAlive: form.isAlive,
        deathDate: form.deathDate,
        governorate: form.governorate,
        city: form.city,
        address: form.address,
        phone: form.phone,
        rankID: form.rankID,
        branchID: form.branchID,
        notes: form.notes,
    };
}

export interface InitialFormResult {
    form: FormState;
    /** المسودة استُعيدت فعلاً — تُعرض رسالة استئناف للمستخدم. */
    draftRestored: boolean;
}

/**
 * يبني الحالة الابتدائية.
 * مسودة متقاعد آخر تُهمَل بالكامل (لا تُصفّر حقول فقط) حتى لا تتسرّب PII أو ملاحظات غائب.
 */
export function resolveInitialForm(params: {
    mode: "create" | "edit";
    initialRetireeId?: number;
    rawDraft: string | null;
}): InitialFormResult {
    const { mode, initialRetireeId, rawDraft } = params;
    if (mode === "edit" || !rawDraft) return { form: initialFormState, draftRestored: false };

    let draft: Partial<DraftFields>;
    try {
        draft = JSON.parse(rawDraft) as Partial<DraftFields>;
    } catch {
        return { form: initialFormState, draftRestored: false };
    }
    if (typeof draft !== "object" || draft === null) {
        return { form: initialFormState, draftRestored: false };
    }

    if (initialRetireeId && draft.retireeID !== initialRetireeId) {
        return { form: initialFormState, draftRestored: false };
    }

    const merged: FormState = { ...initialFormState, ...draft };
    if (!draft.gender) merged.gender = initialFormState.gender;
    if (!draft.socialStatusID) merged.socialStatusID = initialFormState.socialStatusID;
    if (!draft.healthStatusID) merged.healthStatusID = initialFormState.healthStatusID;
    return { form: merged, draftRestored: true };
}

// ─── الجلب المسبق ───────────────────────────────────────

export function mapRetireeToForm(
    fullData: RetireeFullData,
    notes: string,
    initialName?: string,
): Partial<FormState> {
    return {
        retireeID: fullData.retireeID,
        retireeDisplayName: initialName
            ? `${initialName} — ${fullData.nationalID}`
            : `${fullData.fullName} — ${fullData.nationalID}`,
        nationalID: fullData.nationalID || "",
        gender: fullData.gender?.toString() || initialFormState.gender,
        birthDate: normalizeDateInputValue(fullData.birthDate),
        socialStatusID: fullData.socialStatusID?.toString() || initialFormState.socialStatusID,
        healthStatusID: fullData.healthStatusID?.toString() || initialFormState.healthStatusID,
        isAlive: fullData.isAlive ?? true,
        deathDate: normalizeDateInputValue(fullData.deathDate),
        governorate: fullData.governorate || "",
        city: fullData.city || "",
        address: fullData.address || "",
        phone: fullData.phone || "",
        notes,
    };
}

// ─── التحقق ─────────────────────────────────────────────

const NATIONAL_ID_PATTERN = /^\d{12}$/;
const YEMEN_PHONE_PATTERN = /^7\d{8}$/;

/** المقارنة نصية على YYYY-MM-DD: لا تحويل ولا انزياح منطقة زمنية. */
function isFutureDateOnly(value: string, today: string): boolean {
    return value > today;
}

export interface ValidateContext {
    existingFile?: ExistingFile | null;
    /** فشل فحص الملفات النشطة: يُعامَل كمانع (fail-closed) حتى إعادة المحاولة. */
    existingCheckFailed?: boolean;
    /** فشل جلب بيانات المتقاعد أو الملف في وضع التعديل: يمنع الحفظ فوق قيم افتراضية. */
    prefillError?: string | null;
    today?: string;
    lifeStatusReadOnly?: boolean;
}

export function validateStep(
    form: FormState,
    step: number,
    context: ValidateContext = {},
): Record<string, string> {
    const today = context.today ?? todayYemen();
    const errors: Record<string, string> = {};

    if (context.prefillError) {
        errors.prefill = context.prefillError;
    }

    if (step === 0) {
        if (!form.retireeID) errors.retireeID = "يجب اختيار المتقاعد أولاً";
        else if (context.existingFile) {
            errors.retireeID = "لا يمكن المتابعة، لهذا المتقاعد ملف نشط بالفعل";
        } else if (context.existingCheckFailed) {
            errors.retireeID = "تعذّر التحقق من ملفات المتقاعد. أعد المحاولة قبل المتابعة.";
        }
    }

    if (step === 1) {
        if (form.isAlive && !form.birthDate) {
            errors.birthDate = "تاريخ الميلاد مطلوب";
        } else if (form.birthDate && isFutureDateOnly(form.birthDate, today)) {
            errors.birthDate = "تاريخ الميلاد لا يمكن أن يكون في المستقبل";
        }

        if (form.isAlive) {
            if (!form.nationalID.trim()) {
                errors.nationalID = "الرقم الوطني مطلوب لغير المتوفين";
            } else if (!NATIONAL_ID_PATTERN.test(form.nationalID.trim())) {
                errors.nationalID = "الرقم الوطني يجب أن يكون 12 رقماً ويحتوي على أرقام فقط";
            }
        } else if (form.nationalID.trim() && !NATIONAL_ID_PATTERN.test(form.nationalID.trim())) {
            errors.nationalID = "الرقم الوطني يجب أن يكون 12 رقماً ويحتوي على أرقام فقط";
        }

        if (!form.isAlive && !context.lifeStatusReadOnly) {
            if (!form.deathDate) {
                errors.deathDate = "تاريخ الوفاة مطلوب";
            } else if (isFutureDateOnly(form.deathDate, today)) {
                errors.deathDate = "تاريخ الوفاة لا يمكن أن تكون في المستقبل";
            } else if (form.birthDate && form.deathDate < form.birthDate) {
                errors.deathDate = "تاريخ الوفاة لا يمكن أن يكون قبل تاريخ الميلاد";
            }
        }
    }

    if (step === 2 && form.phone && !YEMEN_PHONE_PATTERN.test(form.phone)) {
        errors.phone = "رقم الهاتف يجب أن يتكون من 9 أرقام ويبدأ بالرقم 7";
    }

    return errors;
}

export function isStepValid(errors: Record<string, string>): boolean {
    return Object.keys(errors).length === 0;
}

/** أخطاء الخطوة 0 التي يجب أن تعطّل زر «التالي» فوراً، لا عند الضغط. */
export function isRetireeStepBlocked(context: ValidateContext = {}): boolean {
    if (context.prefillError) return true;
    if (context.existingCheckFailed) return true;
    return !!context.existingFile;
}

/**
 * أول ملف غير مغلق يمنع فتح ملف جديد لنفس المتقاعد.
 * `statusID = null` يُعامل كغير مغلق (حالة غير معروفة يجب ألا تفتح الباب).
 */
export function findActiveFile<T extends Pick<ExistingFile, "statusID">>(
    files: readonly T[],
    closedStatusId: number,
): T | undefined {
    return files.find((file) => file.statusID !== closedStatusId);
}

/**
 * Enter في خطوة المتقاعد: إن كان لديه ملف نشط، افتح تفاصيله بدل مسار
 * «التالي» المعطّل (لا خطأ يُعرض أصلًا في فرع «المتقاعد محدَّد»).
 * وضع التعديل مستثنى: أول خطوة فيه 1 ولا تُفحص له ملفات.
 */
export function shouldOpenExistingFileOnEnter(params: {
    mode: "create" | "edit";
    step: number;
    existingFile?: ExistingFile | null;
}): boolean {
    const { mode, step, existingFile } = params;
    if (mode !== "create") return false;
    if (step !== 0) return false;
    return !!existingFile;
}

// ─── الإرسال ────────────────────────────────────────────

/**
 * التواريخ تُرسل نص YYYY-MM-DD وفق عقد الـAPI (ADR 0001).
 * عند تعديل بيانات الملف لا تُرسل حالة الحياة؛ تغييرها يتم عبر إجراء مستقل.
 */
export function buildFormData(form: FormState, includeLifeStatus = true): FormData {
    const data = new FormData();
    data.append("retireeID", String(form.retireeID));
    data.append("notes", form.notes);
    data.append("nationalID", form.nationalID.trim());
    data.append("gender", form.gender);
    if (form.birthDate) data.append("birthDate", normalizeDateInputValue(form.birthDate));
    data.append("socialStatusID", form.socialStatusID);
    if (includeLifeStatus) {
        data.append("healthStatusID", form.healthStatusID);
        data.append("isAlive", String(form.isAlive));
        if (!form.isAlive && form.deathDate) {
            data.append("deathDate", normalizeDateInputValue(form.deathDate));
        }
    } else if (form.isAlive && !["4", "5"].includes(form.healthStatusID)) {
        data.append("healthStatusID", form.healthStatusID);
    }
    data.append("governorate", form.governorate);
    data.append("city", form.city);
    data.append("address", form.address);
    data.append("phone", form.phone);
    if (form.rankID) data.append("rankID", form.rankID);
    if (form.branchID) data.append("branchID", form.branchID);
    if (form.personalPhoto) data.append("personalPhoto", form.personalPhoto);
    if (form.idDocument) data.append("idDocument", form.idDocument);
    if (form.deathCertificate) data.append("deathCertificate", form.deathCertificate);
    if (form.primaryDocument) data.append("primaryDocument", form.primaryDocument);
    return data;
}

// ─── الإغلاق ────────────────────────────────────────────

/**
 * الإلغاء الصريح: يسأل دائمًا عند وجود بيانات، ويحذف المسودة بعد التأكيد.
 */
export function shouldConfirmExplicitCancel(
    mode: "create" | "edit",
    form: FormState,
    step: number,
    firstStep: number,
): boolean {
    if (step > firstStep) return true;
    if (mode === "create" && form.retireeID > 0) return true;
    return form.notes.trim().length > 0;
}

/**
 * الإغلاق العرضي (X / Escape / النقر خارجًا):
 * - التعديل: يُسأل عند وجود تغييرات غير محفوظة (لا مسودة في وضع التعديل).
 * - الإنشاء: المسودة محفوظة تلقائيًا، فلا شيء يُفقد، ولا سؤال.
 */
export function shouldConfirmDismissal(mode: "create" | "edit", isDirty: boolean): boolean {
    return mode === "edit" && isDirty;
}
