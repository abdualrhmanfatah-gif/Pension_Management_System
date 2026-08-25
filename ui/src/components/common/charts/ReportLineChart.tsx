import React, { useMemo } from 'react';
import { 
  LineChart, 
  Line, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Area, 
  AreaChart,
  ResponsiveContainer // تمت الإضافة لدعم التجاوب
} from 'recharts';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { ChartContainer, ChartTooltip, ChartTooltipContent, ChartLegend, ChartLegendContent } from '@/components/ui/chart';
import type { ChartConfig } from '@/components/ui/chart';
import { chartColors } from '@/lib/chartColors';
import { cn, formatNumber } from '@/lib/utils';

interface ReportLineChartProps {
  data: Record<string, any>[];
  xKey: string;
  lines: { key: string; color?: string; label: string }[];
  title?: string;
  height?: number;
  showGrid?: boolean;
  showLegend?: boolean;
  area?: boolean;
  formatValue?: (value: number) => string;
  className?: string;
}

const DEFAULT_COLORS = [
  chartColors.primary,
  chartColors.success,
  chartColors.danger,
  chartColors.warning,
  chartColors.purple,
  chartColors.teal,
];

export const ReportLineChart: React.FC<ReportLineChartProps> = ({
  data,
  xKey,
  lines,
  title,
  height = 350,
  showGrid = true,
  showLegend = true,
  area = false,
  formatValue,
  className,
}) => {
  if (!data || data.length === 0) return null;

  const ChartComponent = area ? AreaChart : LineChart;

  // إعداد إعدادات المخطط باستخدام useMemo لتحسين الأداء
  const chartConfig = useMemo(() => {
    const config: ChartConfig = {};
    lines.forEach((line, index) => {
      config[line.key] = {
        label: line.label,
        color: line.color || DEFAULT_COLORS[index % DEFAULT_COLORS.length],
      };
    });
    return config;
  }, [lines]);

  return (
    <Card className={cn("h-full border border-border/60 shadow-sm transition-shadow hover:shadow-md", className)}>
      {title && (
        <CardHeader className="pb-0 pt-1 px-3 space-y-0">
          <CardTitle className="text-sm font-bold text-foreground">{title}</CardTitle>
        </CardHeader>
      )}
      <CardContent className="p-0">
        <div role="img" aria-label={title || 'Chart'} className="w-full">
          <ChartContainer config={chartConfig} className="w-full !aspect-auto justify-start" style={{ height }}>
            {/* استخدام ResponsiveContainer لضمان تجاوب المخطط مع الشاشات المختلفة */}
            <ResponsiveContainer width="100%" height="100%">
              <ChartComponent data={data} margin={{ top: 5, right: 10, left: 0, bottom: 0 }}>
                {/* إضافة التدرج اللوني (Gradient) لـ Area Chart */}
                {area && (
                  <defs>
                    {lines.map((line, index) => {
                      const color = line.color || DEFAULT_COLORS[index % DEFAULT_COLORS.length];
                      return (
                        <linearGradient key={`fill-${line.key}`} id={`color-${line.key}`} x1="0" y1="0" x2="0" y2="1">
                          <stop offset="5%" stopColor={color} stopOpacity={0.3} />
                          <stop offset="95%" stopColor={color} stopOpacity={0} />
                        </linearGradient>
                      );
                    })}
                  </defs>
                )}

                {showGrid && (
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="hsl(var(--border))" />
                )}
                
                <XAxis
                  dataKey={xKey}
                  tickLine={false}
                  axisLine={false}
                  tick={{ fontSize: 12, fill: 'hsl(var(--muted-foreground))' }}
                  tickMargin={10}
                />
                
                <YAxis
                  tickLine={false}
                  axisLine={false}
                  tick={{ fontSize: 12, fill: 'hsl(var(--muted-foreground))' }}
                  tickMargin={10}
                  tickFormatter={(val) => {
                    if (val >= 1000000) return `${(val / 1000000).toFixed(1)}M`;
                    if (val >= 1000) return `${(val / 1000).toFixed(0)}K`;
                    return val;
                  }}
                />
                
                <ChartTooltip
                  cursor={{ stroke: 'hsl(var(--muted))', strokeWidth: 2, strokeDasharray: '4 4' }}
                  content={
                    <ChartTooltipContent
                      formatter={(value) => (
                        <span className="font-mono font-medium text-foreground tabular-nums">
                          {formatValue ? formatValue(Number(value)) : formatNumber(Number(value))}
                        </span>
                      )}
                    />
                  }
                />
                
                {showLegend && <ChartLegend content={<ChartLegendContent />} className="mt-0 pt-0" />}
                
                {lines.map((line, index) => {
                  const color = line.color || DEFAULT_COLORS[index % DEFAULT_COLORS.length];
                  
                  if (area) {
                    return (
                      <Area
                        key={line.key}
                        type="monotone"
                        dataKey={line.key}
                        name={line.label}
                        stroke={color}
                        fill={`url(#color-${line.key})`}
                        strokeWidth={2}
                        activeDot={{ r: 6, stroke: 'hsl(var(--background))', strokeWidth: 2 }}
                      />
                    );
                  }
                  
                  return (
                    <Line
                      key={line.key}
                      type="monotone"
                      dataKey={line.key}
                      name={line.label}
                      stroke={color}
                      strokeWidth={2}
                      dot={false} // إخفاء النقاط العادية ليكون الخط أنظف
                      activeDot={{ r: 6, fill: color, stroke: 'hsl(var(--background))', strokeWidth: 2 }}
                    />
                  );
                })}
              </ChartComponent>
            </ResponsiveContainer>
          </ChartContainer>
        </div>
      </CardContent>
    </Card>
  );
};