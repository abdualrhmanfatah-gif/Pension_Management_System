import { useMemo } from 'react';

<<<<<<< HEAD
import type { ChartColorConfig,ChartConfig } from '@/api/reports';

import { ReportBarChart } from './ReportBarChart';
import { ReportLineChart } from './ReportLineChart';
import { ReportPieChart } from './ReportPieChart';
=======
import { ReportBarChart } from './ReportBarChart';
import { ReportPieChart } from './ReportPieChart';
import { ReportLineChart } from './ReportLineChart';
import type { ChartConfig, ChartColorConfig } from '@/api/reports';
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface ReportChartSectionProps {
    charts: ChartConfig[];
    data: Record<string, unknown>[];
}

export function ReportChartSection({ charts, data }: ReportChartSectionProps) {
    if (!charts?.length || !data?.length) {
        return null;
    }

    return (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
            {charts.map((chart, index) => (
                <ChartRenderer
                    key={`${chart.type}-${chart.title ?? 'untitled'}-${index}`}
                    chart={chart}
                    data={data}
                />
            ))}
        </div>
    );
}

interface ChartRendererProps {
    chart: ChartConfig;
    data: Record<string, unknown>[];
}

function getColorString(c: string | ChartColorConfig): string {
    return typeof c === 'string' ? c : c.color;
}

function getMissingRange(count: number): string {
    if (count <= 2) return '1-2 نقص';
    if (count <= 5) return '3-5 نقص';
    if (count <= 8) return '6-8 نقص';
    return '9+ نقص';
}

function getDaysRange(days: number): string {
    if (days <= 30) return '1-30 يوم';
    if (days <= 60) return '31-60 يوم';
    if (days <= 90) return '61-90 يوم';
    if (days <= 180) return '91-180 يوم';
    return '180+ يوم';
}

function aggregateByKey(
    data: Record<string, unknown>[],
    nameKey: string,
    valueKey?: string,
    mode: 'count' | 'sum' | 'avg' = 'count'
): Array<{ name: string; value: number }> {
    const groups = new Map<string, number[]>();

    data.forEach(row => {
        let name: string;

        if (nameKey === '_docType') {
            const docsList = String(row['missingDocsList'] || '');
            const docs = docsList.split(/[،,]/).map(s => s.trim()).filter(Boolean);
            docs.forEach(doc => {
                const docType = doc.replace(/\s*\(.*\)\s*/, '').trim();
                if (docType) {
                    if (!groups.has(docType)) groups.set(docType, []);
                    groups.get(docType)!.push(1);
                }
            });
            return;
        }

        if (nameKey === '_missingRange') {
            const count = Number(row['missingCount']) || 0;
            name = getMissingRange(count);
            if (!groups.has(name)) groups.set(name, []);
            groups.get(name)!.push(1);
            return;
        }

        if (nameKey === '_daysRange') {
            const days = Number(row['maxDaysMissing']) || 0;
            name = getDaysRange(days);
            if (!groups.has(name)) groups.set(name, []);
            groups.get(name)!.push(1);
            return;
        }

        const rawName = row[nameKey];
        name = rawName != null ? String(rawName) : 'غير محدد';

        let rawVal: number;
        if (!valueKey || valueKey === 'count' || valueKey === '_docTypeCount' || valueKey === '_missingRangeCount' || valueKey === '_daysRangeCount') {
            rawVal = 1;
        } else {
            rawVal = Number(row[valueKey]) || 0;
        }

        if (!groups.has(name)) groups.set(name, []);
        groups.get(name)!.push(rawVal);
    });

    return Array.from(groups.entries()).map(([name, vals]) => {
        let value: number;
        if (mode === 'avg') {
            value = vals.reduce((a, b) => a + b, 0) / vals.length;
        } else if (mode === 'sum') {
            value = vals.reduce((a, b) => a + b, 0);
        } else {
            value = vals.length;
        }
        return { name, value: Math.round(value * 100) / 100 };
    }).filter(item => item.value > 0);
}

function aggregateByBin(
    data: Record<string, unknown>[],
    binKey: string,
    binSize?: number
): Array<{ name: string; value: number }> {
    const values = data
        .map(d => Number(d[binKey]))
        .filter(v => !Number.isNaN(v));

    if (values.length === 0) return [];

    const max = Math.max(...values);
    const min = Math.min(...values);
    const size = binSize || Math.max(1, Math.ceil((max - min) / 8));

    const groups = new Map<string, number>();
    values.forEach(v => {
        const binStart = Math.floor((v - min) / size) * size + min;
        const label = `${binStart}–${binStart + size}`;
        groups.set(label, (groups.get(label) || 0) + 1);
    });

    return Array.from(groups.entries()).map(([name, value]) => ({ name, value }));
}

