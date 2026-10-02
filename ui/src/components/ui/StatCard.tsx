import type { ReactNode } from "react";

import { cn } from "@/lib/utils";

import { Card } from "./Card";
import { Skeleton } from "./Skeleton";

interface StatCardProps {
    title: string;
    value: string | number;
    icon: ReactNode;
    description?: string;
    loading?: boolean;
    isLoading?: boolean;
    variant?: "blue" | "orange" | "emerald" | "purple" | "rose" | "amber" | "cyan" | "indigo" | "teal" | "lime" | "success" | "warning" | "destructive" | "muted" | "default" | "error" | "info";
    className?: string;
    trend?: {
        value: number;
        isPositive: boolean;
    };
}

const variantConfig: Record<string, { icon: string; gradient: string; blur: string }> = {
    blue: { icon: "bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/20", gradient: "bg-gradient-to-l from-blue-500 to-blue-600", blur: "bg-blue-500" },
    orange: { icon: "bg-orange-500/10 text-orange-600 dark:text-orange-400 border-orange-500/20", gradient: "bg-gradient-to-l from-orange-500 to-orange-600", blur: "bg-orange-500" },
    emerald: { icon: "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20", gradient: "bg-gradient-to-l from-emerald-500 to-emerald-600", blur: "bg-emerald-500" },
    purple: { icon: "bg-purple-500/10 text-purple-600 dark:text-purple-400 border-purple-500/20", gradient: "bg-gradient-to-l from-purple-500 to-purple-600", blur: "bg-purple-500" },
    rose: { icon: "bg-rose-500/10 text-rose-600 dark:text-rose-400 border-rose-500/20", gradient: "bg-gradient-to-l from-rose-500 to-rose-600", blur: "bg-rose-500" },
    amber: { icon: "bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/20", gradient: "bg-gradient-to-l from-amber-500 to-amber-600", blur: "bg-amber-500" },
    cyan: { icon: "bg-cyan-500/10 text-cyan-600 dark:text-cyan-400 border-cyan-500/20", gradient: "bg-gradient-to-l from-cyan-500 to-cyan-600", blur: "bg-cyan-500" },
    indigo: { icon: "bg-indigo-500/10 text-indigo-600 dark:text-indigo-400 border-indigo-500/20", gradient: "bg-gradient-to-l from-indigo-500 to-indigo-600", blur: "bg-indigo-500" },
    teal: { icon: "bg-teal-500/10 text-teal-600 dark:text-teal-400 border-teal-500/20", gradient: "bg-gradient-to-l from-teal-500 to-teal-600", blur: "bg-teal-500" },
    lime: { icon: "bg-lime-500/10 text-lime-600 dark:text-lime-400 border-lime-500/20", gradient: "bg-gradient-to-l from-lime-500 to-lime-600", blur: "bg-lime-500" },
    success: { icon: "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20", gradient: "bg-gradient-to-l from-emerald-500 to-emerald-600", blur: "bg-emerald-500" },
    warning: { icon: "bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/20", gradient: "bg-gradient-to-l from-amber-500 to-amber-600", blur: "bg-amber-500" },
    destructive: { icon: "bg-rose-500/10 text-rose-600 dark:text-rose-400 border-rose-500/20", gradient: "bg-gradient-to-l from-rose-500 to-rose-600", blur: "bg-rose-500" },
    muted: { icon: "bg-muted text-muted-foreground border-border", gradient: "bg-gradient-to-l from-muted-foreground/40 to-muted-foreground/60", blur: "bg-muted-foreground" },
    default: { icon: "bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/20", gradient: "bg-gradient-to-l from-blue-500 to-blue-600", blur: "bg-blue-500" },
    error: { icon: "bg-rose-500/10 text-rose-600 dark:text-rose-400 border-rose-500/20", gradient: "bg-gradient-to-l from-rose-500 to-rose-600", blur: "bg-rose-500" },
    info: { icon: "bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/20", gradient: "bg-gradient-to-l from-blue-500 to-blue-600", blur: "bg-blue-500" },
};

export function StatCard({
    title,
    value,
    icon,
    description,
    loading,
    isLoading,
    variant = "blue",
    className,
    trend
}: StatCardProps) {
    const isBusy = loading ?? isLoading ?? false;
    const v = variantConfig[variant] ?? variantConfig.blue;

    return (
        <Card className={cn("p-3 sm:p-5 border-border bg-card overflow-hidden relative group transition-all duration-300", className)}>
            {/* Gradient accent bar */}
            <div className={cn("absolute top-0 start-0 end-0 h-1", v.gradient)} />
            <div className="flex items-center justify-between relative z-10 gap-2 sm:gap-3">
                <div className="space-y-1 sm:space-y-2 flex-1 min-w-0">
                    <p className="text-caption sm:text-xs font-bold text-muted-foreground uppercase tracking-wider line-clamp-2">{title}</p>
                    {isBusy ? (
                        <Skeleton className="h-6 sm:h-8 w-20 sm:w-24" />
                    ) : (
                        <div className="flex items-baseline gap-1.5 sm:gap-2 flex-wrap">
                            <h3 className="text-lg sm:text-2xl font-bold text-foreground tracking-tight break-keep" dir="rtl">{value}</h3>
                            {trend && (
                                <span className={cn(
                                    "text-xsmall sm:text-xs font-bold px-1 sm:px-1.5 py-0.5 rounded-full whitespace-nowrap shrink-0",
                                    trend.isPositive ? "bg-emerald-100 text-emerald-700" : "bg-rose-100 text-rose-700"
                                )}>
                                    {trend.isPositive ? "+" : "-"}{trend.value}%
                                </span>
                            )}
                        </div>
                    )}
                    {description && (
                        <p className="text-xsmall sm:text-xs text-muted-foreground font-medium opacity-80 truncate">{description}</p>
                    )}
                </div>
                <div className={cn("shrink-0 p-2 sm:p-2.5 rounded-lg sm:rounded-xl transition-all duration-300 group-hover:scale-110 group-hover:rotate-3", v.icon)}>
                    {icon}
                </div>
            </div>
            {/* Background Decorative Element */}
            <div className={cn("absolute -bottom-6 -end-6 w-20 sm:w-24 h-20 sm:h-24 blur-[40px] opacity-20 pointer-events-none transition-opacity duration-500 group-hover:opacity-40", v.blur)} />
        </Card>
    );
}
