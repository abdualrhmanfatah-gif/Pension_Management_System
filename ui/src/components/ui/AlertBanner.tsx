/**
 * AlertBanner — A dismissable alert strip at the top of a page/section.
 *
 * Usage:
 *   <AlertBanner
 *       variant="warning"
 *       icon={<AlertTriangle />}
 *       message="يوجد 3 وثائق ناقصة"
 *       action={{ label: "عرض النواقص", onClick: () => {} }}
 *   />
 */

import { ChevronDown, X } from "lucide-react";
import React, { useState } from "react";

import { cn } from "@/lib/utils";

import { Button } from "./Button";

type AlertVariant = "warning" | "info" | "danger" | "success" | "destructive";

interface AlertBannerProps {
    variant?: AlertVariant;
    icon?: React.ReactNode;
    message?: React.ReactNode;
    title?: React.ReactNode;
    children?: React.ReactNode;
    description?: React.ReactNode;
    action?: { label: string; onClick: () => void };
    dismissable?: boolean;
    className?: string;
}

const variantStyles: Record<AlertVariant, string> = {
    warning:
<<<<<<< HEAD
        "bg-amber-50 border-amber-200 text-amber-800 dark:bg-amber-950/30 dark:border-amber-800 dark:text-amber-200",
    info:
        "bg-blue-50 border-blue-200 text-blue-800 dark:bg-blue-950/30 dark:border-blue-800 dark:text-blue-200",
    danger:
        "bg-red-50 border-red-200 text-red-800 dark:bg-red-950/30 dark:border-red-800 dark:text-red-200",
    success:
        "bg-emerald-50 border-emerald-200 text-emerald-800 dark:bg-emerald-950/30 dark:border-emerald-800 dark:text-emerald-200",
    destructive:
        "bg-red-50 border-red-200 text-red-800 dark:bg-red-950/30 dark:border-red-800 dark:text-red-200",
=======
        "bg-amber-50 border-amber-200 text-amber-800 dark:bg-amber-950/30 dark:border-amber-800/50 dark:text-amber-200",
    info:
        "bg-blue-50 border-blue-200 text-blue-800 dark:bg-blue-950/30 dark:border-blue-800/50 dark:text-blue-200",
    danger:
        "bg-red-50 border-red-200 text-red-800 dark:bg-red-950/30 dark:border-red-800/50 dark:text-red-200",
    success:
        "bg-emerald-50 border-emerald-200 text-emerald-800 dark:bg-emerald-950/30 dark:border-emerald-800/50 dark:text-emerald-200",
    destructive:
        "bg-red-50 border-red-200 text-red-800 dark:bg-red-950/30 dark:border-red-800/50 dark:text-red-200",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
};

const actionStyles: Record<AlertVariant, string> = {
    warning: "text-amber-700 hover:text-amber-900 dark:text-amber-300 dark:hover:text-amber-100",
    info: "text-blue-700 hover:text-blue-900 dark:text-blue-300 dark:hover:text-blue-100",
    danger: "text-red-700 hover:text-red-900 dark:text-red-300 dark:hover:text-red-100",
    success: "text-emerald-700 hover:text-emerald-900 dark:text-emerald-300 dark:hover:text-emerald-100",
    destructive: "text-red-700 hover:text-red-900 dark:text-red-300 dark:hover:text-red-100",
};

