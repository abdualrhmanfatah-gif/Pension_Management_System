import type { ReactNode } from "react";

import { cn } from "@/lib/utils";

interface SummaryItemProps {
    label: string;
    value: ReactNode;
    className?: string;
    dir?: "ltr" | "rtl";
}

export function SummaryItem({ label, value, className, dir }: SummaryItemProps) {
    if (value === null || value === undefined || value === "") return null;

    return (
        <div className={cn("flex flex-col gap-1 text-end", className)}>
            <span className="text-xs text-muted-foreground">{label}</span>
            <span className="text-sm font-semibold" dir={dir}>
                {value}
            </span>
        </div>
    );
}

interface SummaryRowProps {
    children: ReactNode;
    className?: string;
}

export function SummaryRow({ children, className }: SummaryRowProps) {
    return (
        <div className={cn("grid grid-cols-1 sm:grid-cols-2 gap-2 sm:gap-4", className)}>
            {children}
        </div>
    );
}

interface SummaryCardProps {
    children: ReactNode;
    title?: string;
    icon?: ReactNode;
    className?: string;
}

export function SummaryCard({ children, title, icon, className }: SummaryCardProps) {
    return (
        <div className={cn("rounded-xl border bg-card p-4", className)}>
            {(title || icon) && (
                <div className="flex items-center gap-2 mb-4 pb-3 border-b border-muted">
                    {icon && <span className="text-primary">{icon}</span>}
                    {title && <h3 className="text-sm font-semibold">{title}</h3>}
                </div>
            )}
            <div className="space-y-4">{children}</div>
        </div>
    );
}
