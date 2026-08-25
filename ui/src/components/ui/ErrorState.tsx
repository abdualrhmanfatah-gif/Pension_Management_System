import { AlertTriangle } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "./Button";

interface ErrorStateProps {
    title?: string;
    message: string;
    onRetry?: () => void;
    className?: string;
}

/**
 * ErrorState — Reusable error state for failed data fetches.
 * Replaces inline error patterns across 27+ files.
 */
export function ErrorState({
    title = "حدث خطأ",
    message,
    onRetry,
    className,
}: ErrorStateProps) {
    return (
        <div
            className={cn(
                "flex flex-col items-center justify-center p-6 sm:p-12 text-center",
                "border-2 border-dashed border-destructive/20 rounded-2xl bg-destructive/5",
                className
            )}
            role="alert"
        >
            <div className="flex items-center justify-center w-14 h-14 rounded-full bg-destructive/10 mb-4">
                <AlertTriangle className="w-7 h-7 text-destructive/50" aria-hidden="true" />
            </div>

            <h3 className="text-lg font-bold text-foreground mb-2">{title}</h3>
            <p className="text-sm text-muted-foreground max-w-xs mb-6 leading-relaxed">{message}</p>

            {onRetry && (
                <Button variant="outline" onClick={onRetry} className="gap-2">
                    إعادة المحاولة
                </Button>
            )}
        </div>
    );
}
