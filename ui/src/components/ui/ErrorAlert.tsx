import { AlertCircle } from "lucide-react";

import { cn } from "@/lib/utils";

interface ErrorAlertProps {
  /** Optional error message. If empty, component renders nothing. */
  message?: string;
  /** Custom classes for the alert container. */
  className?: string;
}

/**
 * Standardized Error Display for Dialogs and Forms.
 * 
 * * Features:
 * - Institutional light-red palette.
 * - Semantic Alert icon.
 * - Entrance animation (Slide-down + Fade).
 */
export function ErrorAlert({ message, className }: ErrorAlertProps) {
  if (!message) return null;

  return (
    <div
      className={cn(
        "bg-red-50/60 border border-red-200 text-red-900 px-5 py-4 rounded-2xl flex items-start gap-4 text-body animate-in fade-in slide-in-from-top-1 backdrop-blur-sm",
        className
      )}
      role="alert"
    >
      <div className="shrink-0 p-2 bg-red-100/80 rounded-xl border border-white">
        <AlertCircle className="w-5 h-5 text-red-600 stroke-[2.5px]" />
      </div>
      <div className="flex-1 flex flex-col pt-0.5">
        <span className="font-bold mb-0.5">خطأ في الإدخال</span>
        <span className="text-red-800 leading-relaxed">{message}</span>
      </div>
    </div>
  );
}
