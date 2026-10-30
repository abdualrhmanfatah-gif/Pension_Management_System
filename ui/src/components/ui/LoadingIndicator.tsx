import { Loader } from "lucide-react";
<<<<<<< HEAD

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
            <Loader className="w-6 h-6 animate-spin text-primary" aria-hidden="true" />
=======
            <Loader className="w-6 h-6 animate-spin text-primary/40" aria-hidden="true" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            <p>{message}</p>
        </div>
    );
}
