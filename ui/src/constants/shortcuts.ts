/**
 * Central keyboard shortcuts registry.
 * All shortcut definitions, matching logic, and shortcut hooks live here.
 */

import { type RefObject,useCallback, useEffect, useSyncExternalStore } from "react";
import { useNavigate } from "react-router-dom";

import { claimDialogEvent, isAnyDialogOpen,isTopmostDialog } from "@/lib/dialog-stack";
import { useAuthStore } from "@/stores/authStore";

// ─── Types ───────────────────────────────────────────────────────────

export interface ShortcutDef {
    label: string;
    code?: string | string[];
    key?: string | string[];
    ctrl?: boolean;
    alt?: boolean;
    shift?: boolean;
}

interface DialogKeyboardOptions {
    isOpen: boolean;
    isDirty: boolean;
    onClose: () => void;
    onSubmit: () => void;
    onSaveAndNew?: () => void | Promise<void>;
    isLoading?: boolean;
    dialogRef: RefObject<HTMLElement | null>;
}

// ─── Shortcut Definitions ────────────────────────────────────────────

export const SHORTCUT_CREATE_NEW: ShortcutDef = {
    label: "فتح نافذة إنشاء جديد",
    code: "KeyA",
    key: ["a", "A"],
    alt: true,
};

export const SHORTCUT_GLOBAL_NEW_FILE: ShortcutDef = {
    label: "فتح نموذج إنشاء ملف تقاعدي",
    code: ["KeyN", "KeyQ"],
    key: ["n", "N", "q", "Q"],
    alt: true,
};

export const SHORTCUT_GLOBAL_SEARCH: ShortcutDef = {
    label: "فتح نافذة البحث السريع",
    code: "KeyF",
    ctrl: true,
    shift: true,
};

export const SHORTCUT_SAVE: ShortcutDef = {
    label: "حفظ",
    code: "KeyS",
    ctrl: true,
};

export const SHORTCUT_SUBMIT: ShortcutDef = {
    label: "تأكيد وحفظ",
    key: "Enter",
    ctrl: true,
};

export const SHORTCUT_SAVE_AND_NEW: ShortcutDef = {
    label: "حفظ وإضافة جديد",
    key: "Enter",
    shift: true,
};

export const SHORTCUT_NEXT_TAB: ShortcutDef = {
    label: "التبويب التالي",
    key: "Tab",
    ctrl: true,
};

export const SHORTCUT_PREV_TAB: ShortcutDef = {
    label: "التبويب السابق",
    key: "Tab",
    ctrl: true,
    shift: true,
};

// ─── Record navigation inside a form dialog ──────────────────────────
// Alt+↑ يحمّل السجل التالي من قائمة الملف داخل الحوار المفتوح
// (أول ضغطة من نموذج فارغ = أول سجل)، Alt+↓ السجل السابق.
// المستهلكون: المستحقون، الوكلاء، الأوصياء، دفاتر الصرف —
// راجع useRecordNavigationShortcut و useRecordNavigation أدناه.

export const SHORTCUT_RECORD_NEXT: ShortcutDef = {
    label: "السجل التالي داخل النموذج",
    key: "ArrowUp",
    alt: true,
};

export const SHORTCUT_RECORD_PREV: ShortcutDef = {
    label: "السجل السابق داخل النموذج",
    key: "ArrowDown",
    alt: true,
};

// ─── File Details: main sections + sub tabs ──────────────────────────
// Alt+1..4 للانتقال المباشر للأقسام الرئيسية في صفحة تفاصيل الملف،
// و Alt+ArrowLeft/ArrowRight للتنقل بين التبويبات الفرعية (RTL: اليسار = التالي).

export const SHORTCUT_FILE_SECTION_1: ShortcutDef = {
    label: "القسم الرئيسي الأول: البيانات الأساسية",
    code: "Digit1",
    alt: true,
};

export const SHORTCUT_FILE_SECTION_2: ShortcutDef = {
    label: "القسم الرئيسي الثاني: الوثائق والإجراءات",
    code: "Digit2",
    alt: true,
};

