import * as React from "react";
import * as RechartsPrimitive from "recharts";

import { cn } from "@/lib/utils";

// ─── Config ─────────────────────────────────────────────

type ChartConfig = Record<string, { label: string; color?: string }>;

// ─── Context ────────────────────────────────────────────

const ChartContext = React.createContext<ChartConfig | null>(null);

function useChartConfig() {
    return React.useContext(ChartContext);
}

// ─── ChartContainer ─────────────────────────────────────

interface ChartContainerProps extends React.HTMLAttributes<HTMLDivElement> {
    config: ChartConfig;
    children: React.ReactNode;
}

function ChartContainer({ config, className, children, ...props }: ChartContainerProps) {
    return (
        <ChartContext.Provider value={config}>
            <div
                data-chart=""
                className={cn(
                    "flex aspect-video justify-center text-xs [&_.recharts-cartesian-axis_tick_text]:fill-muted-foreground [&_.recharts-cartesian-grid_line[stroke='#ccc']]:stroke-border/50 [&_.recharts-curve.recharts-tooltip-cursor]:stroke-border [&_.recharts-dot[stroke='#fff']]:stroke-transparent [&_.recharts-layer]:outline-none [&_.recharts-polar-grid_[stroke='#ccc']]:stroke-border [&_.recharts-radial-bar-background-sector]:fill-muted [&_.recharts-rectangle.recharts-tooltip-cursor]:fill-muted [&_.recharts-reference-line_[stroke='#ccc']]:stroke-border [&_.recharts-sector[stroke='#fff']]:stroke-transparent [&_.recharts-sector]:outline-none [&_.recharts-surface]:outline-none",
                    className,
                )}
                {...props}
            >
                <ChartStyle id="chart" config={config} />
                <RechartsPrimitive.ResponsiveContainer>{children as React.ReactElement || <div />}</RechartsPrimitive.ResponsiveContainer>
            </div>
        </ChartContext.Provider>
    );
}

// ─── ChartStyle (CSS vars for colors) ───────────────────

function ChartStyle({ config }: { id?: string; config: ChartConfig }) {
    const colorConfig = Object.entries(config).filter(([_, cfg]) => cfg.color);

    if (!colorConfig.length) return null;

    return (
        <style
            dangerouslySetInnerHTML={{
                __html: colorConfig
                    .map(([key, cfg]) => `--color-${key}: ${cfg.color};`)
                    .join("\n"),
            }}
        />
    );
}

// ─── ChartTooltip ───────────────────────────────────────

const ChartTooltip = RechartsPrimitive.Tooltip;

// ─── ChartTooltipContent ────────────────────────────────

interface ChartTooltipContentProps extends React.HTMLAttributes<HTMLDivElement> {
    active?: boolean;
    payload?: Array<{ name: string; value: number; color?: string; dataKey?: string }>;
    label?: string;
    labelKey?: string;
    nameKey?: string;
    labelFormatter?: (label: string) => React.ReactNode;
    formatter?: (value: number, name: string, payload: unknown) => React.ReactNode;
    indicator?: "dot" | "line" | "dashed";
    hideLabel?: boolean;
    hideIndicator?: boolean;
    labelClassName?: string;
    indicatorClassName?: string;
    indicatorStyle?: React.CSSProperties;
}

