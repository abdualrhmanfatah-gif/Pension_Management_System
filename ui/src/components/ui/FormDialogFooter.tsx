import * as React from "react";

import { Button } from "@/components/ui/Button";
import type { ButtonVariant } from "@/components/ui/Dialog"; // استيراد النوع الموحد
import { cn } from "@/lib/utils";

export interface FormDialogFooterProps {
  isLoading?: boolean;
  disabled?: boolean;
  saveLabel?: string;
  cancelLabel?: string;
  saveAndNewLabel?: string;
  onSave: () => void;
  onCancel: () => void;
<<<<<<< HEAD
  onSaveAndNew?: () => void | Promise<void>;
=======
  onSaveAndNew?: () => void;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  variant?: ButtonVariant; // أصبح موحداً مع BaseFormDialog
  className?: string;
  showKeyboardHints?: boolean;
  saveIcon?: React.ReactNode;
}

export function FormDialogFooter({
  isLoading = false,
  disabled = false,
  saveLabel = "حفظ",
  cancelLabel = "إلغاء",
  saveAndNewLabel = "حفظ وإضافة آخر",
  onSave,
  onCancel,
  onSaveAndNew,
  variant = "primary",
  className,
  showKeyboardHints = true,
  saveIcon
}: FormDialogFooterProps) {
  return (
    <div
      className={cn("flex flex-col-reverse sm:flex-row items-center justify-end gap-2 sm:gap-3 w-full", className)}
    >
      {/* 1. Primary Action (Save / Semantic) */}
      <Button
        type="button"
<<<<<<< HEAD
        data-enter-chain-target
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        variant={variant}
        onClick={(e) => {
          e.preventDefault();
          onSave();
        }}
        loading={isLoading}
        disabled={disabled || isLoading}
        className="w-full sm:w-auto min-w-[140px] min-h-[44px] order-1 sm:order-3"
      >
        <span className="flex items-center justify-center gap-2 w-full">
          {saveIcon}
          {saveLabel}
          {showKeyboardHints && !isLoading && (
            <kbd className={cn(
<<<<<<< HEAD
              "hidden lg:inline-flex h-4 select-none items-center gap-1 rounded border px-1 font-sans text-xsmall font-medium transition-colors opacity-100",
              ["destructive", "primary", "success", "warning", "info"].includes(variant)
                ? "border-white bg-black/20 text-white" 
                : "border-muted-foreground bg-muted text-muted-foreground"
=======
              "hidden lg:inline-flex h-4 select-none items-center gap-1 rounded border px-1 font-sans text-xsmall font-medium transition-colors opacity-80",
              ["destructive", "primary", "success", "warning", "info"].includes(variant)
                ? "border-white/40 bg-black/20 text-white" 
                : "border-muted-foreground/30 bg-muted text-muted-foreground"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            )}>
              <span className="text-caption sm:text-xs">⌘</span>
              <span className="text-caption sm:text-xs">S</span>
            </kbd>
          )}
        </span>
      </Button>

      {/* 2. Optional: Save & New Action */}
      {onSaveAndNew && (
        <Button
          type="button"
          variant="outline"
          onClick={onSaveAndNew}
          disabled={isLoading}
          className="w-full sm:w-auto min-w-[140px] min-h-[44px] border-dashed border-2 hover:border-solid hover:bg-muted/50 order-2 sm:order-2"
        >
          <span className="flex items-center justify-center gap-2 w-full">
            {saveAndNewLabel}
            {showKeyboardHints && !isLoading && (
<<<<<<< HEAD
              <kbd className="hidden lg:inline-flex h-4 select-none items-center gap-1 rounded border bg-muted px-1 font-sans text-xsmall font-medium text-muted-foreground opacity-100">
=======
              <kbd className="hidden lg:inline-flex h-4 select-none items-center gap-1 rounded border bg-muted px-1 font-sans text-xsmall font-medium text-muted-foreground opacity-70">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
<span className="text-caption sm:text-xs">⇧</span>
              <span className="text-caption sm:text-xs">↵</span>
              </kbd>
            )}
          </span>
        </Button>
      )}

      {/* 3. Cancel Action */}
      <Button
        type="button"
        variant="outline"
        onClick={onCancel}
        disabled={isLoading}
<<<<<<< HEAD
        className="w-full sm:w-auto min-w-[110px] min-h-[44px] text-muted-foreground hover:text-foreground hover:bg-muted/50 border-border order-3 sm:order-1"
=======
        className="w-full sm:w-auto min-w-[110px] min-h-[44px] text-muted-foreground hover:text-foreground hover:bg-muted/50 border-border/60 order-3 sm:order-1"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
      >
        {cancelLabel}
      </Button>
    </div>
  );
<<<<<<< HEAD
}
=======
}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