export const SHORTCUT_FILE_SECTION_3: ShortcutDef = {
    label: "القسم الرئيسي الثالث: المالية والمعاشات",
    code: "Digit3",
    alt: true,
};

export const SHORTCUT_FILE_SECTION_4: ShortcutDef = {
    label: "القسم الرئيسي الرابع: المراجعة والاعتماد",
    code: "Digit4",
    alt: true,
};

export const SHORTCUT_FILE_NEXT_SUB_TAB: ShortcutDef = {
    label: "التبويب الفرعي التالي",
    key: "ArrowLeft",
    alt: true,
};

export const SHORTCUT_FILE_PREV_SUB_TAB: ShortcutDef = {
    label: "التبويب الفرعي السابق",
    key: "ArrowRight",
    alt: true,
};

/** أكواد الأرقام المقبولة لأقسام الملف (لوحة رئيسية + رقمية) */
export const FILE_SECTION_CODES: Record<string, number> = {
    Digit1: 0,
    Digit2: 1,
    Digit3: 2,
    Digit4: 3,
    Numpad1: 0,
    Numpad2: 1,
    Numpad3: 2,
    Numpad4: 3,
};

// ─── Matcher ─────────────────────────────────────────────────────────

export function matchesShortcut(e: KeyboardEvent, def: ShortcutDef): boolean {
    if (def.ctrl && !(e.ctrlKey || e.metaKey)) return false;
    if (def.alt && !e.altKey) return false;
    if (def.shift && !e.shiftKey) return false;
    if (!def.ctrl && (e.ctrlKey || e.metaKey)) return false;
    if (!def.alt && e.altKey) return false;

    if (def.code) {
        if (Array.isArray(def.code)) {
            if (def.code.includes(e.code)) return true;
        } else if (e.code === def.code) {
            return true;
        }
    }
    if (def.key) {
        if (Array.isArray(def.key)) {
            if (def.key.some((k) => k === e.key || k.toLowerCase() === e.key?.toLowerCase())) return true;
        } else if (e.key === def.key || e.key?.toLowerCase() === def.key.toLowerCase()) {
            return true;
        }
    }
    return false;
}

// ─── Hooks ───────────────────────────────────────────────────────────

/**
 * Alt+A shortcut for opening create/add dialogs.
 */
export function useCreateShortcut(handler: () => void, enabled: boolean) {
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (matchesShortcut(e, SHORTCUT_CREATE_NEW)) {
                e.preventDefault();
                e.stopPropagation();
                if (enabled) handler();
            }
        };
        document.addEventListener("keydown", handleKeyDown, true);
        return () => document.removeEventListener("keydown", handleKeyDown, true);
    }, [handler, enabled]);
}

/**
 * Dialog keyboard shortcuts: Ctrl+S, Ctrl+Enter, Shift+Enter.
 * Handles dialog stack, textarea awareness, and loading guards.
 */
export function useDialogKeyboard({
    isOpen, onSubmit, onSaveAndNew, isLoading, dialogRef,
}: DialogKeyboardOptions) {
    const handleKeyDown = useCallback(
        (event: KeyboardEvent) => {
            if (!isOpen || event.defaultPrevented || !isTopmostDialog(dialogRef.current)) return;

            const target = event.target as HTMLElement;
            const isTextarea = target.tagName === "TEXTAREA";

            // Ctrl/Cmd + S
            if (matchesShortcut(event, SHORTCUT_SAVE)) {
                event.preventDefault();
                if (!isLoading && !event.repeat && claimDialogEvent(event)) onSubmit();
                return;
            }

            // Ctrl/Cmd + Enter (+ optional Shift for Save & New)
            if ((event.ctrlKey || event.metaKey) && event.key === "Enter") {
                if (isTextarea) return;
                event.preventDefault();
                if (!isLoading && !event.repeat && claimDialogEvent(event)) {
                    if (event.shiftKey && onSaveAndNew) void onSaveAndNew();
                    else onSubmit();
                }
                return;
            }

            // Shift + Enter: Save & New
            if (event.shiftKey && event.key === "Enter" && onSaveAndNew) {
                if (isTextarea) return;
                event.preventDefault();
                if (!isLoading && !event.repeat && claimDialogEvent(event)) void onSaveAndNew();
            }
        },
        [isOpen, onSubmit, onSaveAndNew, isLoading, dialogRef],
    );

    useEffect(() => {
        if (isOpen) {
            window.addEventListener("keydown", handleKeyDown, true);
        }
        return () => window.removeEventListener("keydown", handleKeyDown, true);
    }, [isOpen, handleKeyDown]);
}

