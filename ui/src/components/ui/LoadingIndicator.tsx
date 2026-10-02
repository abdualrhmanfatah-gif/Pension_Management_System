import { Loader } from "lucide-react";

import { cn } from "@/lib/utils";

interface LoadingIndicatorProps {
    message?: string;
    className?: string;
    size?: "sm" | "md" | "lg";
}

const SIZE_CLASSES = {
    sm: "py-4 text-xs",
    md: "py-8 text-sm",
    lg: "py-12 text-base",
} as const;

/**
 * LoadingIndicator — Reusable loading state for async data.
 * Replaces inline `animate-pulse font-bold` patterns across 15+ files.
 */
export function LoadingIndicator({
    message = "جاري التحميل...",
    className,
    size = "md",
}: LoadingIndicatorProps) {
    return (
        <div
            className={cn(
                "flex flex-col items-center justify-center gap-3 text-center text-muted-foreground animate-pulse font-bold",
                SIZE_CLASSES[size],
                className
            )}
            role="status"
            aria-live="polite"
        >
            <Loader className="w-6 h-6 animate-spin text-primary/40" aria-hidden="true" />
            <p>{message}</p>
        </div>
    );
}
