import { useCallback,useEffect } from "react";

interface KeyboardOptions {
  /** Whether the dialog is currently visible. */
  isOpen: boolean;
  /** Whether the form has unsaved changes. */
  isDirty: boolean;
  /** Callback to close the dialog. */
  onClose: () => void;
  /** Primary submission handler. */
  onSubmit: () => void;
  /** Optional handler for "Save & New" workflow. */
  onSaveAndNew?: () => void;
  /** Disables shortcuts while processing. */
  isLoading?: boolean;
}

/**
 * Robust Keyboard Shortcut Handler for Standardized Dialogs.
 * 
 * Features:
 * - Esc: Closes dialog with a dirty-state check. Blocks if a Select/Dropdown is open.
 * - Ctrl/Cmd + S: Triggers primary submission (prevents browser "Save Page").
 * - Ctrl/Cmd + Enter: Triggers primary submission.
 * - Ctrl/Cmd + Shift + Enter: Triggers "Save & New" if supported.
 * - Context Awareness: Smartly ignores Enter-shortcuts inside textareas to preserve multi-line editing.
 * - Uses event.code for letter keys to work with any keyboard layout (Arabic, etc).
 */
export function useDialogKeyboard({
  isOpen, isDirty, onClose, onSubmit, onSaveAndNew, isLoading
}: KeyboardOptions) {

  const handleKeyDown = useCallback(
    (event: KeyboardEvent) => {
      if (!isOpen || isLoading) return;

      const target = event.target as HTMLElement;
      const isTextarea = target.tagName === "TEXTAREA";
      
      // 1. ESC: Standard Close with Safety Warning
      if (event.key === "Escape") {
        // RADIX-UI CHECK: If a popup (Select, Tooltip, etc.) is open, let it handle Esc
        const isPopperOpen = document.querySelector('[data-radix-popper-content-wrapper]');
        if (isPopperOpen) return;

        if (isDirty) {
          event.preventDefault();
          // Semantic confirmation for data loss prevention
          if (window.confirm("لديك تغييرات غير محفوظة، هل أنت متأكد من الإغلاق؟")) {
            onClose();
          }
        } else {
          onClose();
        }
      }

      // 2. Ctrl/Cmd + S: Universal Save shortcut
      // Uses event.code ("KeyS") instead of event.key ("s") to work regardless of keyboard layout
      // (Arabic keyboard sends "س" as event.key, but event.code is always "KeyS")
      if ((event.ctrlKey || event.metaKey) && event.code === "KeyS") {
        event.preventDefault();
        onSubmit();
      }

      // 3. Submit Shortcuts (Ctrl/Cmd + Enter)
      if ((event.ctrlKey || event.metaKey) && event.key === "Enter") {
        // IGNORE inside textarea: User might want Ctrl+Enter for other purposes or we avoid conflict
        // with standard textarea behavior where plain Enter is newline.
        if (isTextarea) return;

        event.preventDefault();
        onSubmit();
      }

      // 4. Save & New: Shift + Enter
      if (event.shiftKey && event.key === "Enter" && onSaveAndNew) {
        if (isTextarea) return;
        event.preventDefault();
        onSaveAndNew();
      }
    },
    [isOpen, isDirty, onClose, onSubmit, onSaveAndNew, isLoading]
  );

  useEffect(() => {
    if (isOpen) {
      // Capture phase ensures we intercept browser shortcuts (e.g. Ctrl+S) before the browser handles them
      window.addEventListener("keydown", handleKeyDown, true);
    }
    return () => window.removeEventListener("keydown", handleKeyDown, true);
  }, [isOpen, handleKeyDown]);
}
