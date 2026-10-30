import React, { useMemo } from 'react';
<<<<<<< HEAD
import { Cell, Pie, PieChart, ResponsiveContainer } from 'recharts';

import { Card, CardContent } from '@/components/ui/Card';
import type { ChartConfig } from '@/components/ui/chart';
import { ChartContainer, ChartTooltip, ChartTooltipContent } from '@/components/ui/chart';
=======
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';

import { Card, CardContent } from '@/components/ui/Card';
import { ChartContainer, ChartTooltip, ChartTooltipContent } from '@/components/ui/chart';
import type { ChartConfig } from '@/components/ui/chart';
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { chartColors } from '@/lib/chartColors';
import { cn, formatNumber } from '@/lib/utils';

interface ReportPieChartProps {
  data: Record<string, unknown>[];
  nameKey: string;
  valueKey: string;
  title?: string;
  subtitle?: string;
  height?: number;
  colors?: string[];
  showLegend?: boolean;
  innerRadius?: number | string;
  formatValue?: (value: number) => string;
  className?: string;
  /** عتبة النسبة المئوية للقطع الصغيرة التي تُجمع في "أخرى" */
  minPercent?: number;
  /** تسمية القسم المجمّع */
  otherLabel?: string;
  /** لون قسم "أخرى" */
  otherColor?: string;
}

interface CustomLabelProps {
  cx: number;
  cy: number;
  midAngle: number;
  innerRadius: number;
  outerRadius: number;
  percent: number;
}

const DEFAULT_COLORS = [
  chartColors.primary,
  chartColors.success,
  chartColors.danger,
  chartColors.warning,
  chartColors.purple,
  chartColors.teal,
  chartColors.pink,
];

const RADIAN = Math.PI / 180;

const renderCustomLabel = ({
  cx,
  cy,
  midAngle,
  innerRadius,
  outerRadius,
  percent,
}: CustomLabelProps) => {
  // إخفاء النصوص للقطع الصغيرة جداً أو الكبيرة جداً لتجنب التداخل
  if (percent < 0.08 || percent > 0.85) return null;

  const radius = innerRadius + (outerRadius - innerRadius) * 0.5;
  const x = cx + radius * Math.cos(-midAngle * RADIAN);
  const y = cy + radius * Math.sin(-midAngle * RADIAN);

  return (
    <text
      x={x}
      y={y}
      fill="white"
      textAnchor="middle"
      dominantBaseline="central"
      fontSize={12}
      fontWeight="bold"
      className="drop-shadow-sm pointer-events-none"
    >
      {`${(percent * 100).toFixed(0)}%`}
    </text>
  );
};