export function AlertBanner({
    variant = "info",
    icon,
    message,
    title,
    children,
<<<<<<< HEAD
    description,
=======
    description: _description,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    action,
    dismissable = true,
    className,
}: AlertBannerProps) {
    const [dismissed, setDismissed] = useState(false);
    const [expanded, setExpanded] = useState(false);

    if (dismissed) return null;

<<<<<<< HEAD
    const content = message ?? title ?? children;
    const hasSeparateChildren = children != null && (message != null || title != null);
=======
    const content = message || title || children;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    const el = React.isValidElement(content) ? (content as React.ReactElement<{ children: React.ReactNode }>) : null;
    const childCount = el ? React.Children.toArray(el.props.children).length : 0;
    const hasMultipleChildren = childCount > 1;

    const firstChild = hasMultipleChildren && el
        ? React.Children.toArray(el.props.children)[0]
        : null;
    const restChildren = hasMultipleChildren && el
        ? React.Children.toArray(el.props.children).slice(1)
        : [];

    return (
        <div
            className={cn(
                "relative flex flex-col sm:flex-row sm:items-center gap-1 sm:gap-3 rounded-xl border px-2.5 py-1.5 sm:px-4 sm:py-3 text-caption-md sm:text-sm animate-in fade-in slide-in-from-top-2 duration-300",
                variantStyles[variant],
                className
            )}
            role="alert"
        >
            {dismissable && (
                <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setDismissed(true)}
                    className="absolute top-1 left-1 sm:top-2 sm:left-2 p-0.5 rounded hover:bg-black/5 dark:hover:bg-white/10 transition-colors min-h-[24px] min-w-[24px] sm:min-h-[32px] sm:min-w-[32px]"
                    aria-label="إغلاق"
                >
                    <X className="w-3 h-3 sm:w-4 sm:h-4" />
                </Button>
            )}
<<<<<<< HEAD
            <div className="flex min-w-0 flex-col gap-1 sm:flex-1">
                <div className="flex min-w-0 items-center gap-1 sm:gap-3">
                    {icon && <span className="shrink-0 [&>svg]:w-3.5 [&>svg]:h-3.5 sm:[&>svg]:w-5 sm:[&>svg]:h-5">{icon}</span>}
                    {/* Mobile: first child only */}
                    {hasMultipleChildren && (
                        <span className="flex-1 font-medium break-words sm:hidden">{firstChild}</span>
                    )}
                    {/* Mobile: full content if single child */}
                    {!hasMultipleChildren && (
                        <span className="flex-1 font-medium break-words sm:hidden">{content}</span>
                    )}
                    {/* Desktop: full content */}
                    <span className="hidden sm:inline sm:flex-1 sm:font-medium sm:break-words">{content}</span>
                    {hasMultipleChildren && (
                        <button
                            type="button"
                            onClick={() => setExpanded(!expanded)}
                            className="sm:hidden shrink-0 p-0.5 rounded hover:bg-black/5 dark:hover:bg-white/10 transition-colors"
                            aria-label={expanded ? "طي" : "توسيع"}
                        >
                            <ChevronDown
                                className={cn("w-3.5 h-3.5 transition-transform duration-200", expanded && "rotate-180")}
                            />
                        </button>
                    )}
                </div>
                {description && <div className="text-xs leading-relaxed sm:text-sm">{description}</div>}
=======
            <div className="flex items-center gap-1 sm:gap-3 sm:flex-1 min-w-0">
                {icon && <span className="shrink-0 [&>svg]:w-3.5 [&>svg]:h-3.5 sm:[&>svg]:w-5 sm:[&>svg]:h-5">{icon}</span>}
                {/* Mobile: first child only */}
                {hasMultipleChildren && (
                    <span className="flex-1 font-medium break-words sm:hidden">{firstChild}</span>
                )}
                {/* Mobile: full content if single child */}
                {!hasMultipleChildren && (
                    <span className="flex-1 font-medium break-words sm:hidden">{content}</span>
                )}
                {/* Desktop: full content */}
                <span className="hidden sm:inline sm:flex-1 sm:font-medium sm:break-words">{content}</span>
                {hasMultipleChildren && (
                    <button
                        type="button"
                        onClick={() => setExpanded(!expanded)}
                        className="sm:hidden shrink-0 p-0.5 rounded hover:bg-black/5 dark:hover:bg-white/10 transition-colors"
                        aria-label={expanded ? "طي" : "توسيع"}
                    >
                        <ChevronDown
                            className={cn("w-3.5 h-3.5 transition-transform duration-200", expanded && "rotate-180")}
                        />
                    </button>
                )}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            </div>
            {/* Mobile expanded content */}
            {expanded && hasMultipleChildren && (
                <div className="sm:hidden w-full font-medium text-xs leading-relaxed animate-in fade-in duration-200">
                    {restChildren.map((child, i) => (
                        <div key={i}>{child}</div>
                    ))}
                </div>
            )}
<<<<<<< HEAD
            {hasSeparateChildren && <div className="w-full sm:w-auto">{children}</div>}
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            {action && (
                <div className="flex items-center gap-1.5 sm:gap-2 sm:shrink-0">
                    <Button
                        variant="ghost"
                        onClick={action.onClick}
                        className={cn(
                            "shrink-0 text-caption sm:text-sm font-semibold underline-offset-2 hover:underline transition-colors min-h-[28px] sm:min-h-[44px]",
                            actionStyles[variant]
                        )}
                    >
                        {action.label}
                    </Button>
                </div>
            )}
        </div>
    );
}