/**
 * Alt+↑ / Alt+↓ للتنقل بين سجلات قائمة الملف داخل حوار مفتوح.
 * `onNavigate(+1)` = التالي، `onNavigate(-1)` = السابق.
 * منطق الحدث (تحذير dirty، نهاية القائمة، وضع الإضافة) مسؤولية المستهلك
 * — عادة عبر useRecordNavigation.
 */
export function useRecordNavigationShortcut(
    isOpen: boolean,
    onNavigate: (delta: 1 | -1) => void,
    enabled = true
) {
    const handleKeyDown = useCallback(
        (event: KeyboardEvent) => {
            if (!isOpen || !enabled || event.defaultPrevented || event.repeat) return;

            if (matchesShortcut(event, SHORTCUT_RECORD_NEXT)) {
                if (!claimDialogEvent(event)) return;
                event.preventDefault();
                onNavigate(1);
            } else if (matchesShortcut(event, SHORTCUT_RECORD_PREV)) {
                if (!claimDialogEvent(event)) return;
                event.preventDefault();
                onNavigate(-1);
            }
        },
        [isOpen, enabled, onNavigate]
    );

    useEffect(() => {
        if (isOpen && enabled) {
            window.addEventListener("keydown", handleKeyDown, true);
        }
        return () => window.removeEventListener("keydown", handleKeyDown, true);
    }, [isOpen, enabled, handleKeyDown]);
}

// ─── Global "new retirement file" intent ─────────────────────────────
// Alt+N / Alt+Q from any page: sets a one-shot intent then navigates to /files.
// The intent is a module-level store (survives navigation) consumed by the
// pages that render the create dialog.

let createFileIntent = false;
const createFileIntentListeners = new Set<() => void>();

function emitCreateFileIntent() {
    createFileIntentListeners.forEach((listener) => listener());
}

function subscribeCreateFileIntent(listener: () => void) {
    createFileIntentListeners.add(listener);
    return () => {
        createFileIntentListeners.delete(listener);
    };
}

export function requestCreateFileIntent() {
    createFileIntent = true;
    emitCreateFileIntent();
}

export function consumeCreateFileIntent() {
    if (!createFileIntent) return;
    createFileIntent = false;
    emitCreateFileIntent();
}

export function useCreateFileIntent() {
    const pending = useSyncExternalStore(
        subscribeCreateFileIntent,
        () => createFileIntent,
        () => false,
    );
    return { pending, consume: consumeCreateFileIntent };
}

/**
 * Alt+N / Alt+Q — opens the "create retirement file" dialog from any page.
 * Mount once inside the router (App.tsx GlobalShortcuts).
 */
export function useGlobalCreateFileShortcut() {
    const navigate = useNavigate();
    const permissions = useAuthStore((s) => s.permissions);
    const permissionsLoaded = useAuthStore((s) => s.permissionsLoaded);
    const isHydrated = useAuthStore((s) => s.isHydrated);

    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (!matchesShortcut(e, SHORTCUT_GLOBAL_NEW_FILE)) return;
            if (e.repeat || isAnyDialogOpen()) return;
            if (!isHydrated || !permissionsLoaded || !permissions.includes("File.Create")) return;

            e.preventDefault();
            e.stopPropagation();
            requestCreateFileIntent();
            navigate("/files");
        };
        document.addEventListener("keydown", handleKeyDown, true);
        return () => document.removeEventListener("keydown", handleKeyDown, true);
    }, [navigate, permissions, permissionsLoaded, isHydrated]);
}
