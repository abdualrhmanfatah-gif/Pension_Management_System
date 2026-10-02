import React from "react";

import { cn } from "@/lib/utils";

export type PrintFieldVariant = 'inline' | 'column' | 'row';

export interface PrintFieldProps {
    label: string;
    value: React.ReactNode;
    variant?: PrintFieldVariant;
    fullWidth?: boolean;
    className?: string;
}

export function PrintField({ label, value, variant = 'inline', fullWidth, className }: PrintFieldProps) {
    if (variant === 'row') {
        return (
            <div className={cn("flex items-start gap-2 p-2", !fullWidth && "border-b border-gray-200 print:border-black even:bg-gray-50/50", className)}>
                <span className="font-bold text-[10px] text-gray-600 uppercase tracking-wider whitespace-nowrap min-w-[80px]">
                    {label}
                </span>
                <span className="text-[11px] font-semibold text-gray-900">
                    {value || "—"}
                </span>
            </div>
        );
    }

    if (variant === 'column') {
        return (
            <div className={cn("flex flex-col gap-0.5 border-b border-dashed border-gray-300 print:border-gray-400 pb-1", className)}>
                <span className="font-bold text-gray-500 print:text-gray-700 text-[10px]">{label}</span>
                <span className="font-semibold text-[12px]">{value || "—"}</span>
            </div>
        );
    }

    // Default inline
    return (
        <div className={cn("flex gap-1 text-[11px]", className)}>
            <span className="font-semibold">{label}:</span>
            <span>{value || "—"}</span>
        </div>
    );
}
