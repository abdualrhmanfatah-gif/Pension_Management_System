import React, { useMemo } from "react";
import { Bar, BarChart, CartesianGrid, Cell,ResponsiveContainer, XAxis, YAxis } from "recharts";

import { Heading } from "@/components/ui";
import { Card } from "@/components/ui/Card";
import type { ChartConfig } from "@/components/ui/chart";
import {
    ChartContainer,
    ChartLegend,
    ChartLegendContent,
    ChartTooltip,
    ChartTooltipContent,
} from "@/components/ui/chart";
import { chartColors } from "@/lib/chartColors";
import { cn, formatNumber } from "@/lib/utils";

/* =========================================================
   Types
========================================================= */

export interface YKeyConfig {
    key: string;
    color?: string;
    label: string;
    stackId?: string;
}

export interface ReportBarChartProps {
    data: Record<string, unknown>[];
    xKey: string;
    yKeys: YKeyConfig[];
    title?: string;
    horizontal?: boolean;
    height?: number;
    showGrid?: boolean;
    showLegend?: boolean;
    formatValue?: (value: number) => string;
    className?: string;
    /** فرز تنازلي حسب القيمة — قاعدة الخبراء: الأطول في الأعلى عند غياب ترتيب منطقي */
    sortByValue?: boolean;
    /** أقصى عدد عناصر قبل تجميع الباقي في "أخرى" — يمنع الازدحام البصري */
    maxItems?: number;
    /** تسمية القسم المجمّع */
    otherLabel?: string;
}

/* =========================================================
   Colors
========================================================= */

const DEFAULT_COLORS = [
    chartColors.primary,
    chartColors.success,
    chartColors.danger,
    chartColors.warning,
    chartColors.purple,
    chartColors.teal,
];

/* =========================================================
   Helpers
========================================================= */

const formatTick = (value: number): string => {
    if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
    if (value >= 1_000) return `${(value / 1_000).toFixed(0)}K`;
    return String(value);
};

/* =========================================================
   Horizontal Bar Chart (HTML-based for better RTL support)
========================================================= */

