import { useMemo } from 'react';
import {
    BarChart3, TrendingUp, TrendingDown, Minus,
    FileText, Users, Clock, AlertTriangle, CheckCircle, XCircle,
    Calculator, DollarSign, Percent, Calendar, MapPin,
    Folder, Building2, Shield, Wrench, Link,
    User, AlertOctagon
} from 'lucide-react';

import { StatCard } from '@/components/ui/StatCard';
import type { SummaryCardConfig } from '@/api/reports';

interface ReportSummaryCardsProps {
    cards: SummaryCardConfig[];
    data: Record<string, any>[];
    summaryData?: Record<string, any>;
}

const ICON_MAP: Record<string, React.ComponentType<{ className?: string }>> = {
    BarChart3, TrendingUp, TrendingDown, Minus,
    FileText, Users, Clock, AlertTriangle, CheckCircle, XCircle,
    Calculator, DollarSign, Percent, Calendar, MapPin,
    Folder, Building2, Shield, Wrench, Link, User, AlertOctagon,
};

const COLOR_TO_VARIANT: Record<string, string> = {
    blue: 'blue', green: 'emerald', red: 'rose', yellow: 'amber',
    purple: 'purple', orange: 'orange', cyan: 'cyan', emerald: 'emerald',
    rose: 'rose', amber: 'amber',
};

function getNestedValue(obj: Record<string, any>, key: string): any {
    return key.split('.').reduce((acc, k) => acc?.[k], obj);
}

export function ReportSummaryCards({ cards, data, summaryData }: ReportSummaryCardsProps) {
    const values = useMemo(() => {
        const result: Record<string, number> = {};
        const source = summaryData || {};

        cards.forEach(card => {
            const raw = getNestedValue(source, card.valueKey);
            if (raw !== undefined && raw !== null) {
                result[card.valueKey] = Number(raw) || 0;
                return;
            }
            if (data && data.length > 0) {
                const vals = data.map(d => Number(d[card.valueKey]) || 0);
                const sum = vals.reduce((a, b) => a + b, 0);
                result[card.valueKey] = sum;
            } else {
                result[card.valueKey] = 0;
            }
        });
        return result;
    }, [cards, data, summaryData]);

    if (!cards || cards.length === 0) return null;

    return (
        <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4 mb-6">
            {cards.map((card, index) => {
                const value = values[card.valueKey] ?? 0;
                const formatted = card.format === 'currency'
                    ? `${value.toLocaleString('ar-YE')} ر.ي`
                    : card.format === 'percent'
                        ? `${value.toFixed(1)}%`
                        : card.format === 'date'
                            ? new Date(value).toLocaleDateString('ar-YE')
                            : value.toLocaleString('ar-YE');

                const Icon = card.icon ? (ICON_MAP[card.icon] || BarChart3) : BarChart3;
                const variant = (card.variant || COLOR_TO_VARIANT[card.color || 'blue'] || 'blue') as any;

                return (
                    <StatCard
                        key={index}
                        title={card.title}
                        value={formatted}
                        icon={<Icon className="h-4 w-4" />}
                        description={card.description}
                        variant={variant}
                    />
                );
            })}
        </div>
    );
}