function buildHistogramData(data: Record<string, unknown>[], key: string): Array<{ range: string; count: number }> {
    return aggregateByBin(data, key).map(item => ({ range: item.name, count: item.value }));
}

function buildGaugeData(data: Record<string, unknown>[], key: string): Array<{ name: string; value: number }> {
    const total = data.length;
    if (total === 0) return [{ name: 'النسبة', value: 0 }];
    const value = data.filter(d => Boolean(d[key])).length;
    return [{ name: 'النسبة', value: Math.round((value / total) * 100) }];
}

function ChartRenderer({ chart, data }: ChartRendererProps) {
    const { type, dataKey, nameKey, valueKey, title, height, colors, showLegend, showGrid, horizontal, formatValue } = chart;

    const chartData = useMemo(() => {
        switch (type) {
            case 'histogram':
                return dataKey ? buildHistogramData(data, dataKey) : [];
            case 'gauge':
                return dataKey ? buildGaugeData(data, dataKey) : [];
            case 'pie':
            case 'donut':
                return nameKey ? aggregateByKey(data, nameKey, valueKey, 'count') : [];
            case 'bar':
            case 'horizontalBar':
                return nameKey && valueKey && valueKey !== 'count'
                    ? aggregateByKey(data, nameKey, valueKey, 'sum')
                    : nameKey
                        ? aggregateByKey(data, nameKey, undefined, 'count')
                        : [];
            case 'groupedBar':
            case 'stackedBar':
                return nameKey && valueKey && valueKey !== 'count'
                    ? aggregateByKey(data, nameKey, valueKey, 'sum')
                    : nameKey
                        ? aggregateByKey(data, nameKey, undefined, 'count')
                        : [];
            case 'line':
            case 'area':
                return nameKey && valueKey && valueKey !== 'count'
                    ? aggregateByKey(data, nameKey, valueKey, 'sum')
                    : nameKey
                        ? aggregateByKey(data, nameKey, undefined, 'count')
                        : [];
            default:
                return data;
        }
    }, [data, type, dataKey, nameKey, valueKey]);

    const firstColor = colors?.length ? getColorString(colors[0]) : 'hsl(var(--primary))';

    const commonProps = {
        title,
        height: height ?? 300,
        showGrid,
        showLegend,
        formatValue,
    };

    switch (type) {
        case 'pie':
        case 'donut':
            return (
                <ReportPieChart
                    data={chartData}
                    nameKey="name"
                    valueKey="value"
                    innerRadius={type === 'donut' ? 60 : 0}
                    colors={colors?.map(getColorString)}
                    {...commonProps}
                />
            );

        case 'bar':
        case 'horizontalBar':
            return (
                <ReportBarChart
                    data={chartData}
                    xKey="name"
                    yKeys={[{
                        key: 'value',
                        color: firstColor,
                        label: title ?? ''
                    }]}
                    horizontal={type === 'horizontalBar' || horizontal}
                    {...commonProps}
                />
            );

        case 'groupedBar':
        case 'stackedBar':
            return (
                <ReportBarChart
                    data={chartData}
                    xKey="name"
                    yKeys={[{
                        key: 'value',
                        color: firstColor,
                        label: title ?? ''
                    }]}
                    {...commonProps}
                />
            );

        case 'line':
        case 'area':
            return (
                <ReportLineChart
                    data={chartData}
                    xKey="name"
                    lines={[{
                        key: 'value',
                        color: firstColor,
                        label: title ?? ''
                    }]}
                    area={type === 'area'}
                    {...commonProps}
                />
            );

        case 'histogram':
            return (
                <ReportBarChart
                    data={chartData}
                    xKey="range"
                    yKeys={[{ key: 'count', color: firstColor, label: 'التكرار' }]}
                    {...commonProps}
                    showLegend={false}
                />
            );

        case 'gauge':
            return (
                <ReportPieChart
                    data={chartData}
                    nameKey="name"
                    valueKey="value"
                    {...commonProps}
                />
            );

        default:
            return null;
    }
}