function HorizontalBarChart({
    data,
    xKey,
    yKeys,
    height,
    formatValue,
    sortByValue = true,
    maxItems,
    otherLabel = "أخرى",
}: {
    data: Record<string, unknown>[];
    xKey: string;
    yKeys: YKeyConfig[];
    height: number;
    formatValue?: (value: number) => string;
    sortByValue?: boolean;
    maxItems?: number;
    otherLabel?: string;
}) {
    // ─── الفرز التنازلي + تجميع الفائض في "أخرى" ───
    const items = useMemo(() => {
        const firstKey = yKeys[0]?.key;
        if (!firstKey) return data;

        const arr = [...data].filter((d) => String(d[xKey] ?? "") !== "");
        if (sortByValue) {
            arr.sort((a, b) => Number(b[firstKey] ?? 0) - Number(a[firstKey] ?? 0));
        }

        if (maxItems && arr.length > maxItems) {
            const head = arr.slice(0, maxItems);
            const tail = arr.slice(maxItems);
            const otherSum = tail.reduce((s, d) => s + Number(d[firstKey] ?? 0), 0);
            if (otherSum > 0) {
                head.push({
                    [xKey]: `${otherLabel} (${tail.length})`,
                    [firstKey]: otherSum,
                });
            }
            return head;
        }

        return arr;
    }, [data, xKey, yKeys, sortByValue, maxItems, otherLabel]);

    // استخدام useMemo لتحسين الأداء
    const { maxVal, totalVal } = useMemo(() => {
        const firstKey = yKeys[0]?.key;
        if (!firstKey) return { maxVal: 1, totalVal: 0 };

        return {
            maxVal: Math.max(...items.map((d) => Number(d[firstKey] ?? 0)), 1),
            totalVal: items.reduce((sum, d) => sum + Number(d[firstKey] ?? 0), 0),
        };
    }, [items, yKeys]);

    return (
        <div
            className="w-full flex-1 px-4 py-4 space-y-4 overflow-y-auto custom-scrollbar"
            style={{ minHeight: height, maxHeight: height + 50 }}
        >
            {items.map((d, i) => {
                const val = Number(d[yKeys[0]?.key] ?? 0);
                const pctOfMax = Math.round((val / maxVal) * 100);
                const barWidth = val > 0 ? Math.max(pctOfMax, 2) : 0;

                const pctOfTotal = totalVal > 0 ? Math.round((val / totalVal) * 100) : 0;
                const explicitPct = typeof d.percentage === "number" ? Math.round(d.percentage) : null;
                const displayPct = explicitPct ?? pctOfTotal;

                const color = yKeys[0]?.color || DEFAULT_COLORS[i % DEFAULT_COLORS.length];

                return (
                    <div
                        key={`${String(d[xKey])}-${i}`}
                        className="group relative flex flex-col gap-2 rounded-lg transition-colors hover:bg-muted/30 p-2 -mx-2"
                        dir="rtl"
                    >
                        <div className="flex items-center justify-between text-sm">
                            <span className="truncate flex-1 font-semibold text-foreground transition-colors group-hover:text-foreground text-start">
                                {String(d[xKey])}
                            </span>
                            <div className="shrink-0 flex items-center gap-2 tabular-nums">
                                <span className="font-bold text-foreground">
                                    {formatValue ? formatValue(val) : formatNumber(val)}
                                </span>
                                {val > 0 && displayPct > 0 && (
                                    <span className="text-[11px] font-medium text-muted-foreground bg-muted px-1.5 py-0.5 rounded-md">
                                        {displayPct}%
                                    </span>
                                )}
                            </div>
                        </div>
                        <div className="h-2.5 w-full overflow-hidden rounded-full bg-secondary/50 dark:bg-secondary">
                            <div
                                className="h-full rounded-full transition-all duration-1000 ease-in-out"
                                style={{ width: `${barWidth}%`, backgroundColor: color }}
                            />
                        </div>
                    </div>
                );
            })}
        </div>
    );
}

/* =========================================================
   Vertical Bar Chart — shadcn + Recharts
========================================================= */

