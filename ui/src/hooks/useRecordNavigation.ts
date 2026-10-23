import { type QueryKey,useQuery } from "@tanstack/react-query";
import { type RefObject,useCallback, useEffect, useMemo, useRef, useState } from "react";
import type { FieldValues, UseFormReturn } from "react-hook-form";
import { toast } from "sonner";

import { useRecordNavigationShortcut } from "@/constants/shortcuts";

export interface UseRecordNavigationOptions<R, D, F extends FieldValues> {
    open: boolean;
    mode: "create" | "edit";
    /** معرّف السجل عند فتح الحوار في وضع التعديل (يُشتق عادة من initialData) */
    editRecordId?: number;
    /** استعلام قائمة الملف — استخدم نفس مفتاح cache كالتبويب لمشاركة الطلب */
    listKey: QueryKey;
    fetchList: () => Promise<R[]>;
    getId: (row: R) => number;
    listLoading?: boolean;
    /** تفاصيل السجل الواحد (اختياري — إن غاب يُستخدم صف القائمة مباشرة) */
    detailKey?: (id: number) => QueryKey;
    fetchDetail?: (id: number) => Promise<D>;
    getDetailId?: (detail: D) => number;
    /** نص toast عند التنقل ولديك سجلات */
    emptyMessage: string;
    /** قيم النموذج الفارغ — تُستدعى عند العودة للنموذج الفارغ (Alt+↓ من الأول) */
    emptyDefaults: F;
    /** تحويل السجل (تفاصيل أو صف قائمة) إلى قيم النموذج */
    toFormValues: (record: R | D) => F;
    form: UseFormReturn<F>;
    isDirty: boolean;
    /**
     * مرجع حارس أثناء الحفظ (مثل mutation.isPending) — يُقرأ عند الضغط فقط
     * لتفادي الاعتماد الدائري بين الطفرة وهوك التنقل.
     */
    isBusyRef?: RefObject<boolean>;
    /** إعادة تثبيت focus على حقل بعد التنقل (مثل سلوك "حفظ وإضافة آخر") */
    focusField?: string;
}

export interface RecordNavigationResult<R, D> {
    /** فهرس السجل الحالي في القائمة؛ null = نموذج فارغ */
    activeIndex: number | null;
    /** معرّف السجل الحالي (المتنقَّل إليه أو محرّر الحوار) */
    activeId: number | undefined;
    /** السجل الحالي: تفاصيل طازجة إن وافقت المعرّف، وإلا صف القائمة */
    activeRecord: R | D | undefined;
    /** الحفظ محظور حتى تصل تفاصيل السجل المستهدف */
    detailReady: boolean;
    /** وضع فعلي: التنقل إلى سجل يجعل الحفظ تعديلاً عليه (PUT) بدل إنشاء نسخة */
    effectiveMode: "create" | "edit";
    navigate: (delta: 1 | -1) => void;
}

/**
 * تنقّل Alt+↑/↓ بين سجلات قائمة ملف واحد داخل حوار نموذج مفتوح.
 * يشترك: قائمة الملف (cache مشترك مع التبويب)، تحذير dirty، حدود القائمة
 * (↑ من فارغ = الأول، ↓ من الأول في الإضافة = فارغ، التعديل يتوقف عند
 * الأول، نهاية بلا ترحيل)، وتعبئة النموذج بعد وصول بيانات السجل.
 */
