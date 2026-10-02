import { ChevronDown, Filter, X } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui";
import { NativeSelect, type NativeSelectOption } from "@/components/ui/NativeSelect";
import { cn } from "@/lib/utils";

// ─── Types ─────────────────────────────────────────────

export interface FilterDef {
    key: string;
    label: string;
    options: NativeSelectOption[];
    placeholder?: string;
}

interface FilterBarProps {
    filters: FilterDef[];
    values: Record<string, string>;
    onChange: (key: string, value: string) => void;
    onClearAll: () => void;
    activeCount: number;
    totalCount?: number;
    className?: string;
}

// ─── Component ─────────────────────────────────────────

export function FilterBar({ filters, values, onChange, onClearAll, activeCount, totalCount, className }: FilterBarProps) {
    const [open, setOpen] = useState(false);

    return (
        <div className={cn("rounded-xl border border-border/60 bg-card/40 px-3 py-2", className)}>
            {/* ── Header Row ─────────────────────────────── */}
            <div className="flex items-center justify-between gap-2">
                <div className="flex items-center gap-3">
                    <button
                        type="button"
                        onClick={() => setOpen((v) => !v)}
                        className="flex items-center gap-1.5 text-xs font-semibold text-foreground/70 hover:text-foreground transition-colors"
                        aria-expanded={open}
                    >
                        <div className="relative">
                            <Filter className="w-3.5 h-3.5" />
                            {activeCount > 0 && (
                                <span className="absolute -top-1.5 -right-1.5 flex items-center justify-center min-w-[14px] h-3.5 px-0.5 rounded-full bg-primary text-primary-foreground text-[9px] font-bold leading-none">
                                    {activeCount}
                                </span>
                            )}
                        </div>
                        <span>الفلاتر</span>
                        <ChevronDown
                            className={cn(
                                "w-3.5 h-3.5 text-muted-foreground transition-transform duration-200",
                                open && "rotate-180"
                            )}
                        />
                    </button>

                    {/* ── Active Filter Tags ─────────────────── */}
                    {activeCount > 0 && (
                        <div className="flex items-center gap-1 flex-wrap">
                            {filters.map((f) => {
                                const val = values[f.key];
                                if (!val) return null;
                                const opt = f.options.find((o) => o.value === val);
                                return (
                                    <span
                                        key={f.key}
                                        className="inline-flex items-center gap-0.5 px-1.5 py-0.5 rounded-full bg-primary/10 text-primary text-[11px] font-medium"
                                    >
                                        {opt?.label || val}
                                        <button
                                            onClick={() => onChange(f.key, "")}
                                            className="hover:text-primary/70"
                                        >
                                            <X className="w-2.5 h-2.5" />
                                        </button>
                                    </span>
                                );
                            })}
                            <Button
                                variant="ghost"
                                size="sm"
                                onClick={onClearAll}
                                className="h-5 px-1.5 text-[10px] text-destructive hover:text-destructive"
                            >
                                <X className="w-2.5 h-2.5" />
                                مسح الكل
                            </Button>
                        </div>
                    )}
                </div>

                {/* ── Total Count ─────────────────────────── */}
                {totalCount !== undefined && (
                    <span className="text-[11px] text-muted-foreground font-medium whitespace-nowrap">
                        <span className="text-foreground font-bold">{totalCount.toLocaleString("ar-YE")}</span>
                        {" "}نتيجة
                    </span>
                )}
            </div>

            {/* ── Filters Grid ─────────────────────────── */}
            {open && (
                <div className="grid gap-2 mt-2 pt-2 border-t border-border/40" style={{ gridTemplateColumns: `repeat(${filters.length}, minmax(0, 1fr))` }}>
                    {filters.map((f) => (
                        <NativeSelect
                            key={f.key}
                            label={f.label}
                            value={values[f.key] || ""}
                            onChange={(v) => onChange(f.key, v)}
                            options={f.options}
                            placeholder={f.placeholder || "الكل"}
                            className="h-8 text-xs border-primary/30 focus:border-primary"
                        />
                    ))}
                </div>
            )}
        </div>
    );
}
