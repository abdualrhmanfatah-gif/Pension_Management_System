import type { ReactNode } from "react";

import { cn } from "@/lib/utils";

// ─── Types ─────────────────────────────────────────────

interface InfoPairProps {
    label: string;
    value: ReactNode;
    icon?: ReactNode;
    dir?: "ltr" | "rtl" | "auto";
    className?: string;
    /** Compact layout for sidebar (row), or stacked for main content (column) */
    layout?: "row" | "column";
    /** Use monospace font for the value */
    mono?: boolean;
}

// ─── Component ─────────────────────────────────────────

function InfoPair({ label, value, icon, dir, className, layout = "column", mono }: InfoPairProps) {
    if (value === null || value === undefined || value === "") return null;

    const isPrimitive = typeof value === "string" || typeof value === "number";
    const titleText = isPrimitive ? String(value) : undefined;

    if (layout === "row") {
        return (
            <div
                className={cn(
                    "flex justify-between items-center flex-wrap gap-2 text-base border-b border-muted/50 py-3 last:border-0 last:pb-0",
                    className
                )}
            >
                <div className="flex items-center gap-2">
                    {icon && <span className="text-muted-foreground/60">{icon}</span>}
                    <span className="text-muted-foreground font-medium">{label}</span>
                </div>
                <span
                    className={cn("font-semibold text-foreground", mono && "font-mono")}
                    dir={dir}
                    title={titleText}
                >
                    {value}
                </span>
            </div>
        );
    }

    return (
        <div className={cn(className)}>
            <div className="flex items-center gap-1.5 mb-0.5">
                {icon && <span className="text-muted-foreground/60">{icon}</span>}
                <p className="text-xs text-muted-foreground">{label}</p>
            </div>
            <p className={cn("text-sm font-medium", mono && "font-mono")} dir={dir}>
                {value}
            </p>
        </div>
    );
}

export { InfoPair };
export type { InfoPairProps };