export const ReportPieChart: React.FC<ReportPieChartProps> = ({
  data,
  nameKey,
  valueKey,
  title,
  subtitle,
  height = 280,
  colors,
  showLegend = true,
  innerRadius = "62%",
  formatValue,
  className,
  minPercent = 3,
  otherLabel = "أخرى",
  otherColor = "hsl(220, 9%, 60%)",
}) => {
  if (!data || data.length === 0) return null;

  const palette = colors || DEFAULT_COLORS;

  // ─── ترتيب البيانات تنازلياً حسب القيمة ───
  const sortedData = useMemo(() => {
    return [...data].sort((a, b) => Number(b[valueKey] ?? 0) - Number(a[valueKey] ?? 0));
  }, [data, valueKey]);

  // ─── حساب الإجمالي ───
  const totalVal = useMemo(() => {
    return sortedData.reduce((sum, item) => sum + Number(item[valueKey] ?? 0), 0);
  }, [sortedData, valueKey]);

  // ─── تجميع القطع الصغيرة في "أخرى" ───
  const processedData = useMemo(() => {
    if (totalVal === 0) return sortedData;

    const major: Record<string, unknown>[] = [];
    let otherSum = 0;
    let otherCount = 0;
    const otherNames: string[] = [];

    sortedData.forEach((item) => {
      const value = Number(item[valueKey] ?? 0);
      const percent = (value / totalVal) * 100;
      if (percent < minPercent) {
        otherSum += value;
        otherCount += 1;
        const n = String(item[nameKey] ?? "");
        if (n) otherNames.push(n);
      } else {
        major.push(item);
      }
    });

    if (otherSum > 0) {
      major.push({
        [nameKey]: otherCount > 1 ? `${otherLabel} (${otherCount})` : otherLabel,
        [valueKey]: otherSum,
        _isOther: true,
        _originalNames: otherNames,
      });
    }

    return major;
  }, [sortedData, totalVal, valueKey, nameKey, minPercent, otherLabel]);

  // ─── إعداد إعدادات المخطط بألوان ثابتة حسب الترتيب ───
  const chartConfig = useMemo(() => {
    const config: ChartConfig = {};
    processedData.forEach((entry, i) => {
      const key = String(entry[nameKey]);
      const isOther = (entry as Record<string, unknown>)._isOther === true;
      config[key] = {
        label: key,
        color: isOther ? otherColor : (palette[i % palette.length] ?? otherColor),
      };
    });
    return config;
  }, [processedData, nameKey, palette, otherColor]);

  // ─── تنسيق القيمة للعرض في Legend ───
  const fmt = (v: number) => (formatValue ? formatValue(v) : formatNumber(v));

  // عدد الأعمدة للـ Legend — بحسب عدد العناصر
  void (processedData.length <= 4 ? 2 : processedData.length <= 8 ? 3 : 4);

  return (
    <Card className={cn(
<<<<<<< HEAD
      "border border-border bg-card shadow-sm transition-shadow duration-200 hover:shadow-md flex flex-col overflow-hidden",
=======
      "border border-border/60 bg-card shadow-sm transition-shadow duration-200 hover:shadow-md flex flex-col overflow-hidden",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
      className
    )}>

      {/* Header Section */}
      {(title || subtitle) && (
        <div className="pt-3 pb-1 px-3 shrink-0 text-center">
          {title && <h3 className="text-sm font-bold text-foreground leading-tight">{title}</h3>}
          {subtitle && <p className="text-xs text-muted-foreground leading-tight mt-0.5">{subtitle}</p>}
        </div>
      )}

      <CardContent className="p-2 pt-2 flex-1 flex flex-col md:flex-row items-center justify-start gap-2">

        {/* Donut Chart Canvas */}
        <div
          className="relative w-full md:w-2/3 flex items-center justify-center max-h-[280px] md:max-h-none"
          style={{ minHeight: height }}
        >
          <ChartContainer config={chartConfig} className="w-full h-full absolute inset-0">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={processedData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={renderCustomLabel}
                  outerRadius="92%"
                  innerRadius={innerRadius}
                  dataKey={valueKey}
                  nameKey={nameKey}
                  paddingAngle={processedData.length > 1 ? 2 : 0}
                  stroke="none"
                  isAnimationActive
                >
                  {processedData.map((entry, index) => {
                    const key = String(entry[nameKey]);
                    return (
                      <Cell
                        key={`cell-${index}`}
                        fill={chartConfig[key]?.color}
<<<<<<< HEAD
                        className="stroke-background hover:opacity-100 transition-opacity duration-200 outline-none"
=======
                        className="stroke-background hover:opacity-85 transition-opacity duration-200 outline-none"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        strokeWidth={2}
                      />
                    );
                  })}
                </Pie>
                <ChartTooltip
                  cursor={false}
                  content={
                    <ChartTooltipContent
                      formatter={(value, name) => (
                        <span className="font-mono font-bold text-foreground tabular-nums">
                          {fmt(Number(value))}
                          <span className="text-muted-foreground font-normal ms-2">({name})</span>
                        </span>
                      )}
                    />
                  }
                />
              </PieChart>
            </ResponsiveContainer>
          </ChartContainer>

          {/* Donut Hole Total Indicator */}
          {totalVal > 0 && (
            <div className="absolute inset-0 flex flex-col items-center justify-center pointer-events-none z-10">
              <span className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest">المجموع</span>
              <span className="text-2xl font-black text-foreground tabular-nums tracking-tight mt-0.5">
                {fmt(totalVal)}
              </span>
            </div>
          )}
        </div>

        {/* Legend Section — أفقي على الهاتف، جانبي على سطح المكتب */}
        {showLegend && processedData.length > 0 && (
          <div className="w-full md:w-1/3 pt-2 md:pt-0 md:pe-2 flex flex-wrap items-center justify-center md:flex-col md:items-start md:justify-center gap-x-3 gap-y-1">
            {processedData.map((entry, i) => {
              const name = String(entry[nameKey]);
              const color = chartConfig[name]?.color;
              const value = Number(entry[valueKey] ?? 0);
              const percent = totalVal > 0 ? ((value / totalVal) * 100).toFixed(1) : "0";

              return (
                <div
                  key={`legend-${name}-${i}`}
                  className="flex items-center gap-1.5 py-0.5 px-1.5 rounded-md hover:bg-muted/40 transition-colors cursor-default min-w-0"
                  title={`${name} — ${fmt(value)} (${percent}%)`}
                >
                  <span
                    className="w-3 h-3 rounded-full shrink-0 shadow-sm ring-1 ring-background"
                    style={{ backgroundColor: color }}
                  />
<<<<<<< HEAD
                  <span className="text-foreground font-medium text-sm truncate">
=======
                  <span className="text-foreground/90 font-medium text-sm truncate">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    {name}
                  </span>
                </div>
              );
            })}
          </div>
        )}
      </CardContent>
    </Card>
  );
};