function VerticalBarChart({
    data,
    xKey,
    yKeys,
    height,
    showGrid,
    showLegend,
    formatValue,
}: {
    data: Record<string, unknown>[];
    xKey: string;
    yKeys: YKeyConfig[];
    height: number;
    showGrid: boolean;
    showLegend: boolean;
    formatValue?: (value: number) => string;
}) {
    const chartConfig = useMemo(() => {
        const config: ChartConfig = {};
        yKeys.forEach((yk, i) => {
            config[yk.key] = {
                label: yk.label,
                color: yk.color || DEFAULT_COLORS[i % DEFAULT_COLORS.length],
            };
        });
        return config;
    }, [yKeys]);

    const isManyItems = data.length > 6;

    return (
        <div className="w-full p-4 pt-2 flex-1">
            <ChartContainer config={chartConfig} className="w-full" style={{ height }}>
                {/* إضافة ResponsiveContainer لجعل المخطط متجاوباً */}
                <ResponsiveContainer width="100%" height="100%">
                    <BarChart 
                        data={data} 
                        accessibilityLayer 
                        margin={{ top: 0, right: 30, left: -30, bottom:-20}}
                    >
                        {showGrid && (
                            <CartesianGrid 
                                vertical={false} 
                                strokeDasharray="4 4" 
                                stroke="hsl(var(--border))" 
                                opacity={0.6} 
                            />
                        )}
                        <XAxis
                            dataKey={xKey}
                            tickLine={true}
                            axisLine={true}
                            tick={{ fontSize: 12, fontWeight: 600, fill: "hsl(var(--muted-foreground))" }}
                            tickMargin={35}
                            angle={isManyItems ? -45 : 0}
                            textAnchor={isManyItems ? "end" : "middle"}
                            interval={0}
                            height={isManyItems ? 80 : 30}
                        />
                        <YAxis
                            tickLine={true}
                            axisLine={true}
                            
                            tick={{ fontSize: 12, fill: "hsl(var(--muted-foreground))" }}
                            tickFormatter={formatTick}
                            tickMargin={15}
                        />
                        <ChartTooltip
                            cursor={{ fill: "hsl(var(--muted))", opacity: 0.4 }}
                            content={
                                <ChartTooltipContent
                                    formatter={(value) => (
                                        <span className="font-mono font-bold text-foreground tabular-nums">
                                            {formatValue ? formatValue(Number(value)) : formatNumber(Number(value))}
                                        </span>
                                    )}
                                />
                            }
                        />
                        {showLegend && <ChartLegend content={<ChartLegendContent />} className="mt-2" />}
                        {yKeys.map((yk, index) => (
                            <Bar
                                key={`${yk.key}-${index}`}
                                dataKey={yk.key}
                                name={yk.label}
                                radius={[4, 4, 0, 0]} 
                                maxBarSize={50}
                                minPointSize={10}
                                stackId={yk.stackId}
                                className="transition-opacity hover:opacity-100"
                            >
                                {data.map((_, i) => (
                                    <Cell
                                        key={`cell-${i}`}
                                        fill={DEFAULT_COLORS[i % DEFAULT_COLORS.length]}
                                    />
                                ))}
                            </Bar>
                        ))}
                    </BarChart>
                </ResponsiveContainer>
            </ChartContainer>
        </div>
    );
}

/* =========================================================
   ReportBarChart — Main Export
========================================================= */

export const ReportBarChart = React.memo(function ReportBarChart({
    data,
    xKey,
    yKeys,
    title,
    horizontal = false,
    height = 350, // زيادة الارتفاع الافتراضي قليلاً ليعطي مساحة للمحاور
    showGrid = true,
    showLegend = true,
    formatValue,
    className,
    sortByValue = true,
    maxItems,
    otherLabel = "أخرى",
}: ReportBarChartProps) {
    const hasData = Array.isArray(data) && data.length > 0 && Array.isArray(yKeys) && yKeys.length > 0;

    if (!hasData) {
        return (
            <Card className={cn("border border-border bg-card shadow-sm flex flex-col items-center justify-center min-h-[300px]", className)}>
                <div className="flex flex-col items-center gap-2 text-muted-foreground">
                    <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" className="opacity-100">
                        <path d="M3 3v18h18" />
                        <path d="M18 17V9" />
                        <path d="M13 17V5" />
                        <path d="M8 17v-3" />
                    </svg>
                    <p className="text-sm font-medium">لا توجد بيانات لعرضها</p>
                </div>
            </Card>
        );
    }

    return (
        <Card className={cn("border border-border bg-card shadow-sm transition-shadow duration-200 hover:shadow-md flex flex-col overflow-hidden", className)}>
            {title && (
                <div dir="rtl" className="px-5 pt-5 pb-2 shrink-0 border-b border-border">
                    <Heading level="h3" className="text-sm font-bold text-foreground">
                        {title}
                    </Heading>
                </div>
            )}

            {horizontal ? (
                <HorizontalBarChart
                    data={data}
                    xKey={xKey}
                    yKeys={yKeys}
                    height={height}
                    formatValue={formatValue}
                    sortByValue={sortByValue}
                    maxItems={maxItems}
                    otherLabel={otherLabel}
                />
            ) : (
                <VerticalBarChart
                    data={data}
                    xKey={xKey}
                    yKeys={yKeys}
                    height={height}
                    showGrid={showGrid}
                    showLegend={showLegend}
                    formatValue={formatValue}
                />
            )}
        </Card>
    );
});