function ChartTooltipContent({
    active,
    payload,
    label,
    nameKey,
    labelFormatter,
    formatter,
    indicator = "dot",
    hideLabel = false,
    hideIndicator = false,
    labelClassName,
    indicatorClassName,
    indicatorStyle,
    className,
}: ChartTooltipContentProps) {
    const config = useChartConfig();

    if (!active || !payload?.length) return null;

    const formattedLabel = labelFormatter ? labelFormatter(label ?? "") : label;

    return (
        <div
            className={cn(
<<<<<<< HEAD
                "grid min-w-[8rem] items-start gap-1.5 rounded-lg border border-border bg-background px-2.5 py-1.5 text-xs shadow-xl",
=======
                "grid min-w-[8rem] items-start gap-1.5 rounded-lg border border-border/50 bg-background px-2.5 py-1.5 text-xs shadow-xl",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                className,
            )}
        >
            {!hideLabel && formattedLabel && (
                <div className={cn("font-medium", labelClassName)}>{formattedLabel}</div>
            )}
            <div className="grid gap-1.5">
                {payload.map((item, index) => {
                    const key = nameKey ? item[nameKey as keyof typeof item] : item.name;
                    const itemConfig = (config ?? {})[key as string] ?? (config ?? {})[item.dataKey as string] ?? {};
                    const color = item.color || itemConfig.color;

                    return (
                        <div
                            key={item.dataKey ?? index}
                            className={cn(
                                "flex w-full flex-wrap items-stretch gap-2 [&>svg]:h-2.5 [&>svg]:w-2.5 [&>svg]:text-muted-foreground",
                                indicatorClassName,
                            )}
                        >
                            {!hideIndicator && (
                                <div
                                    className={cn(
                                        "shrink-0 rounded-[2px] bg-[--color-bg]",
                                        {
                                            "h-2.5 w-2.5": indicator === "dot",
                                            "w-1": indicator === "line",
                                            "w-0 border-[1.5px] border-dashed": indicator === "dashed",
                                        },
                                    )}
                                    style={{
                                        ...indicatorStyle,
                                        backgroundColor: color,
                                        borderColor: color,
                                        "--color-bg": color,
                                    } as React.CSSProperties}
                                />
                            )}
                            <div className="flex flex-1 justify-between leading-none">
                                <span className="text-muted-foreground">{itemConfig.label || item.name}</span>
                                {formatter ? (
                                    <span className="font-mono font-medium tabular-nums text-foreground">
                                        {formatter(item.value, item.name, item)}
                                    </span>
                                ) : (
                                    <span className="font-mono font-medium tabular-nums text-foreground">
                                        {item.value?.toLocaleString()}
                                    </span>
                                )}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}

// ─── ChartLegend ────────────────────────────────────────

const ChartLegend = RechartsPrimitive.Legend;

// ─── ChartLegendContent ─────────────────────────────────

interface ChartLegendContentProps extends React.HTMLAttributes<HTMLDivElement> {
    payload?: Array<{ value: string; color?: string; type?: string }>;
    verticalAlign?: "top" | "bottom";
    hideIcon?: boolean;
    nameKey?: string;
}

function ChartLegendContent({
    payload,
    verticalAlign = "bottom",
    hideIcon = false,
    nameKey,
    className,
}: ChartLegendContentProps) {
    const config = useChartConfig();

    if (!payload?.length) return null;

    return (
        <div
            className={cn(
                "flex flex-wrap items-center justify-center gap-x-4 gap-y-2 [&>svg]:h-3 [&>svg]:w-3",
                verticalAlign === "top" && "pb-3",
                verticalAlign === "bottom" && "pt-3",
                className,
            )}
        >
            {payload.map((item) => {
                const key = nameKey ? item[nameKey as keyof typeof item] : item.value;
                const itemConfig = (config ?? {})[key as string] ?? {};

                return (
                    <div
                        key={item.value}
                        className="flex items-center gap-1.5 [&>svg]:h-3 [&>svg]:w-3 [&>svg]:text-muted-foreground"
                    >
                        {!hideIcon && (
                            <div
                                className="h-2 w-2 shrink-0 rounded-[2px]"
                                style={{ backgroundColor: item.color || itemConfig.color }}
                            />
                        )}
                        {itemConfig.label || item.value}
                    </div>
                );
            })}
        </div>
    );
}

export {
    ChartContainer,
    ChartLegend,
    ChartLegendContent,
    ChartStyle,
    ChartTooltip,
    ChartTooltipContent,
    useChartConfig,
};
export type { ChartConfig };
