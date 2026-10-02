import { AlertCircle, AlertTriangle, Info } from "lucide-react";

import { Button } from "@/components/ui/Button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/Dialog";
import { cn } from "@/lib/utils";

// ─── Types ──────────────────────────────────────────────

export type ConfirmVariant = "default" | "warning" | "danger" | "info";

export interface ConfirmDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: ConfirmVariant;
  isLoading?: boolean;
  onConfirm: () => void | Promise<void>;
}

// ─── Component ──────────────────────────────────────────

/**
 * Standardized Confirmation Dialog.
 * Optimized for critical user decisions with distinct variants.
 */
export function ConfirmDialog({
  open,
  onOpenChange,
  title,
  description,
  confirmLabel = "تأكيد",
  cancelLabel = "إلغاء",
  variant = "default",
  isLoading = false,
  onConfirm,
}: ConfirmDialogProps) {
  
  const handleConfirm = async () => {
    try {
      await onConfirm();
      onOpenChange(false);
    } catch (error) {
      console.error("Confirmation error:", error);
    }
  };

  const Icon = variant === "danger" || variant === "warning" 
    ? AlertTriangle 
    : variant === "info" 
    ? Info 
    : AlertCircle;

  const iconColor = {
    default: "text-primary",
    warning: "text-amber-500",
    danger: "text-red-500",
    info: "text-blue-500",
  }[variant];

  const buttonVariant = variant === "danger" ? "destructive" : "primary";

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent size="sm" className="p-0 overflow-hidden">
        <DialogHeader className="border-b-0 p-4 sm:p-6">
          <div className="flex items-start gap-3 sm:gap-4">
            <div className={cn("p-2 sm:p-3 rounded-full bg-background border shrink-0", iconColor)}>
              <Icon className="w-5 h-5 sm:w-6 sm:h-6" />
            </div>
            <div className="space-y-1 min-w-0">
              <DialogTitle className="text-lg sm:text-xl font-bold tracking-tight">
                {title}
              </DialogTitle>
              {description && (
                <DialogDescription className="text-muted-foreground text-sm leading-relaxed">
                  {description}
                </DialogDescription>
              )}
            </div>
          </div>
        </DialogHeader>

        <DialogFooter className="p-4 sm:p-6">
          <Button
            type="button"
            variant="ghost"
            onClick={() => onOpenChange(false)}
            disabled={isLoading}
            className="min-w-[80px] min-h-[44px]"
          >
            {cancelLabel}
          </Button>
          <Button
            type="button"
            variant={buttonVariant}
            onClick={handleConfirm}
            loading={isLoading}
            className="min-w-[100px] min-h-[44px]"
          >
            {confirmLabel}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
