import * as React from "react";
import type { DefaultValues, FieldErrors, FieldValues, UseFormReset } from "react-hook-form";

<<<<<<< HEAD
import { useConfirm } from "@/components/ui";
import type { ButtonVariant } from "@/components/ui/Dialog"; // استيراد النوع الموحد
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/Dialog";
<<<<<<< HEAD
import { useDialogKeyboard } from "@/constants/shortcuts";
import { useDialogReset } from "@/hooks/useDialogReset";
import { cn } from "@/lib/utils";

import { ErrorAlert } from "./ErrorAlert";
import { FormDialogFooter } from "./FormDialogFooter";
=======
import type { ButtonVariant } from "@/components/ui/Dialog"; // استيراد النوع الموحد
import { useDialogKeyboard } from "@/hooks/useDialogKeyboard";
import { useDialogReset } from "@/hooks/useDialogReset";
import { cn } from "@/lib/utils";
import { useConfirm } from "@/components/ui";

import { FormDialogFooter } from "./FormDialogFooter";
import { ErrorAlert } from "./ErrorAlert";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export interface BaseOverlayProps {
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
  isOpen?: boolean;
  onClose?: () => void;
}

export interface BaseFormDialogProps<T extends FieldValues = FieldValues> extends BaseOverlayProps {
  title: string;
  description?: string;
  size?: "sm" | "md" | "lg" | "xl" | "2xl" | "3xl" | "4xl" | "5xl" | "6xl" | "full";
  isLoading?: boolean;
  isDirty?: boolean;
  onSubmit?: (e?: React.BaseSyntheticEvent) => Promise<void> | void;
<<<<<<< HEAD
  onSaveAndNew?: () => void | Promise<void>;
=======
  onSaveAndNew?: () => void;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  children: React.ReactNode;
  submitLabel?: string;
  cancelLabel?: string;
  className?: string;
  formId?: string;
  resetOnOpen?: boolean;
  reset?: UseFormReset<T>;
  initialValues?: DefaultValues<T> | T;
  showGlobalError?: boolean;
  formErrors?: FieldErrors<T>;
  dirtyWarningMessage?: string;
  enableKeyboardShortcuts?: boolean;
  footer?: React.ReactNode;
  submitIcon?: React.ReactNode;
  submitVariant?: ButtonVariant; // تم توحيد النوع
  portal?: boolean;
}