export function useRecordNavigation<R, D = R, F extends FieldValues = FieldValues>({
    open,
    mode,
    editRecordId,
    listKey,
    fetchList,
    getId,
    listLoading,
    detailKey,
    fetchDetail,
    getDetailId,
    emptyMessage,
    emptyDefaults,
    toFormValues,
    form,
    isDirty,
    isBusyRef,
    focusField,
}: UseRecordNavigationOptions<R, D, F>): RecordNavigationResult<R, D> {
    const { data: rowsData, isLoading: rowsQueryLoading } = useQuery({
        queryKey: listKey,
        queryFn: fetchList,
        enabled: open,
        staleTime: 30_000,
    });
    const rows = useMemo(() => rowsData ?? [], [rowsData]);
    const loading = listLoading ?? rowsQueryLoading;

    const [activeIndex, setActiveIndex] = useState<number | null>(null);
    // رقم التنقل الذي لم تصل بياناته بعد — يمنع إعادة التصفير أثناء التحميل
    const pendingNavRef = useRef<number | null>(null);

    // عند إغلاق الحوار: تصفير الحالة ليعاد بناؤه عند الفتح التالي
    useEffect(() => {
        if (open) return;
        setActiveIndex(null);
        pendingNavRef.current = null;
    }, [open]);

    // في وضع التعديل: البدء من فهرس السجل المفتوح بمجرد وصول القائمة
    useEffect(() => {
        if (!open || mode !== "edit" || activeIndex !== null) return;
        if (!editRecordId || rows.length === 0) return;
        const idx = rows.findIndex(r => getId(r) === editRecordId);
        if (idx >= 0) setActiveIndex(idx);
    }, [open, mode, activeIndex, editRecordId, rows, getId]);

    const activeId = activeIndex != null
        ? (rows[activeIndex] != null ? getId(rows[activeIndex]) : undefined)
        : (mode === "edit" ? editRecordId : undefined);

    const detailQueryEnabled =
        !!open && !!activeId && !!fetchDetail && !!detailKey && activeId > 0;
    const { data: detail } = useQuery({
        // مفتاح ثابت عند عدم توفر تفاصيل — الاستعلام معطّل أصلاً
        queryKey: detailKey && activeId != null ? detailKey(activeId) : ["record-navigation-no-detail"],
        queryFn: () => fetchDetail!(activeId!),
        enabled: detailQueryEnabled,
        staleTime: 10_000,
    });

    const detailMatches = useMemo(() => {
        if (detail == null || activeId == null || !getDetailId) return false;
        return getDetailId(detail) === activeId;
    }, [detail, activeId, getDetailId]);

    const detailReady = !fetchDetail || activeId == null || detailMatches;

    const activeRecord = useMemo<R | D | undefined>(() => {
        if (activeId == null) return undefined;
        if (detailMatches && detail != null) return detail;
        const row = rows[activeIndex ?? -1];
        return row;
    }, [activeId, detail, detailMatches, rows, activeIndex]);

    const navigate = useCallback((delta: 1 | -1) => {
        if (isBusyRef?.current || loading) return;
        if (isDirty) {
            toast.warning("لديك تعديلات غير محفوظة — احفظ أو تجاهل التعديلات قبل الانتقال");
            return;
        }
        if (rows.length === 0) {
            toast.info(emptyMessage);
            return;
        }
        let next: number | null;
        if (activeIndex == null) {
            // من النموذج الفارغ: ↑ يحمّل أول سجل، ↓ بلا أثر
            if (delta === -1) return;
            next = 0;
        } else {
            const candidate = activeIndex + delta;
            if (candidate < 0) {
                // العودة للنموذج الفارغ متاحة في وضع الإضافة فقط؛ التعديل يتوقف عند الأول
                if (mode !== "create") return;
                next = null;
            } else if (candidate >= rows.length) {
                // نهاية القائمة — بلا ترحيل دائري
                return;
            } else {
                next = candidate;
            }
        }
        if (next === activeIndex) return;
        if (next === null) {
            pendingNavRef.current = null;
            setActiveIndex(null);
            form.reset({ ...emptyDefaults });
            return;
        }
        pendingNavRef.current = next;
        setActiveIndex(next);
    }, [isBusyRef, loading, isDirty, rows, activeIndex, mode, form, emptyDefaults, emptyMessage]);

    // بعد التنقل: تعبئة النموذج ببيانات السجل الجديد بمجرد وصولها
    useEffect(() => {
        if (!open) {
            pendingNavRef.current = null;
            return;
        }
        if (pendingNavRef.current == null) return;
        const targetRow = activeIndex != null ? rows[activeIndex] : undefined;
        if (!targetRow) {
            pendingNavRef.current = null;
            return;
        }
        const targetId = getId(targetRow);
        let source: R | D;
        if (fetchDetail) {
            if (!detailMatches || getDetailId?.(detail as D) !== targetId) return;
            source = detail as D;
        } else {
            source = targetRow;
        }
        form.reset(toFormValues(source));
        pendingNavRef.current = null;
        if (focusField) {
            setTimeout(() => form.setFocus(focusField as never), 150);
        }
    }, [open, activeIndex, rows, detail, detailMatches, fetchDetail, getDetailId, form, toFormValues, focusField, getId]);

    useRecordNavigationShortcut(open, navigate);

    return { activeIndex, activeId, activeRecord, detailReady, effectiveMode: effectiveModeOf(activeIndex, rows, getId, mode), navigate };
}

function effectiveModeOf<R>(
    activeIndex: number | null,
    rows: R[],
    getId: (row: R) => number,
    mode: "create" | "edit"
): "create" | "edit" {
    if (activeIndex != null && rows[activeIndex] != null && getId(rows[activeIndex]) > 0) return "edit";
    return mode;
}
