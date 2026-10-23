import React from "react";

import { cn } from "@/lib/utils";
import { useNavStore } from "@/stores/navStore";

interface DetailLayoutProps {
    sidebar: React.ReactNode;
    children: React.ReactNode;
    breadcrumb?: React.ReactNode;
    headerTitle?: React.ReactNode;
    headerActions?: React.ReactNode;
    extraHeader?: React.ReactNode;
    statusBadge?: React.ReactNode;
    banner?: React.ReactNode;
    stickyHeader?: boolean;
    className?: string;
}

/**
 * DetailLayout — A standard layout for detail pages (e.g., Retirement File Details).
 * Features a sticky profile sidebar on the right (in RTL) and a scrollable content area.
 */
export function DetailLayout({
    sidebar,
    children,
    breadcrumb,
    headerTitle,
    headerActions,
    extraHeader,
    statusBadge,
    banner,
    stickyHeader = false,
    className
}: DetailLayoutProps) {
    const hasSecondaryNav = useNavStore((state) => state.hasSecondaryNav);

    return (
        <div className={cn(
            "animate-fade-in",
            stickyHeader ? "-mt-4 space-y-2 sm:-mt-6 md:space-y-3 print:mt-0" : "space-y-3 md:space-y-6",
            className
        )}>
            {/* ── Header Section ─────────────────────────────────── */}
            <div className={cn(
                "space-y-2 md:space-y-4",
                stickyHeader && "sticky top-14 z-30 -mx-4 bg-background px-4 py-1 sm:-mx-6 sm:px-6 print:static print:z-auto",
                stickyHeader && (hasSecondaryNav ? "lg:top-24" : "lg:top-14")
            )}>
                {breadcrumb && <div>{breadcrumb}</div>}
                
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 sm:gap-4">
                    {headerTitle}
                    <div className="flex gap-1.5 sm:gap-2 items-center flex-wrap">
                        {headerActions}
                    </div>
                </div>

                {extraHeader && <div className="pt-1 sm:pt-2">{extraHeader}</div>}

                {statusBadge && (
                    <div className="rounded-lg border bg-card px-3 py-2 sm:px-4 sm:py-2.5 flex items-center justify-between flex-wrap gap-2 sm:gap-3">
                        {statusBadge}
                    </div>
                )}

                {banner && <div className="animate-in slide-in-from-top-2 duration-300">{banner}</div>}
            </div>

            {/* ── Main Content & Sidebar ────────────────────────── */}
            <div className="grid grid-cols-1 lg:grid-cols-4 gap-4 md:gap-6 items-start">
                {/* 
                    Sidebar: Sticky Anchor 
                    In RTL: DOM order places the first element on the right naturally.
                */}
                <aside className="lg:col-span-1 order-first">
                    <div className="lg:sticky lg:top-20 space-y-4 md:space-y-6">
                        {sidebar}
                    </div>
                </aside>

                {/* Main Action Area */}
                <main className="lg:col-span-3 min-w-0 space-y-4 md:space-y-6">
                    {children}
                </main>
            </div>
        </div>
    );
}