export function BaseFormDialog<T extends FieldValues = FieldValues>({
  open: controlledOpen,
  onOpenChange: controlledOnOpenChange,
  isOpen: legacyOpen,
  onClose: legacyClose,
  title,
  description,
  size = "md",
<<<<<<< HEAD
  isLoading: providedIsLoading = false,
=======
  isLoading = false,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  isDirty = false,
  onSubmit,
  onSaveAndNew,
  children,
  footer,
  submitLabel = "حفظ",
  cancelLabel = "إلغاء",
  className,
  formId,
  resetOnOpen = true,
  reset,
  initialValues,
  showGlobalError = false,
  formErrors,
<<<<<<< HEAD
  dirtyWarningMessage = "لديك تغييرات غير محفوظة، هل أنت متأكد من الإغلاق؟",
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  enableKeyboardShortcuts = true,
  submitIcon,
  submitVariant = "primary",
  portal = true,
}: BaseFormDialogProps<T>) {
<<<<<<< HEAD
  const [submitting, setSubmitting] = React.useState(false);
  const submitInFlight = React.useRef(false);
  const dialogRef = React.useRef<HTMLDivElement>(null);
  const isLoading = providedIsLoading || submitting;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

  const open = controlledOpen !== undefined ? controlledOpen : !!legacyOpen;
  const onOpenChange = controlledOnOpenChange || ((v: boolean) => { if (!v) legacyClose?.(); });
  const { confirm, ConfirmDialogElement } = useConfirm();

  // 1. إصلاح الحلقة المفرغة: فصل طلب الإغلاق عن الإغلاق الفعلي
  const requestClose = React.useCallback(async () => {
    if (isDirty && !isLoading) {
<<<<<<< HEAD
      if (import.meta.env.DEV) {
        console.warn("[BaseFormDialog] unsaved-changes confirm", new Error("trace").stack);
      }
      const ok = await confirm({
        title: dirtyWarningMessage,
=======
      const ok = await confirm({
        title: "لديك تغييرات غير محفوظة، هل أنت متأكد من الإغلاق؟",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        variant: "warning",
      });
      if (ok) {
        onOpenChange?.(false);
      }
    } else if (!isLoading) {
      onOpenChange?.(false);
    }
<<<<<<< HEAD
  }, [isDirty, isLoading, onOpenChange, confirm, dirtyWarningMessage]);
=======
  }, [isDirty, isLoading, onOpenChange, confirm]);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

  const dummyReset = React.useCallback(() => {}, []);
  useDialogReset(
    open,
    (reset as UseFormReset<T>) || (dummyReset as unknown as UseFormReset<T>),
    initialValues,
    resetOnOpen
  );

  const autoId = React.useId();
  const internalFormId = formId || `form-${autoId.replace(/:/g, "")}`;

<<<<<<< HEAD
  const triggerSubmit = React.useCallback(async (event?: React.BaseSyntheticEvent) => {
    if (!onSubmit || isLoading || submitInFlight.current) return;
    submitInFlight.current = true;
    setSubmitting(true);
    try {
      await onSubmit(event);
    } catch {
      // The mutation/form owns its error UI. Keep the form open and allow retry.
    } finally {
      submitInFlight.current = false;
      setSubmitting(false);
    }
  }, [onSubmit, isLoading]);

  const triggerSaveAndNew = React.useCallback(async () => {
    if (!onSaveAndNew || isLoading || submitInFlight.current) return;
    submitInFlight.current = true;
    setSubmitting(true);
    try {
      await onSaveAndNew();
    } catch {
      // The mutation/form owns its error UI. Keep the form open and allow retry.
    } finally {
      submitInFlight.current = false;
      setSubmitting(false);
    }
  }, [onSaveAndNew, isLoading]);

=======
  const triggerSubmit = React.useCallback(() => {
    if (onSubmit && !isLoading) {
      onSubmit();
    }
  }, [onSubmit, isLoading]);

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  useDialogKeyboard({
    isOpen: enableKeyboardShortcuts ? open : false,
    isDirty,
    isLoading,
    onClose: requestClose,
    onSubmit: triggerSubmit,
<<<<<<< HEAD
    onSaveAndNew: triggerSaveAndNew,
    dialogRef,
=======
    onSaveAndNew,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  });

  const handleOpenChange = React.useCallback((openState: boolean) => {
    if (!openState) {
      requestClose();
    } else {
      onOpenChange?.(true);
    }
  }, [requestClose, onOpenChange]);

  const handleCancel = React.useCallback(() => {
    requestClose();
  }, [requestClose]);

  // ─── حظر زر الرجوع في المتصفح عند وجود تغييرات غير محفوظة ───
  const dirtyRef = React.useRef(isDirty);
<<<<<<< HEAD
  const requestCloseRef = React.useRef(requestClose);
  const loadingRef = React.useRef(isLoading);
  React.useEffect(() => {
    dirtyRef.current = isDirty;
    loadingRef.current = isLoading;
    requestCloseRef.current = requestClose;
  }, [isDirty, isLoading, requestClose]);

  React.useEffect(() => {
    if (!open || !isDirty) return;

    const token = {};
    window.history.pushState({ __dlgBlock: token }, "");

    const handlePopState = () => {
      // pop ناتج عن تنظيف نافذة أخرى (لا ازاحة إلى صفحة سابقة) — لا نطلب إغلاق.
      if (window.history.state?.__dlgBlock) return;
      if (dirtyRef.current && !loadingRef.current) {
        window.history.pushState({ __dlgBlock: token }, "");
=======
  dirtyRef.current = isDirty;
  const requestCloseRef = React.useRef(requestClose);
  requestCloseRef.current = requestClose;

  React.useEffect(() => {
    if (!open || !isDirty || isLoading) return;

    window.history.pushState({ __dlgBlock: true }, "");

    const handlePopState = () => {
      if (dirtyRef.current) {
        window.history.pushState({ __dlgBlock: true }, "");
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        requestCloseRef.current();
      }
    };

    window.addEventListener("popstate", handlePopState);

    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (dirtyRef.current) {
        e.preventDefault();
        e.returnValue = "";
      }
    };
    window.addEventListener("beforeunload", handleBeforeUnload);

    return () => {
      window.removeEventListener("popstate", handlePopState);
      window.removeEventListener("beforeunload", handleBeforeUnload);
      if (window.history.state?.__dlgBlock) {
        window.history.back();
      }
    };
<<<<<<< HEAD
  }, [open, isDirty]);
=======
  }, [open, isDirty, isLoading]);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

  // 2. إصلاح مشكلة النماذج: الحاوية دائماً form
  const hasErrors = formErrors && Object.keys(formErrors).length > 0;

  return (
    <>
      <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
<<<<<<< HEAD
        ref={dialogRef}
        aria-labelledby={`${internalFormId}-title`}
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        size={size}
        className={cn("flex flex-col p-0 overflow-visible outline-none", className)}
        removePadding // تمرير البروب بدلاً من البحث في النصوص
        portal={portal}
        onOpenAutoFocus={(e) => {
          const dialogEl = e.target as HTMLElement;
          const autoFocusEl = dialogEl.querySelector<HTMLElement>("[autofocus], [data-autofocus]");
          if (autoFocusEl) {
            e.preventDefault();
            setTimeout(() => autoFocusEl.focus(), 0);
          }
        }}
<<<<<<< HEAD
=======
        onKeyDown={(e) => {
          if ((e.ctrlKey || e.metaKey) && e.nativeEvent.code === "KeyS") {
            e.preventDefault();
            e.stopPropagation();
            if (!isLoading && enableKeyboardShortcuts) {
              triggerSubmit();
            }
          }
        }}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        onEscapeKeyDown={(e) => {
          // السماح بالإغلاق دائماً لتجنب حجز المستخدم
          if (isLoading) e.preventDefault(); 
        }}
        onInteractOutside={(e) => {
          // منع الإغلاق بالضغط خارج النافذة إذا كان هناك تغييرات أو تحميل
          if (isDirty || isLoading) e.preventDefault();
        }}
      >
        <form
          id={internalFormId}
          onSubmit={(e: React.FormEvent<HTMLFormElement>) => {
            e.preventDefault();
<<<<<<< HEAD
            void triggerSubmit(e);
=======
            if (!isLoading) onSubmit?.(e);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
          }}
          className="flex flex-col h-full min-h-0"
          noValidate
        >
          <DialogHeader className="shrink-0">
<<<<<<< HEAD
            <DialogTitle id={`${internalFormId}-title`} className="flex items-center gap-2 text-sm sm:text-base">
=======
            <DialogTitle className="flex items-center gap-2 text-sm sm:text-base">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
              {title}
            </DialogTitle>
            {description && (
              <DialogDescription className="max-w-full sm:max-w-[85%] text-xs sm:text-sm leading-relaxed">
                {description}
              </DialogDescription>
            )}
          </DialogHeader>

          <div className="flex-1 overflow-y-auto min-h-0 bg-muted/10 relative custom-scrollbar -webkit-overflow-scrolling-touch">
            <div className="p-[var(--dlg-section-p)] space-y-[var(--dlg-body-gap)] pb-4 sm:pb-0">
              {showGlobalError && hasErrors && (
                <ErrorAlert
                  message="يرجى مراجعة البيانات الإلزامية وتصحيح الأخطاء للمتابعة."
<<<<<<< HEAD
                  className="border-destructive animate-in slide-in-from-top-2"
=======
                  className="border-destructive/20 animate-in slide-in-from-top-2"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                />
              )}
              {children}
            </div>
          </div>

          <DialogFooter className="shrink-0">
            {footer !== undefined ? (
              footer
            ) : (
              <FormDialogFooter
                isLoading={isLoading}
                saveLabel={submitLabel}
                cancelLabel={cancelLabel}
                onSave={triggerSubmit}
                onCancel={handleCancel}
<<<<<<< HEAD
                onSaveAndNew={triggerSaveAndNew}
=======
                onSaveAndNew={onSaveAndNew}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                showKeyboardHints={enableKeyboardShortcuts}
                saveIcon={submitIcon}
                variant={submitVariant}
                className="w-full"
              />
            )}
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
    {ConfirmDialogElement}
    </>
  );
<<<<<<< HEAD
}
=======
}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
