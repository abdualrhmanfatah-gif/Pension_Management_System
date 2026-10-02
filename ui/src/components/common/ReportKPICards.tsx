import React from 'react';

import type { ReportDataRow } from '@/api/reports';
import { Card } from "@/components/ui/Card";
import { kpiToneClass } from "@/lib/utils";

interface ReportKPICardsProps {
    data?: ReportDataRow[];
    columns?: { key: string; label: string; type?: 'currency' | 'number' | 'date' | 'percent' | 'boolean' }[];
    customKPIs?: { label: string; value: string | number; color?: string }[];
    maxCards?: number;
}

const formatValue = (val: number, type?: string) => {
    if (type === 'currency') return new Intl.NumberFormat('ar-YE', { style: 'currency', currency: 'YER' }).format(val);
    if (type === 'percent') return `${val.toFixed(2)}%`;
    return new Intl.NumberFormat('ar-YE').format(val);
};

export const ReportKPICards: React.FC<ReportKPICardsProps> = ({ data, columns, customKPIs, maxCards = 4 }) => {
    // Custom KPIs mode
    if (customKPIs && customKPIs.length > 0) {
        const visibleKPIs = customKPIs.slice(0, maxCards);
        return (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6 text-start">
                {visibleKPIs.map((kpi, idx) => (
                    <Card key={idx} className="p-5">
                        <h4 className="text-caption sm:text-xs font-bold text-muted-foreground uppercase tracking-wider mb-2">{kpi.label}</h4>
                        <div className={`text-2xl font-black font-mono ${kpiToneClass(kpi.color)}`}>
                            {typeof kpi.value === 'number'
                                ? new Intl.NumberFormat('ar-YE').format(kpi.value)
                                : kpi.value}
                        </div>
                    </Card>
                ))}
            </div>
        );
    }

    // Data-driven mode
    if (!data || data.length === 0 || !columns) return null;

    const numericColumns = columns.filter(c => c.type === 'currency' || c.type === 'number' || c.type === 'percent');
    if (numericColumns.length === 0) return null;

    const topMetrics = numericColumns.slice(0, maxCards - 1);

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6 text-start">
            <Card className="p-5">
                <h4 className="text-caption sm:text-xs font-bold text-muted-foreground uppercase tracking-wider mb-2">إجمالي السجلات</h4>
                <div className="text-2xl font-black text-foreground font-mono">
                    {data.length}
                </div>
            </Card>

            {topMetrics.map((metric, idx) => {
                const total = data.reduce((acc, row) => acc + (Number(row[metric.key]) || 0), 0);
                const val = metric.type === 'percent' ? total / data.length : total;

                return (
                    <Card key={idx} className="p-5">
                        <h4 className="text-caption sm:text-xs font-bold text-muted-foreground uppercase tracking-wider mb-2">{metric.label}</h4>
                        <div className="text-2xl font-black text-primary font-mono">
                            {formatValue(val, metric.type)}
                        </div>
                    </Card>
                );
            })}
        </div>
    );
};
